"""
Test suite for Revit MCP Server

This test suite validates the Revit MCP Server functionality including:
- 70+ tools covering BIM operations, geometry creation, curve operations,
  transformations, parameters, schedules, UI interactions, and family modeling
- Connection handling
- Tool schemas and input validation
- Error handling without active Revit connection
"""

import pytest
import asyncio
import json
from revit_mcp_server import app, RevitConnection, list_resources, list_tools, call_tool


class TestRevitConnection:
    """Test RevitConnection class"""
    
    @pytest.mark.asyncio
    async def test_connection_initialization(self):
        """Test that connection initializes correctly"""
        conn = RevitConnection()
        assert conn.connected == False
        assert conn.pipe_handle is None
        assert hasattr(conn, 'pipe_name')
        assert hasattr(conn, 'lock')
    
    @pytest.mark.asyncio
    async def test_connect(self):
        """Test connection attempt"""
        conn = RevitConnection()
        # This will fail without Revit running, which is expected
        result = await conn.connect()
        # Result should be boolean
        assert isinstance(result, bool)


class TestMCPServer:
    """Test MCP Server functionality"""
    
    @pytest.mark.asyncio
    async def test_list_resources(self):
        """Test that resources are listed correctly"""
        resources = await list_resources()
        
        assert len(resources) > 0
        assert any(r.name == "Project Information" for r in resources)
        assert any(r.name == "All Elements" for r in resources)
        assert any(r.name == "Active View" for r in resources)
        assert any(r.name == "Loaded Families" for r in resources)
    
    @pytest.mark.asyncio
    async def test_list_tools(self):
        """Test that tools are listed correctly"""
        tools = await list_tools()
        
        # Should have 70+ tools
        assert len(tools) >= 70, f"Expected at least 70 tools, got {len(tools)}"
        
        tool_names = [t.name for t in tools]
        
        # Basic BIM Operations
        assert "get_elements_by_category" in tool_names
        assert "get_element_parameters" in tool_names
        assert "set_parameter_value" in tool_names
        assert "create_view" in tool_names
        assert "export_to_ifc" in tool_names
        assert "query_elements" in tool_names
        assert "get_project_info" in tool_names
        assert "select_view_type" in tool_names
        assert "get_selected_elements" in tool_names
        assert "set_active_view" in tool_names
        
        # Geometry Creation
        assert "create_wall" in tool_names
        assert "create_grid_line" in tool_names
        assert "create_grid_arc" in tool_names
        assert "create_bounded_line" in tool_names
        assert "create_detail_line" in tool_names
        
        # Curve Operations
        assert "create_curves_from_points" in tool_names
        assert "create_hermite_spline" in tool_names
        assert "create_offset_curve" in tool_names
        assert "evaluate_curve" in tool_names
        assert "curve_intersect" in tool_names
        assert "curve_distance_to_point" in tool_names
        
        # Point Operations
        assert "create_point" in tool_names
        assert "create_point_on_element" in tool_names
        assert "create_point_markup" in tool_names
        
        # Shape Creation
        assert "create_detail_shapes" in tool_names
        assert "create_model_shapes" in tool_names
        assert "create_symbolic_shapes" in tool_names
        
        # Transformations
        assert "rotate_elements" in tool_names
        assert "transform_elements" in tool_names
        assert "modify_element" in tool_names
        
        # Parameters (Family & Project)
        assert "add_family_shared_parameter" in tool_names
        assert "remove_family_parameter" in tool_names
        assert "get_family_parameters" in tool_names
        assert "add_project_shared_parameter" in tool_names
        assert "remove_project_shared_parameter" in tool_names
        assert "get_project_shared_parameters" in tool_names
        assert "detect_document_type" in tool_names
        
        # Reference Planes
        assert "create_reference_plane" in tool_names
        assert "get_reference_planes" in tool_names
        
        # Graphics & Visualization
        assert "set_graphic_overrides" in tool_names
        
        # Schedules
        assert "create_schedule_view" in tool_names
        assert "get_table_data" in tool_names
        assert "modify_schedule" in tool_names
        
        # Element Queries
        assert "get_family_types" in tool_names
        assert "get_instances" in tool_names
        assert "find_family_type" in tool_names
        assert "find_elements" in tool_names
        assert "get_last_placed_element" in tool_names
        
        # UI Tools
        assert "selection_tool" in tool_names
        assert "task_dialog" in tool_names
        assert "ribbon_tool" in tool_names
        
        # Advanced Family Modeling
        assert "family_points_tool" in tool_names
        assert "revolve_tool" in tool_names
        assert "dimension_tool" in tool_names
        assert "family_instance_tool" in tool_names
        assert "family_modeling_tool" in tool_names
        assert "connector_tool" in tool_names
    
    @pytest.mark.asyncio
    async def test_tool_schemas(self):
        """Test that tools have proper input schemas"""
        tools = await list_tools()
        
        # Verify we have the expected number of tools (70+)
        assert len(tools) >= 70, f"Expected at least 70 tools, got {len(tools)}"
        
        for tool in tools:
            # Every tool must have a name
            assert tool.name is not None and len(tool.name) > 0, f"Tool missing name"
            
            # Every tool must have a description
            assert tool.description is not None and len(tool.description) > 0, f"Tool {tool.name} missing description"
            
            # Every tool must have an input schema
            assert tool.inputSchema is not None, f"Tool {tool.name} missing inputSchema"
            assert "type" in tool.inputSchema, f"Tool {tool.name} inputSchema missing 'type'"
            assert tool.inputSchema["type"] == "object", f"Tool {tool.name} inputSchema type should be 'object'"
            assert "properties" in tool.inputSchema, f"Tool {tool.name} inputSchema missing 'properties'"


