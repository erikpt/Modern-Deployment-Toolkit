using Microsoft.AspNetCore.Mvc;
using MDT.Core.Interfaces;
using MDT.Core.Models;
using MDT.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace MDT.WebUI.Controllers;

/// <summary>
/// Controller for managing Windows PE boot media creation and download
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BootMediaController : ControllerBase
{
    private readonly ILogger<BootMediaController> _logger;
    private readonly IBootMediaBuilder _bootMediaBuilder;
    private readonly MdtDbContext _dbContext;

    public BootMediaController(
        ILogger<BootMediaController> logger,
        IBootMediaBuilder bootMediaBuilder,
        MdtDbContext dbContext)
    {
        _logger = logger;
        _bootMediaBuilder = bootMediaBuilder;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Queue a new boot media build
    /// </summary>
    /// <param name="options">Build configuration options</param>
    /// <returns>Build tracking information</returns>
    [HttpPost("build")]
    public async Task<IActionResult> BuildBootMedia([FromBody] BootMediaBuildOptions options)
    {
        try
        {
            _logger.LogInformation("Received boot media build request for architecture {Architecture}", options.Architecture);

            // Validate inputs
            if (string.IsNullOrWhiteSpace(options.ServerUrl))
            {
                return BadRequest("ServerUrl is required");
            }

            if (string.IsNullOrWhiteSpace(options.Architecture))
            {
                options.Architecture = "amd64";
            }

            if (options.Architecture != "amd64" && options.Architecture != "x86")
            {
                return BadRequest("Architecture must be 'amd64' or 'x86'");
            }

            var build = await _bootMediaBuilder.BuildAsync(options);

            return Ok(new
            {
                BuildId = build.Id,
                Status = build.Status.ToString(),
                Progress = build.Progress,
                CurrentStep = build.CurrentStep,
                Message = "Boot media build queued successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue boot media build");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Check the status of a build operation
    /// </summary>
    /// <param name="buildId">Build ID to check</param>
    /// <returns>Current build status</returns>
    [HttpGet("status/{buildId}")]
    public async Task<IActionResult> GetBuildStatus(string buildId)
    {
        try
        {
            var build = await _bootMediaBuilder.GetStatusAsync(buildId);

            if (build == null)
            {
                return NotFound(new { Error = $"Build with ID '{buildId}' not found" });
            }

            return Ok(new
            {
                BuildId = build.Id,
                Status = build.Status.ToString(),
                Progress = build.Progress,
                CurrentStep = build.CurrentStep,
                StartTime = build.StartTime,
                EndTime = build.EndTime,
                ErrorMessage = build.ErrorMessage,
                ResultingBootMediaId = build.ResultingBootMediaId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get build status for {BuildId}", buildId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Download a completed boot media ISO
    /// </summary>
    /// <param name="buildId">Build ID or boot media ID</param>
    /// <returns>ISO file stream</returns>
    [HttpGet("download/{buildId}")]
    public async Task<IActionResult> DownloadBootMedia(string buildId)
    {
        try
        {
            // Try to find as boot media first
            var media = await _dbContext.BootMedias.FindAsync(buildId);

            // If not found, check if it's a build ID and get the resulting media
            if (media == null)
            {
                var build = await _dbContext.BootMediaBuilds.FindAsync(buildId);
                if (build != null && !string.IsNullOrEmpty(build.ResultingBootMediaId))
                {
                    media = await _dbContext.BootMedias.FindAsync(build.ResultingBootMediaId);
                }
            }

            if (media == null)
            {
                return NotFound(new { Error = $"Boot media with ID '{buildId}' not found" });
            }

            if (!System.IO.File.Exists(media.FilePath))
            {
                _logger.LogWarning("ISO file not found at path: {FilePath}", media.FilePath);
                return NotFound(new { Error = "ISO file not found on disk" });
            }

            _logger.LogInformation("Streaming ISO file: {FileName}", media.FileName);

            var stream = new FileStream(media.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            return File(stream, "application/octet-stream", media.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download boot media {BuildId}", buildId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// List all available boot media
    /// </summary>
    /// <returns>List of boot media</returns>
    [HttpGet]
    public async Task<IActionResult> ListBootMedia()
    {
        try
        {
            var mediaList = await _bootMediaBuilder.GetAllAsync();

            return Ok(mediaList.Select(m => new
            {
                Id = m.Id,
                FileName = m.FileName,
                FileSize = m.FileSize,
                FileSizeMB = m.FileSize / (1024.0 * 1024.0),
                Architecture = m.Architecture,
                ServerUrl = m.ServerUrl,
                CreatedDate = m.CreatedDate,
                Status = m.Status,
                IncludedDrivers = m.IncludedDrivers,
                OptionalComponents = m.OptionalComponents
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list boot media");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a boot media and its ISO file
    /// </summary>
    /// <param name="buildId">Build ID or boot media ID to delete</param>
    /// <returns>Success status</returns>
    [HttpDelete("{buildId}")]
    public async Task<IActionResult> DeleteBootMedia(string buildId)
    {
        try
        {
            var deleted = await _bootMediaBuilder.DeleteAsync(buildId);

            if (!deleted)
            {
                return NotFound(new { Error = $"Boot media with ID '{buildId}' not found" });
            }

            _logger.LogInformation("Boot media {BuildId} deleted successfully", buildId);

            return Ok(new { Message = "Boot media deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete boot media {BuildId}", buildId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed information about a specific boot media
    /// </summary>
    /// <param name="buildId">Boot media ID</param>
    /// <returns>Detailed boot media information</returns>
    [HttpGet("{buildId}")]
    public async Task<IActionResult> GetBootMedia(string buildId)
    {
        try
        {
            var media = await _dbContext.BootMedias.FindAsync(buildId);

            if (media == null)
            {
                return NotFound(new { Error = $"Boot media with ID '{buildId}' not found" });
            }

            return Ok(new
            {
                Id = media.Id,
                FileName = media.FileName,
                FilePath = media.FilePath,
                FileSize = media.FileSize,
                FileSizeMB = media.FileSize / (1024.0 * 1024.0),
                Architecture = media.Architecture,
                ServerUrl = media.ServerUrl,
                CreatedDate = media.CreatedDate,
                Status = media.Status,
                IncludedDrivers = media.IncludedDrivers,
                OptionalComponents = media.OptionalComponents,
                BuildLog = media.BuildLog
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get boot media {BuildId}", buildId);
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}
