# Task Sequence Editor User Guide

## Introduction

The Modern Deployment Toolkit Task Sequence Editor is a web-based visual editor for creating and managing task sequences. It provides an intuitive interface for building complex deployment workflows without manually editing XML, JSON, or YAML files.

## Getting Started

### Accessing the Editor

1. Start the MDT.WebUI application:
   ```bash
   cd MDT.WebUI
   dotnet run
   ```

2. Open your web browser and navigate to:
   - https://localhost:5001 (or http://localhost:5000)

3. The Task Sequence Editor will load automatically

### Interface Overview

The editor consists of four main areas:

1. **Header** - Task sequence metadata (name, description, version) and variables editor
2. **Toolbar** - Actions like New, Import, Export, Save, and Validate
3. **Step Library** (Left Panel) - Available step types you can add
4. **Step Canvas** (Center) - Tree view of your task sequence steps
5. **Properties Panel** (Right Panel) - Edit properties of the selected step

## Creating a Task Sequence

### Basic Information

1. Click **New** to start a fresh task sequence
2. Enter the following in the header:
   - **Name**: A descriptive name for your task sequence (required)
   - **Description**: Detailed information about what the sequence does
   - **Version**: Version number (e.g., 1.0.0)

### Adding Variables

Task sequence variables can be used across all steps:

1. In the Variables section, click **+ Add Variable**
2. Enter:
   - **Name**: Variable name (e.g., "OSVersion")
   - **Value**: Variable value
   - **RO**: Check if the variable is read-only
   - **Secret**: Check if the value should be hidden (passwords, keys)

## Working with Steps

### Adding Steps

1. Browse the **Step Library** on the left
2. Click on a step type to add it to your task sequence
3. The step appears in the canvas and is automatically selected

### Step Types

Each step type has a specific purpose:

- 🗂️ **Group** - Organize related steps together
- 🖥️ **Install Operating System** - Install the OS
- 🖼️ **Apply Windows Image** - Deploy WIM images
- ⚡ **Apply FFU Image** - Deploy FFU images
- 📦 **Install Application** - Install software
- 🔌 **Install Driver** - Install device drivers
- ⬇️ **Capture User State** - Backup user data (USMT)
- ⬆️ **Restore User State** - Restore user data (USMT)
- 💻 **Run Command Line** - Execute commands
- 📝 **Run PowerShell** - Run PowerShell scripts
- ⚙️ **Set Variable** - Set or modify variables
- 🔄 **Restart Computer** - Reboot the system
- 💾 **Format and Partition** - Prepare disks
- 🔧 **Custom** - User-defined step

## Importing and Exporting

### Import Formats

The editor can import task sequences from:
- **XML** - Traditional MDT format
- **JSON** - JSON format
- **YAML** - Modern, human-readable format

### Export Formats

You can export to any of the three supported formats:

- **Export YAML** (recommended) - Clean, readable format
- **Export JSON** - Structured JSON format
- **Export XML** - Compatible with legacy MDT tools

## Validation and Saving

### Validation

Click **Validate** to check:
- All required fields are filled
- The structure is valid
- No obvious errors

### Saving

1. Ensure your task sequence has a name and at least one step
2. Click **Save** in the toolbar
3. The task sequence is saved to the server

## Support and Feedback

For issues, feature requests, or contributions:
- GitHub Repository: Modern-Deployment-Toolkit
- API Documentation: Access Swagger UI at `/swagger`

## Version History

### v1.0.0 (Current)
- Initial release of Task Sequence Editor
- Support for all 14 step types
- Import/Export for XML, JSON, YAML
- Visual step management
- Properties and conditions editing
- Variables management
- Save/Load functionality
