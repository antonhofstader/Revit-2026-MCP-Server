# Revit 2026 MCP Server

This is not an official product from Autodesk. Is a research used for explain the possibilities around the MODEL CONTEXT PROTOCOLS WITH REVIT.
A Model Context Protocol (MCP) server implementation for Autodesk Revit 2026, enabling AI assistants like Claude to interact with Revit projects through a standardized interface.

## Overview

This MCP server provides a bridge between AI assistants and Autodesk Revit 2026, allowing natural language queries and commands to interact with BIM models, extract data, modify parameters, and automate workflows.

## Features

- **Project Information Access**: Query project metadata, statistics, and settings
- **Element Management**: Retrieve, filter, and query elements by category, type, or parameters
- **Parameter Control**: Read and write element parameters (both instance and type)
- **View Management**: Create views, switch active views, create schedules
- **Geometry Creation**: Create walls, grids, lines, curves, splines, and points
- **Curve Operations**: Evaluate, transform, intersect, clone, reverse, offset curves
- **Reference Planes**: Create and query reference planes for dimensioning
- **Graphic Overrides**: Set halftone, colors, transparency, visibility per element/category
- **Export Capabilities**: Export to IFC, DWG, and other formats
- **Advanced Filtering**: Query elements using complex filter criteria
- **Schedule Management**: Create schedules, get table data, modify filters and grouping
- **Element Transformation**: Move, copy, mirror, rotate, create arrays
- **Family Operations**: Add/remove shared parameters in families and projects
- **Selection Tools**: Get/set selection, pick objects, points, faces, edges interactively
- **Task Dialogs**: Display customizable popup dialogs with buttons and command links
- **Ribbon UI**: Create custom ribbon tabs, panels, buttons, combo boxes, and text boxes
- **Mass Family Tools**: Create reference points, curves, loft forms in conceptual mass families
- **Revolve Forms**: Create revolve geometry with axis and profile curves
- **Cap Forms**: Create cap forms from points or lines using NewFormByCap
- **Extrusion Forms**: Create extrusion forms with direction vector, box rows from curves
- **Plane Creation**: Create sketch planes using World XYZ coordinates
- **Model Curves**: Draw lines, arcs, sine waves, spirals, helixes using mathematical formulas
- **Divided Surfaces**: Create divided surfaces on forms with UV grid divisions
- **Linear Dimensions**: Create dimensions between walls, grids, reference planes, and elements
- **Radial Dimensions**: Create radius and diameter dimensions on arcs and circles
- **Family Instances**: Place family instances at points, on hosts, faces, and references
- **Family Modeling**: Create extrusions, blends, revolutions, sweeps, swept blends, loft forms, model text, openings, and symbolic curves in family documents
- **MEP Connectors**: Create duct, pipe, electrical, cable tray, and conduit connectors on family geometry faces

## Prerequisites

- Python 3.10 or higher
- Autodesk Revit 2026 installed
- Windows operating system (required for Revit API)
- .NET Framework 8.0

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/revit-mcp-server.git
cd revit-mcp-server
```

### 2. Create Virtual Environment

```bash
python -m venv venv
venv\Scripts\activate  # On Windows
```

### 3. Install Dependencies

```bash
pip install -e .
```

### 4. Configure Revit Add-in

Copy the Revit add-in files to the Revit addins folder:

```bash
copy revit_addin\*.* "%APPDATA%\Autodesk\Revit\Addins\2026\"
```

## Configuration

### MCP Client Configuration

Add this server to your MCP client configuration (e.g., Claude Desktop):

**Windows (Claude Desktop):**
Edit `%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "revit": {
      "command": "C:\\Users\\username\\Downloads\\Revit2026-MCP\\venv\\Scripts\\python.exe",
      "args": [
        "C:\\Users\\username\\Downloads\\Revit2026-MCP\\revit_mcp_server.py"
      ]
    }
  }
}
```

**macOS/Linux:**
Note: This server requires Windows due to Revit API dependencies.

### Revit Configuration

The server communicates with Revit through a C# add-in. Ensure:
1. Revit 2026 is running
2. The MCP add-in is loaded (check Add-Ins tab)
3. A project is open

## Usage

### Starting the Server

```bash
python revit_mcp_server.py
```

The server will:
1. Attempt to connect to the running Revit instance
2. Listen for MCP protocol messages
3. Provide resources and tools to connected clients

### Available Resources

#### `revit://project/info`
Returns current project information including name, number, author, and status.

