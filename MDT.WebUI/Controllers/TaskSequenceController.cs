using Microsoft.AspNetCore.Mvc;
using MDT.Core.Interfaces;
using MDT.Core.Models;
using MDT.Core.Services;
using MDT.TaskSequence.Parsers;
using MDT.TaskSequence.Executors;
using System.Text;

namespace MDT.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskSequenceController : ControllerBase
{
    private readonly ILogger<TaskSequenceController> _logger;
    private readonly TaskSequenceEngine _engine;
    private readonly IEnumerable<ITaskSequenceParser> _parsers;
    private readonly StepTypeMetadataService _stepTypeMetadataService;
    private static readonly Dictionary<string, Core.Models.TaskSequence> _taskSequenceStore = new();

    public TaskSequenceController(
        ILogger<TaskSequenceController> logger,
        TaskSequenceEngine engine,
        IEnumerable<ITaskSequenceParser> parsers,
        StepTypeMetadataService stepTypeMetadataService)
    {
        _logger = logger;
        _engine = engine;
        _parsers = parsers;
        _stepTypeMetadataService = stepTypeMetadataService;
    }

    [HttpPost("parse")]
    public IActionResult ParseTaskSequence([FromBody] ParseRequest request)
    {
        try
        {
            var parser = _parsers.FirstOrDefault(p => p.CanParse(request.Content));
            if (parser == null)
            {
                return BadRequest("Unable to determine task sequence format");
            }

            var taskSequence = parser.Parse(request.Content);
            return Ok(taskSequence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteTaskSequence([FromBody] Core.Models.TaskSequence taskSequence)
    {
        try
        {
            var result = await _engine.ExecuteAsync(taskSequence);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("execute-parallel")]
    public async Task<IActionResult> ExecuteTaskSequenceParallel(
        [FromBody] Core.Models.TaskSequence taskSequence,
        [FromQuery] int maxParallelism = 4)
    {
        try
        {
            var result = await _engine.ExecuteParallelAsync(taskSequence, maxParallelism);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute task sequence in parallel");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("validate")]
    public IActionResult ValidateTaskSequence([FromBody] Core.Models.TaskSequence taskSequence)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(taskSequence.Name))
        {
            errors.Add("Task sequence name is required");
        }

        if (taskSequence.Steps.Count == 0)
        {
            errors.Add("Task sequence must have at least one step");
        }

        if (errors.Count > 0)
        {
            return BadRequest(new { Errors = errors });
        }

        return Ok(new { Valid = true });
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportTaskSequence(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var parser = _parsers.FirstOrDefault(p => p.CanParse(content));
            if (parser == null)
            {
                return BadRequest("Unable to determine task sequence format");
            }

            var taskSequence = parser.Parse(content);
            return Ok(taskSequence);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("export")]
    public IActionResult ExportTaskSequence([FromBody] Core.Models.TaskSequence taskSequence, [FromQuery] string format = "yaml")
    {
        try
        {
            ITaskSequenceParser? parser = format.ToLowerInvariant() switch
            {
                "yaml" => _parsers.OfType<YamlTaskSequenceParser>().FirstOrDefault(),
                "json" => _parsers.OfType<JsonTaskSequenceParser>().FirstOrDefault(),
                "xml" => _parsers.OfType<XmlTaskSequenceParser>().FirstOrDefault(),
                _ => null
            };

            if (parser == null)
            {
                return BadRequest($"Unsupported format: {format}");
            }

            var serialized = parser.Serialize(taskSequence);
            var contentType = format.ToLowerInvariant() switch
            {
                "yaml" => "text/yaml",
                "json" => "application/json",
                "xml" => "application/xml",
                _ => "text/plain"
            };

            var extension = format.ToLowerInvariant() switch
            {
                "yaml" => "yaml",
                "json" => "json",
                "xml" => "xml",
                _ => "txt"
            };

            var fileName = $"{taskSequence.Name.Replace(" ", "_")}_{taskSequence.Id}.{extension}";
            return File(Encoding.UTF8.GetBytes(serialized), contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("step-types")]
    public IActionResult GetStepTypes()
    {
        try
        {
            var stepTypes = _stepTypeMetadataService.GetAllStepTypes();
            return Ok(stepTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get step types");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("save")]
    public IActionResult SaveTaskSequence([FromBody] Core.Models.TaskSequence taskSequence)
    {
        try
        {
            if (string.IsNullOrEmpty(taskSequence.Id))
            {
                taskSequence.Id = Guid.NewGuid().ToString();
            }

            taskSequence.ModifiedDate = DateTime.UtcNow;
            _taskSequenceStore[taskSequence.Id] = taskSequence;

            return Ok(new { Id = taskSequence.Id, Message = "Task sequence saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("load/{id}")]
    public IActionResult LoadTaskSequence(string id)
    {
        try
        {
            if (_taskSequenceStore.TryGetValue(id, out var taskSequence))
            {
                return Ok(taskSequence);
            }

            return NotFound($"Task sequence with ID '{id}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("list")]
    public IActionResult ListTaskSequences()
    {
        try
        {
            var taskSequences = _taskSequenceStore.Values.Select(ts => new
            {
                ts.Id,
                ts.Name,
                ts.Description,
                ts.Version,
                ts.ModifiedDate
            });

            return Ok(taskSequences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list task sequences");
            return BadRequest(ex.Message);
        }
    }
}

public class ParseRequest
{
    public string Content { get; set; } = string.Empty;
}
