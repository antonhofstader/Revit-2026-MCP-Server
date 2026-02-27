#!/usr/bin/env python3
"""
Revit 2026 MCP Server
A Model Context Protocol server for interacting with Autodesk Revit 2026
"""

import asyncio
import json
import logging
import threading
from typing import Any, Optional
from collections.abc import Sequence

import mcp
from mcp.server import Server
from mcp.types import AnyUrl, EmbeddedResource, ImageContent, Resource, TextContent, Tool

try:
    import win32file
    import win32pipe
    import pywintypes
    PIPE_AVAILABLE = True
except Exception:
    PIPE_AVAILABLE = False

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s"
)
logger = logging.getLogger("revit-mcp-server")

# Initialize MCP server
app = Server("revit-mcp-server")


class RevitConnection:
    """Manages connection to Revit 2026 via named pipe"""
    
    def __init__(self):
        self.connected = False
        self.pipe_handle = None
        self.pipe_name = r'\\.\pipe\RevitMCP'
        self.lock = threading.Lock()
        
    async def connect(self):
        """Establish connection to running Revit instance via named pipe"""
        if not PIPE_AVAILABLE:
            logger.error("Named pipe communication not available - missing win32pipe")
            return False
            
        try:
            logger.info("Attempting to connect to Revit 2026 via named pipe...")
            
            # Try to connect to the named pipe in a separate thread
            def connect_pipe():
                try:
                    self.pipe_handle = win32file.CreateFile(
                        self.pipe_name,
                        win32file.GENERIC_READ | win32file.GENERIC_WRITE,
                        0, None,
                        win32file.OPEN_EXISTING,
                        0, None
                    )
                    return True
                except pywintypes.error as e:
                    logger.error(f"Failed to connect to Revit pipe: {e}")
                    return False
            
            # Run connection attempt in thread pool
            loop = asyncio.get_event_loop()
            result = await loop.run_in_executor(None, connect_pipe)
            
            if result:
                self.connected = True
                logger.info("Successfully connected to Revit 2026")
                return True
            else:
                logger.error("Failed to connect to Revit - make sure Revit is running with the MCP add-in loaded")
                return False
                
        except Exception as e:
            logger.error(f"Failed to connect to Revit: {e}")
            return False
    
    async def disconnect(self):
        """Close connection to Revit"""
        with self.lock:
            if self.pipe_handle:
                try:
                    win32file.CloseHandle(self.pipe_handle)
                except:
                    pass
                self.pipe_handle = None
            self.connected = False
    
    async def send_command(self, command: str, parameters: dict = None) -> dict:
        """Send a command to Revit and get response"""
        logger.info(f"Sending command to Revit: {command} with params: {parameters}")
        
        if not self.connected or not self.pipe_handle:
            logger.error("Not connected to Revit pipe")
            return {"success": False, "error": "Not connected to Revit"}
        
        request = {
            "Command": command,
            "Parameters": parameters or {}
        }
        
        try:
            with self.lock:
                # Send request
                request_json = json.dumps(request) + "\n"
                logger.debug(f"Sending to Revit: {request_json.strip()}")
                win32file.WriteFile(self.pipe_handle, request_json.encode('utf-8'))
                
                # Read response
                response_data = b""
                while True:
                    try:
                        hr, data = win32file.ReadFile(self.pipe_handle, 4096)
                        response_data += data
                        if b'\n' in response_data:
                            break
                    except pywintypes.error:
                        break
                
                response_str = response_data.decode('utf-8').strip()
                logger.info(f"Received from Revit: {response_str}")
                return json.loads(response_str)
                
        except Exception as e:
            logger.error(f"Error communicating with Revit: {e}")
            return {"success": False, "error": str(e)}


# Global Revit connection
revit = RevitConnection()


@app.list_resources()
async def list_resources() -> list[Resource]:
    """List available Revit project resources"""
    resources = [
        Resource(
            uri=AnyUrl("revit://project/info"),
            name="Project Information",
            mimeType="application/json",
            description="Current Revit project metadata and information"
        ),
        Resource(
            uri=AnyUrl("revit://elements/all"),
            name="All Elements",
            mimeType="application/json",
            description="List of all elements in the active Revit project"
        ),
        Resource(
            uri=AnyUrl("revit://views/active"),
            name="Active View",
            mimeType="application/json",
            description="Information about the currently active view"
        ),
        Resource(
            uri=AnyUrl("revit://families/loaded"),
            name="Loaded Families",
            mimeType="application/json",
            description="List of all loaded families in the project"
        )
    ]
    return resources


@app.read_resource()
async def read_resource(uri: AnyUrl) -> str:
    """Read a specific Revit resource"""
    uri_str = str(uri)
    
    if not revit.connected:
        return json.dumps({"error": "Not connected to Revit"})
    
    try:
        if uri_str == "revit://project/info":
            result = await revit.send_command("get_project_info")
            return json.dumps(result, indent=2)
        
        elif uri_str == "revit://elements/all":
            # Get elements from all main categories
            categories = ["Walls", "Doors", "Windows", "Floors", "Roofs", "Columns", "Beams"]
            all_elements = []
            
            for category in categories:
                result = await revit.send_command("get_elements_by_category", {"category": category})
                if result.get("success"):
                    all_elements.extend(result.get("elements", []))
            
            return json.dumps({
                "count": len(all_elements),
                "categories": list(set(e.get("category", "") for e in all_elements)),
                "elements": all_elements
            }, indent=2)
        
        elif uri_str == "revit://views/active":
            # This would need a new command in the C# add-in
            return json.dumps({
                "name": "Active View",
                "type": "Unknown",
                "note": "Active view info not yet implemented"
            }, indent=2)
        
        elif uri_str == "revit://families/loaded":
            # This would need a new command in the C# add-in
            return json.dumps({
                "count": 0,
                "families": [],
                "note": "Family loading info not yet implemented"
            }, indent=2)
    
    except Exception as e:
        return json.dumps({"error": str(e)})
    
    return json.dumps({"error": "Resource not found"})


