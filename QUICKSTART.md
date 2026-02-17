# Quick Start Guide - Revit MCP Server

This guide will help you get the Revit MCP Server up and running quickly.

## Prerequisites Checklist

- [ ] Windows 10/11
- [ ] Python 3.10 or higher installed
- [ ] Autodesk Revit 2026 installed
- [ ] Visual Studio 2019 or later (for building C# add-in, but it has been added to the folder structure)
- [ ] Claude Desktop app installed

## Installation Steps

### 1. Install Python Dependencies

```bash
# Navigate to the project directory
cd revit-mcp-server

# Run the setup script
python setup.py
```

This will:
- Create a virtual environment
- Install all Python dependencies
- Copy add-in files to Revit folder
- Create example configuration

### 2. Build the Revit Add-in

```bash
# Open the C# project in Visual Studio
start RevitMCPAddin.csproj
```

In Visual Studio:
1. Restore NuGet packages (right-click solution → Restore NuGet Packages)
2. Build the solution (press F7 or Build → Build Solution)
3. The DLL will automatically copy to `%APPDATA%\Autodesk\Revit\Addins\2026\`

### 3. Configure Claude Desktop

Edit `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "revit": {
      "command": "python",
      "args": [
        "C:\\path\\to\\revit-mcp-server\\revit_mcp_server.py"
      ]
    }
  }
}
```

**Important:** Replace `C:\\path\\to\\revit-mcp-server\\` with the actual path to your installation.

### 4. Start Everything

1. **Start Revit 2026**
   - Open Revit
   - Verify the MCP Server add-in is loaded (check Add-Ins ribbon)
   - Open any project

2. **Start Claude Desktop**
   - The MCP server will start automatically
   - You should see "revit" in the available tools

## Verify Installation

In Claude Desktop, try these commands:

```
Can you connect to my Revit project?
```

```
What's the project information for my current Revit project?
```

```
Get all walls in my project
```

If you see responses with project data, congratulations! The server is working.

## Common Issues

### Issue: "Cannot find Revit API DLLs"

**Solution:**
- Verify Revit 2026 is installed
- Check the paths in `RevitMCPAddin.csproj`:
  ```xml
  <HintPath>C:\Program Files\Autodesk\Revit 2026\RevitAPI.dll</HintPath>
  ```
- Update paths if Revit is installed elsewhere

### Issue: "Add-in not loading in Revit"

**Solution:**
1. Check if files exist:
   - `%APPDATA%\Autodesk\Revit\Addins\2026\RevitMCPAddin.dll`
   - `%APPDATA%\Autodesk\Revit\Addins\2026\RevitMCPAddin.addin`

2. Check Revit version in the `.addin` file matches your installed version

3. Check Windows Event Viewer for error details

### Issue: "Server not responding in Claude Desktop"

**Solution:**
1. Check the path in `claude_desktop_config.json` is correct
2. Try running the server manually:
   ```bash
   venv\Scripts\python revit_mcp_server.py
   ```
3. Check for error messages in the console

### Issue: "pythonnet import errors"

**Solution:**
- Ensure pythonnet is installed: `pip install pythonnet`
- You may need .NET Framework 4.8 installed
- Try: `python -c "import clr"` to test

## Testing the Installation

Run the test suite:

```bash
# Activate virtual environment
venv\Scripts\activate

# Run tests
pytest test_revit_mcp_server.py -v
```

Tests will show which components are working correctly.

## Next Steps

Once everything is working:

1. **Explore the API** - Try different commands in Claude Desktop
2. **Customize** - Add your own tools in `MCPCommandHandler.cs`
3. **Automate** - Create workflows for common tasks
4. **Share** - Document useful patterns for your team

## Example Use Cases

### Query Project Data
```
How many doors and windows are in the current project?
```

### Modify Parameters
```
Set the fire rating parameter to "2 Hour" for all doors on Level 1
```

### Create Views
```
Create a 3D view named "Structural Analysis" 
```

### Switch Views
```
Switch to the 3D view
```

### Export Data
```
Export the current project to IFC format at C:\exports\project.ifc
```

### Adaptive Family - Cap Forms
```
Create a cap form from 4 points at (0,0,0), (10,0,0), (10,10,0), (0,10,0)
```

### Adaptive Family - Extrusion Forms
```
Create an extrusion from a square profile with 15 feet height
Create a row of 10 boxes along a sine wave curve
```

### Adaptive Family - Model Curves
```
Draw a sine wave with amplitude 3 and frequency 0.5 from x=0 to x=30
Draw a helix with radius 5, pitch 2, and 5 turns
Draw a spiral with initial radius 1 and growth rate 0.3
```

### Adaptive Family - Divided Surfaces
```
Create a divided surface on form face with 10x8 UV divisions
Get all forms in the family to apply divided surfaces
```

### Adaptive Family - Sketch Planes
```
Create an XY plane at Z offset 10 feet
Create a plane from three points
```

### Linear Dimensions
```
Create a dimension between wall A and wall B
Dimension all the grids on grid line row 1
Add a dimension with "TYP" text override
Get all dimension types in the project
```

### Radial Dimensions
```
Create a radius dimension on this arc
Add a diameter dimension to the circle
```

## Getting Help

- **GitHub Issues**: Report bugs and request features
- **Documentation**: See README.md for detailed information
- **MCP Docs**: https://modelcontextprotocol.io
- **Revit API Docs**: https://www.revitapidocs.com/2026

## Advanced Configuration

### Custom Tools

To add your own tools:

1. Add a new case in `MCPCommandHandler.cs` → `Execute()` method
2. Implement your logic using Revit API
3. Add corresponding tool definition in `revit_mcp_server.py` → `list_tools()`
4. Rebuild the C# project

### Logging

Enable detailed logging:

```python
# In revit_mcp_server.py
logging.basicConfig(level=logging.DEBUG)
```

### Performance

For large projects:
- Use filtered element collectors efficiently
- Cache frequently accessed data
- Consider implementing pagination for large result sets

---

**Need more help?** Check the full documentation in README.md or open an issue on GitHub.