#### `revit://elements/all`
Lists all elements in the active project with count and categories.

#### `revit://views/active`
Information about the currently active view.

#### `revit://families/loaded`
All families loaded in the current project.

### Available Tools

#### Element & Project Tools

| Tool | Description |
|------|-------------|
| `get_elements_by_category` | Retrieve all elements of a specific category (Walls, Doors, Windows, etc.) |
| `get_element_parameters` | Get all parameters and values for a specific element |
| `set_parameter_value` | Modify a parameter value for an element |
| `query_elements` | Query elements using filters (category, parameter, type, level) |
| `get_project_info` | Get basic project information |
| `get_selected_elements` | Get currently selected elements in Revit UI |
| `get_last_placed_element` | Get the most recently created element |
| `get_family_types` | Get all family types/symbols with optional filtering |
| `get_instances` | Get all instances of a family, type, or category |
| `find_family_type` | Find a specific family type by name |
| `find_elements` | Find elements using various search criteria |
| `modify_element` | Modify element parameters, location, rotation, flip, pin |

#### View Tools

| Tool | Description |
|------|-------------|
| `create_view` | Create a new view (FloorPlan, Section, Elevation, 3D, Schedule) |
| `set_active_view` | Switch to a different active view |
| `select_view_type` | Get information about available view types |
| `set_graphic_overrides` | Set halftone, colors, transparency, visibility overrides |
| `create_schedule_view` | Create a schedule view for a category |
| `get_table_data` | Get table data from a schedule |
| `modify_schedule` | Modify schedule filters, sorting, grouping |

#### Geometry Creation Tools

| Tool | Description |
|------|-------------|
| `create_wall` | Create a wall with start/end points and level |
| `create_grid_line` | Create a linear grid line |
| `create_grid_arc` | Create an arc grid |
| `create_bounded_line` | Create a model line |
| `create_detail_line` | Create a detail line in active view |
| `create_curves_from_points` | Create connected model curves from points |
| `create_hermite_spline` | Create a Hermite spline from points |
| `create_hermite_spline_with_tangents` | Create a Hermite spline with endpoint tangents |
| `create_point` | Create a point element |
| `create_point_on_element` | Create a point on an element's geometry |
| `create_point_markup` | Create markup symbols (cross, circle, square) |
| `create_reference_plane` | Create a reference plane |
| `get_reference_planes` | Get reference planes from model or element |

#### Curve Operations

| Tool | Description |
|------|-------------|
| `evaluate_curve` | Evaluate point at parameter on curve |
| `curve_distance_to_point` | Get shortest distance from point to curve |
| `curve_get_end_point` | Get start or end point of curve |
| `curve_get_end_parameter` | Get parameter value at curve end |
| `curve_get_end_point_reference` | Get stable reference to curve endpoint |
| `create_clone_curve` | Clone a curve element |
| `create_offset_curve` | Create offset curve |
| `curve_create_reversed` | Create reversed curve |
| `curve_create_transformed` | Create transformed curve |
| `curve_compute_closest_points` | Compute closest points between two curves |
| `curve_compute_derivatives` | Compute derivatives at parameter |
| `curve_compute_normalized_parameter` | Convert raw to normalized parameter |
| `curve_compute_raw_parameter` | Convert normalized to raw parameter |
| `curve_intersect` | Find intersection points between curves |
| `curve_point_location_on_curve` | Project point onto curve |
| `calculate_line_direction` | Calculate direction vector of line |