@app.list_tools()
async def list_tools() -> list[Tool]:
    """List available Revit tools"""
    return [
        Tool(
            name="get_elements_by_category",
            description="Retrieve all elements of a specific category from the active Revit project",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Revit category name (e.g., 'Walls', 'Doors', 'Windows')"
                    }
                },
                "required": ["category"]
            }
        ),
        Tool(
            name="get_element_parameters",
            description="Get all parameters and their values for a specific element",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ElementId of the Revit element"
                    }
                },
                "required": ["element_id"]
            }
        ),
        Tool(
            name="set_parameter_value",
            description="Set a parameter value for a specific element",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "string",
                        "description": "The ElementId of the Revit element"
                    },
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the parameter to modify"
                    },
                    "value": {
                        "type": "string",
                        "description": "New value for the parameter"
                    }
                },
                "required": ["element_id", "parameter_name", "value"]
            }
        ),
        Tool(
            name="create_view",
            description="Create a new view in the Revit project",
            inputSchema={
                "type": "object",
                "properties": {
                    "view_type": {
                        "type": "string",
                        "description": "Type of view (FloorPlan, Section, 3D, etc.)",
                        "enum": ["FloorPlan", "Section", "Elevation", "3D", "Schedule"]
                    },
                    "name": {
                        "type": "string",
                        "description": "Name for the new view"
                    },
                    "level": {
                        "type": "string",
                        "description": "Level name (for floor plans)",
                        "default": "Level 1"
                    }
                },
                "required": ["view_type", "name"]
            }
        ),
        Tool(
            name="export_to_ifc",
            description="Export the current Revit project to IFC format",
            inputSchema={
                "type": "object",
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "Full path where the IFC file should be saved"
                    },
                    "ifc_version": {
                        "type": "string",
                        "description": "IFC version to use",
                        "enum": ["IFC2x3", "IFC4"],
                        "default": "IFC4"
                    }
                },
                "required": ["file_path"]
            }
        ),
        Tool(
            name="export_image",
            description="Export images from Revit views using ImageExportOptions. Supports multiple image formats (BMP, JPEG variants, PNG, TARGA, TIFF) with configurable resolution, zoom, and size settings.",
            inputSchema={
                "type": "object",
                "properties": {
                    "output_path": {
                        "type": "string",
                        "description": "Full path for the output image file (without extension - Revit adds it automatically based on file_type)"
                    },
                    "file_type": {
                        "type": "string",
                        "description": "Image file format to export",
                        "enum": ["BMP", "JPEGLossless", "JPEGMedium", "JPEGSmallest", "PNG", "TARGA", "TIFF"],
                        "default": "PNG"
                    },
                    "dpi": {
                        "type": "integer",
                        "description": "Image resolution in dots per inch",
                        "enum": [72, 150, 300, 600],
                        "default": 150
                    },
                    "zoom_type": {
                        "type": "string",
                        "description": "How to fit the view in the image. FitToPage fits entire view, Zoom uses zoom percentage",
                        "enum": ["FitToPage", "Zoom"],
                        "default": "FitToPage"
                    },
                    "zoom": {
                        "type": "integer",
                        "description": "Zoom percentage (1-400) when zoom_type is 'Zoom'",
                        "minimum": 1,
                        "maximum": 400,
                        "default": 100
                    },
                    "fit_direction": {
                        "type": "string",
                        "description": "Direction to fit the view when using FitToPage",
                        "enum": ["Horizontal", "Vertical"],
                        "default": "Horizontal"
                    },
                    "export_range": {
                        "type": "string",
                        "description": "Which views to export. CurrentView exports active view only, VisibleViews exports visible region, SpecificViews uses view_ids",
                        "enum": ["CurrentView", "VisibleViews", "SpecificViews"],
                        "default": "CurrentView"
                    },
                    "view_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Array of view element IDs to export (required when export_range is 'SpecificViews')"
                    },
                    "create_website": {
                        "type": "boolean",
                        "description": "Whether to create an HTML website with the exported images",
                        "default": False
                    }
                },
                "required": ["output_path"]
            }
        ),
        Tool(
            name="query_elements",
            description="Query elements using Revit filters and criteria",
            inputSchema={
                "type": "object",
                "properties": {
                    "filter_type": {
                        "type": "string",
                        "description": "Type of filter to apply",
                        "enum": ["category", "parameter", "type", "level"]
                    },
                    "criteria": {
                        "type": "object",
                        "description": "Filter criteria as key-value pairs"
                    }
                },
                "required": ["filter_type", "criteria"]
            }
        ),
        Tool(
            name="get_project_info",
            description="Get basic information about the current Revit project",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="select_view_type",
            description="Select and return information about one available view type",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="get_selected_elements",
            description="Retrieve the currently selected elements in the active Revit UI. Works with both project documents and family documents. In family documents, can select reference points, model curves, symbolic curves, forms, and other family-specific elements.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Selection environment to use. For project documents, this parameter is ignored. For family documents: 'project' to get standard project elements, 'family' to get family-specific elements (reference points, model curves, forms, etc.), or omit to auto-detect based on document type.",
                        "enum": ["project", "family"]
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="set_active_view",
            description="Set the active view in Revit. Works with both project and family documents. For family documents, supports switching between family views.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Operation to perform (for family documents). Use 'by_name', 'by_type', 'reference_level', or 'list_views'. For project documents, omit this parameter.",
                        "enum": ["by_name", "by_type", "reference_level", "list_views"]
                    },
                    "view_type": {
                        "type": "string",
                        "description": "The type of view to activate (e.g., 'FloorPlan', 'ThreeDimensional', 'Section', 'Elevation'). Use this for project views or when operation is 'by_type' in family documents.",
                        "enum": ["FloorPlan", "CeilingPlan", "Section", "Elevation", "ThreeDimensional", "Schedule", "DrawingSheet", "Report", "Drafting", "Legend", "ProjectBrowser", "SystemBrowser", "Walkthrough"]
                    },
                    "view_name": {
                        "type": "string",
                        "description": "The name of the view to activate. Use this when operation is 'by_name' in family documents."
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="create_wall",
            description="Create a new wall element in the Revit project",
            inputSchema={
                "type": "object",
                "properties": {
                    "start_x": {
                        "type": "number",
                        "description": "X coordinate of wall start point",
                        "default": 0
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Y coordinate of wall start point",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "X coordinate of wall end point",
                        "default": 10
                    },
                    "end_y": {
                        "type": "number",
                        "description": "Y coordinate of wall end point",
                        "default": 0
                    },
                    "level": {
                        "type": "string",
                        "description": "Level name where the wall will be created",
                        "default": "Level 1"
                    },
                    "wall_type": {
                        "type": "string",
                        "description": "Wall type name (optional, uses default if not specified)"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="create_grid_line",
            description="Create a line grid",
            inputSchema={
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Grid name (optional)"
                    },
                    "start_x": {
                        "type": "number",
                        "description": "Start point X",
                        "default": 0
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Start point Y",
                        "default": 0
                    },
                    "start_z": {
                        "type": "number",
                        "description": "Start point Z",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "End point X",
                        "default": 10
                    },
                    "end_y": {
                        "type": "number",
                        "description": "End point Y",
                        "default": 0
                    },
                    "end_z": {
                        "type": "number",
                        "description": "End point Z",
                        "default": 0
                    }
                },
                "required": ["start_x", "start_y", "end_x", "end_y"]
            }
        ),
        Tool(
            name="create_grid_arc",
            description="Create an arc grid using center, start, and end points",
            inputSchema={
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Grid name (optional)"
                    },
                    "start_x": {
                        "type": "number",
                        "description": "Start point X"
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Start point Y"
                    },
                    "start_z": {
                        "type": "number",
                        "description": "Start point Z",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "End point X"
                    },
                    "end_y": {
                        "type": "number",
                        "description": "End point Y"
                    },
                    "end_z": {
                        "type": "number",
                        "description": "End point Z",
                        "default": 0
                    },
                    "center_x": {
                        "type": "number",
                        "description": "Arc center point X"
                    },
                    "center_y": {
                        "type": "number",
                        "description": "Arc center point Y"
                    },
                    "center_z": {
                        "type": "number",
                        "description": "Arc center point Z",
                        "default": 0
                    }
                },
                "required": ["start_x", "start_y", "end_x", "end_y", "center_x", "center_y"]
            }
        ),
        Tool(
            name="create_bounded_line",
            description="Create a bounded line as a model curve",
            inputSchema={
                "type": "object",
                "properties": {
                    "start_x": {
                        "type": "number",
                        "description": "Start point X",
                        "default": 0
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Start point Y",
                        "default": 0
                    },
                    "start_z": {
                        "type": "number",
                        "description": "Start point Z",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "End point X",
                        "default": 10
                    },
                    "end_y": {
                        "type": "number",
                        "description": "End point Y",
                        "default": 0
                    },
                    "end_z": {
                        "type": "number",
                        "description": "End point Z",
                        "default": 0
                    }
                },
                "required": ["start_x", "start_y", "end_x", "end_y"]
            }
        ),
        Tool(
            name="create_detail_line",
            description="Create a bounded line as a detail curve in the active view",
            inputSchema={
                "type": "object",
                "properties": {
                    "start_x": {
                        "type": "number",
                        "description": "Start point X",
                        "default": 0
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Start point Y",
                        "default": 0
                    },
                    "start_z": {
                        "type": "number",
                        "description": "Start point Z",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "End point X",
                        "default": 10
                    },
                    "end_y": {
                        "type": "number",
                        "description": "End point Y",
                        "default": 0
                    },
                    "end_z": {
                        "type": "number",
                        "description": "End point Z",
                        "default": 0
                    }
                },
                "required": ["start_x", "start_y", "end_x", "end_y"]
            }
        ),
        Tool(
            name="create_curves_from_points",
            description="Create connected model curves from a list of points",
            inputSchema={
                "type": "object",
                "properties": {
                    "points": {
                        "type": "array",
                        "description": "List of point objects with x, y, z",
                        "items": {
                            "type": "object",
                            "properties": {
                                "x": {"type": "number"},
                                "y": {"type": "number"},
                                "z": {"type": "number"}
                            },
                            "required": ["x", "y"]
                        },
                        "minItems": 2
                    },
                    "closed": {
                        "type": "boolean",
                        "description": "Connect the last point back to the first",
                        "default": False
                    }
                },
                "required": ["points"]
            }
        ),
        Tool(
            name="create_hermite_spline",
            description="Create a Hermite spline model curve from a list of points",
            inputSchema={
                "type": "object",
                "properties": {
                    "points": {
                        "type": "array",
                        "description": "List of point objects with x, y, z",
                        "items": {
                            "type": "object",
                            "properties": {
                                "x": {"type": "number"},
                                "y": {"type": "number"},
                                "z": {"type": "number"}
                            },
                            "required": ["x", "y"]
                        },
                        "minItems": 2
                    },
                    "closed": {
                        "type": "boolean",
                        "description": "Close the spline by connecting the last point to the first",
                        "default": False
                    }
                },
                "required": ["points"]
            }
        )
        ,
        Tool(
            name="create_hermite_spline_with_tangents",
            description="Create a Hermite spline with specified endpoint tangency",
            inputSchema={
                "type": "object",
                "properties": {
                    "points": {
                        "type": "array",
                        "description": "List of point objects with x, y, z",
                        "items": {
                            "type": "object",
                            "properties": {
                                "x": {"type": "number"},
                                "y": {"type": "number"},
                                "z": {"type": "number"}
                            },
                            "required": ["x", "y"]
                        },
                        "minItems": 2
                    },
                    "start_tangent": {
                        "type": "object",
                        "description": "Start tangent vector",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    },
                    "end_tangent": {
                        "type": "object",
                        "description": "End tangent vector",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    },
                    "closed": {
                        "type": "boolean",
                        "description": "Close the spline by connecting the last point to the first",
                        "default": False
                    }
                },
                "required": ["points", "start_tangent", "end_tangent"]
            }
        ),
        Tool(
            name="create_offset_curve",
            description="Create a curve that is an offset of an existing curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the source curve element"
                    },
                    "offset": {
                        "type": "number",
                        "description": "Offset distance (model units)"
                    },
                    "normal": {
                        "type": "object",
                        "description": "Offset plane normal (defaults to Z axis)",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    }
                },
                "required": ["curve_element_id", "offset"]
            }
        ),
        Tool(
            name="evaluate_curve",
            description="Evaluate a point along a curve element at a parameter",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "parameter": {
                        "type": "number",
                        "description": "Curve parameter value"
                    },
                    "normalized": {
                        "type": "boolean",
                        "description": "Whether parameter is normalized (0-1)",
                        "default": False
                    }
                },
                "required": ["curve_element_id", "parameter"]
            }
        ),
        Tool(
            name="curve_distance_to_point",
            description="Return the shortest distance from a point to a curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "point": {
                        "type": "object",
                        "description": "Point to measure from",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    }
                },
                "required": ["curve_element_id", "point"]
            }
        ),
        Tool(
            name="curve_get_end_point",
            description="Return the 3D point at the start or end of a curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "end": {
                        "type": "string",
                        "description": "Which end to evaluate",
                        "enum": ["start", "end"],
                        "default": "end"
                    }
                },
                "required": ["curve_element_id"]
            }
        ),
        Tool(
            name="curve_get_end_parameter",
            description="Return the raw parameter value at the start or end of a curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "end": {
                        "type": "string",
                        "description": "Which end to evaluate",
                        "enum": ["start", "end"],
                        "default": "end"
                    }
                },
                "required": ["curve_element_id"]
            }
        ),
        Tool(
            name="curve_get_end_point_reference",
            description="Return a stable reference to the start or end point of a curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "end": {
                        "type": "string",
                        "description": "Which end to evaluate",
                        "enum": ["start", "end"],
                        "default": "end"
                    }
                },
                "required": ["curve_element_id"]
            }
        ),
        Tool(
            name="create_clone_curve",
            description="Clone a curve element and create a new curve",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    }
                },
                "required": ["curve_element_id"]
            }
        ),
        Tool(
            name="curve_compute_closest_points",
            description="Compute closest point pairs between two curve elements",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id_1": {
                        "type": "number",
                        "description": "ElementId of the first curve element"
                    },
                    "curve_element_id_2": {
                        "type": "number",
                        "description": "ElementId of the second curve element"
                    },
                    "within_this_curve_bounds": {
                        "type": "boolean",
                        "description": "Restrict results to the bounds of the first curve",
                        "default": True
                    },
                    "within_other_curve_bounds": {
                        "type": "boolean",
                        "description": "Restrict results to the bounds of the second curve",
                        "default": True
                    },
                    "return_all_critical_points": {
                        "type": "boolean",
                        "description": "Return all local closest pairs",
                        "default": False
                    }
                },
                "required": ["curve_element_id_1", "curve_element_id_2"]
            }
        ),
        Tool(
            name="curve_compute_derivatives",
            description="Compute derivatives at a parameter on a curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "parameter": {
                        "type": "number",
                        "description": "Curve parameter value"
                    },
                    "normalized": {
                        "type": "boolean",
                        "description": "Whether parameter is normalized (0-1)",
                        "default": False
                    }
                },
                "required": ["curve_element_id", "parameter"]
            }
        ),
        Tool(
            name="curve_compute_normalized_parameter",
            description="Compute normalized parameter from a curve parameter value",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "parameter": {
                        "type": "number",
                        "description": "Curve parameter value"
                    }
                },
                "required": ["curve_element_id", "parameter"]
            }
        ),
        Tool(
            name="curve_compute_raw_parameter",
            description="Compute raw parameter from a normalized curve parameter value",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "normalized_parameter": {
                        "type": "number",
                        "description": "Normalized parameter value (0-1)"
                    }
                },
                "required": ["curve_element_id", "normalized_parameter"]
            }
        ),
        Tool(
            name="curve_create_reversed",
            description="Create a reversed curve from an existing curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    }
                },
                "required": ["curve_element_id"]
            }
        ),
        Tool(
            name="curve_create_transformed",
            description="Create a transformed curve from an existing curve element",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "transform": {
                        "type": "object",
                        "description": "Transform with origin and basis vectors",
                        "properties": {
                            "origin": {
                                "type": "object",
                                "properties": {
                                    "x": {"type": "number"},
                                    "y": {"type": "number"},
                                    "z": {"type": "number"}
                                },
                                "required": ["x", "y"]
                            },
                            "basisX": {
                                "type": "object",
                                "properties": {
                                    "x": {"type": "number"},
                                    "y": {"type": "number"},
                                    "z": {"type": "number"}
                                },
                                "required": ["x", "y"]
                            },
                            "basisY": {
                                "type": "object",
                                "properties": {
                                    "x": {"type": "number"},
                                    "y": {"type": "number"},
                                    "z": {"type": "number"}
                                },
                                "required": ["x", "y"]
                            },
                            "basisZ": {
                                "type": "object",
                                "properties": {
                                    "x": {"type": "number"},
                                    "y": {"type": "number"},
                                    "z": {"type": "number"}
                                },
                                "required": ["x", "y"]
                            }
                        },
                        "required": ["origin", "basisX", "basisY", "basisZ"]
                    }
                },
                "required": ["curve_element_id", "transform"]
            }
        ),
        Tool(
            name="curve_intersect",
            description="Intersect two curve elements and return intersection points",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id_1": {
                        "type": "number",
                        "description": "ElementId of the first curve element"
                    },
                    "curve_element_id_2": {
                        "type": "number",
                        "description": "ElementId of the second curve element"
                    }
                },
                "required": ["curve_element_id_1", "curve_element_id_2"]
            }
        ),
        Tool(
            name="create_point",
            description="Create a point element in the project",
            inputSchema={
                "type": "object",
                "properties": {
                    "x": {"type": "number", "description": "Point X"},
                    "y": {"type": "number", "description": "Point Y"},
                    "z": {"type": "number", "description": "Point Z", "default": 0},
                    "point": {
                        "type": "object",
                        "description": "Point object with x, y, z",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="create_point_on_element",
            description="Create a point element at the closest point on an element's geometry",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "number",
                        "description": "ElementId of the source element"
                    },
                    "point": {
                        "type": "object",
                        "description": "Reference point used to find the closest point",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    }
                },
                "required": ["element_id", "point"]
            }
        ),
        Tool(
            name="curve_point_location_on_curve",
            description="Project a point onto a curve element and return location data",
            inputSchema={
                "type": "object",
                "properties": {
                    "curve_element_id": {
                        "type": "number",
                        "description": "ElementId of the curve element"
                    },
                    "point": {
                        "type": "object",
                        "description": "Point to project onto the curve",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    }
                },
                "required": ["curve_element_id", "point"]
            }
        ),
        Tool(
            name="calculate_line_direction",
            description="Calculate the direction vector of a line from start point to end point",
            inputSchema={
                "type": "object",
                "properties": {
                    "start_point": {
                        "type": "object",
                        "description": "Start point of the line",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    },
                    "end_point": {
                        "type": "object",
                        "description": "End point of the line",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        },
                        "required": ["x", "y"]
                    },
                    "normalize": {
                        "type": "boolean",
                        "description": "Whether to return a unit vector (normalized)",
                        "default": True
                    }
                },
                "required": ["start_point", "end_point"]
            }
        ),
        Tool(
            name="create_point_markup",
            description="Create markup symbols (cross, circle, or square) at specified points using detail lines",
            inputSchema={
                "type": "object",
                "properties": {
                    "points": {
                        "type": "array",
                        "description": "List of points where markups will be placed",
                        "items": {
                            "type": "object",
                            "properties": {
                                "x": {"type": "number"},
                                "y": {"type": "number"},
                                "z": {"type": "number"}
                            },
                            "required": ["x", "y"]
                        },
                        "minItems": 1
                    },
                    "markup_type": {
                        "type": "string",
                        "description": "Type of markup symbol to create",
                        "enum": ["cross", "circle", "square"],
                        "default": "cross"
                    },
                    "size": {
                        "type": "number",
                        "description": "Size of the markup symbol (radius for circle, half-width for cross/square)",
                        "default": 1.0
                    }
                },
                "required": ["points"]
            }
        ),
        Tool(
            name="create_detail_shapes",
            description="Create geometric shapes (rectangles, circles, polygons) as detail lines in a specific view",
            inputSchema={
                "type": "object",
                "properties": {
                    "shape_type": {
                        "type": "string",
                        "description": "Type of shape to create",
                        "enum": ["rectangle", "circle", "polygon"],
                        "default": "rectangle"
                    },
                    "center_x": {
                        "type": "number",
                        "description": "X coordinate of shape center",
                        "default": 0
                    },
                    "center_y": {
                        "type": "number",
                        "description": "Y coordinate of shape center",
                        "default": 0
                    },
                    "center_z": {
                        "type": "number",
                        "description": "Z coordinate of shape center",
                        "default": 0
                    },
                    "width": {
                        "type": "number",
                        "description": "Width for rectangle (half-width of rectangle from center)",
                        "default": 5
                    },
                    "height": {
                        "type": "number",
                        "description": "Height for rectangle (half-height of rectangle from center)",
                        "default": 5
                    },
                    "radius": {
                        "type": "number",
                        "description": "Radius for circle or polygon",
                        "default": 5
                    },
                    "sides": {
                        "type": "integer",
                        "description": "Number of sides for polygon (minimum 3)",
                        "default": 6
                    },
                    "rotation": {
                        "type": "number",
                        "description": "Rotation angle in degrees",
                        "default": 0
                    },
                    "view_id": {
                        "type": "integer",
                        "description": "View ID where the shape will be created (detail lines)"
                    }
                },
                "required": ["shape_type"]
            }
        ),
        Tool(
            name="create_model_shapes",
            description="Create geometric shapes (rectangles, circles, polygons) as model lines in 3D space",
            inputSchema={
                "type": "object",
                "properties": {
                    "shape_type": {
                        "type": "string",
                        "description": "Type of shape to create",
                        "enum": ["rectangle", "circle", "polygon"],
                        "default": "rectangle"
                    },
                    "center_x": {
                        "type": "number",
                        "description": "X coordinate of shape center",
                        "default": 0
                    },
                    "center_y": {
                        "type": "number",
                        "description": "Y coordinate of shape center",
                        "default": 0
                    },
                    "center_z": {
                        "type": "number",
                        "description": "Z coordinate of shape center",
                        "default": 0
                    },
                    "width": {
                        "type": "number",
                        "description": "Width for rectangle (half-width of rectangle from center)",
                        "default": 5
                    },
                    "height": {
                        "type": "number",
                        "description": "Height for rectangle (half-height of rectangle from center)",
                        "default": 5
                    },
                    "radius": {
                        "type": "number",
                        "description": "Radius for circle or polygon",
                        "default": 5
                    },
                    "sides": {
                        "type": "integer",
                        "description": "Number of sides for polygon (minimum 3)",
                        "default": 6
                    },
                    "rotation": {
                        "type": "number",
                        "description": "Rotation angle in degrees",
                        "default": 0
                    },
                    "plane_normal_x": {
                        "type": "number",
                        "description": "X component of plane normal (for shape orientation)",
                        "default": 0
                    },
                    "plane_normal_y": {
                        "type": "number",
                        "description": "Y component of plane normal (for shape orientation)",
                        "default": 0
                    },
                    "plane_normal_z": {
                        "type": "number",
                        "description": "Z component of plane normal (for shape orientation)",
                        "default": 1
                    }
                },
                "required": ["shape_type"]
            }
        ),
        Tool(
            name="create_symbolic_shapes",
            description="Create geometric shapes (rectangles, circles, polygons) as symbolic lines in a family document",
            inputSchema={
                "type": "object",
                "properties": {
                    "shape_type": {
                        "type": "string",
                        "description": "Type of shape to create",
                        "enum": ["rectangle", "circle", "polygon"],
                        "default": "rectangle"
                    },
                    "center_x": {
                        "type": "number",
                        "description": "X coordinate of shape center",
                        "default": 0
                    },
                    "center_y": {
                        "type": "number",
                        "description": "Y coordinate of shape center",
                        "default": 0
                    },
                    "center_z": {
                        "type": "number",
                        "description": "Z coordinate of shape center",
                        "default": 0
                    },
                    "width": {
                        "type": "number",
                        "description": "Width for rectangle (half-width of rectangle from center)",
                        "default": 5
                    },
                    "height": {
                        "type": "number",
                        "description": "Height for rectangle (half-height of rectangle from center)",
                        "default": 5
                    },
                    "radius": {
                        "type": "number",
                        "description": "Radius for circle or polygon",
                        "default": 5
                    },
                    "sides": {
                        "type": "integer",
                        "description": "Number of sides for polygon (minimum 3)",
                        "default": 6
                    },
                    "rotation": {
                        "type": "number",
                        "description": "Rotation angle in degrees",
                        "default": 0
                    },
                    "sketch_plane_id": {
                        "type": "integer",
                        "description": "Sketch plane ID for the symbolic curves (optional, uses default if not specified)"
                    }
                },
                "required": ["shape_type"]
            }
        ),
        Tool(
            name="rotate_elements",
            description="Rotate one or more elements around an axis using ElementTransformUtils. The axis is defined by a point and a direction vector. By default, rotates around the Z-axis (vertical) for plan-view rotation. In family documents, the default rotation axis is at the Reference Level elevation.",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "number",
                        "description": "Single ElementId to rotate (use either element_id or element_ids)"
                    },
                    "element_ids": {
                        "type": "array",
                        "items": {"type": "number"},
                        "description": "Array of ElementIds to rotate (use either element_id or element_ids)"
                    },
                    "angle": {
                        "type": "number",
                        "description": "Rotation angle in degrees (positive = counterclockwise when looking along axis direction)"
                    },
                    "axis_point_x": {
                        "type": "number",
                        "description": "X coordinate of a point on the rotation axis",
                        "default": 0
                    },
                    "axis_point_y": {
                        "type": "number",
                        "description": "Y coordinate of a point on the rotation axis",
                        "default": 0
                    },
                    "axis_point_z": {
                        "type": "number",
                        "description": "Z coordinate of a point on the rotation axis. Defaults to 0 in project documents, or Reference Level elevation in family documents.",
                        "default": 0
                    },
                    "axis_direction_x": {
                        "type": "number",
                        "description": "X component of the axis direction vector",
                        "default": 0
                    },
                    "axis_direction_y": {
                        "type": "number",
                        "description": "Y component of the axis direction vector",
                        "default": 0
                    },
                    "axis_direction_z": {
                        "type": "number",
                        "description": "Z component of the axis direction vector (default 1 for vertical axis)",
                        "default": 1
                    }
                },
                "required": ["angle"]
            }
        ),
        Tool(
            name="add_family_shared_parameter",
            description="Add a shared parameter to the current family document. Must be run while in the Family Editor with a family open. Requires a shared parameter file (.txt) containing the parameter definition.",
            inputSchema={
                "type": "object",
                "properties": {
                    "shared_parameter_file": {
                        "type": "string",
                        "description": "Full path to the shared parameter file (.txt)"
                    },
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the shared parameter to add (must exist in the shared parameter file)"
                    },
                    "parameter_group": {
                        "type": "string",
                        "description": "Parameter group to place the parameter in (e.g., 'PG_GENERAL', 'PG_GEOMETRY', 'PG_IDENTITY_DATA')",
                        "default": "PG_GENERAL"
                    },
                    "is_instance": {
                        "type": "boolean",
                        "description": "True for instance parameter, False for type parameter",
                        "default": True
                    }
                },
                "required": ["shared_parameter_file", "parameter_name"]
            }
        ),
        Tool(
            name="remove_family_parameter",
            description="Remove a parameter from the current family document. Must be run while in the Family Editor with a family open.",
            inputSchema={
                "type": "object",
                "properties": {
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the parameter to remove"
                    }
                },
                "required": ["parameter_name"]
            }
        ),
        Tool(
            name="get_family_parameters",
            description="Get all parameters in the current family document. Must be run while in the Family Editor with a family open.",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="detect_document_type",
            description="Detect whether the current document is a family (.rfa) or a project document (.rvt). Returns document type and additional information.",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="add_project_shared_parameter",
            description="Add a shared parameter to a project document and bind it to specified categories. Must be run in a project document (not a family).",
            inputSchema={
                "type": "object",
                "properties": {
                    "shared_parameter_file": {
                        "type": "string",
                        "description": "Full path to the shared parameter file (.txt)"
                    },
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the shared parameter to add (must exist in the shared parameter file)"
                    },
                    "categories": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": "Array of category names to bind the parameter to (e.g., ['Walls', 'Floors', 'Doors'])"
                    },
                    "parameter_group": {
                        "type": "string",
                        "description": "Parameter group to place the parameter in (e.g., 'General', 'Geometry', 'Identity')",
                        "default": "General"
                    },
                    "is_instance": {
                        "type": "boolean",
                        "description": "True for instance parameter, False for type parameter",
                        "default": True
                    }
                },
                "required": ["shared_parameter_file", "parameter_name", "categories"]
            }
        ),
        Tool(
            name="remove_project_shared_parameter",
            description="Remove a shared parameter binding from a project document. Must be run in a project document (not a family).",
            inputSchema={
                "type": "object",
                "properties": {
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the parameter to remove"
                    }
                },
                "required": ["parameter_name"]
            }
        ),
        Tool(
            name="get_project_shared_parameters",
            description="Get all shared parameters bound in the project document. Must be run in a project document (not a family).",
            inputSchema={
                "type": "object",
                "properties": {},
                "required": []
            }
        ),
        Tool(
            name="get_last_placed_element",
            description="Get the last placed element in the document (the element with the highest ElementId, which is typically the most recently created). Optionally filter by category.",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Optional category to filter by (e.g., 'Walls', 'Doors', 'Windows')"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="create_reference_plane",
            description="Create a reference plane in a specific view type. Reference planes can be created in Section views, Floor Plans, Ceiling Plans, Structural Plans, Elevations, Drafting views, Detail views, and 3D views.",
            inputSchema={
                "type": "object",
                "properties": {
                    "bubble_x": {
                        "type": "number",
                        "description": "X coordinate of the bubble end (where the reference plane symbol appears)"
                    },
                    "bubble_y": {
                        "type": "number",
                        "description": "Y coordinate of the bubble end"
                    },
                    "bubble_z": {
                        "type": "number",
                        "description": "Z coordinate of the bubble end",
                        "default": 0
                    },
                    "free_x": {
                        "type": "number",
                        "description": "X coordinate of the free end (opposite end from the bubble)"
                    },
                    "free_y": {
                        "type": "number",
                        "description": "Y coordinate of the free end"
                    },
                    "free_z": {
                        "type": "number",
                        "description": "Z coordinate of the free end",
                        "default": 0
                    },
                    "cut_vector_x": {
                        "type": "number",
                        "description": "X component of the cut vector (perpendicular to the plane)"
                    },
                    "cut_vector_y": {
                        "type": "number",
                        "description": "Y component of the cut vector"
                    },
                    "cut_vector_z": {
                        "type": "number",
                        "description": "Z component of the cut vector"
                    },
                    "name": {
                        "type": "string",
                        "description": "Optional name for the reference plane"
                    },
                    "view_type": {
                        "type": "string",
                        "description": "Type of view to create the reference plane in",
                        "enum": ["Section", "FloorPlan", "CeilingPlan", "StructuralPlan", "Elevation", "Drafting", "Detail", "3D"]
                    },
                    "view_id": {
                        "type": "number",
                        "description": "Specific view ElementId to create the reference plane in (overrides view_type)"
                    }
                },
                "required": ["bubble_x", "bubble_y", "free_x", "free_y"]
            }
        ),
        Tool(
            name="get_reference_planes",
            description="Get reference planes from the model or from a specific element. Can filter by name and retrieve reference data for dimensioning.",
            inputSchema={
                "type": "object",
                "properties": {
                    "name": {
                        "type": "string",
                        "description": "Optional filter - only return reference planes containing this name"
                    },
                    "element_id": {
                        "type": "number",
                        "description": "Optional - get reference planes/references from a specific family instance element"
                    },
                    "include_unnamed": {
                        "type": "boolean",
                        "description": "Whether to include unnamed reference planes (default: true)",
                        "default": True
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="set_graphic_overrides",
            description="Set graphic overrides (halftone, colors, transparency, visibility) for elements by category or by specific element IDs. Overrides are applied to the active view or a specified view.",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Category name to apply overrides to (e.g., 'Walls', 'Doors')"
                    },
                    "element_id": {
                        "type": "number",
                        "description": "Single element ID to apply overrides to"
                    },
                    "element_ids": {
                        "type": "array",
                        "items": {"type": "number"},
                        "description": "Array of element IDs to apply overrides to"
                    },
                    "view_id": {
                        "type": "number",
                        "description": "Optional view ID to apply overrides in (defaults to active view)"
                    },
                    "halftone": {
                        "type": "boolean",
                        "description": "Set halftone display (true/false)"
                    },
                    "transparency": {
                        "type": "number",
                        "description": "Set surface transparency (0-100, where 100 is fully transparent)"
                    },
                    "visible": {
                        "type": "boolean",
                        "description": "Set visibility (true=visible, false=hidden)"
                    },
                    "projection_line_color": {
                        "type": "string",
                        "description": "Projection line color as hex string (e.g., '#FF0000' for red)"
                    },
                    "projection_line_weight": {
                        "type": "number",
                        "description": "Projection line weight (1-16)"
                    },
                    "cut_line_color": {
                        "type": "string",
                        "description": "Cut line color as hex string"
                    },
                    "cut_line_weight": {
                        "type": "number",
                        "description": "Cut line weight (1-16)"
                    },
                    "surface_foreground_color": {
                        "type": "string",
                        "description": "Surface foreground pattern color as hex string"
                    },
                    "surface_background_color": {
                        "type": "string",
                        "description": "Surface background pattern color as hex string"
                    },
                    "cut_foreground_color": {
                        "type": "string",
                        "description": "Cut foreground pattern color as hex string"
                    },
                    "cut_background_color": {
                        "type": "string",
                        "description": "Cut background pattern color as hex string"
                    },
                    "detail_level": {
                        "type": "string",
                        "description": "Detail level override",
                        "enum": ["Coarse", "Medium", "Fine"]
                    },
                    "reset": {
                        "type": "boolean",
                        "description": "Reset all overrides to default (ignores other settings)",
                        "default": False
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="create_schedule_view",
            description="Create a schedule view for a specific category. Schedules are tabular views that list element properties and quantities. You can specify which fields to include and how to group the data.",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Category name to create schedule for (e.g., 'Walls', 'Doors', 'Rooms', 'Windows')"
                    },
                    "name": {
                        "type": "string",
                        "description": "Name for the schedule view. Defaults to '<Category> Schedule'"
                    },
                    "is_key_schedule": {
                        "type": "boolean",
                        "description": "Create a key schedule instead of a regular schedule (default: false)",
                        "default": False
                    },
                    "fields": {
                        "type": "array",
                        "items": {"type": "string"},
                        "description": "List of field/parameter names to include in the schedule. If not specified, common default fields are added."
                    },
                    "group_by": {
                        "type": "string",
                        "description": "Field name to group schedule rows by"
                    },
                    "itemize_every_instance": {
                        "type": "boolean",
                        "description": "If true, each element instance gets its own row. If false, identical items are grouped with counts (default: true)",
                        "default": True
                    }
                },
                "required": ["category"]
            }
        ),
        Tool(
            name="get_table_data",
            description="Get table data from a schedule view. Returns headers and all rows of data from the schedule. Can retrieve by schedule ID or name.",
            inputSchema={
                "type": "object",
                "properties": {
                    "schedule_id": {
                        "type": "number",
                        "description": "The element ID of the schedule view"
                    },
                    "schedule_name": {
                        "type": "string",
                        "description": "The name of the schedule view to retrieve data from"
                    },
                    "include_headers": {
                        "type": "boolean",
                        "description": "Include column headers in the response (default: true)",
                        "default": True
                    },
                    "include_hidden_fields": {
                        "type": "boolean",
                        "description": "Include hidden fields in the output (default: false)",
                        "default": False
                    },
                    "max_rows": {
                        "type": "number",
                        "description": "Maximum number of rows to return (default: all rows)"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="modify_schedule",
            description="Modify schedule settings including filters, sorting/grouping, field formatting, and calculations. Full control over schedule appearance and behavior including totals, alignment, column width, and field order.",
            inputSchema={
                "type": "object",
                "properties": {
                    "schedule_id": {
                        "type": "number",
                        "description": "The element ID of the schedule view"
                    },
                    "schedule_name": {
                        "type": "string",
                        "description": "The name of the schedule view to modify"
                    },
                    "itemize_every_instance": {
                        "type": "boolean",
                        "description": "If true, each element instance gets its own row. If false, identical items are grouped with counts."
                    },
                    "add_filter": {
                        "type": "object",
                        "description": "Add a filter to the schedule",
                        "properties": {
                            "field_name": {
                                "type": "string",
                                "description": "Name of the field to filter on"
                            },
                            "filter_type": {
                                "type": "string",
                                "description": "Type of filter",
                                "enum": ["Equal", "NotEqual", "GreaterThan", "GreaterThanOrEqual", "LessThan", "LessThanOrEqual", "Contains", "NotContains", "BeginsWith", "EndsWith", "HasValue", "HasNoValue"]
                            },
                            "value": {
                                "type": "string",
                                "description": "Filter value (not required for HasValue/HasNoValue)"
                            }
                        }
                    },
                    "clear_filters": {
                        "type": "boolean",
                        "description": "Remove all existing filters from the schedule",
                        "default": False
                    },
                    "add_sort_group": {
                        "type": "object",
                        "description": "Add sorting/grouping to the schedule",
                        "properties": {
                            "field_name": {
                                "type": "string",
                                "description": "Name of the field to sort/group by"
                            },
                            "ascending": {
                                "type": "boolean",
                                "description": "Sort ascending (true) or descending (false)",
                                "default": True
                            },
                            "show_header": {
                                "type": "boolean",
                                "description": "Show group header",
                                "default": True
                            },
                            "show_footer": {
                                "type": "boolean",
                                "description": "Show group footer with totals",
                                "default": False
                            },
                            "show_count": {
                                "type": "boolean",
                                "description": "Show item count in header",
                                "default": True
                            }
                        }
                    },
                    "remove_sort_group": {
                        "type": "string",
                        "description": "Remove a specific sort/group field by field name"
                    },
                    "clear_sort_groups": {
                        "type": "boolean",
                        "description": "Remove all existing sort/group fields",
                        "default": False
                    },
                    "format_field": {
                        "type": "object",
                        "description": "Format a schedule field's appearance and behavior",
                        "properties": {
                            "field_name": {
                                "type": "string",
                                "description": "Name of the field to format"
                            },
                            "heading": {
                                "type": "string",
                                "description": "Custom column heading text"
                            },
                            "alignment": {
                                "type": "string",
                                "description": "Horizontal alignment of field values",
                                "enum": ["Left", "Center", "Right"]
                            },
                            "width": {
                                "type": "number",
                                "description": "Column width in inches"
                            },
                            "hidden": {
                                "type": "boolean",
                                "description": "Hide this field from the schedule display"
                            },
                            "calculate_totals": {
                                "type": "boolean",
                                "description": "Calculate totals/sum for this field (numeric fields only)"
                            }
                        }
                    },
                    "add_calculated_field": {
                        "type": "object",
                        "description": "Add calculation to a field (totals, minimum, maximum)",
                        "properties": {
                            "field_name": {
                                "type": "string",
                                "description": "Name of the field to add calculation to"
                            },
                            "calculation_type": {
                                "type": "string",
                                "description": "Type of calculation to perform",
                                "enum": ["sum", "total", "totals", "minimum", "min", "maximum", "max"]
                            }
                        }
                    },
                    "reorder_field": {
                        "type": "object",
                        "description": "Change the position/order of a field in the schedule",
                        "properties": {
                            "field_name": {
                                "type": "string",
                                "description": "Name of the field to reorder"
                            },
                            "position": {
                                "type": "integer",
                                "description": "New column position (0-based index)"
                            }
                        }
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="modify_element",
            description="Modify an element by changing its parameters, location, rotation, or other properties. Can move, rotate, flip, and set multiple parameter values.",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "number",
                        "description": "The element ID to modify"
                    },
                    "parameters": {
                        "type": "object",
                        "description": "Dictionary of parameter names and values to set. Example: {'Mark': 'A1', 'Comments': 'Updated'}",
                        "additionalProperties": True
                    },
                    "move": {
                        "type": "object",
                        "description": "Move the element by a vector or to a new location",
                        "properties": {
                            "x": {"type": "number", "description": "X translation in feet (or new X if 'absolute' is true)"},
                            "y": {"type": "number", "description": "Y translation in feet (or new Y if 'absolute' is true)"},
                            "z": {"type": "number", "description": "Z translation in feet (or new Z if 'absolute' is true)"},
                            "absolute": {"type": "boolean", "description": "If true, x/y/z are absolute coordinates, not relative translation", "default": False}
                        }
                    },
                    "rotate": {
                        "type": "object",
                        "description": "Rotate the element around an axis",
                        "properties": {
                            "angle": {"type": "number", "description": "Rotation angle in degrees"},
                            "axis_x": {"type": "number", "description": "X component of rotation axis point"},
                            "axis_y": {"type": "number", "description": "Y component of rotation axis point"},
                            "axis_z": {"type": "number", "description": "Z component of rotation axis point (default: 0)"},
                            "axis_direction": {"type": "string", "description": "Axis direction: 'X', 'Y', or 'Z' (default: 'Z' for vertical axis)", "enum": ["X", "Y", "Z"]}
                        }
                    },
                    "flip_facing": {
                        "type": "boolean",
                        "description": "Flip the element's facing orientation (for doors, windows, etc.)"
                    },
                    "flip_hand": {
                        "type": "boolean",
                        "description": "Flip the element's hand orientation (left/right swing for doors)"
                    },
                    "flip_workplane": {
                        "type": "boolean",
                        "description": "Flip the element about its work plane"
                    },
                    "mirror": {
                        "type": "object",
                        "description": "Mirror the element across a plane",
                        "properties": {
                            "plane_origin_x": {"type": "number"},
                            "plane_origin_y": {"type": "number"},
                            "plane_origin_z": {"type": "number", "default": 0},
                            "plane_normal_x": {"type": "number", "description": "X component of plane normal"},
                            "plane_normal_y": {"type": "number", "description": "Y component of plane normal"},
                            "plane_normal_z": {"type": "number", "description": "Z component of plane normal", "default": 0}
                        }
                    },
                    "pin": {
                        "type": "boolean",
                        "description": "Pin or unpin the element"
                    }
                },
                "required": ["element_id"]
            }
        ),
        Tool(
            name="transform_elements",
            description="Transform multiple elements using ElementTransformUtils methods: Move, Copy, Mirror, or create Linear/Radial arrays. Operates on one or more elements.",
            inputSchema={
                "type": "object",
                "properties": {
                    "element_id": {
                        "type": "number",
                        "description": "Single element ID to transform"
                    },
                    "element_ids": {
                        "type": "array",
                        "items": {"type": "number"},
                        "description": "Array of element IDs to transform"
                    },
                    "operation": {
                        "type": "string",
                        "description": "Transform operation to perform",
                        "enum": ["move", "copy", "mirror", "linear_array", "radial_array"]
                    },
                    "translation": {
                        "type": "object",
                        "description": "Translation vector for move/copy operations",
                        "properties": {
                            "x": {"type": "number", "description": "X translation in feet"},
                            "y": {"type": "number", "description": "Y translation in feet"},
                            "z": {"type": "number", "description": "Z translation in feet"}
                        }
                    },
                    "rotation": {
                        "type": "object",
                        "description": "Rotation parameters (can combine with move/copy)",
                        "properties": {
                            "angle": {"type": "number", "description": "Angle in degrees"},
                            "center_x": {"type": "number", "description": "X of rotation center"},
                            "center_y": {"type": "number", "description": "Y of rotation center"},
                            "center_z": {"type": "number", "description": "Z of rotation center"},
                            "axis": {"type": "string", "enum": ["X", "Y", "Z"], "description": "Rotation axis (default: Z)"}
                        }
                    },
                    "mirror_plane": {
                        "type": "object",
                        "description": "Mirror plane definition for mirror operation",
                        "properties": {
                            "origin_x": {"type": "number"},
                            "origin_y": {"type": "number"},
                            "origin_z": {"type": "number", "default": 0},
                            "normal_x": {"type": "number", "description": "X component of plane normal"},
                            "normal_y": {"type": "number", "description": "Y component of plane normal"},
                            "normal_z": {"type": "number", "description": "Z component of plane normal", "default": 0}
                        }
                    },
                    "array_count": {
                        "type": "number",
                        "description": "Number of copies for array operations (total including original)"
                    },
                    "array_spacing": {
                        "type": "object",
                        "description": "Spacing between array copies",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        }
                    },
                    "radial_center": {
                        "type": "object",
                        "description": "Center point for radial array",
                        "properties": {
                            "x": {"type": "number"},
                            "y": {"type": "number"},
                            "z": {"type": "number"}
                        }
                    },
                    "radial_angle": {
                        "type": "number",
                        "description": "Total angle span for radial array in degrees (default: 360)"
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="get_family_types",
            description="Retrieve all family types (FamilySymbols) from the document. Can filter by family name, category, or return all. Returns type names, family names, and IDs.",
            inputSchema={
                "type": "object",
                "properties": {
                    "family_name": {
                        "type": "string",
                        "description": "Filter by family name (partial match supported)"
                    },
                    "category": {
                        "type": "string",
                        "description": "Filter by category name (e.g., 'Doors', 'Windows', 'Furniture')"
                    },
                    "include_parameters": {
                        "type": "boolean",
                        "description": "Include type parameter values in results (default: false)",
                        "default": False
                    },
                    "max_results": {
                        "type": "number",
                        "description": "Maximum number of results to return"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="get_instances",
            description="Retrieve all instances of a specific family, family type, category, or element class. Returns element IDs, locations, and basic properties.",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Category name (e.g., 'Doors', 'Walls', 'Rooms')"
                    },
                    "family_name": {
                        "type": "string",
                        "description": "Family name to filter instances"
                    },
                    "type_name": {
                        "type": "string",
                        "description": "Type name to filter instances"
                    },
                    "element_class": {
                        "type": "string",
                        "description": "Element class name (e.g., 'Wall', 'Floor', 'FamilyInstance', 'Room')"
                    },
                    "include_location": {
                        "type": "boolean",
                        "description": "Include location data in results (default: true)",
                        "default": True
                    },
                    "include_parameters": {
                        "type": "boolean",
                        "description": "Include instance parameter values (default: false)",
                        "default": False
                    },
                    "max_results": {
                        "type": "number",
                        "description": "Maximum number of results to return"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="find_family_type",
            description="Find a specific family type by exact or partial name match. Returns detailed information about the type including all type parameters.",
            inputSchema={
                "type": "object",
                "properties": {
                    "family_name": {
                        "type": "string",
                        "description": "Family name to search for"
                    },
                    "type_name": {
                        "type": "string",
                        "description": "Type name to search for"
                    },
                    "category": {
                        "type": "string",
                        "description": "Category to search within"
                    },
                    "exact_match": {
                        "type": "boolean",
                        "description": "Require exact name match (default: false for partial match)",
                        "default": False
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="find_elements",
            description="Find specific element instances using various search criteria including parameter values, levels, phases, and worksets.",
            inputSchema={
                "type": "object",
                "properties": {
                    "category": {
                        "type": "string",
                        "description": "Category name to search within"
                    },
                    "family_name": {
                        "type": "string",
                        "description": "Family name filter"
                    },
                    "type_name": {
                        "type": "string",
                        "description": "Type name filter"
                    },
                    "parameter_filter": {
                        "type": "object",
                        "description": "Filter by parameter value",
                        "properties": {
                            "name": {"type": "string", "description": "Parameter name"},
                            "value": {"type": "string", "description": "Value to match"},
                            "operator": {"type": "string", "enum": ["equals", "contains", "startswith", "endswith", "greater", "less"], "default": "equals"}
                        }
                    },
                    "level_name": {
                        "type": "string",
                        "description": "Filter by level name"
                    },
                    "bounding_box": {
                        "type": "object",
                        "description": "Filter by bounding box region",
                        "properties": {
                            "min_x": {"type": "number"},
                            "min_y": {"type": "number"},
                            "min_z": {"type": "number"},
                            "max_x": {"type": "number"},
                            "max_y": {"type": "number"},
                            "max_z": {"type": "number"}
                        }
                    },
                    "view_specific": {
                        "type": "boolean",
                        "description": "Only return view-specific elements in active view",
                        "default": False
                    },
                    "include_location": {
                        "type": "boolean",
                        "description": "Include location data",
                        "default": True
                    },
                    "max_results": {
                        "type": "number",
                        "description": "Maximum results to return"
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="selection_tool",
            description="Perform selection operations in Revit: get current selection, set selection, pick objects/points/faces interactively with optional filters.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Selection operation to perform",
                        "enum": ["get_selection", "set_selection", "clear_selection", "pick_object", "pick_objects", "pick_point", "pick_face", "pick_edge"]
                    },
                    "element_ids": {
                        "type": "array",
                        "items": {"type": "number"},
                        "description": "Element IDs to select (for set_selection operation)"
                    },
                    "prompt": {
                        "type": "string",
                        "description": "Prompt message to display during pick operations"
                    },
                    "filter_category": {
                        "type": "string",
                        "description": "Category name to filter pickable elements (e.g., 'Walls', 'Doors')"
                    },
                    "filter_class": {
                        "type": "string",
                        "description": "Element class to filter pickable elements (e.g., 'FamilyInstance', 'Wall')"
                    },
                    "include_location": {
                        "type": "boolean",
                        "description": "Include location data for picked elements (default: true)",
                        "default": True
                    },
                    "include_parameters": {
                        "type": "boolean",
                        "description": "Include parameter values for picked elements (default: false)",
                        "default": False
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="task_dialog",
            description="Display TaskDialog popups in Revit. Supports simple message display or customizable dialogs with title, main instruction, content, buttons, command links, footer, and verification checkbox.",
            inputSchema={
                "type": "object",
                "properties": {
                    "mode": {
                        "type": "string",
                        "description": "Mode of TaskDialog: 'simple' for basic message, 'custom' for full customization",
                        "enum": ["simple", "custom"],
                        "default": "simple"
                    },
                    "title": {
                        "type": "string",
                        "description": "Title of the dialog window"
                    },
                    "message": {
                        "type": "string",
                        "description": "Message text to display (for simple mode)"
                    },
                    "main_instruction": {
                        "type": "string",
                        "description": "Main instruction text (large text at top, for custom mode)"
                    },
                    "main_content": {
                        "type": "string",
                        "description": "Main content text (for custom mode)"
                    },
                    "expanded_content": {
                        "type": "string",
                        "description": "Expanded/collapsible content text"
                    },
                    "footer_text": {
                        "type": "string",
                        "description": "Footer text displayed at the bottom"
                    },
                    "common_buttons": {
                        "type": "string",
                        "description": "Common button set to display",
                        "enum": ["None", "Ok", "Cancel", "Yes", "No", "Retry", "Close"]
                    },
                    "command_links": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "text": {"type": "string", "description": "Command link text"},
                                "subtext": {"type": "string", "description": "Optional subtext for command link"}
                            },
                            "required": ["text"]
                        },
                        "description": "Array of command links (up to 4)"
                    },
                    "default_button": {
                        "type": "string",
                        "description": "Default button to focus",
                        "enum": ["None", "Ok", "Cancel", "Yes", "No", "Retry", "Close", "CommandLink1", "CommandLink2", "CommandLink3", "CommandLink4"]
                    },
                    "verification_text": {
                        "type": "string",
                        "description": "Text for a verification checkbox"
                    },
                    "allow_cancellation": {
                        "type": "boolean",
                        "description": "Allow dialog to be cancelled with X button",
                        "default": True
                    },
                    "main_icon": {
                        "type": "string",
                        "description": "Main icon to display",
                        "enum": ["None", "Warning", "Error", "Information"]
                    }
                },
                "required": []
            }
        ),
        Tool(
            name="family_points_tool",
            description="Create reference points in a conceptual mass or adaptive family. Supports single point, row of points, or grid of points with configurable spacing. Includes AdaptiveComponentFamilyUtils methods for managing adaptive placement points.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Point creation operation",
                        "enum": ["create_single_point", "create_point_row", "create_point_grid", "create_point_grid_formula", "get_reference_points", "delete_reference_points", "make_adaptive_points", "get_adaptive_point_ids", "set_adaptive_point_ids", "create_curve_by_points", "create_curves_from_grid", "get_curves_by_points", "create_loft_form", "get_forms"]
                    },
                    "x": {
                        "type": "number",
                        "description": "X coordinate for single point or start X for row/grid (in feet)"
                    },
                    "y": {
                        "type": "number",
                        "description": "Y coordinate for single point or start Y for row/grid (in feet)"
                    },
                    "z": {
                        "type": "number",
                        "description": "Z coordinate for single point or constant Z for row/grid (in feet)",
                        "default": 0
                    },
                    "count_x": {
                        "type": "integer",
                        "description": "Number of points in X direction (for row or grid)",
                        "default": 5
                    },
                    "count_y": {
                        "type": "integer",
                        "description": "Number of points in Y direction (for grid only)",
                        "default": 5
                    },
                    "spacing_x": {
                        "type": "number",
                        "description": "Spacing between points in X direction (in feet)",
                        "default": 1.0
                    },
                    "spacing_y": {
                        "type": "number",
                        "description": "Spacing between points in Y direction (in feet)",
                        "default": 1.0
                    },
                    "direction": {
                        "type": "string",
                        "description": "Direction for point row: 'x', 'y', or 'z'",
                        "enum": ["x", "y", "z"],
                        "default": "x"
                    },
                    "point_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Element IDs of reference points - used for delete_reference_points, make_adaptive_points, or set_adaptive_point_ids operations"
                    },
                    "point_id": {
                        "type": "integer",
                        "description": "Single ElementId of a reference point - used for check_is_point_used operation"
                    },
                    "z_formula": {
                        "type": "string",
                        "description": "Formula for Z coordinate as function of x and y. Supports: sin, cos, tan, sqrt, abs, pow(a,b), exp, log, pi, +, -, *, /. Example: '10*cos(x)+10*sin(y)' or '5*sin(x*0.5)*cos(y*0.5)'"
                    },
                    "curve_direction": {
                        "type": "string",
                        "description": "Direction for creating curves from grid: 'rows' (along X), 'columns' (along Y), or 'both'",
                        "enum": ["rows", "columns", "both", "x", "y"],
                        "default": "rows"
                    },
                    "is_reference_line": {
                        "type": "boolean",
                        "description": "If true, creates reference lines instead of model lines",
                        "default": False
                    },
                    "curve_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Element IDs of curves to use for loft (if not provided, uses all curves)"
                    },
                    "is_solid": {
                        "type": "boolean",
                        "description": "If true, creates solid form; if false, creates void form",
                        "default": True
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="family_manager_tool",
            description="Comprehensive family management tool for Revit family documents (.rfa). Manage family types, parameters, formulas, FamilyParameter properties, and element parameter associations. Operations: create/delete/rename family types, set parameter values, manage formulas, associate/dissociate element parameters, get detailed parameter information. Requires family document to be open.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Family management operation",
                        "enum": ["create_type", "delete_type", "rename_type", "set_current_type", "get_family_types", "set_parameter_value", "add_formula", "get_formula", "associate_element_parameter", "dissociate_element_parameter", "get_family_parameter_info", "get_all_family_parameters", "get_family_category"]
                    },
                    "type_name": {
                        "type": "string",
                        "description": "Name of the family type (for create_type, delete_type, set_current_type)"
                    },
                    "old_name": {
                        "type": "string",
                        "description": "Current name of the type to rename (for rename_type, optional if renaming current type)"
                    },
                    "new_name": {
                        "type": "string",
                        "description": "New name for the family type (for rename_type)"
                    },
                    "parameter_name": {
                        "type": "string",
                        "description": "Name of the family parameter (for set_parameter_value, add_formula, get_formula, get_family_parameter_info)"
                    },
                    "family_parameter_name": {
                        "type": "string",
                        "description": "Name of the family parameter to associate/dissociate (for associate_element_parameter, dissociate_element_parameter)"
                    },
                    "element_id": {
                        "type": "integer",
                        "description": "ElementId of an element in the family that has the parameter to bind (for associate_element_parameter). Can be a reference plane, reference point, nested family, or other family geometry."
                    },
                    "element_parameter_name": {
                        "type": "string",
                        "description": "Name of the parameter on the element to bind to the family parameter (for associate_element_parameter)"
                    },
                    "value": {
                        "description": "Value to set for the parameter (for set_parameter_value). Type depends on parameter storage type: number for Double/Integer, string for String/Text, integer for ElementId",
                        "oneOf": [
                            {"type": "number"},
                            {"type": "string"},
                            {"type": "integer"}
                        ]
                    },
                    "formula": {
                        "type": "string",
                        "description": "Formula to assign to the parameter (for add_formula). Example: 'Width * 2', 'Height + 10 mm'"
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="application_document_tool",
            description="Create new Revit documents from templates using the Application class. Operations: create new family documents (.rft templates) or new project documents (.rte/.rvt templates). This opens a new document in Revit.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Document creation operation",
                        "enum": ["new_family_document", "new_project_document"]
                    },
                    "template_path": {
                        "type": "string",
                        "description": "Path to the template file. For families: provide just the filename (e.g., 'Metric Generic Model.rft') to search in C:\\ProgramData\\Autodesk\\RVT 2026\\Family Templates, or provide full path. For projects: provide just the filename (e.g., 'Commercial-Default.rte') to search in C:\\ProgramData\\Autodesk\\RVT 2026\\Templates, or provide full path. Template extensions: .rft or .rfa for families, .rte or .rvt for projects."
                    }
                },
                "required": ["operation", "template_path"]
            }
        ),
        Tool(
            name="revolve_tool",
            description="Create revolve forms in Mass/Adaptive families. Create an axis line (model curve), a profile curve (CurveByPoints), and revolve the profile around the axis.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Revolve operation to perform",
                        "enum": ["create_axis_line", "create_profile_curve", "create_revolve", "get_revolve_forms"]
                    },
                    "axis_start_x": {
                        "type": "number",
                        "description": "X coordinate of axis start point (in feet)",
                        "default": 0
                    },
                    "axis_start_y": {
                        "type": "number",
                        "description": "Y coordinate of axis start point (in feet)",
                        "default": 0
                    },
                    "axis_start_z": {
                        "type": "number",
                        "description": "Z coordinate of axis start point (in feet)",
                        "default": 0
                    },
                    "axis_end_x": {
                        "type": "number",
                        "description": "X coordinate of axis end point (in feet)",
                        "default": 0
                    },
                    "axis_end_y": {
                        "type": "number",
                        "description": "Y coordinate of axis end point (in feet)",
                        "default": 0
                    },
                    "axis_end_z": {
                        "type": "number",
                        "description": "Z coordinate of axis end point (in feet)",
                        "default": 10
                    },
                    "profile_points": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "x": {"type": "number"},
                                "y": {"type": "number"},
                                "z": {"type": "number"}
                            }
                        },
                        "description": "Array of points defining the profile curve [{x,y,z}, ...]. Minimum 2 points."
                    },
                    "axis_line_id": {
                        "type": "integer",
                        "description": "Element ID of the axis model curve (from create_axis_line)"
                    },
                    "profile_curve_id": {
                        "type": "integer",
                        "description": "Element ID of the profile CurveByPoints (from create_profile_curve)"
                    },
                    "start_angle": {
                        "type": "number",
                        "description": "Start angle in degrees (0-360)",
                        "default": 0
                    },
                    "end_angle": {
                        "type": "number",
                        "description": "End angle in degrees (0-360). Use 360 for full revolution.",
                        "default": 360
                    },
                    "is_solid": {
                        "type": "boolean",
                        "description": "If true, creates solid form; if false, creates void form",
                        "default": True
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="dimension_tool",
            description="Create and manage linear and radial dimensions in projects or families. Dimension walls, grids, arcs, circles, and other elements.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Dimension operation to perform",
                        "enum": ["create_linear_dimension", "create_dimension_from_references", "create_dimension_between_walls", "create_dimension_between_grids", "create_radial_dimension", "modify_dimension", "get_dimension_types", "get_dimensions"]
                    },
                    "start_x": {
                        "type": "number",
                        "description": "X coordinate of start point (for create_linear_dimension)",
                        "default": 0
                    },
                    "start_y": {
                        "type": "number",
                        "description": "Y coordinate of start point (for create_linear_dimension)",
                        "default": 0
                    },
                    "start_z": {
                        "type": "number",
                        "description": "Z coordinate of start point (for create_linear_dimension)",
                        "default": 0
                    },
                    "end_x": {
                        "type": "number",
                        "description": "X coordinate of end point (for create_linear_dimension)",
                        "default": 10
                    },
                    "end_y": {
                        "type": "number",
                        "description": "Y coordinate of end point (for create_linear_dimension)",
                        "default": 0
                    },
                    "end_z": {
                        "type": "number",
                        "description": "Z coordinate of end point (for create_linear_dimension)",
                        "default": 0
                    },
                    "offset": {
                        "type": "number",
                        "description": "Offset distance for dimension line from elements",
                        "default": 2
                    },
                    "references": {
                        "type": "array",
                        "items": {"type": ["integer", "string"]},
                        "description": "Array of element IDs or stable representations for create_dimension_from_references"
                    },
                    "line_x1": {
                        "type": "number",
                        "description": "X coordinate of dimension line start",
                        "default": 0
                    },
                    "line_y1": {
                        "type": "number",
                        "description": "Y coordinate of dimension line start",
                        "default": 0
                    },
                    "line_z1": {
                        "type": "number",
                        "description": "Z coordinate of dimension line start",
                        "default": 0
                    },
                    "line_x2": {
                        "type": "number",
                        "description": "X coordinate of dimension line end",
                        "default": 10
                    },
                    "line_y2": {
                        "type": "number",
                        "description": "Y coordinate of dimension line end",
                        "default": 0
                    },
                    "line_z2": {
                        "type": "number",
                        "description": "Z coordinate of dimension line end",
                        "default": 0
                    },
                    "wall_id_1": {
                        "type": "integer",
                        "description": "Element ID of first wall (for create_dimension_between_walls)"
                    },
                    "wall_id_2": {
                        "type": "integer",
                        "description": "Element ID of second wall (for create_dimension_between_walls)"
                    },
                    "face": {
                        "type": "string",
                        "description": "Wall face to dimension (center, interior, exterior)",
                        "enum": ["center", "interior", "exterior"],
                        "default": "center"
                    },
                    "grid_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Array of grid element IDs (for create_dimension_between_grids)"
                    },
                    "arc_element_id": {
                        "type": "integer",
                        "description": "Element ID of arc or circle (for create_radial_dimension)"
                    },
                    "dimension_style": {
                        "type": "string",
                        "description": "Style for radial dimension (radius or diameter)",
                        "enum": ["radius", "diameter"],
                        "default": "radius"
                    },
                    "location_x": {
                        "type": "number",
                        "description": "X coordinate for dimension text location (for create_radial_dimension)"
                    },
                    "location_y": {
                        "type": "number",
                        "description": "Y coordinate for dimension text location (for create_radial_dimension)"
                    },
                    "location_z": {
                        "type": "number",
                        "description": "Z coordinate for dimension text location (for create_radial_dimension)"
                    },
                    "dimension_id": {
                        "type": "integer",
                        "description": "Element ID of dimension to modify"
                    },
                    "dimension_type_id": {
                        "type": "integer",
                        "description": "Element ID of dimension type to use"
                    },
                    "value_override": {
                        "type": "string",
                        "description": "Text to override dimension value"
                    },
                    "above": {
                        "type": "string",
                        "description": "Text to display above dimension"
                    },
                    "below": {
                        "type": "string",
                        "description": "Text to display below dimension"
                    },
                    "prefix": {
                        "type": "string",
                        "description": "Prefix text for dimension"
                    },
                    "suffix": {
                        "type": "string",
                        "description": "Suffix text for dimension"
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="family_instance_tool",
            description="Create and place family instances using various methods. Supports placing components at points, on hosts, along lines, on faces, and using references.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Family instance placement operation",
                        "enum": ["place_at_point", "place_at_point_in_view", "place_on_host", "place_on_host_with_direction", "place_along_line", "place_along_line_in_view", "place_on_face", "place_on_face_at_point", "place_on_reference", "get_family_symbols"]
                    },
                    "family_symbol_id": {
                        "type": "integer",
                        "description": "Element ID of the family type (FamilySymbol) to place"
                    },
                    "x": {
                        "type": "number",
                        "description": "X coordinate of placement location",
                        "default": 0
                    },
                    "y": {
                        "type": "number",
                        "description": "Y coordinate of placement location",
                        "default": 0
                    },
                    "z": {
                        "type": "number",
                        "description": "Z coordinate of placement location",
                        "default": 0
                    },
                    "structural_type": {
                        "type": "string",
                        "description": "Structural type for structural elements",
                        "enum": ["NonStructural", "Beam", "Brace", "Column", "Footing"],
                        "default": "NonStructural"
                    },
                    "view_id": {
                        "type": "integer",
                        "description": "Element ID of view to place instance in (uses active view if not specified)"
                    },
                    "host_id": {
                        "type": "integer",
                        "description": "Element ID of host element (wall, floor, ceiling, etc.)"
                    },
                    "host_element_id": {
                        "type": "integer",
                        "description": "Element ID of host element for face-based placement"
                    },
                    "reference_element_id": {
                        "type": "integer",
                        "description": "Element ID for reference-based placement"
                    },
                    "face_index": {
                        "type": "integer",
                        "description": "Index of face on host element (0-based)",
                        "default": 0
                    },
                    "direction_x": {
                        "type": "number",
                        "description": "X component of direction vector",
                        "default": 1
                    },
                    "direction_y": {
                        "type": "number",
                        "description": "Y component of direction vector",
                        "default": 0
                    },
                    "direction_z": {
                        "type": "number",
                        "description": "Z component of direction vector",
                        "default": 0
                    },
                    "line_start_x": {
                        "type": "number",
                        "description": "X coordinate of line start point",
                        "default": 0
                    },
                    "line_start_y": {
                        "type": "number",
                        "description": "Y coordinate of line start point",
                        "default": 0
                    },
                    "line_start_z": {
                        "type": "number",
                        "description": "Z coordinate of line start point",
                        "default": 0
                    },
                    "line_end_x": {
                        "type": "number",
                        "description": "X coordinate of line end point",
                        "default": 10
                    },
                    "line_end_y": {
                        "type": "number",
                        "description": "Y coordinate of line end point",
                        "default": 0
                    },
                    "line_end_z": {
                        "type": "number",
                        "description": "Z coordinate of line end point",
                        "default": 0
                    },
                    "family_name": {
                        "type": "string",
                        "description": "Filter results by family name (for get_family_symbols)"
                    },
                    "category": {
                        "type": "string",
                        "description": "Filter results by category name (for get_family_symbols)"
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="load_and_place_family",
            description="Comprehensive family management tool: list families in project by category, list family types, load families from .rfa files, and place family instances using various methods (point, host, face, line). Combines family discovery, loading, and placement in one unified tool.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Family operation to perform",
                        "enum": ["list_families", "list_family_types", "load_family", "place_family"]
                    },
                    "category": {
                        "type": "string",
                        "description": "Filter by category name (for list_families). Example: 'Doors', 'Windows', 'Furniture', 'Structural Framing'"
                    },
                    "include_system_families": {
                        "type": "boolean",
                        "description": "Include system families in results (for list_families)",
                        "default": False
                    },
                    "family_name": {
                        "type": "string",
                        "description": "Family name (for list_family_types and place_family)"
                    },
                    "family_id": {
                        "type": "integer",
                        "description": "Family element ID (for list_family_types)"
                    },
                    "file_path": {
                        "type": "string",
                        "description": "Full path to .rfa family file to load (for load_family)"
                    },
                    "type_id": {
                        "type": "integer",
                        "description": "Family symbol/type ID to place (for place_family)"
                    },
                    "type_name": {
                        "type": "string",
                        "description": "Family type name (for place_family, used with family_name)"
                    },
                    "placement_method": {
                        "type": "string",
                        "description": "Placement method (for place_family)",
                        "enum": ["point", "point_in_view", "host", "face", "line"],
                        "default": "point"
                    },
                    "x": {
                        "type": "number",
                        "description": "X coordinate for placement",
                        "default": 0
                    },
                    "y": {
                        "type": "number",
                        "description": "Y coordinate for placement",
                        "default": 0
                    },
                    "z": {
                        "type": "number",
                        "description": "Z coordinate for placement",
                        "default": 0
                    },
                    "structural_type": {
                        "type": "string",
                        "description": "Structural type for point placement method",
                        "enum": ["NonStructural", "Column", "Beam", "Brace", "Footing"],
                        "default": "NonStructural"
                    },
                    "view_id": {
                        "type": "integer",
                        "description": "View ID for point_in_view or line placement methods"
                    },
                    "host_id": {
                        "type": "integer",
                        "description": "Host element ID for host placement method (wall, floor, ceiling, etc.)"
                    },
                    "face_element_id": {
                        "type": "integer",
                        "description": "Element ID containing the face for face placement method"
                    },
                    "face_index": {
                        "type": "integer",
                        "description": "Face index on the element for face placement method",
                        "default": 0
                    },
                    "line_start_x": {
                        "type": "number",
                        "description": "Line start X coordinate for line placement method",
                        "default": 0
                    },
                    "line_start_y": {
                        "type": "number",
                        "description": "Line start Y coordinate for line placement method",
                        "default": 0
                    },
                    "line_start_z": {
                        "type": "number",
                        "description": "Line start Z coordinate for line placement method",
                        "default": 0
                    },
                    "line_end_x": {
                        "type": "number",
                        "description": "Line end X coordinate for line placement method",
                        "default": 10
                    },
                    "line_end_y": {
                        "type": "number",
                        "description": "Line end Y coordinate for line placement method",
                        "default": 0
                    },
                    "line_end_z": {
                        "type": "number",
                        "description": "Line end Z coordinate for line placement method",
                        "default": 0
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="load_family_tool",
            description="Load a Revit family (.rfa file) into the current project using Document.LoadFamily. Optionally save the file path to a text file for future reference and tracking of loaded families.",
            inputSchema={
                "type": "object",
                "properties": {
                    "file_path": {
                        "type": "string",
                        "description": "Full path to the .rfa family file to load. Example: 'C:\\Families\\Door.rfa'"
                    },
                    "save_path_to_file": {
                        "type": "boolean",
                        "description": "If true, saves the family file path to a text file for future reference",
                        "default": False
                    },
                    "path_storage_file": {
                        "type": "string",
                        "description": "Optional: Custom path to the text file where family paths should be stored. If not provided and save_path_to_file is true, will use 'LoadedFamilies.txt' in the project directory or temp folder."
                    }
                },
                "required": ["file_path"]
            }
        ),
        Tool(
            name="divided_surface_tool",
            description="Create and manage divided surfaces on form faces in conceptual mass or adaptive family documents. Divided surfaces allow creating parametric grid patterns on form geometry for panelization and pattern-based design. Only works in family (.rfa) documents.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Divided surface operation to perform",
                        "enum": ["create_divided_surface", "set_uv_divisions", "set_grid_properties", "get_divided_surfaces", "get_forms"]
                    },
                    "form_id": {
                        "type": "integer",
                        "description": "Element ID of the Form element to create divided surface on (for create_divided_surface)"
                    },
                    "face_index": {
                        "type": "integer",
                        "description": "Index of the face on the form (0-based). Use get_forms to see faceCount for each form.",
                        "default": 0
                    },
                    "divided_surface_id": {
                        "type": "integer",
                        "description": "Element ID of the divided surface to modify (for set_uv_divisions or set_grid_properties)"
                    },
                    "u_divisions": {
                        "type": "integer",
                        "description": "Number of divisions in U direction (for set_uv_divisions)",
                        "default": 10
                    },
                    "v_divisions": {
                        "type": "integer",
                        "description": "Number of divisions in V direction (for set_uv_divisions)",
                        "default": 8
                    },
                    "show_nodes": {
                        "type": "boolean",
                        "description": "Show or hide intersecting grid nodes (for set_grid_properties)"
                    },
                    "u_grid_lines": {
                        "type": "integer",
                        "description": "Number of U grid lines (for set_grid_properties)"
                    },
                    "v_grid_lines": {
                        "type": "integer",
                        "description": "Number of V grid lines (for set_grid_properties)"
                    },
                    "justification": {
                        "type": "string",
                        "description": "Grid justification alignment (for set_grid_properties)",
                        "enum": ["beginning", "start", "middle", "center", "end", "ending"]
                    }
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="family_modeling_tool",
            description="Create geometry in family documents using FamilyCreate methods. Supports extrusions, blends, revolutions, sweeps, swept blends, loft forms, openings, model text, symbolic curves, and dimensions. Only works in family (.rfa) files.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Family modeling operation to perform",
                        "enum": ["new_extrusion", "new_blend", "new_revolution", "new_sweep", "new_swept_blend", "new_loft_form", "new_form_by_cap", "new_form_by_thicken", "new_revolve_form", "new_extrusion_form", "new_swept_blend_form", "new_model_text", "new_opening", "new_symbolic_curve", "new_control", "new_diameter_dimension", "convert_symbolic_to_model", "get_forms", "get_sketch_planes"]
                    },
                    "is_solid": {
                        "type": "boolean",
                        "description": "Whether to create solid (true) or void (false) geometry",
                        "default": True
                    },
                    "element_id": {
                        "type": "integer",
                        "description": "Element ID of symbolic curve to convert (for convert_symbolic_to_model operation)"
                    },
                    "element_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Array of symbolic curve element IDs to convert to model curves (for convert_symbolic_to_model operation)"
                    },
                    "profile_points": {
                        "type": "array",
                        "items": {"type": "object", "properties": {"x": {"type": "number"}, "y": {"type": "number"}, "z": {"type": "number"}}},
                        "description": "Array of XYZ points defining the profile"
                    },
                    "bottom_profile_points": {
                        "type": "array",
                        "items": {"type": "object", "properties": {"x": {"type": "number"}, "y": {"type": "number"}, "z": {"type": "number"}}},
                        "description": "Array of XYZ points for bottom profile (for blend)"
                    },
                    "top_profile_points": {
                        "type": "array",
                        "items": {"type": "object", "properties": {"x": {"type": "number"}, "y": {"type": "number"}, "z": {"type": "number"}}},
                        "description": "Array of XYZ points for top profile (for blend)"
                    },
                    "extrusion_start": {
                        "type": "number",
                        "description": "Start offset for extrusion",
                        "default": 0
                    },
                    "extrusion_end": {
                        "type": "number",
                        "description": "End offset for extrusion",
                        "default": 10
                    },
                    "sketch_plane_id": {
                        "type": "integer",
                        "description": "Element ID of existing sketch plane to use"
                    },
                    "sketch_plane_name": {
                        "type": "string",
                        "description": "Name of sketch plane to find (e.g., 'Ref. Level')"
                    },
                    "plane_origin_x": {"type": "number", "description": "X coordinate of plane origin", "default": 0},
                    "plane_origin_y": {"type": "number", "description": "Y coordinate of plane origin", "default": 0},
                    "plane_origin_z": {"type": "number", "description": "Z coordinate of plane origin", "default": 0},
                    "plane_normal_x": {"type": "number", "description": "X component of plane normal", "default": 0},
                    "plane_normal_y": {"type": "number", "description": "Y component of plane normal", "default": 0},
                    "plane_normal_z": {"type": "number", "description": "Z component of plane normal", "default": 1},
                    "axis_start_x": {"type": "number", "description": "X of axis start (for revolution)"},
                    "axis_start_y": {"type": "number", "description": "Y of axis start (for revolution)"},
                    "axis_start_z": {"type": "number", "description": "Z of axis start (for revolution)"},
                    "axis_end_x": {"type": "number", "description": "X of axis end (for revolution)"},
                    "axis_end_y": {"type": "number", "description": "Y of axis end (for revolution)"},
                    "axis_end_z": {"type": "number", "description": "Z of axis end (for revolution)"},
                    "start_angle": {"type": "number", "description": "Start angle in degrees", "default": 0},
                    "end_angle": {"type": "number", "description": "End angle in degrees", "default": 360},
                    "path_curve_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Element IDs of path curves (for sweep, swept blend)"
                    },
                    "profile_curve_ids": {
                        "type": "array",
                        "items": {"type": "integer"},
                        "description": "Element IDs of existing profile curves (for new_extrusion, loft, cap, extrusion form). For new_extrusion, all curves will be collected into single array for one extrusion operation. Alternative to profile_points parameter."
                    },
                    "axis_line_id": {"type": "integer", "description": "Element ID of axis line (for revolve form)"},
                    "profile_curve_id": {"type": "integer", "description": "Element ID of profile curve (for revolve form)"},
                    "direction_x": {"type": "number", "description": "X component of direction", "default": 0},
                    "direction_y": {"type": "number", "description": "Y component of direction", "default": 0},
                    "direction_z": {"type": "number", "description": "Z component of direction (extrusion height)", "default": 10},
                    "surface_form_id": {"type": "integer", "description": "Element ID of surface form (for thicken)"},
                    "thickness": {"type": "number", "description": "Thickness for surface thickening", "default": 1.0},
                    "text": {"type": "string", "description": "Text content (for model text)", "default": "Text"},
                    "depth": {"type": "number", "description": "Depth of model text", "default": 1.0},
                    "x": {"type": "number", "description": "X coordinate for positioning", "default": 0},
                    "y": {"type": "number", "description": "Y coordinate for positioning", "default": 0},
                    "z": {"type": "number", "description": "Z coordinate for positioning", "default": 0},
                    "horizontal_align": {"type": "string", "enum": ["left", "center", "right"], "default": "left"},
                    "host_element_id": {"type": "integer", "description": "Element ID of host (for opening)"},
                    "curve_type": {"type": "string", "enum": ["line", "arc", "circle"], "description": "Type of symbolic curve", "default": "line"},
                    "start_x": {"type": "number", "description": "X of line start"},
                    "start_y": {"type": "number", "description": "Y of line start"},
                    "start_z": {"type": "number", "description": "Z of line start"},
                    "end_x": {"type": "number", "description": "X of line end"},
                    "end_y": {"type": "number", "description": "Y of line end"},
                    "end_z": {"type": "number", "description": "Z of line end"},
                    "center_x": {"type": "number", "description": "X of arc/circle center"},
                    "center_y": {"type": "number", "description": "Y of arc/circle center"},
                    "center_z": {"type": "number", "description": "Z of arc/circle center"},
                    "radius": {"type": "number", "description": "Radius for arc/circle", "default": 5},
                    "arc_element_id": {"type": "integer", "description": "Element ID of arc (for diameter dimension)"},
                    "origin_x": {"type": "number", "description": "X of dimension origin"},
                    "origin_y": {"type": "number", "description": "Y of dimension origin"},
                    "origin_z": {"type": "number", "description": "Z of dimension origin"}
                },
                "required": ["operation"]
            }
        ),
        Tool(
            name="connector_tool",
            description="Create MEP connectors in family documents. Supports duct, pipe, electrical, cable tray, and conduit connectors. Connectors are placed on planar faces of geometry and define connection points for MEP systems. Only works in family (.rfa) files.",
            inputSchema={
                "type": "object",
                "properties": {
                    "operation": {
                        "type": "string",
                        "description": "Connector operation to perform",
                        "enum": ["create_duct_connector", "create_pipe_connector", "create_electrical_connector", "create_cable_tray_connector", "create_conduit_connector", "change_host_reference", "get_connectors"]
                    },
                    "element_id": {
                        "type": "integer",
                        "description": "Element ID of the geometry to place connector on"
                    },
                    "face_index": {
                        "type": "integer",
                        "description": "Index of the planar face to use (0-based)",
                        "default": 0
                    },
                    "edge_index": {
                        "type": "integer",
                        "description": "Optional index of edge loop to use"
                    },
                    "system_type": {
                        "type": "string",
                        "description": "System type for the connector. For duct: SupplyAir/ReturnAir/ExhaustAir/OtherAir/UndefinedSystemType. For pipe: SupplyHydronic/ReturnHydronic/DomesticHotWater/DomesticColdWater/Sanitary/Vent/Fire/OtherPipe. For electrical: PowerCircuit/Data/Telephone/Security/FireAlarm/NurseCall/Controls/Communication."
                    },
                    "profile_type": {
                        "type": "string",
                        "description": "Profile type for duct connectors: Round/Rectangular/Oval",
                        "enum": ["Round", "Rectangular", "Oval"],
                        "default": "Round"
                    },
                    "connector_id": {
                        "type": "integer",
                        "description": "Element ID of existing connector (for change_host_reference)"
                    },
                    "new_element_id": {
                        "type": "integer",
                        "description": "Element ID of new host geometry (for change_host_reference)"
                    },
                    "new_face_index": {
                        "type": "integer",
                        "description": "Face index on new host (for change_host_reference)",
                        "default": 0
                    },
                    "new_edge_index": {
                        "type": "integer",
                        "description": "Optional edge index on new host (for change_host_reference)"
                    }
                },
                "required": ["operation"]
            }
        )
    ]


