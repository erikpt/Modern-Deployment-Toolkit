using MDT.Core.Models;
using MDT.Core.Services;
using MDT.Core.Interfaces;
using MDT.Core.Data;
using MDT.TaskSequence.Parsers;
using MDT.TaskSequence.Executors;
using MDT.WebUI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace MDT.Tests;

public class TaskSequenceControllerTests
{
    private readonly Mock<ILogger<TaskSequenceController>> _mockLogger;
    private readonly TaskSequenceEngine _engine;
    private readonly List<Core.Interfaces.ITaskSequenceParser> _parsers;
    private readonly StepTypeMetadataService _stepTypeMetadataService;
    private readonly MdtDbContext _dbContext;
    private readonly TaskSequenceController _controller;

    public TaskSequenceControllerTests()
    {
        _mockLogger = new Mock<ILogger<TaskSequenceController>>();
        
        // Create real dependencies for TaskSequenceEngine
        var mockEngineLogger = new Mock<ILogger<TaskSequenceEngine>>();
        var variableManager = new VariableManager();
        var conditionEvaluator = new ConditionEvaluator();
        var stepExecutors = new List<IStepExecutor>();
        
        _engine = new TaskSequenceEngine(stepExecutors, variableManager, conditionEvaluator, mockEngineLogger.Object);
        
        _parsers = new List<Core.Interfaces.ITaskSequenceParser>
        {
            new XmlTaskSequenceParser(),
            new JsonTaskSequenceParser(),
            new YamlTaskSequenceParser()
        };
        _stepTypeMetadataService = new StepTypeMetadataService();
        
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<MdtDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MdtDbContext(options);
        
        _controller = new TaskSequenceController(
            _mockLogger.Object,
            _engine,
            _parsers,
            _stepTypeMetadataService,
            _dbContext
        );
    }

    [Fact]
    public void GetStepTypes_ShouldReturnAllStepTypes()
    {
        var result = _controller.GetStepTypes();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var stepTypes = Assert.IsType<List<StepTypeMetadata>>(okResult.Value);
        Assert.Equal(14, stepTypes.Count);
        Assert.Contains(stepTypes, st => st.Type == "Group");
        Assert.Contains(stepTypes, st => st.Type == "ApplyWindowsImage");
    }