#### Transform Tools

| Tool | Description |
|------|-------------|
| `rotate_elements` | Rotate elements around an axis |
| `transform_elements` | Move, copy, mirror, or create arrays |

#### Parameter Management

| Tool | Description |
|------|-------------|
| `add_family_shared_parameter` | Add shared parameter to family |
| `remove_family_parameter` | Remove parameter from family |
| `get_family_parameters` | Get all parameters in family |
| `add_project_shared_parameter` | Add shared parameter to project |
| `remove_project_shared_parameter` | Remove shared parameter from project |
| `get_project_shared_parameters` | Get all shared parameters in project |

#### Export Tools

| Tool | Description |
|------|-------------|
| `export_to_ifc` | Export project to IFC format |

#### Interactive Tools

| Tool | Description |
|------|-------------|
| `selection_tool` | Selection operations: get/set/clear selection, pick objects/points/faces/edges |
| `task_dialog` | Display customizable popup dialogs |

#### Ribbon UI Tools

| Tool | Description |
|------|-------------|
| `ribbon_tool` | Create/manage ribbon tabs, panels, buttons, combo boxes, text boxes |

Operations: `create_tab`, `create_panel`, `create_push_button`, `create_split_button`, `create_pulldown_button`, `create_combo_box`, `create_text_box`, `create_stacked_items`, `list_tabs`, `list_panels`, `get_panel_items`, `get_image_folder`, `list_images`

#### Mass/Adaptive Family Tools

| Tool | Description |
|------|-------------|
| `family_points_tool` | Create reference points, curves, and loft forms in mass families |

Operations:
- `create_single_point` - Create a single reference point
- `create_point_row` - Create a row of points
- `create_point_grid` - Create a grid of points
- `create_point_grid_formula` - Create grid with Z = f(x,y) formula
- `get_reference_points` - List all reference points
- `delete_reference_points` - Delete reference points
- `create_curve_by_points` - Create curve through points
- `create_curves_from_grid` - Create curves from point grid
- `get_curves_by_points` - List all CurveByPoints
- `create_loft_form` - Create loft surface from curves
- `get_forms` - List all forms

| Tool | Description |
|------|-------------|
| `revolve_tool` | Create revolve forms with axis and profile curves |

Operations:
- `create_axis_line` - Create axis reference line
- `create_profile_curve` - Create profile CurveByPoints
- `create_revolve` - Create revolve form
- `get_revolve_forms` - List all forms

| Tool | Description |
|------|-------------|
| `cap_tool` | Create cap forms from points or lines using NewFormByCap |

Operations:
- `create_cap_from_points` - Create cap from list of XYZ points (auto-creates lines)
- `create_cap_from_lines` - Create cap from existing model line element IDs
- `get_cap_forms` - List all forms

| Tool | Description |
|------|-------------|
| `extrusion_tool` | Create extrusion forms with direction vector |

Operations:
- `create_extrusion_from_points` - Create extrusion from points with direction
- `create_extrusion_from_lines` - Create extrusion from line IDs with direction
- `create_box_row_from_curves` - Create row of boxes with heights from two curves
- `get_extrusion_forms` - List all forms

| Tool | Description |
|------|-------------|
| `plane_tool` | Create sketch planes using World XYZ coordinates |

Operations:
- `create_plane_by_normal` - Create plane from origin and normal vector
- `create_plane_by_three_points` - Create plane through three points
- `create_xy_plane` - Create horizontal XY plane at Z offset
- `create_xz_plane` - Create vertical XZ plane at Y offset
- `create_yz_plane` - Create vertical YZ plane at X offset
- `get_sketch_planes` - List all SketchPlanes

| Tool | Description |
|------|-------------|
| `model_curve_tool` | Draw model curves with lines, arcs, and mathematical formulas |

