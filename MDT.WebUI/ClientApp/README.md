# MDT Task Sequence Editor - React Frontend

A modern React-based web UI for creating, editing, and managing Modern Deployment Toolkit task sequences.

## Features

- **Visual Editor**: Create and edit task sequences with an intuitive UI
- **Step Library**: Browse and add from 14 different step types
- **Properties Panel**: Edit step properties, conditions, and settings
- **Import/Export**: Support for XML, JSON, and YAML formats
- **Variables Editor**: Manage task sequence variables
- **Validation**: Real-time validation of task sequences
- **Save/Load**: Save and load task sequences to/from the server

## Technology Stack

- **React 18** with TypeScript
- **Vite** for fast development and building
- **Axios** for API communication
- **React Icons** for step type icons
- **CSS Modules** for styling

## Development

### Prerequisites

- Node.js 18+ and npm
- .NET 8.0 SDK (for backend API)

### Setup

```bash
# Install dependencies
npm install

# Start development server (with hot reload)
npm run dev

# Build for production
npm run build
```

### Development Mode

In development mode, the React dev server runs on port 3000 and proxies API requests to the ASP.NET Core backend on port 5000.

1. Start the backend API:
   ```bash
   cd ..
   dotnet run
   ```

2. In a separate terminal, start the React dev server:
   ```bash
   cd ClientApp
   npm run dev
   ```

3. Open http://localhost:3000 in your browser

### Production Build

The production build is automatically created when building the ASP.NET Core project. The built files are placed in the `wwwroot` directory and served by the backend.

## Project Structure

```
ClientApp/
├── public/                 # Static assets
├── src/
│   ├── components/
│   │   ├── Editor/        # Main editor components
│   │   │   ├── TaskSequenceEditor.tsx
│   │   │   ├── StepLibrary.tsx
│   │   │   ├── StepCanvas.tsx
│   │   │   ├── PropertiesPanel.tsx
│   │   │   ├── VariablesEditor.tsx
│   │   │   └── Toolbar.tsx
│   │   └── Common/        # Reusable components
│   │       ├── FileUpload.tsx
│   │       └── StepIcon.tsx
│   ├── models/            # TypeScript type definitions
│   │   └── TaskSequence.ts
│   ├── services/          # API client
│   │   └── taskSequenceService.ts
│   ├── App.tsx
│   └── index.tsx
├── package.json
├── tsconfig.json
└── vite.config.ts
```

## API Integration

The editor communicates with the backend API through the following endpoints:

- `POST /api/tasksequence/import` - Import task sequence from file
- `POST /api/tasksequence/export?format={yaml|json|xml}` - Export task sequence
- `GET /api/tasksequence/step-types` - Get available step types
- `POST /api/tasksequence/validate` - Validate task sequence
- `POST /api/tasksequence/save` - Save task sequence
- `GET /api/tasksequence/load/{id}` - Load task sequence by ID
- `GET /api/tasksequence/list` - List all saved task sequences

## Usage Guide

### Creating a New Task Sequence

1. Click the "New" button in the toolbar
2. Enter task sequence name, description, and version
3. Add variables if needed

### Adding Steps

1. Click on a step type in the Step Library (left panel)
2. The step will be added to the canvas
3. Select the step to edit its properties in the Properties Panel (right panel)

### Editing Step Properties

1. Select a step from the canvas
2. Edit the step name, description, and properties in the Properties Panel
3. Toggle "Enabled" and "Continue on Error" checkboxes as needed
4. Add conditions if the step should only run under certain circumstances

### Importing Task Sequences

1. Click "Import File" in the toolbar
2. Select an XML, JSON, or YAML file
3. The task sequence will be loaded into the editor

### Exporting Task Sequences

1. Click one of the export buttons (YAML, JSON, or XML)
2. The file will be downloaded to your browser

### Saving and Loading

1. Click "Save" to save the current task sequence to the server
2. Use the Load function to retrieve previously saved task sequences

## Supported Step Types

- **Group** - Organize steps into logical groups
- **Install Operating System** - Install the OS
- **Apply Windows Image** - Apply WIM image
- **Apply FFU Image** - Apply FFU image
- **Install Application** - Install applications
- **Install Driver** - Install device drivers
- **Capture User State** - Capture user data with USMT
- **Restore User State** - Restore user data with USMT
- **Run Command Line** - Execute commands
- **Run PowerShell** - Execute PowerShell scripts
- **Set Variable** - Set task sequence variables
- **Restart Computer** - Restart the system
- **Format and Partition** - Partition disks
- **Custom** - Custom step with user-defined properties

## Browser Support

- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)

## License

Part of the Modern Deployment Toolkit project.
