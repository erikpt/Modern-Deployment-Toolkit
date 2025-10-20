# Modern Deployment Toolkit - Architecture

## Overview

The Modern Deployment Toolkit (MDT) is built on a modular, extensible architecture designed for cloud-native deployments and zero-touch OS installations.

## Core Components

### 1. MDT.Core

The foundational layer containing core models, interfaces, and services.

**Key Components:**
- **Models**: Task sequences, steps, variables, conditions, execution contexts
- **Interfaces**: Contracts for parsers, executors, and managers
- **Services**: Variable manager, condition evaluator
- **Data**: Entity Framework Core database context

**Design Patterns:**
- Repository pattern for data access
- Strategy pattern for parsers and executors
- Dependency injection throughout

### 2. MDT.TaskSequence

Task sequence processing and execution engine.

**Key Components:**
- **Parsers**: XML, JSON, YAML format support
- **Executors**: Task sequence engine with parallel execution support

**Features:**
- Multi-format parsing with auto-detection
- Sequential and parallel execution modes
- Step-by-step execution tracking
- Error handling and rollback support

### 3. MDT.Plugins

Extensible plugin system for step executors.

**Built-in Executors:**
- ApplyWindowsImageExecutor
- ApplyFFUImageExecutor
- InstallApplicationExecutor
- InstallDriverExecutor
- CaptureUserStateExecutor
- RestoreUserStateExecutor
- RunCommandLineExecutor
- SetVariableExecutor

**Extension Model:**
All executors inherit from `BaseStepExecutor` and implement `IStepExecutor` interface.

### 4. MDT.WebUI

ASP.NET Core Web API for management and monitoring.

**Features:**
- RESTful API endpoints
- Swagger/OpenAPI documentation
- CORS support for web clients
- Database-backed configuration

**Controllers:**
- TaskSequenceController: Parse, execute, validate task sequences
- ExecutionController: Monitor and manage executions

### 5. MDT.Engine

Console application for command-line execution.

**Features:**
- File-based task sequence execution
- Progress reporting
- Exit codes for automation
- Support for all task sequence formats

## Data Flow

```
Task Sequence File (XML/JSON/YAML)
    ↓
Parser (Auto-detect format)
    ↓
Task Sequence Model
    ↓
Task Sequence Engine
    ↓
Step Executors (Plugins)
    ↓
Execution Results
    ↓
Database/Logs
```

## Execution Model

### Sequential Execution

1. Load task sequence
2. Initialize variables from sequence
3. Iterate through steps in order
4. For each step:
   - Check if enabled
   - Evaluate conditions
   - Find appropriate executor
   - Execute step
   - Collect results
   - Handle errors
5. Return execution context with results

### Parallel Execution

1. Load task sequence
2. Initialize variables from sequence
3. Create parallel execution pool
4. Execute independent steps in parallel
5. Collect and merge results
6. Return execution context with results

## Variable Management

Variables support:
- Dynamic expansion with `%variable%` syntax
- Read-only variables
- Secret variables (marked but not encrypted in this version)
- Case-insensitive names
- String values only

## Condition Evaluation

Supported operators:
- Equals
- NotEquals
- GreaterThan
- LessThan
- Contains
- Exists

All conditions in a step must be true (AND logic) for the step to execute.

## Database Schema

### TaskSequences Table
- Id (PK)
- Name
- Description
- Content (serialized task sequence)
- CreatedDate
- ModifiedDate

### Executions Table
- Id (PK)
- TaskSequenceId (FK)
- Status
- StartTime
- EndTime
- Variables (JSON)
- Results (JSON)

## Plugin Architecture

Custom step executors can be added by:

1. Creating a class that inherits from `BaseStepExecutor`
2. Implementing required methods
3. Registering in DI container

Example:
```csharp
public class CustomExecutor : BaseStepExecutor
{
    public override StepType SupportedStepType => StepType.Custom;
    
    public override async Task<StepExecutionResult> ExecuteAsync(...)
    {
        // Implementation
    }
}
```

## Security Architecture

Current implementation:
- Basic input validation
- No authentication/authorization (add in production)
- Database connection string configuration
- Variable secrets marking (not encrypted)

Production recommendations:
- Add JWT/OAuth authentication
- Implement authorization policies
- Encrypt secret variables
- Add audit logging
- Validate and sanitize all inputs
- Use HTTPS only
- Implement rate limiting

## Scalability Considerations

The architecture supports:
- Horizontal scaling of Web API
- Database-backed state management
- Containerized deployment
- Cloud-native patterns

Future enhancements:
- Distributed task execution
- Message queue integration
- Caching layer
- Load balancing
- Health checks and monitoring

## Integration Points

The toolkit can integrate with:
- CI/CD pipelines (via API or CLI)
- Configuration management systems
- Monitoring and alerting systems
- File storage systems (network shares, cloud storage)
- Identity providers (for authentication)

## Technology Stack

- **.NET 8.0**: Modern, cross-platform framework
- **Entity Framework Core**: ORM for database access
- **ASP.NET Core**: Web API framework
- **YamlDotNet**: YAML parsing
- **Newtonsoft.Json**: JSON parsing
- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **Docker**: Containerization
- **PostgreSQL/SQLite**: Database backends
