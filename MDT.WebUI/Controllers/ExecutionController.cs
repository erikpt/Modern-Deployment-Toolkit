using Microsoft.AspNetCore.Mvc;
using MDT.Core.Models;

namespace MDT.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExecutionController : ControllerBase
{
    private static readonly Dictionary<string, MDT.Core.Models.ExecutionContext> _executions = new();
    private readonly ILogger<ExecutionController> _logger;

    public ExecutionController(ILogger<ExecutionController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAllExecutions()
    {
        return Ok(_executions.Values.OrderByDescending(e => e.StartTime));
    }

    [HttpGet("{id}")]
    public IActionResult GetExecution(string id)
    {
        if (_executions.TryGetValue(id, out var execution))
        {
            return Ok(execution);
        }
        return NotFound();
    }

    [HttpPost]
    public IActionResult CreateExecution([FromBody] MDT.Core.Models.ExecutionContext execution)
    {
        _executions[execution.ExecutionId] = execution;
        return CreatedAtAction(nameof(GetExecution), new { id = execution.ExecutionId }, execution);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateExecution(string id, [FromBody] MDT.Core.Models.ExecutionContext execution)
    {
        if (!_executions.ContainsKey(id))
        {
            return NotFound();
        }

        _executions[id] = execution;
        return Ok(execution);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteExecution(string id)
    {
        if (_executions.Remove(id))
        {
            return NoContent();
        }
        return NotFound();
    }

    [HttpGet("{id}/status")]
    public IActionResult GetExecutionStatus(string id)
    {
        if (_executions.TryGetValue(id, out var execution))
        {
            return Ok(new
            {
                execution.ExecutionId,
                execution.Status,
                execution.StartTime,
                execution.EndTime,
                CurrentStep = execution.CurrentStepId,
                CompletedSteps = execution.StepResults.Count(r => r.Status == ExecutionStatus.Completed),
                FailedSteps = execution.StepResults.Count(r => r.Status == ExecutionStatus.Failed),
                TotalSteps = execution.StepResults.Count
            });
        }
        return NotFound();
    }
}