Operations:
- `draw_line` - Draw straight line between two points
- `draw_arc` - Draw arc by center/radius/angles or three points
- `draw_curve_by_points` - Draw spline through reference points
- `draw_sine_wave` - Draw z = amplitude * sin(frequency * x)
- `draw_cosine_wave` - Draw z = amplitude * cos(frequency * x)
- `draw_spiral` - Draw 2D Archimedean spiral (r = a + b*theta)
- `draw_helix` - Draw 3D helix with pitch and turns
- `get_model_curves` - List all model curves

| Tool | Description |
|------|-------------|
| `divided_surface_tool` | Create divided surfaces on form faces |

Operations:
- `create_divided_surface` - Create divided surface from form face
- `set_uv_divisions` - Set U and V division counts
- `get_divided_surfaces` - List all divided surfaces
- `get_forms` - List forms available for divided surfaces

| Tool | Description |
|------|-------------|
| `dimension_tool` | Create linear and radial dimensions in projects or families |

Operations:
- `create_linear_dimension` - Create dimension between two XYZ points
- `create_dimension_from_references` - Create dimension from element references
- `create_dimension_between_walls` - Dimension between two walls (center/interior/exterior face)
- `create_dimension_between_grids` - Dimension multiple grid lines
- `create_radial_dimension` - Create radius or diameter dimension on arc/circle
- `modify_dimension` - Set value override, above/below text, prefix/suffix
- `get_dimension_types` - List available dimension types
- `get_dimensions` - List existing dimensions in view

| Tool | Description |
|------|-------------|
| `family_instance_tool` | Place family instances using various methods |

Operations:
- `place_at_point` - Place at XYZ with structural type
- `place_at_point_in_view` - Place at XYZ in specific view
- `place_on_host` - Place on host element (wall/floor/ceiling)
- `place_on_host_with_direction` - Place on host with direction vector
- `place_along_line` - Place on reference along a line
- `place_along_line_in_view` - Place along line in specific view
- `place_on_face` - Place on host face with line
- `place_on_face_at_point` - Place on face at point with direction
- `place_on_reference` - Place on reference at point with direction
- `get_family_symbols` - List available family types

| Tool | Description |
|------|-------------|
| `family_modeling_tool` | Create geometry in family documents (.rfa files) |

Operations:
- `new_extrusion` - Create extrusion from profile points
- `new_blend` - Create blend between bottom and top profiles
- `new_revolution` - Create revolution around an axis
- `new_sweep` - Create sweep along path curves
- `new_swept_blend` - Create swept blend with varying profiles
- `new_loft_form` - Create loft form through multiple profiles
- `new_form_by_cap` - Create cap form from profile curves
- `new_form_by_thicken` - Thicken a surface form
- `new_revolve_form` - Create revolve form (conceptual mass)
- `new_extrusion_form` - Create extrusion form with direction
- `new_swept_blend_form` - Create swept blend form
- `new_model_text` - Create 3D model text
- `new_opening` - Create opening in geometry
- `new_symbolic_curve` - Create symbolic line/arc/circle
- `new_diameter_dimension` - Create diameter dimension on arc
- `get_forms` - List all forms in family
- `get_sketch_planes` - List all sketch planes

| Tool | Description |
|------|-------------|
| `connector_tool` | Create MEP connectors in family documents (.rfa files) |

Operations:
- `create_duct_connector` - Create duct connector on a planar face
- `create_pipe_connector` - Create pipe connector on a planar face
- `create_electrical_connector` - Create electrical connector on a planar face
- `create_cable_tray_connector` - Create cable tray connector on a planar face
- `create_conduit_connector` - Create conduit connector on a planar face
- `change_host_reference` - Change connector to a different face/edge
- `get_connectors` - List all connectors in family

---

## Example Usage

### Query Elements
```python
# Get all walls
{"category": "Walls"}

# Get element parameters
{"element_id": "123456"}
```

