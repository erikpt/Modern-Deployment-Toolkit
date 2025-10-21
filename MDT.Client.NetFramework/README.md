# MDT.Client.NetFramework - WinPE Client

.NET Framework 3.5 client for executing MDT task sequences in Windows PE.

## Overview

This client is designed to run in Windows PE (WinPE) environments where modern .NET runtimes are not available. It communicates with the MDT .NET 8.0 server for task sequence management and progress reporting.

## Features

- **WinPE Compatible**: Built on .NET Framework 3.5
- **MDT XML Parser**: Parses task sequences from Deployment Workbench
- **Server Communication**: Reports progress to .NET 8.0 server
- **10 Priority Step Executors**: Implements critical MDT steps
- **Variable Management**: Full MDT variable expansion support
- **Condition Evaluation**: Supports MDT condition types

## Supported Step Types

1. **SMS_TaskSequence_RunCommandLineAction** - Execute command-line programs
2. **SMS_TaskSequence_SetVariableAction** - Set task sequence variables
3. **BDD_Validate** - Validate deployment prerequisites
4. **Check BIOS** - Verify BIOS/UEFI settings
5. **SMS_TaskSequence_PartitionDiskAction** - Partition and format disks
6. **Configure** - Configure deployment settings
7. **BDD_InstallOS** - Apply Windows image
8. **ZTIWinRE.wsf** - Configure Windows Recovery Environment
9. **BDD_RunPowerShellAction** - Execute PowerShell scripts
10. **SMS_TaskSequence_RebootAction** - Reboot and resume

## Building

### Requirements
- Visual Studio 2008 or later (with .NET Framework 3.5 SDK)
- Or MSBuild with .NET Framework 3.5 targeting pack

### Build Command
```cmd
msbuild MDT.Client.NetFramework.csproj /p:Configuration=Release
```

## Usage

### Command Line
```cmd
MDT.Client.exe <task-sequence-file> [server-url]
```

### Examples

Run locally without server:
```cmd
MDT.Client.exe C:\Deploy\TaskSequence.xml
```

Run with server communication:
```cmd
MDT.Client.exe C:\Deploy\TaskSequence.xml https://mdt.contoso.com
```

### WinPE Deployment

1. Build the client in Release mode
2. Copy `MDT.Client.exe` to WinPE boot image
3. Add to startup script:
```cmd
X:\Deploy\MDT.Client.exe X:\Deploy\TaskSequence.xml https://your-server
```

## Architecture

### Core Components

- **Core/Models**: Task sequence data models
- **Core/Interfaces**: Step executor and parser interfaces
- **Core/Services**: Variable manager and condition evaluator
- **Parsers**: MDT XML parser
- **Engine**: Task sequence execution engine
- **StepExecutors**: Implementation of MDT step types
- **ApiClient**: Server communication client

### Execution Flow

1. Parse XML task sequence file
2. Initialize variable manager
3. Register step executors
4. Execute steps sequentially
5. Evaluate conditions for each step
6. Report progress to server (if configured)
7. Handle errors and continue-on-error logic

## Limitations

- **No Async/Await**: .NET Framework 3.5 doesn't support async
- **Limited LINQ**: Some LINQ features not available
- **No NuGet**: Cannot use NuGet packages in WinPE
- **Basic JSON**: No modern JSON serialization (would need manual implementation)

## Future Enhancements

- Complete implementation of all step executors
- WMI condition evaluation
- Registry condition evaluation
- JSON serialization for server communication
- Retry logic for network operations
- Enhanced logging to file
- Progress UI for WinPE

## Server Communication

The client can optionally communicate with the MDT .NET 8.0 server:

- **GET** `/api/tasksequence/{id}` - Download task sequence
- **POST** `/api/execution/{id}` - Report execution progress
- **POST** `/api/execution/{id}/step` - Report step completion
- **POST** `/api/execution/{id}/log` - Send log messages

Note: JSON serialization/deserialization needs to be implemented for full server integration.

## Compatibility

- **Windows PE 3.0+** (Windows 7 era and later)
- **.NET Framework 3.5** (included in WinPE by default)
- **MDT 2013+** (task sequence XML format)

## License

MIT License
