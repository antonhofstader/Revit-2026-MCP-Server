# Revit 2026 MCP Server - Project Overview

## What is This?

This project provides a complete Model Context Protocol (MCP) server implementation for Autodesk Revit 2026. It enables AI assistants like Claude to interact with Revit projects through natural language, allowing users to query BIM data, modify parameters, create views, and automate workflows without manual coding.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                      User (Natural Language)                 │
└─────────────────────┬───────────────────────────────────────┘
                      │
┌─────────────────────▼───────────────────────────────────────┐
│                   Claude AI Assistant                        │
│             (Interprets requests, calls tools)               │
└─────────────────────┬───────────────────────────────────────┘
                      │ MCP Protocol (stdio/JSON-RPC)
┌─────────────────────▼───────────────────────────────────────┐
│              Python MCP Server (revit_mcp_server.py)         │
│    • Defines resources (project info, elements, views)       │
│    • Defines tools (query, modify, create, export)           │
│    • Translates between MCP protocol and Revit API           │
└─────────────────────┬───────────────────────────────────────┘
                      │ Named Pipe / pythonnet
┌─────────────────────▼───────────────────────────────────────┐
│          C# Revit Add-in (RevitMCPAddin.dll)                 │
│    • Runs inside Revit process                               │
│    • Executes commands via ExternalEvent                     │
│    • Uses Revit API directly (transactions, queries, etc.)   │
└─────────────────────┬───────────────────────────────────────┘
                      │ Revit API
┌─────────────────────▼───────────────────────────────────────┐
│                 Autodesk Revit 2026                          │
│              (The actual BIM application)                    │
└─────────────────────────────────────────────────────────────┘
```

## File Structure

### Python Files

#### `revit_mcp_server.py` (Main Server)
- **Purpose**: MCP protocol implementation
- **Key Components**:
  - `RevitConnection`: Manages connection to Revit
  - `list_resources()`: Exposes Revit data as MCP resources
  - `list_tools()`: Defines available commands
  - `call_tool()`: Executes commands and returns results
  - `main()`: Server entry point

#### `setup.py` (Installation Script)
- Checks prerequisites (Python version, Windows)
- Creates virtual environment
- Installs dependencies
- Copies add-in files to Revit folder
- Creates configuration examples

#### `test_revit_mcp_server.py` (Test Suite)
- Tests for RevitConnection class
- Tests for MCP server functionality
- Tests for tool definitions and schemas
- Can run without Revit installed

### C# Files

#### `Application.cs` (Revit Add-in Entry Point)
- **Purpose**: Revit application lifecycle management
- **Key Components**:
  - `OnStartup()`: Initializes add-in when Revit starts
  - `OnShutdown()`: Cleanup when Revit closes
  - `CreateRibbonPanel()`: Adds UI to Revit ribbon
  - `StartPipeServer()`: Listens for Python server connections
  - `HandleClient()`: Processes incoming requests

#### `MCPCommandHandler.cs` (Command Execution)
- **Purpose**: Executes MCP commands within Revit's valid context
- **Key Components**:
  - `Execute()`: Main dispatcher for all commands
  - `GetElementsByCategory()`: Retrieve elements by category
  - `GetElementParameters()`: Read element parameters
  - `SetParameterValue()`: Modify element parameters (with transactions)
  - `CreateView()`: Create new views
  - `CreateWall()`: Create walls with specified dimensions
  - `SetActiveView()`: Switch active view
  - `QueryElements()`: Advanced element filtering
  - `GetProjectInfo()`: Get project metadata
  - `SelectViewType()`: Select random view type
  - Uses Revit API through ExternalEvent for thread-safety

#### `AssemblyInfo.cs`
- Assembly metadata (version, copyright, GUID)

### Configuration Files

#### `pyproject.toml`
- Python project configuration
- Dependencies: mcp, pydantic, pythonnet
- Development tools: pytest, black, mypy, ruff
- Build system configuration

#### `RevitMCPAddin.csproj`
- C# project configuration for Visual Studio
- References to Revit API DLLs
- Build events (auto-copy DLL to Revit addins folder)
- NuGet package references

#### `RevitMCPAddin.addin`
- Revit add-in manifest
- Tells Revit how to load the add-in
- Specifies assembly name, class, and GUID

#### `packages.config`
- NuGet packages (Newtonsoft.Json for JSON serialization)

#### `.gitignore`
- Specifies files to exclude from version control
- Python cache, virtual environments, build artifacts

### Documentation Files

#### `README.md`
- Comprehensive project documentation
- Features, installation, usage examples
- Architecture details, troubleshooting
- Development guidelines

#### `QUICKSTART.md`
- Step-by-step installation guide
- Common issues and solutions
- Example use cases
- Verification steps

#### `LICENSE`
- MIT License terms

## How It Works

### 1. Startup Sequence

1. User starts Revit 2026
2. Revit loads `RevitMCPAddin.dll` (C# add-in)
3. Add-in starts named pipe server
4. User starts Claude Desktop
5. Claude Desktop starts Python MCP server (via config)
6. Python server connects to C# add-in via named pipe
7. System is ready for commands

### 2. Request Flow

1. User types natural language request in Claude: "Get all walls"
2. Claude interprets request and calls MCP tool: `get_elements_by_category`
3. Python server receives tool call via MCP protocol
4. Python server sends command to C# add-in via named pipe
5. C# add-in queues command in `MCPCommandHandler`
6. Command executes in Revit's valid context via ExternalEvent
7. Revit API queries database for wall elements
8. Results return through C# → Python → MCP → Claude
9. Claude presents results in natural language to user

### 3. Tool Execution Example

**User Request**: "Show me all doors on Level 1"

**Tool Call**:
```json
{
  "tool": "query_elements",
  "arguments": {
    "filter_type": "parameter",
    "criteria": {
      "category": "Doors",
      "level": "Level 1"
    }
  }
}
```

**User Request**: "Switch to the 3D view"

**Tool Call**:
```json
{
  "tool": "set_active_view",
  "arguments": {
    "view_type": "ThreeDimensional"
  }
}
```

**C# Execution** (in `MCPCommandHandler.cs`):
```csharp
// For set_active_view command
case "set_active_view":
    result = SetActiveView(app, _currentRequest.Parameters);
    break;

