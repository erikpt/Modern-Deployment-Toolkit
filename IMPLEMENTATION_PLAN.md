# WinPE-Compatible MDT Client Implementation Plan

## Overview
This document outlines the implementation plan for the WinPE-compatible Modern Deployment Toolkit client based on owner requirements.

## Requirements Summary

### Technical Specifications
- **Server Runtime**: .NET 8.0 (Web API, database, monitoring)
- **Client Runtime**: .NET Framework 3.5 (WinPE compatibility)
- **Communication**: Client → Server via REST API
- **XML Format**: MDT Deployment Workbench task sequence format
- **PowerShell**: Version 2.0+ compatibility required

### Priority Task Sequence Steps (Must-Have)
1. SMS_TaskSequence_RunCommandLineAction
2. SMS_TaskSequence_SetVariableAction
3. BDD_Validate
4. Check BIOS
5. SMS_TaskSequence_PartitionDiskAction
6. Configure
7. BDD_InstallOS
8. ZTIWinRE.wsf
9. BDD_RunPowerShellAction
10. SMS_TaskSequence_RebootAction

## Architecture

### Hybrid Client-Server Model

```
┌─────────────────────────────────────────┐
│         Server Side (.NET 8.0)          │
├─────────────────────────────────────────┤
│ • MDT.WebUI (REST API)                  │
│ • Task Sequence Management              │
│ • Execution Monitoring                  │
│ • Database Storage                      │
│ • Docker/Cloud Deployment               │
└─────────────────────────────────────────┘
                    ▲
                    │ HTTPS/REST
                    ▼
┌─────────────────────────────────────────┐
│    Client Side (.NET Framework 3.5)     │
├─────────────────────────────────────────┤
│ • MDT.Client.NetFramework               │
│   - Task Sequence Engine                │
│   - XML Parser (MDT Format)             │
│   - Step Executors                      │
│   - Server API Client                   │
│   - Progress Reporting                  │
│                                         │
│ • MDT.Client.PowerShell                 │
│   - PowerShell 2.0+ Module              │
│   - Cmdlets for Task Execution          │
│   - Server Communication                │
└─────────────────────────────────────────┘
```

## Implementation Phases

### Phase 1: Foundation & Core Infrastructure

#### 1.1 Project Structure
```
MDT.Client.NetFramework/         (.NET Framework 3.5)
├── Core/
│   ├── Models/
│   │   ├── TaskSequence.cs
│   │   ├── TaskSequenceStep.cs
│   │   ├── TaskSequenceVariable.cs
│   │   └── ExecutionContext.cs
│   ├── Interfaces/
│   │   ├── IStepExecutor.cs
│   │   ├── ITaskSequenceParser.cs
│   │   └── IServerClient.cs
│   └── Services/
│       ├── VariableManager.cs
│       └── ConditionEvaluator.cs
├── Parsers/
│   └── MdtXmlParser.cs           (MDT-specific XML format)
├── Engine/
│   └── TaskSequenceEngine.cs
├── StepExecutors/
│   ├── BaseStepExecutor.cs
│   ├── RunCommandLineExecutor.cs
│   ├── SetVariableExecutor.cs
│   └── [... other executors]
├── ApiClient/
│   └── MdtServerClient.cs
└── Program.cs

MDT.Client.PowerShell/            (PowerShell Module)
├── MDT.Client.psd1               (Module Manifest)
├── MDT.Client.psm1               (Module Script)
└── Cmdlets/
    ├── Invoke-TaskSequence.ps1
    ├── Get-TaskSequence.ps1
    └── Set-MdtVariable.ps1
```