### Create Geometry
```python
# Create wall
{
  "start_x": 0, "start_y": 0,
  "end_x": 20, "end_y": 0,
  "level": "Level 1"
}

# Create spline
{
  "points": [
    {"x": 0, "y": 0, "z": 0},
    {"x": 5, "y": 2, "z": 0},
    {"x": 10, "y": 0, "z": 0}
  ]
}
```

### Mass Family Forms
```python
# Create point grid with formula
{
  "operation": "create_point_grid_formula",
  "x": 0, "y": 0,
  "count_x": 10, "count_y": 10,
  "spacing_x": 1.0, "spacing_y": 1.0,
  "z_formula": "5*sin(x*0.5)*cos(y*0.5)"
}

# Create curves from grid and loft
{"operation": "create_curves_from_grid", "curve_direction": "rows"}
{"operation": "create_loft_form", "is_solid": true}
```

### Revolve Form
```python
# Create axis
{
  "operation": "create_axis_line",
  "axis_start_x": 0, "axis_start_y": 0, "axis_start_z": 0,
  "axis_end_x": 0, "axis_end_y": 0, "axis_end_z": 10
}

# Create profile
{
  "operation": "create_profile_curve",
  "profile_points": [
    {"x": 2, "y": 0, "z": 0},
    {"x": 3, "y": 0, "z": 5},
    {"x": 2, "y": 0, "z": 10}
  ]
}

# Create revolve
{
  "operation": "create_revolve",
  "axis_line_id": 12345,
  "profile_curve_id": 12346,
  "start_angle": 0, "end_angle": 360
}
```

### Cap Form
```python
# Create cap from points (auto-creates closed profile)
{
  "operation": "create_cap_from_points",
  "points": [
    {"x": 0, "y": 0, "z": 0},
    {"x": 10, "y": 0, "z": 0},
    {"x": 10, "y": 10, "z": 0},
    {"x": 0, "y": 10, "z": 0}
  ],
  "is_solid": true
}
```

### Extrusion Form
```python
# Create extrusion from points
{
  "operation": "create_extrusion_from_points",
  "points": [
    {"x": 0, "y": 0, "z": 0},
    {"x": 10, "y": 0, "z": 0},
    {"x": 10, "y": 10, "z": 0},
    {"x": 0, "y": 10, "z": 0}
  ],
  "direction_x": 0, "direction_y": 0, "direction_z": 15
}

# Create row of boxes with heights from curves
{
  "operation": "create_box_row_from_curves",
  "bottom_curve_id": 123456,
  "top_curve_id": 123457,
  "box_count": 10,
  "separation": 0.5,
  "box_width": 2.0
}
```

### Model Curves
```python
# Draw sine wave
{
  "operation": "draw_sine_wave",
  "amplitude": 3,
  "frequency": 0.5,
  "start_x": 0,
  "end_x": 30,
  "point_count": 60
}

# Draw helix
{
  "operation": "draw_helix",
  "radius": 5,
  "pitch": 2,
  "turns": 5,
  "point_count": 150
}

# Draw spiral
{
  "operation": "draw_spiral",
  "initial_radius": 1,
  "growth_rate": 0.3,
  "turns": 4
}
```

### Divided Surface
```python
# Get forms available
{"operation": "get_forms"}

# Create divided surface on form face
{
  "operation": "create_divided_surface",
  "form_id": 123456,
  "face_index": 0
}

# Set UV divisions
{
  "operation": "set_uv_divisions",
  "divided_surface_id": 123457,
  "u_divisions": 10,
  "v_divisions": 8
}
```

