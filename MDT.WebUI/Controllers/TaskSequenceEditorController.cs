using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MDT.Core.Data;
using MDT.Core.Interfaces;

namespace MDT.WebUI.Controllers
{
    [ApiController]
    public class TaskSequenceEditorController : ControllerBase
    {
        private readonly MdtDbContext _db;
        private readonly IEnumerable<ITaskSequenceParser> _parsers;

        public TaskSequenceEditorController(MdtDbContext db, IEnumerable<ITaskSequenceParser> parsers)
        {
            _db = db;
            _parsers = parsers;
        }

        // Serve the editor UI landing route (redirect to static file)
        [HttpGet("/editor")]
        public IActionResult EditorRoot()
        {
            return Redirect("/editor/index.html");
        }

        // List saved task sequences (id + metadata)
        [HttpGet("api/editor/tasks")]
        public async Task<IActionResult> ListTasks()
        {
            var tasks = await _db.TaskSequences
                .OrderByDescending(t => t.ModifiedDate)
                .Select(t => new {
                    id = t.Id,
                    name = t.Name,
                    description = t.Description,
                    modifiedDate = t.ModifiedDate
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // Get specific task sequence content
        [HttpGet("api/editor/tasks/{id}")]
        public async Task<IActionResult> GetTask(string id)
        {
            var entity = await _db.TaskSequences.FindAsync(id);
            if (entity == null)
                return NotFound();

            return Ok(new {
                id = entity.Id,
                name = entity.Name,
                description = entity.Description,
                content = entity.Content,
                createdDate = entity.CreatedDate,
                modifiedDate = entity.ModifiedDate
            });
        }

        // Create a new task sequence
        [HttpPost("api/editor/tasks")]
        public async Task<IActionResult> CreateTask([FromBody] CreateOrUpdateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { error = "Name is required" });

            var entity = new TaskSequenceEntity
            {
                Name = dto.Name,
                Description = dto.Description ?? string.Empty,
                Content = dto.Content ?? string.Empty,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _db.TaskSequences.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = entity.Id }, new { id = entity.Id });
        }

        // Update existing task sequence (save)
        [HttpPut("api/editor/tasks/{id}")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] CreateOrUpdateDto dto)
        {
            var entity = await _db.TaskSequences.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.Name = dto.Name ?? entity.Name;
            entity.Description = dto.Description ?? entity.Description;
            entity.Content = dto.Content ?? entity.Content;
            entity.ModifiedDate = DateTime.UtcNow;

            _db.TaskSequences.Update(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Delete task sequence
        [HttpDelete("api/editor/tasks/{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var entity = await _db.TaskSequences.FindAsync(id);
            if (entity == null)
                return NotFound();

            _db.TaskSequences.Remove(entity);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        // Validate content by delegating to available parsers
        [HttpPost("api/editor/validate")]
        public IActionResult Validate([FromBody] ValidateDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { error = "Content is required" });

            var errors = new System.Collections.Generic.List<string>();

            foreach (var parser in _parsers)
            {
                try
                {
                    var ts = parser.Parse(dto.Content);
                    if (string.IsNullOrWhiteSpace(ts.Name))
                        errors.Add("Task sequence name is required.");
                    if (ts.Steps == null || ts.Steps.Count == 0)
                        errors.Add("Task sequence must have at least one step.");

                    if (errors.Count > 0)
                        return BadRequest(new { Errors = errors });

                    return Ok(new { Valid = true });
                }
                catch
                {
                    // try next parser
                }
            }

            // If no parser succeeded, return a generic error
            return BadRequest(new { Errors = new[] { "Unable to parse content. Ensure XML/JSON/YAML format." } });
        }

        public class CreateOrUpdateDto
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public string Content { get; set; }
        }

        public class ValidateDto
        {
            public string Content { get; set; }
        }
    }
}