@app.call_tool()
async def call_tool(name: str, arguments: Any) -> Sequence[TextContent | ImageContent | EmbeddedResource]:
    """Execute a Revit tool"""
    
    logger.info(f"MCP Tool called: {name} with args: {arguments}")
    
    if not revit.connected:
        logger.warning("MCP server not connected to Revit")
        return [TextContent(
            type="text",
            text=json.dumps({"error": "Not connected to Revit. Please start Revit 2026 first."})
        )]
    
    try:
        if name == "get_elements_by_category":
            category = arguments.get("category")
            result = await revit.send_command("get_elements_by_category", {"category": category})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_element_parameters":
            element_id = arguments.get("element_id")
            result = await revit.send_command("get_element_parameters", {"element_id": element_id})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "set_parameter_value":
            element_id = arguments.get("element_id")
            parameter_name = arguments.get("parameter_name")
            value = arguments.get("value")
            result = await revit.send_command("set_parameter_value", {
                "element_id": element_id,
                "parameter_name": parameter_name,
                "value": value
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_view":
            view_type = arguments.get("view_type")
            name = arguments.get("name")
            level = arguments.get("level", "Level 1")
            result = await revit.send_command("create_view", {
                "view_type": view_type,
                "name": name,
                "level": level
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "export_to_ifc":
            file_path = arguments.get("file_path")
            ifc_version = arguments.get("ifc_version", "IFC4")
            result = await revit.send_command("export_to_ifc", {
                "file_path": file_path,
                "ifc_version": ifc_version
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "export_image":
            output_path = arguments.get("output_path")
            file_type = arguments.get("file_type", "PNG")
            dpi = arguments.get("dpi", 150)
            zoom_type = arguments.get("zoom_type", "FitToPage")
            zoom = arguments.get("zoom", 100)
            fit_direction = arguments.get("fit_direction", "Horizontal")
            export_range = arguments.get("export_range", "CurrentView")
            view_ids = arguments.get("view_ids")
            create_website = arguments.get("create_website", False)
            
            command_params = {
                "output_path": output_path,
                "file_type": file_type,
                "dpi": dpi,
                "zoom_type": zoom_type,
                "zoom": zoom,
                "fit_direction": fit_direction,
                "export_range": export_range,
                "create_website": create_website
            }
            
            if view_ids is not None:
                command_params["view_ids"] = view_ids
                
            result = await revit.send_command("export_image", command_params)
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "query_elements":
            filter_type = arguments.get("filter_type")
            criteria = arguments.get("criteria")
            result = await revit.send_command("query_elements", {
                "filter_type": filter_type,
                "criteria": criteria
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_project_info":
            result = await revit.send_command("get_project_info", {})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "select_view_type":
            result = await revit.send_command("select_view_type", {})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "get_selected_elements":
            operation = arguments.get("operation")
            command_params = {}
            if operation:
                command_params["operation"] = operation
            result = await revit.send_command("get_selected_elements", command_params)
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "set_active_view":
            operation = arguments.get("operation")
            view_type = arguments.get("view_type")
            view_name = arguments.get("view_name")
            
            # For project documents or 'by_type' operation, view_type is often used
            # For family documents with 'by_name' operation, view_name is used
            command_params = {}
            
            if operation:
                command_params["operation"] = operation
            if view_type:
                command_params["view_type"] = view_type
            if view_name:
                command_params["view_name"] = view_name
                
            result = await revit.send_command("set_active_view", command_params)
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_wall":
            start_x = arguments.get("start_x", 0)
            start_y = arguments.get("start_y", 0)
            end_x = arguments.get("end_x", 10)
            end_y = arguments.get("end_y", 0)
            level = arguments.get("level", "Level 1")
            wall_type = arguments.get("wall_type", None)
            result = await revit.send_command("create_wall", {
                "start_x": start_x,
                "start_y": start_y,
                "end_x": end_x,
                "end_y": end_y,
                "level": level,
                "wall_type": wall_type
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_grid_line":
            result = await revit.send_command("create_grid_line", {
                "name": arguments.get("name"),
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_grid_arc":
            if arguments.get("start_x") is None or arguments.get("start_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Arc grids require start_x and start_y"})
                )]
            if arguments.get("end_x") is None or arguments.get("end_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Arc grids require end_x and end_y"})
                )]
            if arguments.get("center_x") is None or arguments.get("center_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Arc grids require center_x and center_y"})
                )]
            result = await revit.send_command("create_grid_arc", {
                "name": arguments.get("name"),
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0),
                "center_x": arguments.get("center_x"),
                "center_y": arguments.get("center_y"),
                "center_z": arguments.get("center_z", 0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_bounded_line":
            if arguments.get("start_x") is None or arguments.get("start_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Bounded line requires start_x and start_y"})
                )]
            if arguments.get("end_x") is None or arguments.get("end_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Bounded line requires end_x and end_y"})
                )]
            result = await revit.send_command("create_bounded_line", {
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_detail_line":
            if arguments.get("start_x") is None or arguments.get("start_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Detail line requires start_x and start_y"})
                )]
            if arguments.get("end_x") is None or arguments.get("end_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Detail line requires end_x and end_y"})
                )]
            result = await revit.send_command("create_detail_line", {
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_curves_from_points":
            points = arguments.get("points")
            if not isinstance(points, list) or len(points) < 2:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "points array must contain at least two items"})
                )]
            result = await revit.send_command("create_curves_from_points", {
                "points": points,
                "closed": arguments.get("closed", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_hermite_spline":
            points = arguments.get("points")
            if not isinstance(points, list) or len(points) < 2:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Hermite spline requires a points array with at least two items"})
                )]
            result = await revit.send_command("create_hermite_spline", {
                "points": points,
                "closed": arguments.get("closed", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_hermite_spline_with_tangents":
            points = arguments.get("points")
            if not isinstance(points, list) or len(points) < 2:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Hermite spline requires a points array with at least two items"})
                )]
            if not isinstance(arguments.get("start_tangent"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "start_tangent is required"})
                )]
            if not isinstance(arguments.get("end_tangent"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "end_tangent is required"})
                )]
            result = await revit.send_command("create_hermite_spline_with_tangents", {
                "points": points,
                "start_tangent": arguments.get("start_tangent"),
                "end_tangent": arguments.get("end_tangent"),
                "closed": arguments.get("closed", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_offset_curve":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if arguments.get("offset") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "offset is required"})
                )]
            result = await revit.send_command("create_offset_curve", {
                "curve_element_id": arguments.get("curve_element_id"),
                "offset": arguments.get("offset"),
                "normal": arguments.get("normal")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "evaluate_curve":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if arguments.get("parameter") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter is required"})
                )]
            result = await revit.send_command("evaluate_curve", {
                "curve_element_id": arguments.get("curve_element_id"),
                "parameter": arguments.get("parameter"),
                "normalized": arguments.get("normalized", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_distance_to_point":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if not isinstance(arguments.get("point"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "point is required"})
                )]
            result = await revit.send_command("curve_distance_to_point", {
                "curve_element_id": arguments.get("curve_element_id"),
                "point": arguments.get("point")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_get_end_point":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            result = await revit.send_command("curve_get_end_point", {
                "curve_element_id": arguments.get("curve_element_id"),
                "end": arguments.get("end", "end")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_get_end_parameter":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            result = await revit.send_command("curve_get_end_parameter", {
                "curve_element_id": arguments.get("curve_element_id"),
                "end": arguments.get("end", "end")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_get_end_point_reference":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            result = await revit.send_command("curve_get_end_point_reference", {
                "curve_element_id": arguments.get("curve_element_id"),
                "end": arguments.get("end", "end")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_clone_curve":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            result = await revit.send_command("create_clone_curve", {
                "curve_element_id": arguments.get("curve_element_id")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_compute_closest_points":
            if arguments.get("curve_element_id_1") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id_1 is required"})
                )]
            if arguments.get("curve_element_id_2") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id_2 is required"})
                )]
            result = await revit.send_command("curve_compute_closest_points", {
                "curve_element_id_1": arguments.get("curve_element_id_1"),
                "curve_element_id_2": arguments.get("curve_element_id_2"),
                "within_this_curve_bounds": arguments.get("within_this_curve_bounds", True),
                "within_other_curve_bounds": arguments.get("within_other_curve_bounds", True),
                "return_all_critical_points": arguments.get("return_all_critical_points", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_compute_derivatives":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if arguments.get("parameter") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter is required"})
                )]
            result = await revit.send_command("curve_compute_derivatives", {
                "curve_element_id": arguments.get("curve_element_id"),
                "parameter": arguments.get("parameter"),
                "normalized": arguments.get("normalized", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_compute_normalized_parameter":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if arguments.get("parameter") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter is required"})
                )]
            result = await revit.send_command("curve_compute_normalized_parameter", {
                "curve_element_id": arguments.get("curve_element_id"),
                "parameter": arguments.get("parameter")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_compute_raw_parameter":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if arguments.get("normalized_parameter") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "normalized_parameter is required"})
                )]
            result = await revit.send_command("curve_compute_raw_parameter", {
                "curve_element_id": arguments.get("curve_element_id"),
                "normalized_parameter": arguments.get("normalized_parameter")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_create_reversed":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            result = await revit.send_command("curve_create_reversed", {
                "curve_element_id": arguments.get("curve_element_id")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_create_transformed":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if not isinstance(arguments.get("transform"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "transform is required"})
                )]
            result = await revit.send_command("curve_create_transformed", {
                "curve_element_id": arguments.get("curve_element_id"),
                "transform": arguments.get("transform")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_intersect":
            if arguments.get("curve_element_id_1") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id_1 is required"})
                )]
            if arguments.get("curve_element_id_2") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id_2 is required"})
                )]
            result = await revit.send_command("curve_intersect", {
                "curve_element_id_1": arguments.get("curve_element_id_1"),
                "curve_element_id_2": arguments.get("curve_element_id_2")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_point":
            if arguments.get("point") is None and (arguments.get("x") is None or arguments.get("y") is None):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "point or x/y is required"})
                )]
            result = await revit.send_command("create_point", {
                "point": arguments.get("point"),
                "x": arguments.get("x"),
                "y": arguments.get("y"),
                "z": arguments.get("z")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "create_point_on_element":
            if arguments.get("element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "element_id is required"})
                )]
            if not isinstance(arguments.get("point"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "point is required"})
                )]
            result = await revit.send_command("create_point_on_element", {
                "element_id": arguments.get("element_id"),
                "point": arguments.get("point")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]

        elif name == "curve_point_location_on_curve":
            if arguments.get("curve_element_id") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "curve_element_id is required"})
                )]
            if not isinstance(arguments.get("point"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "point is required"})
                )]
            result = await revit.send_command("curve_point_location_on_curve", {
                "curve_element_id": arguments.get("curve_element_id"),
                "point": arguments.get("point")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "calculate_line_direction":
            if not isinstance(arguments.get("start_point"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "start_point is required"})
                )]
            if not isinstance(arguments.get("end_point"), dict):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "end_point is required"})
                )]
            result = await revit.send_command("calculate_line_direction", {
                "start_point": arguments.get("start_point"),
                "end_point": arguments.get("end_point"),
                "normalize": arguments.get("normalize", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_point_markup":
            if not isinstance(arguments.get("points"), list):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "points array is required"})
                )]
            result = await revit.send_command("create_point_markup", {
                "points": arguments.get("points"),
                "markup_type": arguments.get("markup_type", "cross"),
                "size": arguments.get("size", 1.0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_detail_shapes":
            if not arguments.get("shape_type"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "shape_type is required (rectangle, circle, polygon)"})
                )]
            result = await revit.send_command("create_detail_shapes", {
                "shape_type": arguments.get("shape_type"),
                "center_x": arguments.get("center_x", 0),
                "center_y": arguments.get("center_y", 0),
                "center_z": arguments.get("center_z", 0),
                "width": arguments.get("width", 5),
                "height": arguments.get("height", 5),
                "radius": arguments.get("radius", 5),
                "sides": arguments.get("sides", 6),
                "rotation": arguments.get("rotation", 0),
                "view_id": arguments.get("view_id")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_model_shapes":
            if not arguments.get("shape_type"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "shape_type is required (rectangle, circle, polygon)"})
                )]
            result = await revit.send_command("create_model_shapes", {
                "shape_type": arguments.get("shape_type"),
                "center_x": arguments.get("center_x", 0),
                "center_y": arguments.get("center_y", 0),
                "center_z": arguments.get("center_z", 0),
                "width": arguments.get("width", 5),
                "height": arguments.get("height", 5),
                "radius": arguments.get("radius", 5),
                "sides": arguments.get("sides", 6),
                "rotation": arguments.get("rotation", 0),
                "plane_normal_x": arguments.get("plane_normal_x", 0),
                "plane_normal_y": arguments.get("plane_normal_y", 0),
                "plane_normal_z": arguments.get("plane_normal_z", 1)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_symbolic_shapes":
            if not arguments.get("shape_type"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "shape_type is required (rectangle, circle, polygon)"})
                )]
            result = await revit.send_command("create_symbolic_shapes", {
                "shape_type": arguments.get("shape_type"),
                "center_x": arguments.get("center_x", 0),
                "center_y": arguments.get("center_y", 0),
                "center_z": arguments.get("center_z", 0),
                "width": arguments.get("width", 5),
                "height": arguments.get("height", 5),
                "radius": arguments.get("radius", 5),
                "sides": arguments.get("sides", 6),
                "rotation": arguments.get("rotation", 0),
                "sketch_plane_id": arguments.get("sketch_plane_id")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "rotate_elements":
            # Validate that either element_id or element_ids is provided
            if arguments.get("element_id") is None and arguments.get("element_ids") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "element_id or element_ids is required"})
                )]
            if arguments.get("angle") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "angle is required (in degrees)"})
                )]
            result = await revit.send_command("rotate_elements", {
                "element_id": arguments.get("element_id"),
                "element_ids": arguments.get("element_ids"),
                "angle": arguments.get("angle"),
                "axis_point_x": arguments.get("axis_point_x", 0),
                "axis_point_y": arguments.get("axis_point_y", 0),
                "axis_point_z": arguments.get("axis_point_z", 0),
                "axis_direction_x": arguments.get("axis_direction_x", 0),
                "axis_direction_y": arguments.get("axis_direction_y", 0),
                "axis_direction_z": arguments.get("axis_direction_z", 1)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "add_family_shared_parameter":
            if not arguments.get("shared_parameter_file"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "shared_parameter_file is required"})
                )]
            if not arguments.get("parameter_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter_name is required"})
                )]
            result = await revit.send_command("add_family_shared_parameter", {
                "shared_parameter_file": arguments.get("shared_parameter_file"),
                "parameter_name": arguments.get("parameter_name"),
                "parameter_group": arguments.get("parameter_group", "PG_GENERAL"),
                "is_instance": arguments.get("is_instance", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "remove_family_parameter":
            if not arguments.get("parameter_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter_name is required"})
                )]
            result = await revit.send_command("remove_family_parameter", {
                "parameter_name": arguments.get("parameter_name")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_family_parameters":
            result = await revit.send_command("get_family_parameters", {})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "detect_document_type":
            result = await revit.send_command("detect_document_type", {})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "add_project_shared_parameter":
            if not arguments.get("shared_parameter_file"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "shared_parameter_file is required"})
                )]
            if not arguments.get("parameter_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter_name is required"})
                )]
            if not isinstance(arguments.get("categories"), list):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "categories array is required"})
                )]
            result = await revit.send_command("add_project_shared_parameter", {
                "shared_parameter_file": arguments.get("shared_parameter_file"),
                "parameter_name": arguments.get("parameter_name"),
                "categories": arguments.get("categories"),
                "parameter_group": arguments.get("parameter_group", "General"),
                "is_instance": arguments.get("is_instance", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "remove_project_shared_parameter":
            if not arguments.get("parameter_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "parameter_name is required"})
                )]
            result = await revit.send_command("remove_project_shared_parameter", {
                "parameter_name": arguments.get("parameter_name")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_project_shared_parameters":
            result = await revit.send_command("get_project_shared_parameters", {})
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_last_placed_element":
            result = await revit.send_command("get_last_placed_element", {
                "category": arguments.get("category")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_reference_plane":
            # Validate required points
            if arguments.get("bubble_x") is None or arguments.get("bubble_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "bubble_x and bubble_y are required"})
                )]
            if arguments.get("free_x") is None or arguments.get("free_y") is None:
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "free_x and free_y are required"})
                )]
            result = await revit.send_command("create_reference_plane", {
                "bubble_x": arguments.get("bubble_x"),
                "bubble_y": arguments.get("bubble_y"),
                "bubble_z": arguments.get("bubble_z", 0),
                "free_x": arguments.get("free_x"),
                "free_y": arguments.get("free_y"),
                "free_z": arguments.get("free_z", 0),
                "cut_vector_x": arguments.get("cut_vector_x"),
                "cut_vector_y": arguments.get("cut_vector_y"),
                "cut_vector_z": arguments.get("cut_vector_z"),
                "name": arguments.get("name"),
                "view_type": arguments.get("view_type"),
                "view_id": arguments.get("view_id")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_reference_planes":
            result = await revit.send_command("get_reference_planes", {
                "name": arguments.get("name"),
                "element_id": arguments.get("element_id"),
                "include_unnamed": arguments.get("include_unnamed", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "set_graphic_overrides":
            # Validate that at least one target is specified
            if not arguments.get("category") and not arguments.get("element_id") and not arguments.get("element_ids"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Either 'category', 'element_id', or 'element_ids' is required"})
                )]
            result = await revit.send_command("set_graphic_overrides", {
                "category": arguments.get("category"),
                "element_id": arguments.get("element_id"),
                "element_ids": arguments.get("element_ids"),
                "view_id": arguments.get("view_id"),
                "halftone": arguments.get("halftone"),
                "transparency": arguments.get("transparency"),
                "visible": arguments.get("visible"),
                "projection_line_color": arguments.get("projection_line_color"),
                "projection_line_weight": arguments.get("projection_line_weight"),
                "cut_line_color": arguments.get("cut_line_color"),
                "cut_line_weight": arguments.get("cut_line_weight"),
                "surface_foreground_color": arguments.get("surface_foreground_color"),
                "surface_background_color": arguments.get("surface_background_color"),
                "cut_foreground_color": arguments.get("cut_foreground_color"),
                "cut_background_color": arguments.get("cut_background_color"),
                "detail_level": arguments.get("detail_level"),
                "reset": arguments.get("reset", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "create_schedule_view":
            result = await revit.send_command("create_schedule_view", {
                "category": arguments.get("category"),
                "name": arguments.get("name"),
                "is_key_schedule": arguments.get("is_key_schedule", False),
                "fields": arguments.get("fields"),
                "group_by": arguments.get("group_by"),
                "itemize_every_instance": arguments.get("itemize_every_instance", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_table_data":
            # Validate that at least one identifier is specified
            if not arguments.get("schedule_id") and not arguments.get("schedule_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Either 'schedule_id' or 'schedule_name' is required"})
                )]
            result = await revit.send_command("get_table_data", {
                "schedule_id": arguments.get("schedule_id"),
                "schedule_name": arguments.get("schedule_name"),
                "include_headers": arguments.get("include_headers", True),
                "include_hidden_fields": arguments.get("include_hidden_fields", False),
                "max_rows": arguments.get("max_rows")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "modify_schedule":
            # Validate that at least one identifier is specified
            if not arguments.get("schedule_id") and not arguments.get("schedule_name"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Either 'schedule_id' or 'schedule_name' is required"})
                )]
            result = await revit.send_command("modify_schedule", {
                "schedule_id": arguments.get("schedule_id"),
                "schedule_name": arguments.get("schedule_name"),
                "itemize_every_instance": arguments.get("itemize_every_instance"),
                "add_filter": arguments.get("add_filter"),
                "clear_filters": arguments.get("clear_filters", False),
                "add_sort_group": arguments.get("add_sort_group"),
                "remove_sort_group": arguments.get("remove_sort_group"),
                "clear_sort_groups": arguments.get("clear_sort_groups", False),
                "format_field": arguments.get("format_field"),
                "add_calculated_field": arguments.get("add_calculated_field"),
                "reorder_field": arguments.get("reorder_field")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "modify_element":
            result = await revit.send_command("modify_element", {
                "element_id": arguments.get("element_id"),
                "parameters": arguments.get("parameters"),
                "move": arguments.get("move"),
                "rotate": arguments.get("rotate"),
                "flip_facing": arguments.get("flip_facing"),
                "flip_hand": arguments.get("flip_hand"),
                "flip_workplane": arguments.get("flip_workplane"),
                "mirror": arguments.get("mirror"),
                "pin": arguments.get("pin")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "transform_elements":
            # Validate that at least one element is specified
            if not arguments.get("element_id") and not arguments.get("element_ids"):
                return [TextContent(
                    type="text",
                    text=json.dumps({"error": "Either 'element_id' or 'element_ids' is required"})
                )]
            result = await revit.send_command("transform_elements", {
                "element_id": arguments.get("element_id"),
                "element_ids": arguments.get("element_ids"),
                "operation": arguments.get("operation"),
                "translation": arguments.get("translation"),
                "rotation": arguments.get("rotation"),
                "mirror_plane": arguments.get("mirror_plane"),
                "array_count": arguments.get("array_count"),
                "array_spacing": arguments.get("array_spacing"),
                "radial_center": arguments.get("radial_center"),
                "radial_angle": arguments.get("radial_angle")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_family_types":
            result = await revit.send_command("get_family_types", {
                "family_name": arguments.get("family_name"),
                "category": arguments.get("category"),
                "include_parameters": arguments.get("include_parameters", False),
                "max_results": arguments.get("max_results")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "get_instances":
            result = await revit.send_command("get_instances", {
                "category": arguments.get("category"),
                "family_name": arguments.get("family_name"),
                "type_name": arguments.get("type_name"),
                "element_class": arguments.get("element_class"),
                "include_location": arguments.get("include_location", True),
                "include_parameters": arguments.get("include_parameters", False),
                "max_results": arguments.get("max_results")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "find_family_type":
            result = await revit.send_command("find_family_type", {
                "family_name": arguments.get("family_name"),
                "type_name": arguments.get("type_name"),
                "category": arguments.get("category"),
                "exact_match": arguments.get("exact_match", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "find_elements":
            result = await revit.send_command("find_elements", {
                "category": arguments.get("category"),
                "family_name": arguments.get("family_name"),
                "type_name": arguments.get("type_name"),
                "parameter_filter": arguments.get("parameter_filter"),
                "level_name": arguments.get("level_name"),
                "bounding_box": arguments.get("bounding_box"),
                "view_specific": arguments.get("view_specific", False),
                "include_location": arguments.get("include_location", True),
                "max_results": arguments.get("max_results")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "selection_tool":
            result = await revit.send_command("selection_tool", {
                "operation": arguments.get("operation"),
                "element_ids": arguments.get("element_ids"),
                "prompt": arguments.get("prompt"),
                "filter_category": arguments.get("filter_category"),
                "filter_class": arguments.get("filter_class"),
                "include_location": arguments.get("include_location", True),
                "include_parameters": arguments.get("include_parameters", False)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "task_dialog":
            result = await revit.send_command("task_dialog", {
                "mode": arguments.get("mode", "simple"),
                "title": arguments.get("title"),
                "message": arguments.get("message"),
                "main_instruction": arguments.get("main_instruction"),
                "main_content": arguments.get("main_content"),
                "expanded_content": arguments.get("expanded_content"),
                "footer_text": arguments.get("footer_text"),
                "common_buttons": arguments.get("common_buttons"),
                "command_links": arguments.get("command_links"),
                "default_button": arguments.get("default_button"),
                "verification_text": arguments.get("verification_text"),
                "allow_cancellation": arguments.get("allow_cancellation", True),
                "main_icon": arguments.get("main_icon")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "family_points_tool":
            result = await revit.send_command("family_points_tool", {
                "operation": arguments.get("operation"),
                "x": arguments.get("x", 0),
                "y": arguments.get("y", 0),
                "z": arguments.get("z", 0),
                "count_x": arguments.get("count_x", 5),
                "count_y": arguments.get("count_y", 5),
                "spacing_x": arguments.get("spacing_x", 1.0),
                "spacing_y": arguments.get("spacing_y", 1.0),
                "direction": arguments.get("direction", "x"),
                "point_ids": arguments.get("point_ids"),
                "z_formula": arguments.get("z_formula"),
                "curve_direction": arguments.get("curve_direction", "rows"),
                "is_reference_line": arguments.get("is_reference_line", False),
                "curve_ids": arguments.get("curve_ids"),
                "is_solid": arguments.get("is_solid", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "family_manager_tool":
            result = await revit.send_command("family_manager_tool", {
                "operation": arguments.get("operation"),
                "type_name": arguments.get("type_name"),
                "old_name": arguments.get("old_name"),
                "new_name": arguments.get("new_name"),
                "parameter_name": arguments.get("parameter_name"),
                "family_parameter_name": arguments.get("family_parameter_name"),
                "element_id": arguments.get("element_id"),
                "element_parameter_name": arguments.get("element_parameter_name"),
                "value": arguments.get("value"),
                "formula": arguments.get("formula")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "application_document_tool":
            result = await revit.send_command("application_document_tool", {
                "operation": arguments.get("operation"),
                "template_path": arguments.get("template_path")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "revolve_tool":
            result = await revit.send_command("revolve_tool", {
                "operation": arguments.get("operation"),
                "axis_start_x": arguments.get("axis_start_x", 0),
                "axis_start_y": arguments.get("axis_start_y", 0),
                "axis_start_z": arguments.get("axis_start_z", 0),
                "axis_end_x": arguments.get("axis_end_x", 0),
                "axis_end_y": arguments.get("axis_end_y", 0),
                "axis_end_z": arguments.get("axis_end_z", 10),
                "profile_points": arguments.get("profile_points"),
                "axis_line_id": arguments.get("axis_line_id"),
                "profile_curve_id": arguments.get("profile_curve_id"),
                "start_angle": arguments.get("start_angle", 0),
                "end_angle": arguments.get("end_angle", 360),
                "is_solid": arguments.get("is_solid", True)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "dimension_tool":
            result = await revit.send_command("dimension_tool", {
                "operation": arguments.get("operation"),
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0),
                "offset": arguments.get("offset", 2),
                "references": arguments.get("references"),
                "line_x1": arguments.get("line_x1", 0),
                "line_y1": arguments.get("line_y1", 0),
                "line_z1": arguments.get("line_z1", 0),
                "line_x2": arguments.get("line_x2", 10),
                "line_y2": arguments.get("line_y2", 0),
                "line_z2": arguments.get("line_z2", 0),
                "wall_id_1": arguments.get("wall_id_1"),
                "wall_id_2": arguments.get("wall_id_2"),
                "face": arguments.get("face", "center"),
                "grid_ids": arguments.get("grid_ids"),
                "arc_element_id": arguments.get("arc_element_id"),
                "dimension_style": arguments.get("dimension_style", "radius"),
                "location_x": arguments.get("location_x"),
                "location_y": arguments.get("location_y"),
                "location_z": arguments.get("location_z"),
                "dimension_id": arguments.get("dimension_id"),
                "dimension_type_id": arguments.get("dimension_type_id"),
                "value_override": arguments.get("value_override"),
                "above": arguments.get("above"),
                "below": arguments.get("below"),
                "prefix": arguments.get("prefix"),
                "suffix": arguments.get("suffix")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "family_instance_tool":
            result = await revit.send_command("family_instance_tool", {
                "operation": arguments.get("operation"),
                "family_symbol_id": arguments.get("family_symbol_id"),
                "x": arguments.get("x", 0),
                "y": arguments.get("y", 0),
                "z": arguments.get("z", 0),
                "structural_type": arguments.get("structural_type", "NonStructural"),
                "view_id": arguments.get("view_id"),
                "host_id": arguments.get("host_id"),
                "host_element_id": arguments.get("host_element_id"),
                "reference_element_id": arguments.get("reference_element_id"),
                "face_index": arguments.get("face_index", 0),
                "direction_x": arguments.get("direction_x", 1),
                "direction_y": arguments.get("direction_y", 0),
                "direction_z": arguments.get("direction_z", 0),
                "line_start_x": arguments.get("line_start_x", 0),
                "line_start_y": arguments.get("line_start_y", 0),
                "line_start_z": arguments.get("line_start_z", 0),
                "line_end_x": arguments.get("line_end_x", 10),
                "line_end_y": arguments.get("line_end_y", 0),
                "line_end_z": arguments.get("line_end_z", 0),
                "family_name": arguments.get("family_name"),
                "category": arguments.get("category"),
                "center_x": arguments.get("center_x", 0),
                "center_y": arguments.get("center_y", 0),
                "center_z": arguments.get("center_z", 0),
                "width": arguments.get("width", 10),
                "height": arguments.get("height", 5),
                "radius": arguments.get("radius", 5),
                "sides": arguments.get("sides", 6),
                "segments": arguments.get("segments", 24)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "load_family_tool":
            result = await revit.send_command("load_family_tool", {
                "file_path": arguments.get("file_path"),
                "save_path_to_file": arguments.get("save_path_to_file", False),
                "path_storage_file": arguments.get("path_storage_file")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "load_and_place_family":
            result = await revit.send_command("load_and_place_family", {
                "operation": arguments.get("operation"),
                "category": arguments.get("category"),
                "include_system_families": arguments.get("include_system_families", False),
                "family_name": arguments.get("family_name"),
                "family_id": arguments.get("family_id"),
                "file_path": arguments.get("file_path"),
                "type_id": arguments.get("type_id"),
                "type_name": arguments.get("type_name"),
                "placement_method": arguments.get("placement_method", "point"),
                "x": arguments.get("x", 0),
                "y": arguments.get("y", 0),
                "z": arguments.get("z", 0),
                "structural_type": arguments.get("structural_type", "NonStructural"),
                "view_id": arguments.get("view_id"),
                "host_id": arguments.get("host_id"),
                "face_element_id": arguments.get("face_element_id"),
                "face_index": arguments.get("face_index", 0),
                "line_start_x": arguments.get("line_start_x", 0),
                "line_start_y": arguments.get("line_start_y", 0),
                "line_start_z": arguments.get("line_start_z", 0),
                "line_end_x": arguments.get("line_end_x", 10),
                "line_end_y": arguments.get("line_end_y", 0),
                "line_end_z": arguments.get("line_end_z", 0)
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "divided_surface_tool":
            result = await revit.send_command("divided_surface_tool", {
                "operation": arguments.get("operation"),
                "form_id": arguments.get("form_id"),
                "face_index": arguments.get("face_index", 0),
                "divided_surface_id": arguments.get("divided_surface_id"),
                "u_divisions": arguments.get("u_divisions", 10),
                "v_divisions": arguments.get("v_divisions", 8),
                "show_nodes": arguments.get("show_nodes"),
                "u_grid_lines": arguments.get("u_grid_lines"),
                "v_grid_lines": arguments.get("v_grid_lines"),
                "justification": arguments.get("justification")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "family_modeling_tool":
            result = await revit.send_command("family_modeling_tool", {
                "operation": arguments.get("operation"),
                "element_id": arguments.get("element_id"),
                "element_ids": arguments.get("element_ids"),
                "is_solid": arguments.get("is_solid", True),
                "profile_points": arguments.get("profile_points"),
                "bottom_profile_points": arguments.get("bottom_profile_points"),
                "top_profile_points": arguments.get("top_profile_points"),
                "extrusion_start": arguments.get("extrusion_start", 0),
                "extrusion_end": arguments.get("extrusion_end", 10),
                "sketch_plane_id": arguments.get("sketch_plane_id"),
                "sketch_plane_name": arguments.get("sketch_plane_name"),
                "plane_origin_x": arguments.get("plane_origin_x", 0),
                "plane_origin_y": arguments.get("plane_origin_y", 0),
                "plane_origin_z": arguments.get("plane_origin_z", 0),
                "plane_normal_x": arguments.get("plane_normal_x", 0),
                "plane_normal_y": arguments.get("plane_normal_y", 0),
                "plane_normal_z": arguments.get("plane_normal_z", 1),
                "axis_start_x": arguments.get("axis_start_x", 0),
                "axis_start_y": arguments.get("axis_start_y", 0),
                "axis_start_z": arguments.get("axis_start_z", 0),
                "axis_end_x": arguments.get("axis_end_x", 0),
                "axis_end_y": arguments.get("axis_end_y", 0),
                "axis_end_z": arguments.get("axis_end_z", 10),
                "start_angle": arguments.get("start_angle", 0),
                "end_angle": arguments.get("end_angle", 360),
                "path_curve_ids": arguments.get("path_curve_ids"),
                "profile_curve_ids": arguments.get("profile_curve_ids"),
                "axis_line_id": arguments.get("axis_line_id"),
                "profile_curve_id": arguments.get("profile_curve_id"),
                "direction_x": arguments.get("direction_x", 0),
                "direction_y": arguments.get("direction_y", 0),
                "direction_z": arguments.get("direction_z", 10),
                "surface_form_id": arguments.get("surface_form_id"),
                "thickness": arguments.get("thickness", 1.0),
                "text": arguments.get("text", "Text"),
                "depth": arguments.get("depth", 1.0),
                "x": arguments.get("x", 0),
                "y": arguments.get("y", 0),
                "z": arguments.get("z", 0),
                "horizontal_align": arguments.get("horizontal_align", "left"),
                "host_element_id": arguments.get("host_element_id"),
                "curve_type": arguments.get("curve_type", "line"),
                "start_x": arguments.get("start_x", 0),
                "start_y": arguments.get("start_y", 0),
                "start_z": arguments.get("start_z", 0),
                "end_x": arguments.get("end_x", 10),
                "end_y": arguments.get("end_y", 0),
                "end_z": arguments.get("end_z", 0),
                "center_x": arguments.get("center_x", 0),
                "center_y": arguments.get("center_y", 0),
                "center_z": arguments.get("center_z", 0),
                "radius": arguments.get("radius", 5),
                "arc_element_id": arguments.get("arc_element_id"),
                "origin_x": arguments.get("origin_x"),
                "origin_y": arguments.get("origin_y"),
                "origin_z": arguments.get("origin_z"),
                "profile_curve_ids_array": arguments.get("profile_curve_ids_array")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        elif name == "connector_tool":
            result = await revit.send_command("connector_tool", {
                "operation": arguments.get("operation"),
                "element_id": arguments.get("element_id"),
                "face_index": arguments.get("face_index", 0),
                "edge_index": arguments.get("edge_index"),
                "system_type": arguments.get("system_type"),
                "profile_type": arguments.get("profile_type", "Round"),
                "connector_id": arguments.get("connector_id"),
                "new_element_id": arguments.get("new_element_id"),
                "new_face_index": arguments.get("new_face_index", 0),
                "new_edge_index": arguments.get("new_edge_index")
            })
            return [TextContent(type="text", text=json.dumps(result, indent=2))]
        
        else:
            return [TextContent(
                type="text",
                text=json.dumps({"error": f"Unknown tool: {name}"})
            )]
    
    except Exception as e:
        logger.error(f"Error executing tool {name}: {e}")
        return [TextContent(
            type="text",
            text=json.dumps({"error": str(e)})
        )]


async def main():
    """Main entry point for the server"""
    try:
        # Attempt to connect to Revit
        await revit.connect()
        
        # Run the server
        async with mcp.server.stdio.stdio_server() as (read_stream, write_stream):
            await app.run(
                read_stream,
                write_stream,
                app.create_initialization_options()
            )
    finally:
        # Ensure we disconnect cleanly
        await revit.disconnect()


if __name__ == "__main__":
    asyncio.run(main())