### Linear Dimension
```python
# Create dimension between two walls
{
  "operation": "create_dimension_between_walls",
  "wall_id_1": 123456,
  "wall_id_2": 123457,
  "face": "center",
  "offset": 3
}

# Create dimension between grids
{
  "operation": "create_dimension_between_grids",
  "grid_ids": [123456, 123457, 123458],
  "offset": 5
}

# Create linear dimension from coordinates
{
  "operation": "create_linear_dimension",
  "start_x": 0, "start_y": 0, "start_z": 0,
  "end_x": 20, "end_y": 0, "end_z": 0,
  "offset": 2
}

# Modify dimension text
{
  "operation": "modify_dimension",
  "dimension_id": 123456,
  "value_override": "TYP",
  "above": "EQ",
  "prefix": "±"
}

# Get available dimension types
{"operation": "get_dimension_types"}

# Create radial dimension on arc
{
  "operation": "create_radial_dimension",
  "arc_element_id": 123456,
  "dimension_style": "radius"
}

# Create diameter dimension
{
  "operation": "create_radial_dimension",
  "arc_element_id": 123456,
  "dimension_style": "diameter",
  "location_x": 10, "location_y": 5, "location_z": 0
}
```

### Family Instance
```python
# Get available family types
{"operation": "get_family_symbols", "category": "Furniture"}

# Place family at point
{
  "operation": "place_at_point",
  "family_symbol_id": 123456,
  "x": 10, "y": 5, "z": 0,
  "structural_type": "NonStructural"
}

# Place family on host (wall, floor, etc.)
{
  "operation": "place_on_host",
  "family_symbol_id": 123456,
  "host_id": 789012,
  "x": 5, "y": 0, "z": 3
}
```

### Family Modeling (in .rfa files)
```python
# Create rectangular extrusion
{
  "operation": "new_extrusion",
  "profile_points": [
    {"x": 0, "y": 0, "z": 0},
    {"x": 10, "y": 0, "z": 0},
    {"x": 10, "y": 5, "z": 0},
    {"x": 0, "y": 5, "z": 0}
  ],
  "extrusion_end": 8,
  "is_solid": true
}

# Create blend between two profiles
{
  "operation": "new_blend",
  "bottom_profile_points": [
    {"x": 0, "y": 0, "z": 0},
    {"x": 10, "y": 0, "z": 0},
    {"x": 10, "y": 10, "z": 0},
    {"x": 0, "y": 10, "z": 0}
  ],
  "top_profile_points": [
    {"x": 2, "y": 2, "z": 10},
    {"x": 8, "y": 2, "z": 10},
    {"x": 8, "y": 8, "z": 10},
    {"x": 2, "y": 8, "z": 10}
  ],
  "is_solid": true
}

# Create revolution (lathe)
{
  "operation": "new_revolution",
  "profile_points": [
    {"x": 2, "y": 0, "z": 0},
    {"x": 3, "y": 0, "z": 5},
    {"x": 2, "y": 0, "z": 10}
  ],
  "axis_start_x": 0, "axis_start_y": 0, "axis_start_z": 0,
  "axis_end_x": 0, "axis_end_y": 0, "axis_end_z": 10,
  "start_angle": 0,
  "end_angle": 360,
  "is_solid": true
}

# Create model text
{
  "operation": "new_model_text",
  "text": "HELLO",
  "depth": 0.5,
  "x": 0, "y": 0, "z": 0,
  "horizontal_align": "center"
}

# Create opening in extrusion
{
  "operation": "new_opening",
  "host_element_id": 123456,
  "profile_points": [
    {"x": 2, "y": 2, "z": 0},
    {"x": 8, "y": 2, "z": 0},
    {"x": 8, "y": 3, "z": 0},
    {"x": 2, "y": 3, "z": 0}
  ]
}

# List forms in family
{"operation": "get_forms"}

# List sketch planes
{"operation": "get_sketch_planes"}
```

### MEP Connectors (in .rfa files)
```python
# Get available connectors in family
{"operation": "get_connectors"}

# Create duct connector on geometry face
{
  "operation": "create_duct_connector",
  "element_id": 123456,
  "face_index": 0,
  "system_type": "SupplyAir",
  "profile_type": "Round"
}

# Create pipe connector
{
  "operation": "create_pipe_connector",
  "element_id": 123456,
  "face_index": 1,
  "system_type": "SupplyHydronic"
}

# Create electrical connector
{
  "operation": "create_electrical_connector",
  "element_id": 123456,
  "face_index": 2,
  "system_type": "PowerCircuit"
}

# Create cable tray connector
{
  "operation": "create_cable_tray_connector",
  "element_id": 123456,
  "face_index": 0
}

# Create conduit connector with edge
{
  "operation": "create_conduit_connector",
  "element_id": 123456,
  "face_index": 0,
  "edge_index": 0
}

# Change connector to a different face
{
  "operation": "change_host_reference",
  "connector_id": 789012,
  "new_element_id": 123456,
  "new_face_index": 3
}
```