    [Fact]
    public async Task SaveTaskSequence_ShouldSaveAndReturnId()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Description = "Test Description",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var result = await _controller.SaveTaskSequence(taskSequence);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task LoadTaskSequence_NonExistentId_ShouldReturnNotFound()
    {
        var result = await _controller.LoadTaskSequence("non-existent-id");

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void ExportTaskSequence_YamlFormat_ShouldReturnFileResult()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Id = "test-001",
            Name = "Test Sequence",
            Description = "Test Description",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep
                {
                    Name = "Test Step",
                    Type = StepType.SetVariable
                }
            }
        };

        var result = _controller.ExportTaskSequence(taskSequence, "yaml");

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("text/yaml", fileResult.ContentType);
        Assert.Contains("Test_Sequence", fileResult.FileDownloadName);
    }

    [Fact]
    public void ExportTaskSequence_InvalidFormat_ShouldReturnBadRequest()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence"
        };

        var result = _controller.ExportTaskSequence(taskSequence, "invalid");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ImportTaskSequence_ValidYaml_ShouldReturnTaskSequence()
    {
        var yamlContent = @"id: test-001
name: Test Sequence
version: 1.0.0
description: Test Description
variables: []
steps:
  - id: step-001
    name: Test Step
    type: SetVariable
    enabled: true
    continueOnError: false
    conditions: []
    properties:
      VariableName: TestVar
      VariableValue: TestValue
    childSteps: []";

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlContent));
        var file = new FormFile(stream, 0, stream.Length, "file", "test.yaml")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/yaml"
        };

        var result = await _controller.ImportTaskSequence(file);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var taskSequence = Assert.IsType<Core.Models.TaskSequence>(okResult.Value);
        Assert.Equal("test-001", taskSequence.Id);
        Assert.Equal("Test Sequence", taskSequence.Name);
        Assert.Single(taskSequence.Steps);
    }

    [Fact]
    public void ValidateTaskSequence_ValidSequence_ShouldReturnOk()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var result = _controller.ValidateTaskSequence(taskSequence);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void ValidateTaskSequence_MissingName_ShouldReturnBadRequest()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var result = _controller.ValidateTaskSequence(taskSequence);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void ValidateTaskSequence_NoSteps_ShouldReturnBadRequest()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Steps = new List<TaskSequenceStep>()
        };

        var result = _controller.ValidateTaskSequence(taskSequence);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SaveTaskSequence_WithStatus_ShouldSaveToDatabase()
    {
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Description = "Test Description",
            Version = "1.0.0",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var result = await _controller.SaveTaskSequence(taskSequence, "Testing");

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        
        // Verify it was saved to database
        var saved = await _dbContext.TaskSequences.FirstOrDefaultAsync();
        Assert.NotNull(saved);
        Assert.Equal("Test Sequence", saved.Name);
        Assert.Equal("Testing", saved.Status);
    }

    [Fact]
    public async Task CommitTaskSequence_ShouldPromoteStatus()
    {
        // First save a task sequence in Development
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Description = "Test Description",
            Version = "1.0.0",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var saveResult = await _controller.SaveTaskSequence(taskSequence, "Development");
        var saveOkResult = Assert.IsType<OkObjectResult>(saveResult);
        var saveValue = saveOkResult.Value;
        Assert.NotNull(saveValue);
        
        // Extract Id using reflection
        var idProperty = saveValue.GetType().GetProperty("Id");
        Assert.NotNull(idProperty);
        var savedId = idProperty.GetValue(saveValue)?.ToString();
        Assert.NotNull(savedId);

        // Now commit it to Testing
        var commitRequest = new CommitRequest
        {
            Id = savedId,
            Status = "Testing"
        };

        var commitResult = await _controller.CommitTaskSequence(commitRequest);
        var commitOkResult = Assert.IsType<OkObjectResult>(commitResult);
        
        // Verify status was updated
        var updated = await _dbContext.TaskSequences.FindAsync(savedId);
        Assert.NotNull(updated);
        Assert.Equal("Testing", updated.Status);
    }

    [Fact]
    public async Task CommitTaskSequence_CannotDemote_ShouldReturnBadRequest()
    {
        // Save a task sequence in Production
        var taskSequence = new Core.Models.TaskSequence
        {
            Name = "Test Sequence",
            Description = "Test Description",
            Version = "1.0.0",
            Steps = new List<TaskSequenceStep>
            {
                new TaskSequenceStep { Name = "Step 1", Type = StepType.SetVariable }
            }
        };

        var saveResult = await _controller.SaveTaskSequence(taskSequence, "Production");
        var saveOkResult = Assert.IsType<OkObjectResult>(saveResult);
        var saveValue = saveOkResult.Value;
        Assert.NotNull(saveValue);
        
        // Extract Id using reflection
        var idProperty = saveValue.GetType().GetProperty("Id");
        Assert.NotNull(idProperty);
        var savedId = idProperty.GetValue(saveValue)?.ToString();
        Assert.NotNull(savedId);

        // Try to demote to Development (should fail)
        var commitRequest = new CommitRequest
        {
            Id = savedId,
            Status = "Development"
        };

        var commitResult = await _controller.CommitTaskSequence(commitRequest);
        Assert.IsType<BadRequestObjectResult>(commitResult);
    }

    [Fact]
    public async Task ListTaskSequences_WithStatusFilter_ShouldReturnFiltered()
    {
        // Save multiple task sequences with different statuses
        var ts1 = new Core.Models.TaskSequence
        {
            Name = "Dev Sequence",
            Steps = new List<TaskSequenceStep> { new TaskSequenceStep { Name = "Step", Type = StepType.SetVariable } }
        };
        await _controller.SaveTaskSequence(ts1, "Development");

        var ts2 = new Core.Models.TaskSequence
        {
            Name = "Prod Sequence",
            Steps = new List<TaskSequenceStep> { new TaskSequenceStep { Name = "Step", Type = StepType.SetVariable } }
        };
        await _controller.SaveTaskSequence(ts2, "Production");

        // List only Production sequences
        var result = await _controller.ListTaskSequences("Production");
        var okResult = Assert.IsType<OkObjectResult>(result);
        var sequences = okResult.Value as IEnumerable<object>;
        Assert.NotNull(sequences);
        Assert.Single(sequences);
    }
}
