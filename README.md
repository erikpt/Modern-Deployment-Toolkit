# Modern Deployment Toolkit (MDT)

A modern, cloud-native Task Sequence Processing Engine for zero-touch OS deployments, designed to process MDT Task Sequences with enhanced capabilities.

## Features

- **Multiple Format Support**: Parse task sequences from XML, JSON, and YAML formats
- **Variable Management**: Dynamic variable expansion and management with read-only and secret variable support
- **Condition Evaluation**: Flexible condition evaluation engine supporting multiple operators
- **Modular Plugin Architecture**: Extensible step executor system for custom deployment steps
- **Parallel Execution**: Support for parallel task execution for faster deployments
- **Web API**: RESTful API for managing and monitoring task sequence executions
- **Database Support**: SQLite and PostgreSQL database backends for configuration storage
- **Containerized**: Docker support for easy deployment and scaling
- **Zero-Touch Deployment**: Full ZTI (Zero Touch Installation) capabilities

## Supported Task Sequence Steps

- **OS Deployment**:
  - Apply Windows Image (WIM)
  - Apply FFU Image
  - Format and Partition Disk
  - Install Operating System

- **Software Management**:
  - Install Application
  - Install Drivers
  - Run Command Line
  - Run PowerShell Scripts

- **User State Migration**:
  - Capture User State (USMT)
  - Restore User State (USMT)

- **System Configuration**:
  - Set Variable
  - Restart Computer
  - Group (for organizing steps)

## Architecture

The toolkit is organized into multiple components:

- **MDT.Core**: Core models, interfaces, and services
- **MDT.TaskSequence**: Task sequence parsing and execution engine
- **MDT.Plugins**: Step executor implementations for various deployment tasks
- **MDT.WebUI**: ASP.NET Core Web API for management and monitoring
- **MDT.Engine**: Console application for command-line task sequence execution
- **MDT.Tests**: Comprehensive unit and integration tests

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Docker (optional, for containerized deployment)

### Building the Solution

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running the Web API

```bash
cd MDT.WebUI
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`)

Access Swagger documentation at: `https://localhost:5001/swagger`

### Running the Console Engine

```bash
cd MDT.Engine
dotnet run -- <path-to-task-sequence-file>
```

Example:
```bash
dotnet run -- ../examples/sample-tasksequence.xml
```

### Docker Deployment

Build and run with Docker Compose:

```bash
docker-compose up -d
```

This will start:
- MDT Web API on port 5000 (HTTP) and 5001 (HTTPS)
- PostgreSQL database for configuration storage

## Task Sequence Examples

### XML Format

```xml
<?xml version="1.0" encoding="utf-8"?>
<TaskSequence id="ts-001" name="Windows 11 Deployment" version="1.0.0">
    <description>Deploy Windows 11 Enterprise</description>
    <variables>
        <variable name="OSVersion" value="Windows11" readonly="false" secret="false" />
    </variables>
    <steps>
        <step id="step-001" name="Apply Windows Image" type="ApplyWindowsImage" enabled="true" continueOnError="false">
            <properties>
                <WimPath>\\server\images\Windows11.wim</WimPath>
                <ImageIndex>1</ImageIndex>
                <TargetDrive>C:</TargetDrive>
            </properties>
        </step>
    </steps>
</TaskSequence>
```

### JSON Format

```json
{
  "id": "ts-002",
  "name": "Windows Server 2022 Deployment",
  "steps": [
    {
      "id": "step-001",
      "name": "Apply FFU Image",
      "type": "ApplyFFUImage",
      "properties": {
        "FfuPath": "\\\\server\\images\\Server2022.ffu",
        "TargetDisk": "0"
      }
    }
  ]
}
```

### YAML Format

```yaml
id: ts-003
name: Linux to Windows Migration
steps:
  - id: step-001
    name: Capture User State
    type: CaptureUserState
    properties:
      StorePath: \\server\usmt\%COMPUTERNAME%
```

## API Endpoints

### Task Sequences

- `POST /api/tasksequence/parse` - Parse a task sequence from content
- `POST /api/tasksequence/execute` - Execute a task sequence
- `POST /api/tasksequence/execute-parallel` - Execute a task sequence in parallel
- `POST /api/tasksequence/validate` - Validate a task sequence

### Executions

- `GET /api/execution` - List all executions
- `GET /api/execution/{id}` - Get execution details
- `GET /api/execution/{id}/status` - Get execution status
- `POST /api/execution` - Create a new execution record
- `PUT /api/execution/{id}` - Update execution record
- `DELETE /api/execution/{id}` - Delete execution record

## Configuration

### Database Configuration

The application supports both SQLite and PostgreSQL databases. Configure in `appsettings.json`:

```json
{
  "Database": {
    "UseSqlite": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=mdt.db"
  }
}
```

For PostgreSQL:

```json
{
  "Database": {
    "UseSqlite": false
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mdt;Username=mdt;Password=password"
  }
}
```

## Extending with Custom Steps

Create a custom step executor by implementing `IStepExecutor`:

```csharp
public class CustomStepExecutor : BaseStepExecutor
{
    public CustomStepExecutor(ILogger<CustomStepExecutor> logger) : base(logger) { }

    public override StepType SupportedStepType => StepType.Custom;

    public override async Task<StepExecutionResult> ExecuteAsync(
        TaskSequenceStep step,
        ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Your custom implementation
        return CreateSuccessResult(step);
    }
}
```

Register in the DI container:

```csharp
services.AddTransient<IStepExecutor, CustomStepExecutor>();
```

## Security Considerations

- Secrets can be marked with `isSecret: true` in variables
- API endpoints should be protected with authentication in production
- Database connections should use secure credentials
- File paths should be validated to prevent path traversal attacks

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License.

## Acknowledgments

Built on modern .NET technologies with inspiration from Microsoft Deployment Toolkit (MDT) for enhanced, cloud-native deployment capabilities.