## Example Conversations

**Querying Project Data:**
- User: "How many doors are in the project?"
- Claude uses `get_elements_by_category` tool with category "Doors"

**Modifying Elements:**
- User: "Set the fire rating of door 101 to 2 hours"
- Claude queries by mark, then uses `set_parameter_value`

**Creating Views:**
- User: "Create a 3D view called 'HVAC Systems'"
- Claude uses `create_view` tool

**Switching Views:**
- User: "Switch to the 3D view"
- Claude uses `set_active_view` tool with view_type "ThreeDimensional"

**Creating Walls:**
- User: "Create a 20-meter wall on Level 1"
- Claude uses `create_wall` tool with coordinates and level

**Getting Project Info:**
- User: "What's the project name and author?"
- Claude uses `get_project_info` tool

## Architecture

```
┌─────────────────┐
│   AI Assistant  │
│    (Claude)     │
└────────┬────────┘
         │ MCP Protocol (stdio)
┌────────▼────────┐
│   MCP Server    │
│   (Python)      │
└────────┬────────┘
         │ Named Pipe (RevitMCP)
┌────────▼────────┐
│  Revit Add-in   │
│     (C#)        │
└────────┬────────┘
         │ Revit API
┌────────▼────────┐
│  Revit 2026     │
└─────────────────┘
```

## Development

### Running Tests
```bash
pytest tests/
```

### Code Formatting
```bash
black .
ruff check .
```

## Implementation Status

### Implemented (50+ Tools)
- **MCP Protocol**: Full MCP server implementation with named pipe communication
- **Element Management**: Query, filter, find elements by category, type, parameters
- **Parameter Control**: Read/write instance and type parameters
- **View Management**: Create views, switch active view, schedules
- **Geometry Creation**: Walls, grids, lines, curves, splines, points
- **Curve Operations**: Evaluate, transform, intersect, offset, clone, reverse curves
- **Reference Planes**: Create and query reference planes
- **Graphic Overrides**: Colors, transparency, halftone, visibility
- **Schedule Views**: Create schedules, get table data, modify filters/grouping
- **Element Transforms**: Move, copy, rotate, mirror, arrays
- **Family Parameters**: Add/remove shared parameters in families and projects
- **Selection Tools**: Interactive picking of objects, points, faces, edges
- **Task Dialogs**: Customizable popup dialogs with buttons and command links
- **Ribbon UI**: Create tabs, panels, buttons, combo boxes, text boxes
- **Mass Family Tools**: Reference points, curves, loft forms with formula support
- **Revolve Forms**: Create revolve geometry with axis and profile curves
- **Transaction Management**: All operations properly wrapped in transactions
- **Error Handling**: Comprehensive error handling and validation

### To Implement
1. Complete IFC/DWG export functionality
2. Dimension creation tools
3. Family instance placement tools
4. Room/space boundary tools
5. Performance optimization for large projects

## Troubleshooting

**Server Won't Start:**
- Verify Python 3.10+ installation
- Check dependencies: `pip list`
- Test pythonnet: `python -c "import clr"`

**Can't Connect to Revit:**
- Confirm Revit 2026 is running
- Verify add-in is loaded
- Check add-in file locations
- Review firewall settings

## License

MIT License - see LICENSE file for details.

## Support

- Issues: GitHub Issues
- MCP Documentation: https://modelcontextprotocol.io
- Revit API Docs: https://www.revitapidocs.com/2026
