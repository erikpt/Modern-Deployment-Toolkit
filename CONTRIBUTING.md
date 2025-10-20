# Contributing to Modern Deployment Toolkit

Thank you for your interest in contributing to the Modern Deployment Toolkit!

## Development Setup

### Prerequisites
- .NET 8.0 SDK or later
- Git
- Docker (optional, for containerized development)
- A code editor (VS Code, Visual Studio, or Rider recommended)

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/Modern-Deployment-Toolkit.git
   cd Modern-Deployment-Toolkit
   ```

3. Restore dependencies:
   ```bash
   dotnet restore
   ```

4. Build the solution:
   ```bash
   dotnet build
   ```

5. Run tests to verify everything works:
   ```bash
   dotnet test
   ```

## Project Structure

```
Modern-Deployment-Toolkit/
├── MDT.Core/              # Core models, interfaces, and services
├── MDT.TaskSequence/      # Task sequence parsing and execution
├── MDT.Plugins/           # Step executor implementations
├── MDT.WebUI/             # ASP.NET Core Web API
├── MDT.Engine/            # Console application
├── MDT.Tests/             # Unit and integration tests
├── examples/              # Sample task sequence files
└── docs/                  # Documentation
```

## Adding New Step Executors

To add a new task sequence step type:

1. Add the step type to `StepType` enum in `MDT.Core/Models/TaskSequenceStep.cs`

2. Create a new executor in `MDT.Plugins/Steps/`:

```csharp
public class MyCustomExecutor : BaseStepExecutor
{
    public MyCustomExecutor(ILogger<MyCustomExecutor> logger) : base(logger) { }

    public override StepType SupportedStepType => StepType.MyCustomStep;

    public override async Task<StepExecutionResult> ExecuteAsync(
        TaskSequenceStep step,
        MDT.Core.Models.ExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        // Implementation here
        return CreateSuccessResult(step);
    }
}
```

3. Register the executor in both `MDT.WebUI/Program.cs` and `MDT.Engine/Program.cs`:

```csharp
builder.Services.AddTransient<IStepExecutor, MyCustomExecutor>();
```

4. Add tests for your executor in `MDT.Tests/`

## Code Style

- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Write unit tests for new functionality

## Testing

### Running Tests

Run all tests:
```bash
dotnet test
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Writing Tests

- Use xUnit for testing
- Follow AAA pattern (Arrange, Act, Assert)
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Mock dependencies using Moq

Example test:
```csharp
[Fact]
public void SetVariable_ShouldStoreVariable()
{
    // Arrange
    var manager = new VariableManager();
    
    // Act
    manager.SetVariable("TestVar", "TestValue");
    
    // Assert
    var value = manager.GetVariable("TestVar");
    Assert.Equal("TestValue", value);
}
```

## Pull Request Process

1. Create a feature branch:
   ```bash
   git checkout -b feature/my-new-feature
   ```

2. Make your changes and commit:
   ```bash
   git add .
   git commit -m "Add: description of changes"
   ```

3. Push to your fork:
   ```bash
   git push origin feature/my-new-feature
   ```

4. Create a Pull Request on GitHub

5. Ensure all CI checks pass

### Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Include tests for new functionality
- Update documentation as needed
- Keep PRs focused and reasonably sized
- Respond to review feedback promptly

## Commit Message Format

Use clear, descriptive commit messages:

- `Add: New feature or functionality`
- `Fix: Bug fix`
- `Update: Changes to existing functionality`
- `Refactor: Code restructuring without behavior change`
- `Docs: Documentation changes`
- `Test: Adding or updating tests`

## Documentation

When adding new features:

- Update README.md if user-facing
- Add XML documentation comments
- Update ARCHITECTURE.md for architectural changes
- Add examples in the `examples/` directory

## Questions or Issues?

- Open an issue for bug reports or feature requests
- Use discussions for questions and general feedback
- Be respectful and constructive in all interactions

## License

By contributing, you agree that your contributions will be licensed under the same license as the project (MIT License).
