using Microsoft.AspNetCore.Mvc;
using MDT.Core.Interfaces;
using MDT.Core.Models;
using MDT.Core.Services;
using MDT.Core.Data;
using MDT.TaskSequence.Parsers;
using MDT.TaskSequence.Executors;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MDT.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskSequenceController : ControllerBase
{
    private readonly ILogger<TaskSequenceController> _logger;
    private readonly TaskSequenceEngine _engine;
    private readonly IEnumerable<ITaskSequenceParser> _parsers;
    private readonly StepTypeMetadataService _stepTypeMetadataService;
    private readonly MdtDbContext _dbContext;

    public TaskSequenceController(
        ILogger<TaskSequenceController> logger,
        TaskSequenceEngine engine,
        IEnumerable<ITaskSequenceParser> parsers,
        StepTypeMetadataService stepTypeMetadataService,
        MdtDbContext dbContext)
    {
        _logger = logger;
        _engine = engine;
        _parsers = parsers;
        _stepTypeMetadataService = stepTypeMetadataService;
        _dbContext = dbContext;
    }

    private bool ValidateStatus(string status, out TaskSequenceStatus parsedStatus, out string errorMessage)
    {
        if (!Enum.TryParse<TaskSequenceStatus>(status, true, out parsedStatus))
        {
            errorMessage = $"Invalid status. Must be one of: {string.Join(", ", Enum.GetNames<TaskSequenceStatus>())}";
            return false;
        }
        errorMessage = string.Empty;
        return true;
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
    public async Task<IActionResult> SaveTaskSequence([FromBody] Core.Models.TaskSequence taskSequence, [FromQuery] string status = "Development")
    {
        try
        {
            // Validate status
            if (!ValidateStatus(status, out var parsedStatus, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            var yamlParser = _parsers.OfType<YamlTaskSequenceParser>().FirstOrDefault();
            if (yamlParser == null)
            {
                return BadRequest("YAML parser not available");
            }

            var isNew = string.IsNullOrEmpty(taskSequence.Id);
            if (isNew)
            {
                taskSequence.Id = Guid.NewGuid().ToString();
                taskSequence.CreatedDate = DateTime.UtcNow;
            }

            taskSequence.ModifiedDate = DateTime.UtcNow;

            var content = yamlParser.Serialize(taskSequence);

            var entity = await _dbContext.TaskSequences.FindAsync(taskSequence.Id);
            if (entity != null)
            {
                entity.Name = taskSequence.Name;
                entity.Description = taskSequence.Description;
                entity.Version = taskSequence.Version;
                entity.Content = content;
                entity.Status = status;
                entity.ModifiedDate = taskSequence.ModifiedDate;
            }
            else
            {
                entity = new TaskSequenceEntity
                {
                    Id = taskSequence.Id,
                    Name = taskSequence.Name,
                    Description = taskSequence.Description,
                    Version = taskSequence.Version,
                    Content = content,
                    Status = status,
                    CreatedDate = taskSequence.CreatedDate,
                    ModifiedDate = taskSequence.ModifiedDate
                };
                _dbContext.TaskSequences.Add(entity);
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new { 
                Id = taskSequence.Id, 
                Status = status,
                Message = $"Task sequence saved successfully to {status}" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("commit")]
    public async Task<IActionResult> CommitTaskSequence([FromBody] CommitRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                return BadRequest("Task sequence ID is required");
            }

            // Validate status
            if (!ValidateStatus(request.Status, out var parsedStatus, out var errorMessage))
            {
                return BadRequest(errorMessage);
            }

            var entity = await _dbContext.TaskSequences.FindAsync(request.Id);
            if (entity == null)
            {
                return NotFound($"Task sequence with ID '{request.Id}' not found");
            }

            // Validate promotion path: Development -> Testing -> Production
            if (!Enum.TryParse<TaskSequenceStatus>(entity.Status, true, out var currentStatus))
            {
                currentStatus = TaskSequenceStatus.Development; // Default to Development if invalid
            }

            if (parsedStatus < currentStatus)
            {
                return BadRequest($"Cannot demote task sequence from {entity.Status} to {request.Status}");
            }

            entity.Status = request.Status;
            entity.ModifiedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new { 
                Id = entity.Id,
                PreviousStatus = currentStatus.ToString(),
                NewStatus = request.Status,
                Message = $"Task sequence committed to {request.Status}" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to commit task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("load/{id}")]
    public async Task<IActionResult> LoadTaskSequence(string id)
    {
        try
        {
            var entity = await _dbContext.TaskSequences.FindAsync(id);
            if (entity == null)
            {
                return NotFound($"Task sequence with ID '{id}' not found");
            }

            var yamlParser = _parsers.OfType<YamlTaskSequenceParser>().FirstOrDefault();
            if (yamlParser == null)
            {
                return BadRequest("YAML parser not available");
            }

            var taskSequence = yamlParser.Parse(entity.Content);
            
            return Ok(new
            {
                TaskSequence = taskSequence,
                Status = entity.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load task sequence");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListTaskSequences([FromQuery] string? status = null)
    {
        try
        {
            var query = _dbContext.TaskSequences.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (!ValidateStatus(status, out _, out var errorMessage))
                {
                    return BadRequest(errorMessage);
                }
                query = query.Where(ts => ts.Status == status);
            }

            var taskSequences = await query
                .OrderByDescending(ts => ts.ModifiedDate)
                .Select(ts => new
                {
                    ts.Id,
                    ts.Name,
                    ts.Description,
                    ts.Version,
                    ts.Status,
                    ts.CreatedDate,
                    ts.ModifiedDate
                })
                .ToListAsync();

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

public class CommitRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    public string Id { get; set; } = string.Empty;
    
    [System.ComponentModel.DataAnnotations.Required]
    public string Status { get; set; } = string.Empty;
}