#### 1.2 Core Models (.NET Framework 3.5 Compatible)
- Simple POCO classes (no advanced C# features)
- No LINQ expressions (limited in .NET 3.5)
- Basic collections (List<T>, Dictionary<K,V>)
- No async/await (not available in .NET 3.5)

#### 1.3 XML Parser for MDT Format
- Parse MDT Deployment Workbench XML structure
- Support for all MDT step types
- Variable substitution
- Condition evaluation
- Group/sequence nesting

### Phase 2: Task Sequence Engine

#### 2.1 Core Engine Features
- Sequential step execution
- Variable expansion and management
- Condition evaluation (WMI queries, registry checks)
- Error handling and continue-on-error logic
- Progress reporting to server
- Logging (local and server-side)

#### 2.2 Server Communication
- REST API client (using WebClient for .NET 3.5)
- Task sequence download from server
- Progress updates
- Variable synchronization
- Log upload
- Execution status reporting

### Phase 3: Priority Step Executors

Each executor must:
- Inherit from BaseStepExecutor
- Implement WinPE-compatible logic
- Report progress to server
- Handle errors gracefully
- Log execution details

#### 3.1 SMS_TaskSequence_RunCommandLineAction
**Purpose**: Execute command-line programs
**Implementation**:
- Use Process.Start for command execution
- Capture stdout/stderr
- Handle exit codes
- Timeout management
- Working directory support

#### 3.2 SMS_TaskSequence_SetVariableAction
**Purpose**: Set task sequence variables
**Implementation**:
- Update variable manager
- Sync with server
- Support for secret variables
- Persistence across reboots

#### 3.3 BDD_Validate
**Purpose**: Validate deployment prerequisites
**Implementation**:
- Check WMI properties
- Verify network connectivity
- Validate storage space
- Check BIOS settings
- Report validation results

#### 3.4 Check BIOS
**Purpose**: Verify BIOS/UEFI settings
**Implementation**:
- WMI BIOS queries
- UEFI detection
- Secure Boot status
- TPM verification
- Virtualization settings

#### 3.5 SMS_TaskSequence_PartitionDiskAction
**Purpose**: Partition and format disks
**Implementation**:
- DiskPart.exe wrapper
- UEFI/BIOS partition schemes
- Volume formatting
- Drive letter assignment

#### 3.6 Configure
**Purpose**: Configure deployment settings
**Implementation**:
- Read configuration from XML
- Apply computer name
- Domain join settings
- OU placement
- Network configuration

#### 3.7 BDD_InstallOS
**Purpose**: Apply Windows image
**Implementation**:
- DISM.exe wrapper
- WIM file application
- Image index selection
- Driver injection
- Windows Setup automation

#### 3.8 ZTIWinRE.wsf
**Purpose**: Configure Windows Recovery Environment
**Implementation**:
- WinRE configuration
- Recovery partition setup
- Boot configuration
- REAgentC.exe wrapper

#### 3.9 BDD_RunPowerShellAction
**Purpose**: Execute PowerShell scripts
**Implementation**:
- PowerShell.exe invocation
- Script parameter passing
- Execution policy handling
- Output capture
- Error handling

#### 3.10 SMS_TaskSequence_RebootAction
**Purpose**: Reboot computer and resume task sequence
**Implementation**:
- Shutdown.exe wrapper
- State persistence
- Resume logic
- Reboot countdown
- Force reboot option

### Phase 4: PowerShell Module

#### 4.1 Core Cmdlets
```powershell
# Main execution cmdlet
Invoke-TaskSequence -ServerUrl "https://server" -TaskSequenceId "TS001"

# Variable management
Get-MdtVariable -Name "ComputerName"
Set-MdtVariable -Name "ComputerName" -Value "PC001"

# Server interaction
Get-MdtTaskSequence -ServerUrl "https://server"
Send-MdtProgress -Status "Installing OS" -Percent 50

# Logging
Write-MdtLog -Message "Starting deployment" -Level Info
```

#### 4.2 PowerShell 2.0 Compatibility
- No advanced functions (if needed, use simple functions)
- No workflows
- No classes
- Basic error handling with try/catch
- Compatible with WinPE PowerShell

### Phase 5: Testing & Validation

#### 5.1 Unit Testing
- Cannot use modern test frameworks in .NET 3.5
- Manual testing scripts
- PowerShell Pester tests for module

#### 5.2 Integration Testing
- Test with actual MDT XML examples
- Server communication tests
- End-to-end deployment scenarios

#### 5.3 WinPE Testing
- Create WinPE boot media
- Test in VM environment
- Validate all step executors
- Network communication tests
- Performance benchmarks

#### 5.4 Compatibility Testing
- Test with various WinPE versions
- Multiple Windows versions (7, 10, 11)
- BIOS and UEFI systems
- Virtual and physical hardware

## Dependencies

### .NET Framework 3.5 Client
- No external NuGet packages (not available in WinPE)
- Built-in XML parser (System.Xml)
- Built-in HTTP client (System.Net.WebClient)
- Standard .NET Framework 3.5 libraries

### PowerShell Module
- PowerShell 2.0+ (built-in to WinPE)
- No external modules
- Pure PowerShell scripts

## Deployment

### Client Deployment to WinPE
1. Compile .NET Framework 3.5 executable
2. Copy to WinPE boot image
3. Include in MDT deployment share
4. Add to WinPE startup scripts

### PowerShell Module Deployment
1. Copy module files to WinPE
2. Import module in task sequence
3. Use cmdlets in MDT scripts

### Server Deployment
- Existing .NET 8.0 server remains unchanged
- Docker deployment
- Cloud hosting options

## Timeline Estimate

- **Phase 1 (Foundation)**: 2-3 days
- **Phase 2 (Engine)**: 2-3 days
- **Phase 3 (Executors)**: 5-7 days
- **Phase 4 (PowerShell)**: 2-3 days
- **Phase 5 (Testing)**: 3-5 days

**Total**: 14-21 days

## Success Criteria

- [ ] Client runs in WinPE without .NET 8.0 runtime
- [ ] Successfully parses MDT Deployment Workbench XML files
- [ ] All 10 priority step executors working
- [ ] Client communicates with .NET 8.0 server
- [ ] PowerShell module provides easy-to-use interface
- [ ] Complete end-to-end deployment test successful
- [ ] Documentation complete

## Open Questions

1. ~~Should server be .NET 8.0 or .NET Framework?~~ ✅ .NET 8.0 confirmed
2. ~~What .NET Framework version for client?~~ ✅ .NET Framework 3.5 confirmed
3. ~~Client/server or standalone?~~ ✅ Client/server confirmed
4. ~~MDT XML format examples?~~ ⏳ Waiting for `/DeployXMLExamples` upload
5. ~~Priority step executors?~~ ✅ 10 steps confirmed

## Notes

- **WinPE Constraints**: Limited APIs, no GUI framework, restricted file access
- **Network**: Must handle network connectivity issues gracefully
- **.NET 3.5 Limitations**: No async/await, limited LINQ, older C# syntax
- **Testing**: Requires actual WinPE environment for validation
- **Compatibility**: Must work with existing MDT infrastructure

## References

- MDT Documentation: https://docs.microsoft.com/en-us/mem/configmgr/mdt/
- WinPE Documentation: https://docs.microsoft.com/en-us/windows-hardware/manufacture/desktop/winpe-intro
- .NET Framework 3.5: https://docs.microsoft.com/en-us/dotnet/framework/
