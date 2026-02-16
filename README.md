# Revit 2026 MCP Server

A Model Context Protocol (MCP) server implementation for Autodesk Revit 2026, enabling AI assistants like Claude to interact with Revit projects through a standardized interface.

## Features

This MCP server provides:

- **Tools**: Execute operations on Revit elements
  - `get_elements` - Get elements from the project, optionally filtered by category
  - `get_element_properties` - Get detailed properties of a specific element
  - `create_element` - Create new elements in the project
  - `modify_element` - Modify properties of existing elements
  - `get_levels` - Get all levels in the project
  - `get_views` - Get all views in the project

- **Resources**: Access Revit project data
  - `revit://project/info` - Project information
  - `revit://project/elements` - All elements in the project
  - `revit://project/levels` - All levels
  - `revit://project/views` - All views

- **Prompts**: Pre-configured prompts for common Revit tasks
  - `revit_project_overview` - Get a comprehensive project overview
  - `element_analysis` - Analyze specific element categories
  - `create_wall` - Guide for creating walls

## Installation

### Prerequisites

- Node.js 18 or higher
- npm or yarn
- Autodesk Revit 2026 (for production use)

### Setup

1. Clone the repository:
```bash
git clone https://github.com/antonhofstader/Revit-2026-MCP-Server.git
cd Revit-2026-MCP-Server
```

2. Install dependencies:
```bash
npm install
```

3. Build the project:
```bash
npm run build
```

## Usage

### With Claude Desktop

Add this configuration to your Claude Desktop config file:

**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`

See `claude_desktop_config.json` in this repository for a template. You'll need to replace `/absolute/path/to/Revit-2026-MCP-Server/` with the actual path to your installation.

Example configuration:

```json
{
  "mcpServers": {
    "revit-2026": {
      "command": "node",
      "args": [
        "/absolute/path/to/Revit-2026-MCP-Server/build/index.js"
      ]
    }
  }
}
```

### Standalone

You can run the server directly:

```bash
npm start
```

## Development

### Watch mode

Run TypeScript compiler in watch mode:

```bash
npm run watch
```

### Architecture

The server uses a bridge pattern to communicate with Revit:

```
Claude/AI Assistant → MCP Server → Revit API Bridge → Revit 2026
```

Currently, the server includes a simulated Revit API bridge for development and testing. For production use with actual Revit projects, you would need to implement a real bridge that connects to the Revit API.

## Implementation Notes

**Note**: This implementation currently uses simulated Revit data for demonstration purposes. To connect to actual Revit 2026:

1. Implement a real `RevitAPIBridge` class that interfaces with Revit's API
2. Consider using:
   - Revit's .NET API for direct integration
   - A WebSocket or HTTP bridge for communication between the Node.js server and a Revit add-in
   - pyRevit for Python-based integration

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

MIT License - see LICENSE file for details

## Acknowledgments

This implementation was developed with assistance from GitHub Copilot.