// Implementation
private object SetActiveView(UIApplication app, Dictionary<string, object> parameters)
{
    string viewTypeName = parameters["view_type"].ToString();
    Document doc = app.ActiveUIDocument.Document;
    
    ViewType viewType;
    if (!Enum.TryParse(viewTypeName, out viewType))
    {
        return new { success = false, error = $"Unknown view type: {viewTypeName}" };
    }
    
    var view = new FilteredElementCollector(doc)
        .OfClass(typeof(View))
        .Cast<View>()
        .FirstOrDefault(v => v.ViewType == viewType && v.CanBePrinted && !v.IsTemplate);
    
    if (view == null)
    {
        return new { success = false, error = $"No view found for type: {viewTypeName}" };
    }
    
    app.ActiveUIDocument.RequestViewChange(view);
    return new { success = true, viewId = view.Id.IntegerValue, viewName = view.Name };
}
```

## Key Technologies

### Python Side
- **mcp**: Model Context Protocol SDK
- **pydantic**: Data validation and settings management
- **pythonnet**: Python-.NET interoperability bridge
- **asyncio**: Asynchronous I/O for server operations

### C# Side
- **Revit API**: Official Autodesk Revit API (.NET)
- **System.IO.Pipes**: Named pipes for inter-process communication
- **Newtonsoft.Json**: JSON serialization/deserialization
- **ExternalEvent**: Revit's mechanism for thread-safe operations

## Available Tools

| Tool | Description | Example Use |
|------|-------------|-------------|
| `get_elements_by_category` | Retrieve all elements of a category | "Get all walls" |
| `get_element_parameters` | Read all parameters of an element | "What are the properties of door 101?" |
| `set_parameter_value` | Modify an element parameter | "Set fire rating to 2 hours" |
| `create_view` | Create a new view | "Create a 3D view named HVAC" |
| `create_wall` | Create a new wall element | "Create a 20-meter wall on Level 1" |
| `set_active_view` | Switch to a different active view | "Switch to 3D view" |
| `get_project_info` | Get project metadata | "What's the project name?" |
| `select_view_type` | Select a random view type | "Pick a view type" |
| `export_to_ifc` | Export project to IFC | "Export to IFC4 format" |
| `query_elements` | Advanced element queries | "Find all walls thicker than 8 inches" |
| `cap_tool` | Create cap forms from points/lines | "Create cap form with 4 points" |
| `create_detail_shapes` | Create shapes (rectangle, circle, polygon) as detail lines | "Create circle details on this view" |
| `create_model_shapes` | Create shapes (rectangle, circle, polygon) as model lines | "Create rectangular outline in 3D" |
| `create_symbolic_shapes` | Create shapes (rectangle, circle, polygon) as symbolic lines | "Create hexagon symbols in family" |
| `extrusion_tool` | Create extrusion forms with direction | "Extrude profile 15 feet up" |
| `plane_tool` | Create sketch planes in World XYZ | "Create XY plane at Z=10" |
| `model_curve_tool` | Draw curves with math formulas | "Draw sine wave z=sin(x)" |
| `divided_surface_tool` | Create divided surfaces on forms | "Divide form face into 10x8 grid" |
| `dimension_tool` | Create linear dimensions | "Dimension between these two walls" |
| `family_manager_tool` | Manage family types, parameters, formulas | "Create new family type called 'Type A'" |
| `application_document_tool` | Create new documents from templates | "Create new family from Metric Generic Model template" |

## Available Resources

| Resource | Description | Data Format |
|----------|-------------|-------------|
| `revit://project/info` | Project metadata | JSON |
| `revit://elements/all` | All project elements | JSON |
| `revit://views/active` | Current view info | JSON |
| `revit://families/loaded` | Loaded families list | JSON |

