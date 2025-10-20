using Microsoft.AspNetCore.Mvc;
using MDT.Core.Interfaces;
using MDT.Core.Models;
using MDT.TaskSequence.Parsers;
using MDT.TaskSequence.Executors;

namespace MDT.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskSequenceController : ControllerBase
{
    private readonly ILogger<TaskSequenceController> _logger;
    private readonly TaskSequenceEngine _engine;
    private readonly IEnumerable<ITaskSequenceParser> _parsers;

    public TaskSequenceController(
        ILogger<TaskSequenceController> logger,
        TaskSequenceEngine engine,
        IEnumerable<ITaskSequenceParser> parsers)
    {
        _logger = logger;
        _engine = engine;
        _parsers = parsers;
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
}

public class ParseRequest
{
    public string Content { get; set; } = string.Empty;
}