class TestToolExecution:
    """Test tool execution (without Revit connection)"""
    
    # Basic BIM Operations Tests
    @pytest.mark.asyncio
    async def test_get_elements_by_category(self):
        """Test get_elements_by_category without connection"""
        result = await call_tool("get_elements_by_category", {"category": "Walls"})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "count" in response

    @pytest.mark.asyncio
    async def test_get_element_parameters(self):
        """Test get_element_parameters without connection"""
        result = await call_tool("get_element_parameters", {"element_id": "12345"})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "parameters" in response

    @pytest.mark.asyncio
    async def test_set_parameter_value(self):
        """Test set_parameter_value without connection"""
        result = await call_tool("set_parameter_value", {
            "element_id": "12345",
            "parameter_name": "Comments",
            "value": "Test"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    @pytest.mark.asyncio
    async def test_get_project_info(self):
        """Test get_project_info without connection"""
        result = await call_tool("get_project_info", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "project_name" in response

    @pytest.mark.asyncio
    async def test_query_elements(self):
        """Test query_elements without connection"""
        result = await call_tool("query_elements", {
            "filter_type": "category",
            "criteria": {"category": "Walls"}
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "elements" in response

    # Geometry Creation Tests
    @pytest.mark.asyncio
    async def test_create_wall(self):
        """Test create_wall without connection"""
        result = await call_tool("create_wall", {
            "start_x": 0,
            "start_y": 0,
            "end_x": 20,
            "end_y": 0,
            "level": "Level 1"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    @pytest.mark.asyncio
    async def test_create_grid_line(self):
        """Test create_grid_line without connection"""
        result = await call_tool("create_grid_line", {
            "start_x": 0,
            "start_y": 0,
            "end_x": 100,
            "end_y": 0
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_create_bounded_line(self):
        """Test create_bounded_line without connection"""
        result = await call_tool("create_bounded_line", {
            "start_x": 0,
            "start_y": 0,
            "end_x": 10,
            "end_y": 10
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    # Curve Operations Tests
    @pytest.mark.asyncio
    async def test_create_hermite_spline(self):
        """Test create_hermite_spline without connection"""
        result = await call_tool("create_hermite_spline", {
            "points": [
                {"x": 0, "y": 0, "z": 0},
                {"x": 10, "y": 10, "z": 0},
                {"x": 20, "y": 0, "z": 0}
            ],
            "closed": False
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_curve_intersect(self):
        """Test curve_intersect without connection"""
        result = await call_tool("curve_intersect", {
            "curve_element_id_1": 12345,
            "curve_element_id_2": 67890
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "intersections" in response

    # Point and Shape Operations Tests
    @pytest.mark.asyncio
    async def test_create_point(self):
        """Test create_point without connection"""
        result = await call_tool("create_point", {"x": 10, "y": 20, "z": 0})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_create_detail_shapes(self):
        """Test create_detail_shapes without connection"""
        result = await call_tool("create_detail_shapes", {
            "shape_type": "circle",
            "center_x": 0,
            "center_y": 0,
            "radius": 5
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_ids" in response

    @pytest.mark.asyncio
    async def test_create_model_shapes(self):
        """Test create_model_shapes without connection"""
        result = await call_tool("create_model_shapes", {
            "shape_type": "rectangle",
            "center_x": 0,
            "center_y": 0,
            "width": 10,
            "height": 5
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_ids" in response

    # Transformation Tests
    @pytest.mark.asyncio
    async def test_rotate_elements(self):
        """Test rotate_elements without connection"""
        result = await call_tool("rotate_elements", {
            "element_id": 12345,
            "angle": 45
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    @pytest.mark.asyncio
    async def test_transform_elements(self):
        """Test transform_elements without connection"""
        result = await call_tool("transform_elements", {
            "element_id": 12345,
            "operation": "move",
            "translation": {"x": 10, "y": 0, "z": 0}
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    @pytest.mark.asyncio
    async def test_modify_element(self):
        """Test modify_element without connection"""
        result = await call_tool("modify_element", {
            "element_id": 12345,
            "parameters": {"Mark": "A1", "Comments": "Updated"}
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    # Parameter Tests
    @pytest.mark.asyncio
    async def test_get_family_parameters(self):
        """Test get_family_parameters without connection"""
        result = await call_tool("get_family_parameters", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "parameters" in response

    @pytest.mark.asyncio
    async def test_get_project_shared_parameters(self):
        """Test get_project_shared_parameters without connection"""
        result = await call_tool("get_project_shared_parameters", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "parameters" in response

    @pytest.mark.asyncio
    async def test_detect_document_type(self):
        """Test detect_document_type without connection"""
        result = await call_tool("detect_document_type", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "document_type" in response

    # Reference Plane Tests
    @pytest.mark.asyncio
    async def test_create_reference_plane(self):
        """Test create_reference_plane without connection"""
        result = await call_tool("create_reference_plane", {
            "bubble_x": 0,
            "bubble_y": 0,
            "free_x": 100,
            "free_y": 0
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_get_reference_planes(self):
        """Test get_reference_planes without connection"""
        result = await call_tool("get_reference_planes", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "reference_planes" in response

    # Graphics and View Tests
    @pytest.mark.asyncio
    async def test_set_graphic_overrides(self):
        """Test set_graphic_overrides without connection"""
        result = await call_tool("set_graphic_overrides", {
            "category": "Walls",
            "halftone": True
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "success" in response

    @pytest.mark.asyncio
    async def test_create_view(self):
        """Test create_view without connection"""
        result = await call_tool("create_view", {
            "view_type": "FloorPlan",
            "name": "Test Floor Plan",
            "level": "Level 1"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "view_id" in response

    # Schedule Tests
    @pytest.mark.asyncio
    async def test_create_schedule_view(self):
        """Test create_schedule_view without connection"""
        result = await call_tool("create_schedule_view", {
            "category": "Walls",
            "name": "Wall Schedule"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "schedule_id" in response

    @pytest.mark.asyncio
    async def test_get_table_data(self):
        """Test get_table_data without connection"""
        result = await call_tool("get_table_data", {
            "schedule_name": "Wall Schedule"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "data" in response

    # Element Query Tests
    @pytest.mark.asyncio
    async def test_get_family_types(self):
        """Test get_family_types without connection"""
        result = await call_tool("get_family_types", {
            "category": "Doors"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "family_types" in response

    @pytest.mark.asyncio
    async def test_get_instances(self):
        """Test get_instances without connection"""
        result = await call_tool("get_instances", {
            "category": "Windows"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "instances" in response

    @pytest.mark.asyncio
    async def test_find_elements(self):
        """Test find_elements without connection"""
        result = await call_tool("find_elements", {
            "category": "Walls",
            "parameter_filter": {
                "name": "Mark",
                "value": "A1",
                "operator": "equals"
            }
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "elements" in response

    @pytest.mark.asyncio
    async def test_get_last_placed_element(self):
        """Test get_last_placed_element without connection"""
        result = await call_tool("get_last_placed_element", {})
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    # UI Tools Tests
    @pytest.mark.asyncio
    async def test_selection_tool(self):
        """Test selection_tool without connection"""
        result = await call_tool("selection_tool", {
            "operation": "get_selection"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "selection" in response

    @pytest.mark.asyncio
    async def test_task_dialog(self):
        """Test task_dialog without connection"""
        result = await call_tool("task_dialog", {
            "mode": "simple",
            "title": "Test",
            "message": "Test message"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "result" in response

    # Advanced Family Modeling Tests
    @pytest.mark.asyncio
    async def test_family_points_tool(self):
        """Test family_points_tool without connection"""
        result = await call_tool("family_points_tool", {
            "operation": "create_single_point",
            "x": 10,
            "y": 20,
            "z": 0
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_dimension_tool(self):
        """Test dimension_tool without connection"""
        result = await call_tool("dimension_tool", {
            "operation": "create_linear_dimension",
            "start_x": 0,
            "start_y": 0,
            "end_x": 10,
            "end_y": 0
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "dimension_id" in response

    @pytest.mark.asyncio
    async def test_family_modeling_tool(self):
        """Test family_modeling_tool without connection"""
        result = await call_tool("family_modeling_tool", {
            "operation": "new_extrusion",
            "profile_points": [
                {"x": 0, "y": 0, "z": 0},
                {"x": 10, "y": 0, "z": 0},
                {"x": 10, "y": 10, "z": 0},
                {"x": 0, "y": 10, "z": 0}
            ],
            "extrusion_start": 0,
            "extrusion_end": 10
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "element_id" in response

    @pytest.mark.asyncio
    async def test_connector_tool(self):
        """Test connector_tool without connection"""
        result = await call_tool("connector_tool", {
            "operation": "get_connectors"
        })
        assert len(result) > 0
        response = json.loads(result[0].text)
        assert "error" in response or "connectors" in response


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