## Extension Points

### Adding New Tools

1. **Define tool in Python** (`revit_mcp_server.py`):
```python
Tool(
    name="my_custom_tool",
    description="Does something useful",
    inputSchema={...}
)
```

2. **Handle tool call in Python**:
```python
elif name == "my_custom_tool":
    # Process arguments and send to C#
```

3. **Implement in C#** (`MCPCommandHandler.cs`):
```csharp
case "my_custom_tool":
    result = MyCustomFunction(doc, parameters);
    break;
```

4. **Add function implementation**:
```csharp
private object MyCustomFunction(Document doc, Dictionary<string, object> parameters)
{
    using (Transaction trans = new Transaction(doc, "My Custom Action"))
    {
        trans.Start();
        // Your Revit API code here
        trans.Commit();
        return result;
    }
}
```

## Security Considerations

- **No authentication**: Current implementation has no access control
- **Full API access**: Tools can modify any part of the Revit model
- **Local only**: Designed for single-user, local machine use
- **Future enhancement**: Add user authentication and permission system

## Performance Tips

1. **Use filtered collectors**: Don't query all elements when you need specific ones
2. **Cache project info**: Store frequently accessed data
3. **Batch operations**: Group multiple changes in single transaction
4. **Pagination**: For large result sets, implement offset/limit
5. **Asynchronous where possible**: Use async/await in Python server

## Debugging

### Python Server
```bash
# Run with debug logging
python revit_mcp_server.py --log-level DEBUG

# Or set in code
logging.basicConfig(level=logging.DEBUG)
```

### C# Add-in
```csharp
// Add to Application.cs or MCPCommandHandler.cs
System.Diagnostics.Debug.WriteLine($"Debug message: {variable}");

// View output in Visual Studio Output window or DebugView
```

### MCP Protocol
- Use Claude Desktop developer tools
- Check `%APPDATA%\Claude\logs\` for MCP communication logs

## Next Steps for Development

### Immediate
1. Implement full pythonnet integration
2. Add error handling and validation
3. Write comprehensive tests
4. Add logging throughout

### Short Term
1. Complete query_elements implementation
2. Finish IFC export functionality
3. Add more view management features
4. Create sample projects and workflows
5. Write user documentation

### Long Term
1. Add authentication/authorization
2. Implement caching layer
3. Support for multiple Revit versions
4. Web-based dashboard
5. Team collaboration features

## Resources

- **MCP Specification**: https://modelcontextprotocol.io
- **Revit API Docs**: https://www.revitapidocs.com/2026
- **pythonnet**: https://pythonnet.github.io/
- **Anthropic Claude**: https://claude.ai

---

This project is a foundational implementation. Contributions, feedback, and enhancements are welcome!
