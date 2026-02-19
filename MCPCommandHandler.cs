using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace RevitMCPAddin
{
    /// <summary>
    /// Handles execution of MCP commands within Revit's valid context
    /// Implements IExternalEventHandler to run commands on Revit's main thread
    /// </summary>
    public class MCPCommandHandler : IExternalEventHandler
    {
        private MCPRequest _currentRequest;
        private object _result;
        private ManualResetEvent _resultEvent = new ManualResetEvent(false);

        /// <summary>
        /// Execute the queued command
        /// </summary>
        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument?.Document;
            
            if (doc == null)
            {
                SetResult(new { success = false, error = "No active document" });
                return;
            }

            try
            {
                object result = null;

                switch (_currentRequest.Command)
                {
                    case "detect_document_type":
                        result = DetectDocumentType(doc);
                        break;

                    case "get_elements_by_category":
                        result = GetElementsByCategory(doc, _currentRequest.Parameters);
                        break;

                    case "get_element_parameters":
                        result = GetElementParameters(doc, _currentRequest.Parameters);
                        break;

                    case "set_parameter_value":
                        result = SetParameterValue(doc, _currentRequest.Parameters);
                        break;

                    case "create_view":
                        result = CreateView(doc, _currentRequest.Parameters);
                        break;

                    case "export_to_ifc":
                        result = ExportToIFC(doc, _currentRequest.Parameters);
                        break;

                    case "export_image":
                        result = ExportImage(doc, _currentRequest.Parameters);
                        break;

                    case "query_elements":
                        result = QueryElements(doc, _currentRequest.Parameters);
                        break;

                    case "get_project_info":
                        result = GetProjectInfo(doc);
                        break;

                    case "select_view_type":
                        result = SelectViewType(doc, _currentRequest.Parameters);
                        break;

                    case "set_active_view":
                        result = SetActiveView(app, _currentRequest.Parameters);
                        break;

                    case "create_wall":
                        result = CreateWall(doc, _currentRequest.Parameters);
                        break;

                    case "get_selected_elements":
                        result = GetSelectedElements(app, _currentRequest.Parameters);
                        break;

                    case "create_grid_line":
                        result = CreateGridLine(doc, _currentRequest.Parameters);
                        break;

                    case "create_grid_arc":
                        result = CreateGridArc(doc, _currentRequest.Parameters);
                        break;

                    case "create_bounded_line":
                        result = CreateBoundedLine(doc, _currentRequest.Parameters);
                        break;

                    case "create_detail_line":
                        result = CreateDetailLine(doc, _currentRequest.Parameters);
                        break;

                    case "create_curves_from_points":
                        result = CreateCurvesFromPoints(doc, _currentRequest.Parameters);
                        break;

                    case "create_hermite_spline":
                        result = CreateHermiteSpline(doc, _currentRequest.Parameters);
                        break;

                    case "create_hermite_spline_with_tangents":
                        result = CreateHermiteSplineWithTangents(doc, _currentRequest.Parameters);
                        break;

                    case "create_offset_curve":
                        result = CreateOffsetCurve(doc, _currentRequest.Parameters);
                        break;

                    case "evaluate_curve":
                        result = EvaluateCurve(doc, _currentRequest.Parameters);
                        break;

                    case "curve_distance_to_point":
                        result = CurveDistanceToPoint(doc, _currentRequest.Parameters);
                        break;

                    case "curve_get_end_point":
                        result = CurveGetEndPoint(doc, _currentRequest.Parameters);
                        break;

                    case "curve_get_end_parameter":
                        result = CurveGetEndParameter(doc, _currentRequest.Parameters);
                        break;

                    case "curve_get_end_point_reference":
                        result = CurveGetEndPointReference(doc, _currentRequest.Parameters);
                        break;

                    case "create_clone_curve":
                        result = CreateCloneCurve(doc, _currentRequest.Parameters);
                        break;

                    case "curve_compute_closest_points":
                        result = CurveComputeClosestPoints(doc, _currentRequest.Parameters);
                        break;

                    case "curve_compute_derivatives":
                        result = CurveComputeDerivatives(doc, _currentRequest.Parameters);
                        break;

                    case "curve_compute_normalized_parameter":
                        result = CurveComputeNormalizedParameter(doc, _currentRequest.Parameters);
                        break;

                    case "curve_compute_raw_parameter":
                        result = CurveComputeRawParameter(doc, _currentRequest.Parameters);
                        break;

                    case "curve_create_reversed":
                        result = CurveCreateReversed(doc, _currentRequest.Parameters);
                        break;

                    case "curve_create_transformed":
                        result = CurveCreateTransformed(doc, _currentRequest.Parameters);
                        break;

                    case "curve_intersect":
                        result = CurveIntersect(doc, _currentRequest.Parameters);
                        break;

                    case "create_point":
                        result = CreatePoint(doc, _currentRequest.Parameters);
                        break;

                    case "create_point_on_element":
                        result = CreatePointOnElement(doc, _currentRequest.Parameters);
                        break;

                    case "curve_point_location_on_curve":
                        result = CurvePointLocationOnCurve(doc, _currentRequest.Parameters);
                        break;

                    case "calculate_line_direction":
                        result = CalculateLineDirection(_currentRequest.Parameters);
                        break;

                    case "create_point_markup":
                        result = CreatePointMarkup(doc, _currentRequest.Parameters);
                        break;

                    case "create_detail_shapes":
                        result = CreateDetailShapes(doc, _currentRequest.Parameters);
                        break;

                    case "create_model_shapes":
                        result = CreateModelShapes(doc, _currentRequest.Parameters);
                        break;

                    case "create_symbolic_shapes":
                        result = CreateSymbolicShapes(doc, _currentRequest.Parameters);
                        break;

                    case "rotate_elements":
                        result = RotateElements(doc, _currentRequest.Parameters);
                        break;

                    case "add_family_shared_parameter":
                        result = AddFamilySharedParameter(doc, _currentRequest.Parameters);
                        break;

                    case "remove_family_parameter":
                        result = RemoveFamilyParameter(doc, _currentRequest.Parameters);
                        break;

                    case "get_family_parameters":
                        result = GetFamilyParameters(doc);
                        break;

                    case "add_project_shared_parameter":
                        result = AddProjectSharedParameter(doc, _currentRequest.Parameters);
                        break;

                    case "remove_project_shared_parameter":
                        result = RemoveProjectSharedParameter(doc, _currentRequest.Parameters);
                        break;

                    case "get_project_shared_parameters":
                        result = GetProjectSharedParameters(doc);
                        break;

                    case "get_last_placed_element":
                        result = GetLastPlacedElement(doc, _currentRequest.Parameters);
                        break;

                    case "create_reference_plane":
                        result = CreateReferencePlane(doc, app, _currentRequest.Parameters);
                        break;

                    case "get_reference_planes":
                        result = GetReferencePlanes(doc, _currentRequest.Parameters);
                        break;

                    case "set_graphic_overrides":
                        result = SetGraphicOverrides(doc, _currentRequest.Parameters);
                        break;

                    case "create_schedule_view":
                        result = CreateScheduleView(doc, _currentRequest.Parameters);
                        break;

                    case "get_table_data":
                        result = GetTableData(doc, _currentRequest.Parameters);
                        break;

                    case "modify_schedule":
                        result = ModifySchedule(doc, _currentRequest.Parameters);
                        break;

                    case "modify_element":
                        result = ModifyElement(doc, _currentRequest.Parameters);
                        break;

                    case "transform_elements":
                        result = TransformElements(doc, _currentRequest.Parameters);
                        break;

                    case "get_family_types":
                        result = GetFamilyTypes(doc, _currentRequest.Parameters);
                        break;

                    case "get_instances":
                        result = GetInstances(doc, _currentRequest.Parameters);
                        break;

                    case "find_family_type":
                        result = FindFamilyType(doc, _currentRequest.Parameters);
                        break;

                    case "find_elements":
                        result = FindElements(doc, _currentRequest.Parameters);
                        break;

                    case "selection_tool":
                        result = SelectionTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "task_dialog":
                        result = TaskDialogTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "ribbon_tool":
                        result = RibbonTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "family_points_tool":
                        result = FamilyPointsTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "revolve_tool":
                        result = RevolveTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "cap_tool":
                        result = CapTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "extrusion_tool":
                        result = ExtrusionTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "plane_tool":
                        result = PlaneTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "model_curve_tool":
                        result = ModelCurveTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "divided_surface_tool":
                        result = DividedSurfaceTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "dimension_tool":
                        result = DimensionTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "family_instance_tool":
                        result = FamilyInstanceTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "family_modeling_tool":
                        result = FamilyModelingTool(app, doc, _currentRequest.Parameters);
                        break;

                    case "load_and_place_family":
                        result = LoadAndPlaceFamily(app, doc, _currentRequest.Parameters);
                        break;

                    case "connector_tool":
                        result = ConnectorTool(app, doc, _currentRequest.Parameters);
                        break;

                    default:
                        result = new { success = false, error = $"Unknown command: {_currentRequest.Command}" };
                        break;
                }

                SetResult(result);
            }
            catch (Exception ex)
            {
                SetResult(new { success = false, error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        public string GetName()
        {
            return "MCP Command Handler";
        }

        /// <summary>
        /// Set the command to execute
        /// </summary>
        public void SetCommand(MCPRequest request)
        {
            _currentRequest = request;
            _resultEvent.Reset();
        }

        /// <summary>
        /// Wait for command result
        /// </summary>
        public object WaitForResult(int timeoutMs)
        {
            if (_resultEvent.WaitOne(timeoutMs))
            {
                return _result;
            }
            else
            {
                // Timeout occurred
                return new { success = false, error = "Command execution timed out" };
            }
        }

        /// <summary>
        /// Set the result of command execution
        /// </summary>
        private void SetResult(object result)
        {
            _result = result;
            _resultEvent.Set();
        }

        #region Command Implementations

        /// <summary>
        /// Get all elements of a specific category
        /// </summary>
        private object GetElementsByCategory(Document doc, Dictionary<string, object> parameters)
        {
            string categoryName = parameters["category"].ToString();
            
            // Get built-in category
            BuiltInCategory builtInCategory;
            if (!Enum.TryParse($"OST_{categoryName}", out builtInCategory))
            {
                return new { success = false, error = $"Unknown category: {categoryName}" };
            }

            // Collect elements
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var elements = collector
                .OfCategory(builtInCategory)
                .WhereElementIsNotElementType()
                .ToList();

            // Build result (use helpers to avoid API differences)
            var elementList = elements.Select(e => new
            {
                id = GetElementIdInt(e.Id),
                name = GetElementName(e),
                category = e.Category?.Name,
                level = GetElementName(doc.GetElement(e.LevelId))
            }).ToList();

            return new
            {
                success = true,
                category = categoryName,
                count = elementList.Count,
                elements = elementList
            };
        }

        /// <summary>
        /// Get all parameters for an element
        /// </summary>
        private object GetElementParameters(Document doc, Dictionary<string, object> parameters)
        {
            int elementId = Convert.ToInt32(parameters["element_id"]);
            Element element = doc.GetElement(new ElementId(elementId));

            if (element == null)
            {
                return new { success = false, error = "Element not found" };
            }

            // Get all parameters
            var paramList = new Dictionary<string, object>();
            foreach (Parameter param in element.Parameters)
            {
                string value = param.HasValue ? param.AsValueString() ?? param.AsString() : "N/A";
                paramList[param.Definition.Name] = new
                {
                    value = value,
                    storageType = param.StorageType.ToString(),
                    isReadOnly = param.IsReadOnly
                };
            }

            return new
            {
                success = true,
                elementId = elementId,
                elementName = element.Name,
                category = element.Category?.Name,
                parameters = paramList
            };
        }

        /// <summary>
        /// Set a parameter value for an element
        /// </summary>
        private object SetParameterValue(Document doc, Dictionary<string, object> parameters)
        {
            int elementId = Convert.ToInt32(parameters["element_id"]);
            string paramName = parameters["parameter_name"].ToString();
            string value = parameters["value"].ToString();

            Element element = doc.GetElement(new ElementId(elementId));
            if (element == null)
            {
                return new { success = false, error = "Element not found" };
            }

            Parameter param = element.LookupParameter(paramName);
            if (param == null)
            {
                return new { success = false, error = $"Parameter '{paramName}' not found" };
            }

            if (param.IsReadOnly)
            {
                return new { success = false, error = $"Parameter '{paramName}' is read-only" };
            }

            // Start transaction
            using (Transaction trans = new Transaction(doc, "Set Parameter"))
            {
                trans.Start();

                try
                {
                    // Set value based on storage type
                    switch (param.StorageType)
                    {
                        case StorageType.String:
                            param.Set(value);
                            break;
                        case StorageType.Integer:
                            param.Set(int.Parse(value));
                            break;
                        case StorageType.Double:
                            param.Set(double.Parse(value));
                            break;
                        default:
                            param.Set(value);
                            break;
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        elementId = elementId,
                        parameter = paramName,
                        newValue = value
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to set parameter: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a new view
        /// </summary>
        private object CreateView(Document doc, Dictionary<string, object> parameters)
        {
            string viewType = parameters["view_type"].ToString();
            string viewName = parameters["name"].ToString();

            using (Transaction trans = new Transaction(doc, "Create View"))
            {
                trans.Start();

                try
                {
                    View newView = null;

                    switch (viewType)
                    {
                        case "3D":
                            ViewFamilyType vft3D = new FilteredElementCollector(doc)
                                .OfClass(typeof(ViewFamilyType))
                                .Cast<ViewFamilyType>()
                                .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);
                            
                            if (vft3D != null)
                            {
                                newView = View3D.CreateIsometric(doc, vft3D.Id);
                                newView.Name = viewName;
                            }
                            break;

                        case "FloorPlan":
                            string levelName = parameters.ContainsKey("level") ? parameters["level"].ToString() : "Level 1";
                            Level level = new FilteredElementCollector(doc)
                                .OfClass(typeof(Level))
                                .Cast<Level>()
                                .FirstOrDefault(l => l.Name == levelName);

                            if (level != null)
                            {
                                ViewFamilyType vftPlan = new FilteredElementCollector(doc)
                                    .OfClass(typeof(ViewFamilyType))
                                    .Cast<ViewFamilyType>()
                                    .FirstOrDefault(x => x.ViewFamily == ViewFamily.FloorPlan);

                                if (vftPlan != null)
                                {
                                    newView = ViewPlan.Create(doc, vftPlan.Id, level.Id);
                                    newView.Name = viewName;
                                }
                            }
                            break;
                    }

                    if (newView != null)
                    {
                        trans.Commit();
                        return new
                        {
                            success = true,
                            viewId = GetElementIdInt(newView.Id),
                            viewName = GetElementName(newView),
                            viewType = viewType
                        };
                    }
                    else
                    {
                        trans.RollBack();
                        return new { success = false, error = "Failed to create view" };
                    }
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = ex.Message };
                }
            }
        }

        /// <summary>
        /// Export to IFC format
        /// </summary>
        private object ExportToIFC(Document doc, Dictionary<string, object> parameters)
        {
            // IFC export implementation
            return new { success = true, message = "IFC export not yet implemented" };
        }

        /// <summary>
        /// Export images from views using ImageExportOptions
        /// </summary>
        private object ExportImage(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get output directory path
                string outputPath = parameters.ContainsKey("output_path") ? parameters["output_path"]?.ToString() : "";
                if (string.IsNullOrEmpty(outputPath))
                {
                    return new { success = false, error = "output_path is required" };
                }

                // Verify directory exists or create it
                string directory = Path.GetDirectoryName(outputPath) ?? "";
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        return new { success = false, error = $"Failed to create directory: {ex.Message}" };
                    }
                }

                // Create ImageExportOptions
                ImageExportOptions options = new ImageExportOptions();

                // Set file type (default: PNG)
                string fileType = parameters.ContainsKey("file_type") ? parameters["file_type"]?.ToString() : "PNG";
                fileType = fileType ?? "PNG";
                
                switch (fileType.ToUpper())
                {
                    case "BMP":
                        options.FilePath = outputPath.EndsWith(".bmp") ? outputPath.Substring(0, outputPath.Length - 4) : outputPath;
                        options.ImageResolution = ImageResolution.DPI_72;
                        options.HLRandWFViewsFileType = ImageFileType.BMP;
                        options.ShadowViewsFileType = ImageFileType.BMP;
                        break;
                    case "JPG":
                    case "JPEG":
                    case "JPEGLOSSLESS":
                        options.FilePath = outputPath.EndsWith(".jpg") || outputPath.EndsWith(".jpeg") ? 
                            outputPath.Substring(0, outputPath.LastIndexOf('.')) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.JPEGLossless;
                        options.ShadowViewsFileType = ImageFileType.JPEGLossless;
                        break;
                    case "JPEGMEDIUM":
                        options.FilePath = outputPath.EndsWith(".jpg") || outputPath.EndsWith(".jpeg") ? 
                            outputPath.Substring(0, outputPath.LastIndexOf('.')) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.JPEGMedium;
                        options.ShadowViewsFileType = ImageFileType.JPEGMedium;
                        break;
                    case "JPEGSMALLEST":
                        options.FilePath = outputPath.EndsWith(".jpg") || outputPath.EndsWith(".jpeg") ? 
                            outputPath.Substring(0, outputPath.LastIndexOf('.')) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.JPEGSmallest;
                        options.ShadowViewsFileType = ImageFileType.JPEGSmallest;
                        break;
                    case "PNG":
                        options.FilePath = outputPath.EndsWith(".png") ? outputPath.Substring(0, outputPath.Length - 4) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.PNG;
                        options.ShadowViewsFileType = ImageFileType.PNG;
                        break;
                    case "TARGA":
                    case "TGA":
                        options.FilePath = outputPath.EndsWith(".tga") ? outputPath.Substring(0, outputPath.Length - 4) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.TARGA;
                        options.ShadowViewsFileType = ImageFileType.TARGA;
                        break;
                    case "TIFF":
                    case "TIF":
                        options.FilePath = outputPath.EndsWith(".tif") || outputPath.EndsWith(".tiff") ? 
                            outputPath.Substring(0, outputPath.LastIndexOf('.')) : outputPath;
                        options.HLRandWFViewsFileType = ImageFileType.TIFF;
                        options.ShadowViewsFileType = ImageFileType.TIFF;
                        break;
                    default:
                        return new { success = false, error = $"Unsupported file type: {fileType}. Supported: BMP, JPEGLossless, JPEGMedium, JPEGSmallest, PNG, TARGA, TIFF" };
                }

                // Set image resolution (DPI)
                int dpi = parameters.ContainsKey("dpi") ? Convert.ToInt32(parameters["dpi"]) : 150;
                switch (dpi)
                {
                    case 72:
                        options.ImageResolution = ImageResolution.DPI_72;
                        break;
                    case 150:
                        options.ImageResolution = ImageResolution.DPI_150;
                        break;
                    case 300:
                        options.ImageResolution = ImageResolution.DPI_300;
                        break;
                    case 600:
                        options.ImageResolution = ImageResolution.DPI_600;
                        break;
                    default:
                        return new { success = false, error = $"Unsupported DPI: {dpi}. Supported: 72, 150, 300, 600" };
                }

                // Set zoom type - note: pixel size control requires different approach in Revit API
                // For custom sizes, users should adjust zoom and resolution instead
                {
                    // Set zoom type
                    string zoomType = parameters.ContainsKey("zoom_type") ? parameters["zoom_type"]?.ToString() : "FitToPage";
                    zoomType = zoomType ?? "FitToPage";
                    
                    if (zoomType == "FitToPage")
                    {
                        options.ZoomType = ZoomFitType.FitToPage;
                    }
                    else if (zoomType == "Zoom")
                    {
                        options.ZoomType = ZoomFitType.Zoom;
                        int zoom = parameters.ContainsKey("zoom") ? Convert.ToInt32(parameters["zoom"]) : 100;
                        if (zoom < 1 || zoom > 400)
                        {
                            return new { success = false, error = "Zoom must be between 1 and 400" };
                        }
                        options.Zoom = zoom;
                    }
                    else
                    {
                        return new { success = false, error = $"Invalid zoom_type: {zoomType}. Use 'FitToPage' or 'Zoom'" };
                    }
                }

                // Set fit direction
                string fitDirection = parameters.ContainsKey("fit_direction") ? parameters["fit_direction"]?.ToString() : "Horizontal";
                fitDirection = fitDirection ?? "Horizontal";
                
                if (fitDirection == "Horizontal")
                {
                    options.FitDirection = FitDirectionType.Horizontal;
                }
                else if (fitDirection == "Vertical")
                {
                    options.FitDirection = FitDirectionType.Vertical;
                }
                else
                {
                    return new { success = false, error = $"Invalid fit_direction: {fitDirection}. Use 'Horizontal' or 'Vertical'" };
                }

                // Set export range
                string exportRange = parameters.ContainsKey("export_range") ? parameters["export_range"]?.ToString() : "CurrentView";
                exportRange = exportRange ?? "CurrentView";
                
                if (exportRange == "CurrentView")
                {
                    options.ExportRange = ExportRange.SetOfViews;
                    // Only export the active view
                    var activeViewId = doc.ActiveView.Id;
                    options.SetViewsAndSheets(new List<ElementId> { activeViewId });
                }
                else if (exportRange == "VisibleViews")
                {
                    options.ExportRange = ExportRange.VisibleRegionOfCurrentView;
                }
                else if (exportRange == "SpecificViews")
                {
                    // Get view IDs from parameters
                    var viewIds = GetElementIdListFromParam(parameters, "view_ids");
                    if (viewIds == null || viewIds.Count == 0)
                    {
                        return new { success = false, error = "view_ids required when export_range is 'SpecificViews'" };
                    }
                    
                    options.ExportRange = ExportRange.SetOfViews;
                    List<ElementId> elementIds = viewIds.Select(id => new ElementId(id)).ToList();
                    options.SetViewsAndSheets(elementIds);
                }
                else
                {
                    return new { success = false, error = $"Invalid export_range: {exportRange}. Use 'CurrentView', 'VisibleViews', or 'SpecificViews'" };
                }

                // Should create website (default: false)
                bool createWebsite = parameters.ContainsKey("create_website") ? Convert.ToBoolean(parameters["create_website"]) : false;
                options.ShouldCreateWebSite = createWebsite;

                // Perform the export
                doc.ExportImage(options);

                return new
                {
                    success = true,
                    message = "Image exported successfully",
                    output_path = options.FilePath,
                    file_type = fileType,
                    dpi = dpi,
                    zoom_type = options.ZoomType.ToString(),
                    fit_direction = options.FitDirection.ToString(),
                    export_range = exportRange,
                    create_website = createWebsite
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Export failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Query elements with filters
        /// </summary>
        private object QueryElements(Document doc, Dictionary<string, object> parameters)
        {
            // Advanced query implementation
            return new { success = true, results = new List<object>() };
        }

        /// <summary>
        /// Get project information
        /// </summary>
        private object GetProjectInfo(Document doc)
        {
            ProjectInfo projInfo = doc.ProjectInformation;
            
            return new
            {
                success = true,
                projectName = projInfo.Name,
                projectNumber = projInfo.Number,
                projectAddress = projInfo.Address,
                author = projInfo.Author,
                organizationName = projInfo.OrganizationName,
                buildingName = projInfo.BuildingName,
                clientName = projInfo.ClientName
            };
        }

        /// <summary>
        /// Select and return one view type from all available view types
        /// </summary>
        private object SelectViewType(Document doc, Dictionary<string, object> parameters)
        {
            // Get all view family types
            var viewTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .ToList();

            if (viewTypes.Count == 0)
            {
                return new { success = false, error = "No view types found in the document" };
            }

            // Select one randomly
            Random rand = new Random();
            var selectedViewType = viewTypes[rand.Next(viewTypes.Count)];

            return new
            {
                success = true,
                message = $"Selected view type: {selectedViewType.Name}",
                viewTypeId = GetElementIdInt(selectedViewType.Id),
                viewTypeName = selectedViewType.Name,
                viewFamily = selectedViewType.ViewFamily.ToString()
            };
        }

        /// <summary>
        /// Set the active view based on the specified view type
        /// </summary>
        private object SetActiveView(UIApplication app, Dictionary<string, object> parameters)
        {
            try
            {
                Document doc = app.ActiveUIDocument.Document;
                
                // Check if we're in a family document
                if (doc.IsFamilyDocument)
                {
                    return SetActiveFamilyView(app, doc, parameters);
                }
                else
                {
                    return SetActiveProjectView(app, doc, parameters);
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SetActiveView error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Set active view in project documents
        /// </summary>
        private object SetActiveProjectView(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("view_type") || parameters["view_type"] == null)
                {
                    return new { success = false, error = "view_type parameter is required for project views" };
                }

                string viewTypeName = parameters["view_type"].ToString();

                // Parse the view type
                ViewType viewType;
                if (!Enum.TryParse(viewTypeName, out viewType))
                {
                    return new { success = false, error = $"Unknown view type: {viewTypeName}" };
                }

                // Find the first view of the specified type that can be shown
                var view = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .FirstOrDefault(v => v.ViewType == viewType && v.CanBePrinted && !v.IsTemplate);

                if (view == null)
                {
                    return new { success = false, error = $"No view found for type: {viewTypeName}" };
                }

                // Request to change to the view
                app.ActiveUIDocument.RequestViewChange(view);

                return new
                {
                    success = true,
                    viewId = GetElementIdInt(view.Id),
                    viewName = view.Name,
                    viewType = viewTypeName,
                    environment = "project"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SetActiveProjectView error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Set active view in family documents
        /// </summary>
        private object SetActiveFamilyView(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : "by_name";

                switch (operation)
                {
                    case "by_name":
                        return SwitchFamilyViewByName(app, doc, parameters);

                    case "by_type":
                        return SwitchFamilyViewByType(app, doc, parameters);

                    case "reference_level":
                        return SwitchToFamilyReferenceLevel(app, doc, parameters);

                    case "list_views":
                        return ListFamilyViews(doc);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown family view operation: {operation}",
                            available_operations = new[] { "by_name", "by_type", "reference_level", "list_views" }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SetActiveFamilyView error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Switch to a family view by name
        /// </summary>
        private object SwitchFamilyViewByName(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("view_name") || parameters["view_name"] == null)
                {
                    return new { success = false, error = "view_name parameter is required" };
                }

                string viewName = parameters["view_name"].ToString();

                // Find view by name
                var view = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .FirstOrDefault(v => v.Name.Equals(viewName, StringComparison.OrdinalIgnoreCase) && !v.IsTemplate);

                if (view == null)
                {
                    return new { success = false, error = $"Family view '{viewName}' not found" };
                }

                // Switch to view
                app.ActiveUIDocument.RequestViewChange(view);

                return new
                {
                    success = true,
                    viewId = GetElementIdInt(view.Id),
                    viewName = view.Name,
                    viewType = view.ViewType.ToString(),
                    environment = "family",
                    operation = "by_name"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SwitchFamilyViewByName error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Switch to a family view by type (FloorPlan, Elevation, Section, 3D, etc.)
        /// </summary>
        private object SwitchFamilyViewByType(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("view_type") || parameters["view_type"] == null)
                {
                    return new { success = false, error = "view_type parameter is required" };
                }

                string viewTypeName = parameters["view_type"].ToString();

                // Parse the view type
                ViewType viewType;
                if (!Enum.TryParse(viewTypeName, out viewType))
                {
                    return new { success = false, error = $"Unknown view type: {viewTypeName}" };
                }

                // Find first view of the specified type
                var view = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .FirstOrDefault(v => v.ViewType == viewType && !v.IsTemplate);

                if (view == null)
                {
                    return new { success = false, error = $"No family view found for type: {viewTypeName}" };
                }

                // Switch to view
                app.ActiveUIDocument.RequestViewChange(view);

                return new
                {
                    success = true,
                    viewId = GetElementIdInt(view.Id),
                    viewName = view.Name,
                    viewType = viewTypeName,
                    environment = "family",
                    operation = "by_type"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SwitchFamilyViewByType error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Switch to the Reference Level view in a family document (standard floor plan at Z=0)
        /// </summary>
        private object SwitchToFamilyReferenceLevel(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Look for "Reference Level" view - the standard family floor plan
                var refLevelView = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .FirstOrDefault(v => v.Name.Equals("Reference Level", StringComparison.OrdinalIgnoreCase) && !v.IsTemplate);

                if (refLevelView == null)
                {
                    // Try to find any FloorPlan view
                    refLevelView = new FilteredElementCollector(doc)
                        .OfClass(typeof(View))
                        .Cast<View>()
                        .FirstOrDefault(v => v.ViewType == ViewType.FloorPlan && !v.IsTemplate);
                }

                if (refLevelView == null)
                {
                    return new { success = false, error = "Reference Level view not found in family document" };
                }

                // Switch to the Reference Level view
                app.ActiveUIDocument.RequestViewChange(refLevelView);

                return new
                {
                    success = true,
                    viewId = GetElementIdInt(refLevelView.Id),
                    viewName = refLevelView.Name,
                    viewType = refLevelView.ViewType.ToString(),
                    environment = "family",
                    operation = "reference_level",
                    message = "Switched to Reference Level (family floor plan view)"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"SwitchToFamilyReferenceLevel error: {ex.Message}" };
            }
        }

        /// <summary>
        /// List all views available in a family document
        /// </summary>
        private object ListFamilyViews(Document doc)
        {
            try
            {
                var views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .Where(v => !v.IsTemplate)
                    .OrderBy(v => v.Name)
                    .ToList();

                var viewList = views.Select(v => new
                {
                    id = GetElementIdInt(v.Id),
                    name = v.Name,
                    type = v.ViewType.ToString(),
                    canBePrinted = v.CanBePrinted,
                    isTemplate = v.IsTemplate
                }).ToList();

                return new
                {
                    success = true,
                    environment = "family",
                    viewCount = viewList.Count,
                    views = viewList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ListFamilyViews error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get all currently selected elements in Revit
        /// </summary>
        private object GetSelectedElements(UIApplication app, Dictionary<string, object> parameters = null)
        {
            try
            {
                UIDocument uidoc = app.ActiveUIDocument;
                if (uidoc == null)
                {
                    return new { success = false, error = "No active UI document" };
                }

                Document doc = uidoc.Document;

                // Determine if we're in a family document and which environment to use
                bool isFamily = doc.IsFamilyDocument;
                string operation = parameters?.ContainsKey("operation") == true 
                    ? parameters["operation"]?.ToString()?.ToLower() 
                    : null;

                // For family documents, check if user explicitly wants family-specific selection
                bool useFamilySelection = isFamily && (operation == "family" || operation == "all" || string.IsNullOrEmpty(operation));

                if (useFamilySelection)
                {
                    return GetSelectedFamilyElements(uidoc, doc);
                }
                else
                {
                    return GetSelectedProjectElements(uidoc, doc);
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get selected elements: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get selected elements from a project document
        /// </summary>
        private object GetSelectedProjectElements(UIDocument uidoc, Document doc)
        {
            try
            {
                Autodesk.Revit.UI.Selection.Selection selection = uidoc.Selection;

                if (selection == null || selection.GetElementIds().Count == 0)
                {
                    return new
                    {
                        success = true,
                        environment = "project",
                        elementCount = 0,
                        elements = new List<object>()
                    };
                }

                // Get all selected element IDs
                var selectedIds = selection.GetElementIds();

                // Build list of selected elements with their information
                var elementList = new List<object>();
                foreach (ElementId elementId in selectedIds)
                {
                    Element element = doc.GetElement(elementId);
                    if (element == null) continue;

                    elementList.Add(new
                    {
                        id = GetElementIdInt(element.Id),
                        name = GetElementName(element),
                        type = element.GetType().Name,
                        category = element.Category?.Name,
                        level = element.LevelId != null && element.LevelId != ElementId.InvalidElementId
                            ? GetElementName(doc.GetElement(element.LevelId))
                            : null,
                        parameters = GetElementParametersDict(element)
                    });
                }

                return new
                {
                    success = true,
                    environment = "project",
                    elementCount = elementList.Count,
                    elements = elementList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get selected project elements: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get selected elements from a family document
        /// Includes reference points, model curves, forms, adaptive points, and other family elements
        /// </summary>
        private object GetSelectedFamilyElements(UIDocument uidoc, Document doc)
        {
            try
            {
                Autodesk.Revit.UI.Selection.Selection selection = uidoc.Selection;

                if (selection == null || selection.GetElementIds().Count == 0)
                {
                    return new
                    {
                        success = true,
                        environment = "family",
                        elementCount = 0,
                        elements = new List<object>()
                    };
                }

                // Get all selected element IDs
                var selectedIds = selection.GetElementIds();

                // Build list of selected family elements with their information
                var elementList = new List<object>();
                foreach (ElementId elementId in selectedIds)
                {
                    Element element = doc.GetElement(elementId);
                    if (element == null) continue;

                    var elementInfo = BuildFamilyElementInfo(element, doc);
                    elementList.Add(elementInfo);
                }

                return new
                {
                    success = true,
                    environment = "family",
                    elementCount = elementList.Count,
                    elements = elementList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get selected family elements: {ex.Message}" };
            }
        }

        /// <summary>
        /// Build detailed information about a family element
        /// </summary>
        private object BuildFamilyElementInfo(Element element, Document doc)
        {
            string elementType = element.GetType().Name;
            var info = new Dictionary<string, object>
            {
                { "id", GetElementIdInt(element.Id) },
                { "name", GetElementName(element) },
                { "type", elementType },
                { "category", element.Category?.Name }
            };

            // Add type-specific information for common family element types
            try
            {
                if (element is ReferencePoint refPt)
                {
                    try
                    {
                        XYZ pt = refPt.Position;
                        info["position"] = new { x = pt.X, y = pt.Y, z = pt.Z };
                    }
                    catch { }
                }
                else if (element is ModelCurve modelCurve)
                {
                    try
                    {
                        Curve curve = modelCurve.GeometryCurve;
                        info["curve_type"] = curve?.GetType().Name;
                        info["curve_length"] = curve?.Length;
                        if (modelCurve.SketchPlane != null)
                        {
                            info["sketch_plane_id"] = GetElementIdInt(modelCurve.SketchPlane.Id);
                            info["sketch_plane_name"] = modelCurve.SketchPlane.Name;
                        }
                    }
                    catch { }
                }
                else if (element is SymbolicCurve symbolicCurve)
                {
                    try
                    {
                        Curve curve = symbolicCurve.GeometryCurve;
                        info["curve_type"] = curve?.GetType().Name;
                        info["curve_length"] = curve?.Length;
                        if (symbolicCurve.SketchPlane != null)
                        {
                            info["sketch_plane_id"] = GetElementIdInt(symbolicCurve.SketchPlane.Id);
                            info["sketch_plane_name"] = symbolicCurve.SketchPlane.Name;
                        }
                    }
                    catch { }
                }
                else if (elementType.Contains("Form") || elementType.Contains("GenericForm"))
                {
                    // Handle form elements generically by type name
                    info["element_subtype"] = "form";
                }
                else if (elementType.Contains("AdaptivePoint") || elementType.Contains("Adaptive"))
                {
                    info["element_subtype"] = "adaptive_point";
                }
                else if (elementType.Contains("CurveByPoints"))
                {
                    // CurveByPoints specific info
                    try
                    {
                        // Try to get IsReferenceLine if available
                        var isRefLineProp = element.GetType().GetProperty("IsReferenceLine");
                        if (isRefLineProp != null)
                        {
                            info["is_reference_line"] = (bool)isRefLineProp.GetValue(element);
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // Add common parameters
            try
            {
                info["parameters"] = GetElementParametersDict(element);
            }
            catch { }

            return info;
        }

        /// <summary>
        /// Helper to get element parameters as a dictionary
        /// </summary>
        private Dictionary<string, object> GetElementParametersDict(Element element)
        {
            var paramDict = new Dictionary<string, object>();
            try
            {
                foreach (Parameter param in element.Parameters)
                {
                    if (param?.Definition == null) continue;
                    string value = param.HasValue ? param.AsValueString() ?? param.AsString() : "N/A";
                    paramDict[param.Definition.Name] = new
                    {
                        value = value,
                        storageType = param.StorageType.ToString(),
                        isReadOnly = param.IsReadOnly
                    };
                }
            }
            catch
            {
                // Silently skip parameters that can't be read
            }
            return paramDict;
        }

        /// <summary>
        /// Create a line grid
        /// </summary>
        private object CreateGridLine(Document doc, Dictionary<string, object> parameters)
        {
            string gridName = parameters.ContainsKey("name") ? parameters["name"]?.ToString() : null;

            if (!HasPoint(parameters, "start") || !HasPoint(parameters, "end"))
            {
                return new { success = false, error = "Line grids require start and end points" };
            }

            using (Transaction trans = new Transaction(doc, "Create Line Grid"))
            {
                trans.Start();

                try
                {
                    XYZ startPoint = GetPoint(parameters, "start");
                    XYZ endPoint = GetPoint(parameters, "end");
                    Line line = Line.CreateBound(startPoint, endPoint);
                    Grid grid = Grid.Create(doc, line);

                    if (!string.IsNullOrWhiteSpace(gridName))
                    {
                        grid.Name = gridName;
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        gridId = GetElementIdInt(grid.Id),
                        gridName = GetElementName(grid),
                        curveType = "line"
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create line grid: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create an arc grid using center, start, and end points
        /// </summary>
        private object CreateGridArc(Document doc, Dictionary<string, object> parameters)
        {
            string gridName = parameters.ContainsKey("name") ? parameters["name"]?.ToString() : null;

            if (!HasPoint(parameters, "start") || !HasPoint(parameters, "end") || !HasPoint(parameters, "center"))
            {
                return new { success = false, error = "Arc grids require center, start, and end points" };
            }

            using (Transaction trans = new Transaction(doc, "Create Arc Grid"))
            {
                trans.Start();

                try
                {
                    XYZ startPoint = GetPoint(parameters, "start");
                    XYZ endPoint = GetPoint(parameters, "end");
                    XYZ centerPoint = GetPoint(parameters, "center");

                    Arc arc = CreateArcFromCenterStartEnd(centerPoint, startPoint, endPoint);
                    if (arc == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Failed to compute arc from center/start/end" };
                    }

                    Grid grid = Grid.Create(doc, arc);
                    if (!string.IsNullOrWhiteSpace(gridName))
                    {
                        grid.Name = gridName;
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        gridId = GetElementIdInt(grid.Id),
                        gridName = GetElementName(grid),
                        curveType = "arc"
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create arc grid: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Build an arc from center, start, and end points
        /// </summary>
        private Arc CreateArcFromCenterStartEnd(XYZ center, XYZ start, XYZ end)
        {
            XYZ vStart = start - center;
            XYZ vEnd = end - center;

            double radius = vStart.GetLength();
            if (radius <= 1e-9)
            {
                return null;
            }

            XYZ normal = vStart.CrossProduct(vEnd);
            if (normal.GetLength() <= 1e-9)
            {
                return null;
            }

            XYZ xAxis = vStart.Normalize();
            XYZ yAxis = normal.Normalize().CrossProduct(xAxis).Normalize();

            double endAngle = Math.Atan2(vEnd.DotProduct(yAxis), vEnd.DotProduct(xAxis));
            if (endAngle <= 0)
            {
                endAngle += Math.PI * 2.0;
            }

            return Arc.Create(center, radius, 0.0, endAngle, xAxis, yAxis);
        }

        /// <summary>
        /// Check for point components in parameters
        /// </summary>
        private bool HasPoint(Dictionary<string, object> parameters, string prefix)
        {
            return parameters.ContainsKey($"{prefix}_x") && parameters.ContainsKey($"{prefix}_y");
        }

        /// <summary>
        /// Parse a 3D point from parameters
        /// </summary>
        private XYZ GetPoint(Dictionary<string, object> parameters, string prefix)
        {
            double x = Convert.ToDouble(parameters.ContainsKey($"{prefix}_x") ? parameters[$"{prefix}_x"] : 0);
            double y = Convert.ToDouble(parameters.ContainsKey($"{prefix}_y") ? parameters[$"{prefix}_y"] : 0);
            double z = Convert.ToDouble(parameters.ContainsKey($"{prefix}_z") ? parameters[$"{prefix}_z"] : 0);
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Create a wall in Revit
        /// </summary>
        private object CreateWall(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Extract parameters
                double startX = Convert.ToDouble(parameters.ContainsKey("start_x") ? parameters["start_x"] : 0);
                double startY = Convert.ToDouble(parameters.ContainsKey("start_y") ? parameters["start_y"] : 0);
                double endX = Convert.ToDouble(parameters.ContainsKey("end_x") ? parameters["end_x"] : 10);
                double endY = Convert.ToDouble(parameters.ContainsKey("end_y") ? parameters["end_y"] : 0);
                double height = Convert.ToDouble(parameters.ContainsKey("height") ? parameters["height"] : 3.0);
                string levelName = parameters.ContainsKey("level") ? parameters["level"].ToString() : "Level 1";
                string wallTypeName = parameters.ContainsKey("wall_type") ? parameters["wall_type"].ToString() : null;

                // Create XYZ points for the wall line
                XYZ startPoint = new XYZ(startX, startY, 0);
                XYZ endPoint = new XYZ(endX, endY, 0);

                // Get the level
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault(l => l.Name == levelName);

                if (level == null)
                {
                    return new { success = false, error = $"Level '{levelName}' not found" };
                }

                using (Transaction trans = new Transaction(doc, "Create Wall Linebased"))
                {
                    trans.Start();

                    try
                    {
                        // Get wall type
                        WallType wallType = null;
                        
                        if (wallTypeName != null)
                        {
                            // Find specific wall type by name
                            wallType = new FilteredElementCollector(doc)
                                .OfClass(typeof(WallType))
                                .Cast<WallType>()
                                .FirstOrDefault(wt => wt.Name == wallTypeName);
                        }

                        if (wallType == null)
                        {
                            // Get the default wall type
                            wallType = new FilteredElementCollector(doc)
                                .OfClass(typeof(WallType))
                                .Cast<WallType>()
                                .FirstOrDefault();
                        }

                        if (wallType == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = "No wall type found in the document" };
                        }

                        // Create the wall using linebased creation method
                        Line wallLine = Line.CreateBound(startPoint, endPoint);
                        var wallLines = new List<Curve> { wallLine };
                        Wall wall = Wall.Create(doc, wallLines, wallType.Id, level.Id, false);
                        wall.WallType = wallType;

                        trans.Commit();

                        return new
                        {
                            success = true,
                            wallId = GetElementIdInt(wall.Id),
                            wallType = wallType.Name,
                            level = level.Name,
                            startPoint = new { x = startX, y = startY },
                            endPoint = new { x = endX, y = endY },
                            height = height,
                            length = wallLine.Length
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to create wall: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in create_wall: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a bounded line as a model curve
        /// Used when you need a specific segment (e.g., wall edges, model lines, detail lines).
        /// </summary>
        private object CreateBoundedLine(Document doc, Dictionary<string, object> parameters)
        {
            if (!HasPoint(parameters, "start") || !HasPoint(parameters, "end"))
            {
                return new { success = false, error = "Bounded line requires start and end points" };
            }

            using (Transaction trans = new Transaction(doc, "Create Bounded Line"))
            {
                trans.Start();

                try
                {
                    XYZ startPoint = GetPoint(parameters, "start");
                    XYZ endPoint = GetPoint(parameters, "end");

                    Line line = Line.CreateBound(startPoint, endPoint);
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, startPoint);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    ModelCurve modelCurve = doc.Create.NewModelCurve(line, sketchPlane);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        lineId = GetElementIdInt(modelCurve.Id),
                        startPoint = new { x = startPoint.X, y = startPoint.Y, z = startPoint.Z },
                        endPoint = new { x = endPoint.X, y = endPoint.Y, z = endPoint.Z }
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create bounded line: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a bounded line as a detail curve in the active view.
        /// </summary>
        private object CreateDetailLine(Document doc, Dictionary<string, object> parameters)
        {
            if (!HasPoint(parameters, "start") || !HasPoint(parameters, "end"))
            {
                return new { success = false, error = "Detail line requires start and end points" };
            }

            View view = doc.ActiveView;
            if (view == null || view.IsTemplate)
            {
                return new { success = false, error = "Active view is not valid for detail curves" };
            }

            if (!CanAddDetailElements(view))
            {
                return new { success = false, error = $"Detail curves are not supported in view type: {view.ViewType}" };
            }

            using (Transaction trans = new Transaction(doc, "Create Detail Line"))
            {
                trans.Start();

                try
                {
                    XYZ startPoint = GetPoint(parameters, "start");
                    XYZ endPoint = GetPoint(parameters, "end");

                    Line line = Line.CreateBound(startPoint, endPoint);
                    DetailCurve detailCurve = doc.Create.NewDetailCurve(view, line);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        lineId = GetElementIdInt(detailCurve.Id),
                        viewId = GetElementIdInt(view.Id),
                        startPoint = new { x = startPoint.X, y = startPoint.Y, z = startPoint.Z },
                        endPoint = new { x = endPoint.X, y = endPoint.Y, z = endPoint.Z }
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create detail line: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create connected model curves from a list of points.
        /// </summary>
        private object CreateCurvesFromPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points array is required" };
            }

            List<XYZ> points = ParsePointList(parameters["points"]);
            if (points.Count < 2)
            {
                return new { success = false, error = "points array must contain at least two points" };
            }

            bool closed = parameters.ContainsKey("closed") && Convert.ToBoolean(parameters["closed"]);

            using (Transaction trans = new Transaction(doc, "Create Curves From Points"))
            {
                trans.Start();

                try
                {
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, points[0]);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    var curveIds = new List<int>();
                    int segmentCount = closed ? points.Count : points.Count - 1;

                    for (int i = 0; i < segmentCount; i++)
                    {
                        XYZ start = points[i];
                        XYZ end = (i == points.Count - 1) ? points[0] : points[i + 1];
                        Line line = Line.CreateBound(start, end);
                        ModelCurve modelCurve = doc.Create.NewModelCurve(line, sketchPlane);
                        curveIds.Add(GetElementIdInt(modelCurve.Id));
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        curveIds = curveIds,
                        segmentCount = curveIds.Count,
                        closed = closed
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create curves from points: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a Hermite spline model curve using default endpoint tangency.
        /// </summary>
        private object CreateHermiteSpline(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points array is required" };
            }

            List<XYZ> points = ParsePointList(parameters["points"]);
            if (points.Count < 2)
            {
                return new { success = false, error = "points array must contain at least two points" };
            }

            bool closed = parameters.ContainsKey("closed") && Convert.ToBoolean(parameters["closed"]);

            using (Transaction trans = new Transaction(doc, "Create Hermite Spline"))
            {
                trans.Start();

                try
                {
                    HermiteSpline spline = HermiteSpline.Create(points, closed);
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, points[0]);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    ModelCurve modelCurve = doc.Create.NewModelCurve(spline, sketchPlane);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        splineId = GetElementIdInt(modelCurve.Id),
                        pointCount = points.Count,
                        closed = closed
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create Hermite spline: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a Hermite spline model curve using specified endpoint tangency.
        /// </summary>
        private object CreateHermiteSplineWithTangents(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points array is required" };
            }

            if (!parameters.ContainsKey("start_tangent") || parameters["start_tangent"] == null)
            {
                return new { success = false, error = "start_tangent is required" };
            }

            if (!parameters.ContainsKey("end_tangent") || parameters["end_tangent"] == null)
            {
                return new { success = false, error = "end_tangent is required" };
            }

            List<XYZ> points = ParsePointList(parameters["points"]);
            if (points.Count < 2)
            {
                return new { success = false, error = "points array must contain at least two points" };
            }

            XYZ startTangent = ParseVector(parameters["start_tangent"]);
            XYZ endTangent = ParseVector(parameters["end_tangent"]);
            bool closed = parameters.ContainsKey("closed") && Convert.ToBoolean(parameters["closed"]);

            using (Transaction trans = new Transaction(doc, "Create Hermite Spline With Tangents"))
            {
                trans.Start();

                try
                {
                    HermiteSplineTangents tangents = CreateHermiteSplineTangents(startTangent, endTangent, out string tangentError);
                    if (tangents == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = tangentError ?? "Failed to create Hermite spline tangents" };
                    }
                    HermiteSpline spline = HermiteSpline.Create(points, closed, tangents);
                    Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, points[0]);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                    ModelCurve modelCurve = doc.Create.NewModelCurve(spline, sketchPlane);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        splineId = GetElementIdInt(modelCurve.Id),
                        pointCount = points.Count,
                        closed = closed
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create Hermite spline with tangents: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a new curve that is an offset of an existing curve element.
        /// </summary>
        private object CreateOffsetCurve(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("offset") || parameters["offset"] == null)
            {
                return new { success = false, error = "offset is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            double offset = Convert.ToDouble(parameters["offset"]);
            XYZ normal = parameters.ContainsKey("normal") ? ParseVector(parameters["normal"]) : XYZ.BasisZ;

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve sourceCurve = curveElement.GeometryCurve;
            if (sourceCurve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            using (Transaction trans = new Transaction(doc, "Create Offset Curve"))
            {
                trans.Start();

                try
                {
                    Curve offsetCurve = sourceCurve.CreateOffset(offset, normal);

                    Element newCurveElement = null;
                    if (curveElement is DetailCurve detailCurve)
                    {
                        View view = GetDetailCurveView(detailCurve, doc);
                        if (view == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to resolve detail curve view" };
                        }
                        newCurveElement = doc.Create.NewDetailCurve(view, offsetCurve);
                    }
                    else if (curveElement is ModelCurve modelCurve)
                    {
                        SketchPlane sketchPlane = modelCurve.SketchPlane;
                        if (sketchPlane == null)
                        {
                            Plane plane = Plane.CreateByNormalAndOrigin(normal, sourceCurve.GetEndPoint(0));
                            sketchPlane = SketchPlane.Create(doc, plane);
                        }
                        newCurveElement = doc.Create.NewModelCurve(offsetCurve, sketchPlane);
                    }
                    else
                    {
                        trans.RollBack();
                        return new { success = false, error = "Unsupported curve element type for offset" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        sourceCurveId = elementId,
                        offsetCurveId = GetElementIdInt(newCurveElement.Id),
                        offset = offset
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create offset curve: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Evaluate a point along a curve element at a given parameter.
        /// </summary>
        private object EvaluateCurve(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("parameter") || parameters["parameter"] == null)
            {
                return new { success = false, error = "parameter is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            double parameter = Convert.ToDouble(parameters["parameter"]);
            bool normalized = parameters.ContainsKey("normalized") && Convert.ToBoolean(parameters["normalized"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                XYZ point = curve.Evaluate(parameter, normalized);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    parameter = parameter,
                    normalized = normalized,
                    point = new { x = point.X, y = point.Y, z = point.Z }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to evaluate curve: {ex.Message}" };
            }
        }

        /// <summary>
        /// Return the shortest distance from a point to a curve element.
        /// </summary>
        private object CurveDistanceToPoint(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("point") || parameters["point"] == null)
            {
                return new { success = false, error = "point is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            XYZ point = ParseVector(parameters["point"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                double distance = curve.Distance(point);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    distance = distance,
                    point = new { x = point.X, y = point.Y, z = point.Z }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to compute distance: {ex.Message}" };
            }
        }

        /// <summary>
        /// Return the 3D point at the start or end of a curve element.
        /// </summary>
        private object CurveGetEndPoint(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            int endIndex = GetEndIndex(parameters);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                XYZ point = curve.GetEndPoint(endIndex);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    end = endIndex == 0 ? "start" : "end",
                    point = new { x = point.X, y = point.Y, z = point.Z }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get end point: {ex.Message}" };
            }
        }

        /// <summary>
        /// Return the raw parameter value at the start or end of a curve element.
        /// </summary>
        private object CurveGetEndParameter(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            int endIndex = GetEndIndex(parameters);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                double parameter = curve.GetEndParameter(endIndex);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    end = endIndex == 0 ? "start" : "end",
                    parameter = parameter
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get end parameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Return a stable reference to the start or end point of a curve element.
        /// </summary>
        private object CurveGetEndPointReference(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            int endIndex = GetEndIndex(parameters);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                Reference reference = curve.GetEndPointReference(endIndex);
                string stable = reference != null ? reference.ConvertToStableRepresentation(doc) : null;
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    end = endIndex == 0 ? "start" : "end",
                    reference = stable
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to get end point reference: {ex.Message}" };
            }
        }

        /// <summary>
        /// Clone a curve element and create a new curve in the same context.
        /// </summary>
        private object CreateCloneCurve(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve sourceCurve = curveElement.GeometryCurve;
            if (sourceCurve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            using (Transaction trans = new Transaction(doc, "Clone Curve"))
            {
                trans.Start();

                try
                {
                    Curve clonedCurve = sourceCurve.Clone();

                    Element newCurveElement = null;
                    if (curveElement is DetailCurve detailCurve)
                    {
                        View view = GetDetailCurveView(detailCurve, doc);
                        if (view == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to resolve detail curve view" };
                        }
                        newCurveElement = doc.Create.NewDetailCurve(view, clonedCurve);
                    }
                    else if (curveElement is ModelCurve modelCurve)
                    {
                        SketchPlane sketchPlane = modelCurve.SketchPlane;
                        if (sketchPlane == null)
                        {
                            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, sourceCurve.GetEndPoint(0));
                            sketchPlane = SketchPlane.Create(doc, plane);
                        }
                        newCurveElement = doc.Create.NewModelCurve(clonedCurve, sketchPlane);
                    }
                    else
                    {
                        trans.RollBack();
                        return new { success = false, error = "Unsupported curve element type for clone" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        sourceCurveId = elementId,
                        clonedCurveId = GetElementIdInt(newCurveElement.Id)
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to clone curve: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Compute closest point pairs between two curve elements.
        /// </summary>
        private object CurveComputeClosestPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id_1") || parameters["curve_element_id_1"] == null)
            {
                return new { success = false, error = "curve_element_id_1 is required" };
            }

            if (!parameters.ContainsKey("curve_element_id_2") || parameters["curve_element_id_2"] == null)
            {
                return new { success = false, error = "curve_element_id_2 is required" };
            }

            int elementId1 = Convert.ToInt32(parameters["curve_element_id_1"]);
            int elementId2 = Convert.ToInt32(parameters["curve_element_id_2"]);

            Element element1 = doc.GetElement(new ElementId(elementId1));
            Element element2 = doc.GetElement(new ElementId(elementId2));

            if (!(element1 is CurveElement curveElement1))
            {
                return new { success = false, error = "curve_element_id_1 must reference a curve element" };
            }

            if (!(element2 is CurveElement curveElement2))
            {
                return new { success = false, error = "curve_element_id_2 must reference a curve element" };
            }

            Curve curve1 = curveElement1.GeometryCurve;
            Curve curve2 = curveElement2.GeometryCurve;
            if (curve1 == null || curve2 == null)
            {
                return new { success = false, error = "Failed to read source curves" };
            }

            bool withinThisCurveBounds = parameters.ContainsKey("within_this_curve_bounds")
                ? Convert.ToBoolean(parameters["within_this_curve_bounds"])
                : true;
            bool withinOtherCurveBounds = parameters.ContainsKey("within_other_curve_bounds")
                ? Convert.ToBoolean(parameters["within_other_curve_bounds"])
                : true;
            bool returnAllCriticalPnts = parameters.ContainsKey("return_all_critical_points")
                ? Convert.ToBoolean(parameters["return_all_critical_points"])
                : false;

            try
            {
                IList<ClosestPointsPairBetweenTwoCurves> pairs;
                curve1.ComputeClosestPoints(
                    curve2,
                    withinThisCurveBounds,
                    withinOtherCurveBounds,
                    returnAllCriticalPnts,
                    out pairs
                );

                var results = pairs.Select(pair =>
                {
                    double? param1 = GetClosestPairParameter(pair, "Parameter1", "ParameterOnCurve1");
                    double? param2 = GetClosestPairParameter(pair, "Parameter2", "ParameterOnCurve2");
                    XYZ point1 = GetClosestPairPoint(pair, "PointOnCurve1", "Point1");
                    XYZ point2 = GetClosestPairPoint(pair, "PointOnCurve2", "Point2");

                    return new
                    {
                        parameter1 = param1,
                        parameter2 = param2,
                        point1 = point1 != null ? new { x = point1.X, y = point1.Y, z = point1.Z } : null,
                        point2 = point2 != null ? new { x = point2.X, y = point2.Y, z = point2.Z } : null
                    };
                }).ToList();

                return new
                {
                    success = true,
                    curveElementId1 = elementId1,
                    curveElementId2 = elementId2,
                    pairCount = results.Count,
                    withinThisCurveBounds = withinThisCurveBounds,
                    withinOtherCurveBounds = withinOtherCurveBounds,
                    returnAllCriticalPoints = returnAllCriticalPnts,
                    pairs = results
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to compute closest points: {ex.Message}" };
            }
        }

        /// <summary>
        /// Compute derivatives at a parameter on a curve element.
        /// </summary>
        private object CurveComputeDerivatives(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("parameter") || parameters["parameter"] == null)
            {
                return new { success = false, error = "parameter is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            double parameter = Convert.ToDouble(parameters["parameter"]);
            bool normalized = parameters.ContainsKey("normalized") && Convert.ToBoolean(parameters["normalized"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                Transform transform = curve.ComputeDerivatives(parameter, normalized);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    parameter = parameter,
                    normalized = normalized,
                    origin = new { x = transform.Origin.X, y = transform.Origin.Y, z = transform.Origin.Z },
                    basisX = new { x = transform.BasisX.X, y = transform.BasisX.Y, z = transform.BasisX.Z },
                    basisY = new { x = transform.BasisY.X, y = transform.BasisY.Y, z = transform.BasisY.Z },
                    basisZ = new { x = transform.BasisZ.X, y = transform.BasisZ.Y, z = transform.BasisZ.Z }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to compute derivatives: {ex.Message}" };
            }
        }

        /// <summary>
        /// Compute normalized parameter on a curve element.
        /// </summary>
        private object CurveComputeNormalizedParameter(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("parameter") || parameters["parameter"] == null)
            {
                return new { success = false, error = "parameter is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            double parameter = Convert.ToDouble(parameters["parameter"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                double normalized = curve.ComputeNormalizedParameter(parameter);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    parameter = parameter,
                    normalizedParameter = normalized
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to compute normalized parameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Compute raw parameter on a curve element from a normalized value.
        /// </summary>
        private object CurveComputeRawParameter(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("normalized_parameter") || parameters["normalized_parameter"] == null)
            {
                return new { success = false, error = "normalized_parameter is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            double normalizedParameter = Convert.ToDouble(parameters["normalized_parameter"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                double parameter = curve.ComputeRawParameter(normalizedParameter);
                return new
                {
                    success = true,
                    curveElementId = elementId,
                    normalizedParameter = normalizedParameter,
                    parameter = parameter
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to compute raw parameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a reversed curve from an existing curve element.
        /// </summary>
        private object CurveCreateReversed(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve sourceCurve = curveElement.GeometryCurve;
            if (sourceCurve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            using (Transaction trans = new Transaction(doc, "Create Reversed Curve"))
            {
                trans.Start();

                try
                {
                    Curve reversed = sourceCurve.CreateReversed();

                    Element newCurveElement = null;
                    if (curveElement is DetailCurve detailCurve)
                    {
                        View view = GetDetailCurveView(detailCurve, doc);
                        if (view == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to resolve detail curve view" };
                        }
                        newCurveElement = doc.Create.NewDetailCurve(view, reversed);
                    }
                    else if (curveElement is ModelCurve modelCurve)
                    {
                        SketchPlane sketchPlane = modelCurve.SketchPlane;
                        if (sketchPlane == null)
                        {
                            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, sourceCurve.GetEndPoint(0));
                            sketchPlane = SketchPlane.Create(doc, plane);
                        }
                        newCurveElement = doc.Create.NewModelCurve(reversed, sketchPlane);
                    }
                    else
                    {
                        trans.RollBack();
                        return new { success = false, error = "Unsupported curve element type for reversed curve" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        sourceCurveId = elementId,
                        reversedCurveId = GetElementIdInt(newCurveElement.Id)
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create reversed curve: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a transformed curve from an existing curve element.
        /// </summary>
        private object CurveCreateTransformed(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("transform") || parameters["transform"] == null)
            {
                return new { success = false, error = "transform is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            if (!TryParseTransform(parameters["transform"], out Transform transform))
            {
                return new { success = false, error = "transform must include origin, basisX, basisY, basisZ" };
            }

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve sourceCurve = curveElement.GeometryCurve;
            if (sourceCurve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            using (Transaction trans = new Transaction(doc, "Create Transformed Curve"))
            {
                trans.Start();

                try
                {
                    Curve transformed = sourceCurve.CreateTransformed(transform);

                    Element newCurveElement = null;
                    if (curveElement is DetailCurve detailCurve)
                    {
                        View view = GetDetailCurveView(detailCurve, doc);
                        if (view == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to resolve detail curve view" };
                        }
                        newCurveElement = doc.Create.NewDetailCurve(view, transformed);
                    }
                    else if (curveElement is ModelCurve modelCurve)
                    {
                        SketchPlane sketchPlane = modelCurve.SketchPlane;
                        if (sketchPlane == null)
                        {
                            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, sourceCurve.GetEndPoint(0));
                            sketchPlane = SketchPlane.Create(doc, plane);
                        }
                        newCurveElement = doc.Create.NewModelCurve(transformed, sketchPlane);
                    }
                    else
                    {
                        trans.RollBack();
                        return new { success = false, error = "Unsupported curve element type for transformed curve" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        sourceCurveId = elementId,
                        transformedCurveId = GetElementIdInt(newCurveElement.Id)
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create transformed curve: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Intersect two curve elements and return intersection points.
        /// </summary>
        private object CurveIntersect(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id_1") || parameters["curve_element_id_1"] == null)
            {
                return new { success = false, error = "curve_element_id_1 is required" };
            }

            if (!parameters.ContainsKey("curve_element_id_2") || parameters["curve_element_id_2"] == null)
            {
                return new { success = false, error = "curve_element_id_2 is required" };
            }

            int elementId1 = Convert.ToInt32(parameters["curve_element_id_1"]);
            int elementId2 = Convert.ToInt32(parameters["curve_element_id_2"]);

            Element element1 = doc.GetElement(new ElementId(elementId1));
            Element element2 = doc.GetElement(new ElementId(elementId2));

            if (!(element1 is CurveElement curveElement1))
            {
                return new { success = false, error = "curve_element_id_1 must reference a curve element" };
            }

            if (!(element2 is CurveElement curveElement2))
            {
                return new { success = false, error = "curve_element_id_2 must reference a curve element" };
            }

            Curve curve1 = curveElement1.GeometryCurve;
            Curve curve2 = curveElement2.GeometryCurve;
            if (curve1 == null || curve2 == null)
            {
                return new { success = false, error = "Failed to read source curves" };
            }

            try
            {
                IntersectionResultArray results;
                SetComparisonResult status = curve1.Intersect(curve2, out results);

                var points = new List<object>();
                if (results != null)
                {
                    foreach (IntersectionResult result in results)
                    {
                        if (result == null) continue;
                        XYZ point = result.XYZPoint;
                        points.Add(new
                        {
                            parameter1 = result.Parameter,
                            point = point != null ? new { x = point.X, y = point.Y, z = point.Z } : null
                        });
                    }
                }

                return new
                {
                    success = true,
                    curveElementId1 = elementId1,
                    curveElementId2 = elementId2,
                    status = status.ToString(),
                    intersectionCount = points.Count,
                    intersections = points
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to intersect curves: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a point element in the project (if supported by the Revit API).
        /// </summary>
        private object CreatePoint(Document doc, Dictionary<string, object> parameters)
        {
            XYZ point;
            if (parameters.ContainsKey("point") && parameters["point"] != null)
            {
                point = ParseVector(parameters["point"]);
            }
            else if (parameters.ContainsKey("x") && parameters.ContainsKey("y"))
            {
                double x = Convert.ToDouble(parameters["x"]);
                double y = Convert.ToDouble(parameters["y"]);
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;
                point = new XYZ(x, y, z);
            }
            else
            {
                return new { success = false, error = "point or x/y is required" };
            }

            using (Transaction trans = new Transaction(doc, "Create Point"))
            {
                trans.Start();

                try
                {
                    Type pointElementType = typeof(Document).Assembly.GetType("Autodesk.Revit.DB.PointElement");
                    if (pointElementType == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "PointElement type is not available in this Revit API" };
                    }

                    MethodInfo createMethod = pointElementType
                        .GetMethods(BindingFlags.Public | BindingFlags.Static)
                        .FirstOrDefault(m => m.Name == "Create"
                            && m.GetParameters().Length == 2
                            && m.GetParameters()[0].ParameterType == typeof(Document)
                            && m.GetParameters()[1].ParameterType == typeof(XYZ));

                    if (createMethod == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "PointElement.Create(Document, XYZ) is not available" };
                    }

                    object created = createMethod.Invoke(null, new object[] { doc, point });
                    if (!(created is Element element))
                    {
                        trans.RollBack();
                        return new { success = false, error = "Failed to create point element" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        pointId = GetElementIdInt(element.Id),
                        point = new { x = point.X, y = point.Y, z = point.Z }
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create point: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a point element based on the closest point on an element's geometry.
        /// </summary>
        private object CreatePointOnElement(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("element_id") || parameters["element_id"] == null)
            {
                return new { success = false, error = "element_id is required" };
            }

            if (!parameters.ContainsKey("point") || parameters["point"] == null)
            {
                return new { success = false, error = "point is required" };
            }

            int elementId = Convert.ToInt32(parameters["element_id"]);
            XYZ referencePoint = ParseVector(parameters["point"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (element == null)
            {
                return new { success = false, error = "Element not found" };
            }

            XYZ closestPoint = GetClosestPointOnElement(element, referencePoint);
            if (closestPoint == null)
            {
                return new { success = false, error = "Failed to find closest point on element geometry" };
            }

            using (Transaction trans = new Transaction(doc, "Create Point On Element"))
            {
                trans.Start();

                try
                {
                    Element created = CreatePointElement(doc, closestPoint, out string error);
                    if (created == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = error ?? "Failed to create point element" };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        elementId = elementId,
                        pointId = GetElementIdInt(created.Id),
                        point = new { x = closestPoint.X, y = closestPoint.Y, z = closestPoint.Z }
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create point on element: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Project a point onto a curve element and return location data.
        /// </summary>
        private object CurvePointLocationOnCurve(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("curve_element_id") || parameters["curve_element_id"] == null)
            {
                return new { success = false, error = "curve_element_id is required" };
            }

            if (!parameters.ContainsKey("point") || parameters["point"] == null)
            {
                return new { success = false, error = "point is required" };
            }

            int elementId = Convert.ToInt32(parameters["curve_element_id"]);
            XYZ point = ParseVector(parameters["point"]);

            Element element = doc.GetElement(new ElementId(elementId));
            if (!(element is CurveElement curveElement))
            {
                return new { success = false, error = "curve_element_id must reference a curve element" };
            }

            Curve curve = curveElement.GeometryCurve;
            if (curve == null)
            {
                return new { success = false, error = "Failed to read source curve" };
            }

            try
            {
                IntersectionResult result = curve.Project(point);
                if (result?.XYZPoint == null)
                {
                    return new { success = false, error = "Failed to project point onto curve" };
                }

                return new
                {
                    success = true,
                    curveElementId = elementId,
                    parameter = result.Parameter,
                    point = new { x = result.XYZPoint.X, y = result.XYZPoint.Y, z = result.XYZPoint.Z },
                    distance = result.Distance
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to project point onto curve: {ex.Message}" };
            }
        }

        /// <summary>
        /// Parse a list of XYZ points from a JSON array.
        /// </summary>
        private List<XYZ> ParsePointList(object pointsObject)
        {
            var points = new List<XYZ>();
            if (pointsObject is JArray pointsArray)
            {
                foreach (var token in pointsArray)
                {
                    if (token is JObject pointObj)
                    {
                        double x = pointObj["x"] != null ? pointObj["x"].ToObject<double>() : 0;
                        double y = pointObj["y"] != null ? pointObj["y"].ToObject<double>() : 0;
                        double z = pointObj["z"] != null ? pointObj["z"].ToObject<double>() : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }
            return points;
        }

        /// <summary>
        /// Check whether detail elements can be added to the given view.
        /// </summary>
        private bool CanAddDetailElements(View view)
        {
            var prop = view.GetType().GetProperty("CanAddDetailElements");
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                return (bool)prop.GetValue(view);
            }

            string viewTypeName = view.ViewType.ToString();
            if (string.Equals(viewTypeName, "Drafting", StringComparison.OrdinalIgnoreCase)
                || string.Equals(viewTypeName, "DraftingView", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            switch (view.ViewType)
            {
                case ViewType.FloorPlan:
                case ViewType.CeilingPlan:
                case ViewType.EngineeringPlan:
                case ViewType.AreaPlan:
                case ViewType.Detail:
                case ViewType.Elevation:
                case ViewType.Section:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Parse an XYZ vector from a JSON object with x, y, z.
        /// </summary>
        private XYZ ParseVector(object vectorObject)
        {
            if (vectorObject is JObject vectorObj)
            {
                double x = vectorObj["x"] != null ? vectorObj["x"].ToObject<double>() : 0;
                double y = vectorObj["y"] != null ? vectorObj["y"].ToObject<double>() : 0;
                double z = vectorObj["z"] != null ? vectorObj["z"].ToObject<double>() : 0;
                return new XYZ(x, y, z);
            }

            return XYZ.Zero;
        }

        /// <summary>
        /// Try to parse a 3D vector from a JSON object with x, y, z.
        /// </summary>
        private bool TryParseVector(object vectorObject, out XYZ vector)
        {
            if (vectorObject is JObject vectorObj && vectorObj["x"] != null && vectorObj["y"] != null)
            {
                double x = vectorObj["x"].ToObject<double>();
                double y = vectorObj["y"].ToObject<double>();
                double z = vectorObj["z"] != null ? vectorObj["z"].ToObject<double>() : 0;
                vector = new XYZ(x, y, z);
                return true;
            }

            vector = XYZ.Zero;
            return false;
        }

        /// <summary>
        /// Try to parse a transform from a JSON object with origin and basis vectors.
        /// </summary>
        private bool TryParseTransform(object transformObject, out Transform transform)
        {
            transform = null;
            if (!(transformObject is JObject transformObj))
            {
                return false;
            }

            if (!TryParseVector(transformObj["origin"], out XYZ origin)) return false;
            if (!TryParseVector(transformObj["basisX"], out XYZ basisX)) return false;
            if (!TryParseVector(transformObj["basisY"], out XYZ basisY)) return false;
            if (!TryParseVector(transformObj["basisZ"], out XYZ basisZ)) return false;

            Transform t = Transform.Identity;
            t.Origin = origin;
            t.BasisX = basisX;
            t.BasisY = basisY;
            t.BasisZ = basisZ;
            transform = t;
            return true;
        }

        /// <summary>
        /// Create Hermite spline tangents using API available in the current Revit version.
        /// </summary>
        private HermiteSplineTangents CreateHermiteSplineTangents(XYZ startTangent, XYZ endTangent, out string error)
        {
            error = null;
            Type tangentsType = typeof(HermiteSplineTangents);

            MethodInfo createMethod = tangentsType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Create"
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(XYZ)
                    && m.GetParameters()[1].ParameterType == typeof(XYZ));

            if (createMethod != null)
            {
                return (HermiteSplineTangents)createMethod.Invoke(null, new object[] { startTangent, endTangent });
            }

            ConstructorInfo ctor = tangentsType.GetConstructor(new[] { typeof(XYZ), typeof(XYZ) });
            if (ctor != null)
            {
                return (HermiteSplineTangents)ctor.Invoke(new object[] { startTangent, endTangent });
            }

            error = "HermiteSplineTangents factory/constructor is not available";
            return null;
        }

        /// <summary>
        /// Resolve the owning view for a detail curve.
        /// </summary>
        private View GetDetailCurveView(DetailCurve detailCurve, Document doc)
        {
            if (detailCurve == null || doc == null) return null;
            ElementId viewId = detailCurve.OwnerViewId;
            if (viewId == null || viewId == ElementId.InvalidElementId) return doc.ActiveView;
            return doc.GetElement(viewId) as View ?? doc.ActiveView;
        }

        /// <summary>
        /// Get parameter value from a closest points pair using reflection.
        /// </summary>
        private double? GetClosestPairParameter(object pair, params string[] propertyNames)
        {
            if (pair == null) return null;
            Type type = pair.GetType();
            foreach (string name in propertyNames)
            {
                PropertyInfo prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    object value = prop.GetValue(pair);
                    if (value is double dbl) return dbl;
                }
            }
            return null;
        }

        /// <summary>
        /// Get XYZ point from a closest points pair using reflection.
        /// </summary>
        private XYZ GetClosestPairPoint(object pair, params string[] propertyNames)
        {
            if (pair == null) return null;
            Type type = pair.GetType();
            foreach (string name in propertyNames)
            {
                PropertyInfo prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    object value = prop.GetValue(pair);
                    if (value is XYZ xyz) return xyz;
                }
            }
            return null;
        }

        /// <summary>
        /// Create a point element using PointElement.Create if available.
        /// </summary>
        private Element CreatePointElement(Document doc, XYZ point, out string error)
        {
            error = null;
            Type pointElementType = typeof(Document).Assembly.GetType("Autodesk.Revit.DB.PointElement");
            if (pointElementType == null)
            {
                error = "PointElement type is not available in this Revit API";
                return null;
            }

            MethodInfo createMethod = pointElementType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "Create"
                    && m.GetParameters().Length == 2
                    && m.GetParameters()[0].ParameterType == typeof(Document)
                    && m.GetParameters()[1].ParameterType == typeof(XYZ));

            if (createMethod == null)
            {
                error = "PointElement.Create(Document, XYZ) is not available";
                return null;
            }

            object created = createMethod.Invoke(null, new object[] { doc, point });
            return created as Element;
        }

        /// <summary>
        /// Find the closest point on an element's geometry to a reference point.
        /// </summary>
        private XYZ GetClosestPointOnElement(Element element, XYZ referencePoint)
        {
            Options options = new Options
            {
                ComputeReferences = false,
                IncludeNonVisibleObjects = false,
                DetailLevel = ViewDetailLevel.Fine
            };

            GeometryElement geometry = element.get_Geometry(options);
            if (geometry == null) return null;

            XYZ closestPoint = null;
            double minDistance = double.MaxValue;

            foreach (GeometryObject obj in geometry)
            {
                UpdateClosestPoint(obj, referencePoint, ref closestPoint, ref minDistance);
            }

            return closestPoint;
        }

        /// <summary>
        /// Update closest point using geometry object and its instances.
        /// </summary>
        private void UpdateClosestPoint(GeometryObject obj, XYZ referencePoint, ref XYZ closestPoint, ref double minDistance)
        {
            if (obj is GeometryInstance instance)
            {
                foreach (GeometryObject instObj in instance.GetInstanceGeometry())
                {
                    UpdateClosestPoint(instObj, referencePoint, ref closestPoint, ref minDistance);
                }
                return;
            }

            if (obj is Solid solid && solid.Faces.Size > 0)
            {
                foreach (Face face in solid.Faces)
                {
                    IntersectionResult result = face.Project(referencePoint);
                    if (result?.XYZPoint == null) continue;
                    double distance = result.XYZPoint.DistanceTo(referencePoint);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPoint = result.XYZPoint;
                    }
                }

                foreach (Edge edge in solid.Edges)
                {
                    Curve edgeCurve = edge.AsCurve();
                    IntersectionResult result = edgeCurve.Project(referencePoint);
                    if (result?.XYZPoint == null) continue;
                    double distance = result.XYZPoint.DistanceTo(referencePoint);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPoint = result.XYZPoint;
                    }
                }

                return;
            }

            if (obj is Curve curve)
            {
                IntersectionResult result = curve.Project(referencePoint);
                if (result?.XYZPoint == null) return;
                double distance = result.XYZPoint.DistanceTo(referencePoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestPoint = result.XYZPoint;
                }
                return;
            }

            if (obj is PolyLine polyLine)
            {
                foreach (XYZ pt in polyLine.GetCoordinates())
                {
                    double distance = pt.DistanceTo(referencePoint);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPoint = pt;
                    }
                }
            }
        }

        /// <summary>
        /// Get the curve end index from parameters (0=start, 1=end).
        /// </summary>
        private int GetEndIndex(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("end") && parameters["end"] != null)
            {
                string endValue = parameters["end"].ToString();
                if (string.Equals(endValue, "start", StringComparison.OrdinalIgnoreCase))
                {
                    return 0;
                }
            }

            return 1;
        }

        #endregion

        /// <summary>
        /// Helper to get integer id from an ElementId using reflection with a fallback.
        /// </summary>
        private int GetElementIdInt(ElementId id)
        {
            if (id == null) return -1;
            var prop = id.GetType().GetProperty("IntegerValue");
            if (prop != null)
            {
                var val = prop.GetValue(id);
                if (val is int) return (int)val;
            }

            // Fallback: try parse ToString()
            if (int.TryParse(id.ToString(), out int parsed)) return parsed;

            return id.GetHashCode();
        }

        /// <summary>
        /// Helper to get a readable name from an element using reflection.
        /// </summary>
        private string GetElementName(Element e)
        {
            if (e == null) return null;
            var prop = e.GetType().GetProperty("Name");
            if (prop != null)
            {
                var val = prop.GetValue(e);
                return val?.ToString();
            }

            var method = e.GetType().GetMethod("get_Name") ?? e.GetType().GetMethod("Name") ?? e.GetType().GetMethod("GetName");
            if (method != null)
            {
                var val = method.Invoke(e, null);
                return val?.ToString();
            }

            return e.ToString();
        }

        /// <summary>
        /// Calculate the direction vector of a line from start point to end point.
        /// </summary>
        private object CalculateLineDirection(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("start_point") || parameters["start_point"] == null)
            {
                return new { success = false, error = "start_point is required" };
            }
            if (!parameters.ContainsKey("end_point") || parameters["end_point"] == null)
            {
                return new { success = false, error = "end_point is required" };
            }

            try
            {
                var startPointDict = parameters["start_point"] as Dictionary<string, object>;
                var endPointDict = parameters["end_point"] as Dictionary<string, object>;

                if (startPointDict == null)
                {
                    startPointDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        parameters["start_point"].ToString());
                }
                if (endPointDict == null)
                {
                    endPointDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        parameters["end_point"].ToString());
                }

                double startX = Convert.ToDouble(startPointDict["x"]);
                double startY = Convert.ToDouble(startPointDict["y"]);
                double startZ = startPointDict.ContainsKey("z") ? Convert.ToDouble(startPointDict["z"]) : 0.0;

                double endX = Convert.ToDouble(endPointDict["x"]);
                double endY = Convert.ToDouble(endPointDict["y"]);
                double endZ = endPointDict.ContainsKey("z") ? Convert.ToDouble(endPointDict["z"]) : 0.0;

                // Calculate direction vector
                double dirX = endX - startX;
                double dirY = endY - startY;
                double dirZ = endZ - startZ;

                // Calculate length
                double length = Math.Sqrt(dirX * dirX + dirY * dirY + dirZ * dirZ);

                if (length < 1e-10)
                {
                    return new { success = false, error = "Start and end points are identical or too close" };
                }

                bool normalize = true;
                if (parameters.ContainsKey("normalize") && parameters["normalize"] != null)
                {
                    normalize = Convert.ToBoolean(parameters["normalize"]);
                }

                double resultX = dirX;
                double resultY = dirY;
                double resultZ = dirZ;

                if (normalize)
                {
                    resultX = dirX / length;
                    resultY = dirY / length;
                    resultZ = dirZ / length;
                }

                return new
                {
                    success = true,
                    direction = new { x = resultX, y = resultY, z = resultZ },
                    length = length,
                    normalized = normalize,
                    start_point = new { x = startX, y = startY, z = startZ },
                    end_point = new { x = endX, y = endY, z = endZ }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to calculate line direction: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create markup symbols (cross, circle, or square) at specified points using detail lines.
        /// </summary>
        private object CreatePointMarkup(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points array is required" };
            }

            View view = doc.ActiveView;
            if (view == null || view.IsTemplate)
            {
                return new { success = false, error = "Active view is not valid for detail curves" };
            }

            if (!CanAddDetailElements(view))
            {
                return new { success = false, error = $"Detail curves are not supported in view type: {view.ViewType}" };
            }

            string markupType = "cross";
            if (parameters.ContainsKey("markup_type") && parameters["markup_type"] != null)
            {
                markupType = parameters["markup_type"].ToString().ToLower();
            }

            double size = 1.0;
            if (parameters.ContainsKey("size") && parameters["size"] != null)
            {
                size = Convert.ToDouble(parameters["size"]);
            }

            var pointsList = new List<Dictionary<string, object>>();
            try
            {
                var pointsArray = parameters["points"] as Newtonsoft.Json.Linq.JArray;
                if (pointsArray != null)
                {
                    foreach (var pt in pointsArray)
                    {
                        pointsList.Add(pt.ToObject<Dictionary<string, object>>());
                    }
                }
                else if (parameters["points"] is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item is Dictionary<string, object> dict)
                        {
                            pointsList.Add(dict);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Failed to parse points: {ex.Message}" };
            }

            if (pointsList.Count == 0)
            {
                return new { success = false, error = "No valid points provided" };
            }

            var createdMarkups = new List<object>();

            using (Transaction trans = new Transaction(doc, "Create Point Markups"))
            {
                trans.Start();

                try
                {
                    foreach (var pointDict in pointsList)
                    {
                        double px = Convert.ToDouble(pointDict["x"]);
                        double py = Convert.ToDouble(pointDict["y"]);
                        double pz = pointDict.ContainsKey("z") ? Convert.ToDouble(pointDict["z"]) : 0.0;
                        XYZ center = new XYZ(px, py, pz);

                        var elementIds = new List<int>();

                        switch (markupType)
                        {
                            case "cross":
                                elementIds.AddRange(CreateCrossMarkup(doc, view, center, size));
                                break;
                            case "circle":
                                elementIds.AddRange(CreateCircleMarkup(doc, view, center, size));
                                break;
                            case "square":
                                elementIds.AddRange(CreateSquareMarkup(doc, view, center, size));
                                break;
                            default:
                                elementIds.AddRange(CreateCrossMarkup(doc, view, center, size));
                                break;
                        }

                        createdMarkups.Add(new
                        {
                            point = new { x = px, y = py, z = pz },
                            elementIds = elementIds
                        });
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        markupType = markupType,
                        size = size,
                        viewId = GetElementIdInt(view.Id),
                        markups = createdMarkups,
                        totalMarkups = createdMarkups.Count
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = $"Failed to create markups: {ex.Message}" };
                }
            }
        }

        /// <summary>
        /// Create a cross markup (two intersecting lines) at the specified center point.
        /// </summary>
        private List<int> CreateCrossMarkup(Document doc, View view, XYZ center, double size)
        {
            var ids = new List<int>();

            // Horizontal line
            XYZ h1 = new XYZ(center.X - size, center.Y, center.Z);
            XYZ h2 = new XYZ(center.X + size, center.Y, center.Z);
            Line hLine = Line.CreateBound(h1, h2);
            DetailCurve hCurve = doc.Create.NewDetailCurve(view, hLine);
            ids.Add(GetElementIdInt(hCurve.Id));

            // Vertical line
            XYZ v1 = new XYZ(center.X, center.Y - size, center.Z);
            XYZ v2 = new XYZ(center.X, center.Y + size, center.Z);
            Line vLine = Line.CreateBound(v1, v2);
            DetailCurve vCurve = doc.Create.NewDetailCurve(view, vLine);
            ids.Add(GetElementIdInt(vCurve.Id));

            return ids;
        }

        /// <summary>
        /// Create a circle markup at the specified center point.
        /// </summary>
        private List<int> CreateCircleMarkup(Document doc, View view, XYZ center, double radius)
        {
            var ids = new List<int>();

            // Create a full circle using two arcs (Revit doesn't support full circles as single curves)
            XYZ normal = XYZ.BasisZ;
            
            // First semicircle (0 to PI)
            Arc arc1 = Arc.Create(center, radius, 0, Math.PI, XYZ.BasisX, XYZ.BasisY);
            DetailCurve curve1 = doc.Create.NewDetailCurve(view, arc1);
            ids.Add(GetElementIdInt(curve1.Id));

            // Second semicircle (PI to 2*PI)
            Arc arc2 = Arc.Create(center, radius, Math.PI, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
            DetailCurve curve2 = doc.Create.NewDetailCurve(view, arc2);
            ids.Add(GetElementIdInt(curve2.Id));

            return ids;
        }

        /// <summary>
        /// Create a square markup at the specified center point.
        /// </summary>
        private List<int> CreateSquareMarkup(Document doc, View view, XYZ center, double halfSize)
        {
            var ids = new List<int>();

            // Four corner points
            XYZ p1 = new XYZ(center.X - halfSize, center.Y - halfSize, center.Z); // bottom-left
            XYZ p2 = new XYZ(center.X + halfSize, center.Y - halfSize, center.Z); // bottom-right
            XYZ p3 = new XYZ(center.X + halfSize, center.Y + halfSize, center.Z); // top-right
            XYZ p4 = new XYZ(center.X - halfSize, center.Y + halfSize, center.Z); // top-left

            // Four edges
            Line line1 = Line.CreateBound(p1, p2); // bottom
            DetailCurve curve1 = doc.Create.NewDetailCurve(view, line1);
            ids.Add(GetElementIdInt(curve1.Id));

            Line line2 = Line.CreateBound(p2, p3); // right
            DetailCurve curve2 = doc.Create.NewDetailCurve(view, line2);
            ids.Add(GetElementIdInt(curve2.Id));

            Line line3 = Line.CreateBound(p3, p4); // top
            DetailCurve curve3 = doc.Create.NewDetailCurve(view, line3);
            ids.Add(GetElementIdInt(curve3.Id));

            Line line4 = Line.CreateBound(p4, p1); // left
            DetailCurve curve4 = doc.Create.NewDetailCurve(view, line4);
            ids.Add(GetElementIdInt(curve4.Id));

            return ids;
        }

        /// <summary>
        /// Create geometric shapes (rectangles, circles, polygons) as detail lines in a view
        /// </summary>
        private object CreateDetailShapes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("shape_type") || parameters["shape_type"] == null)
                {
                    return new { success = false, error = "shape_type is required" };
                }

                string shapeType = parameters["shape_type"].ToString().ToLower();
                
                // Get view - use active view if not specified
                View view = doc.ActiveView;
                if (parameters.ContainsKey("view_id") && parameters["view_id"] != null)
                {
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v)
                    {
                        view = v;
                    }
                }

                if (view == null || view.IsTemplate || !CanAddDetailElements(view))
                {
                    return new { success = false, error = "Active view is not valid for detail curves" };
                }

                double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
                XYZ center = new XYZ(centerX, centerY, centerZ);

                double width = parameters.ContainsKey("width") ? Convert.ToDouble(parameters["width"]) : 5;
                double height = parameters.ContainsKey("height") ? Convert.ToDouble(parameters["height"]) : 5;
                double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
                int sides = parameters.ContainsKey("sides") ? Convert.ToInt32(parameters["sides"]) : 6;
                double rotation = parameters.ContainsKey("rotation") ? Convert.ToDouble(parameters["rotation"]) : 0;

                var createdLines = new List<int>();

                using (Transaction trans = new Transaction(doc, "Create Detail Shape"))
                {
                    trans.Start();

                    try
                    {
                        List<XYZ> shapePoints = GenerateShapePoints(shapeType, center, width, height, radius, sides, rotation);

                        if (shapePoints.Count < 2)
                        {
                            return new { success = false, error = "Invalid shape parameters" };
                        }

                        // Create lines connecting the points
                        for (int i = 0; i < shapePoints.Count; i++)
                        {
                            XYZ start = shapePoints[i];
                            XYZ end = shapePoints[(i + 1) % shapePoints.Count];
                            
                            Line line = Line.CreateBound(start, end);
                            DetailCurve detailCurve = doc.Create.NewDetailCurve(view, line);
                            createdLines.Add(GetElementIdInt(detailCurve.Id));
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            shape_type = shapeType,
                            center = new { x = centerX, y = centerY, z = centerZ },
                            line_count = createdLines.Count,
                            line_ids = createdLines,
                            view_id = GetElementIdInt(view.Id)
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to create detail shape: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateDetailShapes error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create geometric shapes (rectangles, circles, polygons) as model lines in 3D space
        /// </summary>
        private object CreateModelShapes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("shape_type") || parameters["shape_type"] == null)
                {
                    return new { success = false, error = "shape_type is required" };
                }

                string shapeType = parameters["shape_type"].ToString().ToLower();

                double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
                XYZ center = new XYZ(centerX, centerY, centerZ);

                double width = parameters.ContainsKey("width") ? Convert.ToDouble(parameters["width"]) : 5;
                double height = parameters.ContainsKey("height") ? Convert.ToDouble(parameters["height"]) : 5;
                double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
                int sides = parameters.ContainsKey("sides") ? Convert.ToInt32(parameters["sides"]) : 6;
                double rotation = parameters.ContainsKey("rotation") ? Convert.ToDouble(parameters["rotation"]) : 0;

                var createdLines = new List<int>();

                using (Transaction trans = new Transaction(doc, "Create Model Shape"))
                {
                    trans.Start();

                    try
                    {
                        List<XYZ> shapePoints = GenerateShapePoints(shapeType, center, width, height, radius, sides, rotation);

                        if (shapePoints.Count < 2)
                        {
                            return new { success = false, error = "Invalid shape parameters" };
                        }

                        // Create lines connecting the points
                        for (int i = 0; i < shapePoints.Count; i++)
                        {
                            XYZ start = shapePoints[i];
                            XYZ end = shapePoints[(i + 1) % shapePoints.Count];
                            
                            Line line = Line.CreateBound(start, end);
                            ModelCurve modelCurve = doc.Create.NewModelCurve(line, (SketchPlane)null);
                            createdLines.Add(GetElementIdInt(modelCurve.Id));
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            shape_type = shapeType,
                            center = new { x = centerX, y = centerY, z = centerZ },
                            line_count = createdLines.Count,
                            line_ids = createdLines
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to create model shape: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateModelShapes error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create geometric shapes (rectangles, circles, polygons) as symbolic lines in a family document
        /// </summary>
        private object CreateSymbolicShapes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This tool requires a family document. Open a family file." };
                }

                if (!parameters.ContainsKey("shape_type") || parameters["shape_type"] == null)
                {
                    return new { success = false, error = "shape_type is required" };
                }

                string shapeType = parameters["shape_type"].ToString().ToLower();

                double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
                XYZ center = new XYZ(centerX, centerY, centerZ);

                double width = parameters.ContainsKey("width") ? Convert.ToDouble(parameters["width"]) : 5;
                double height = parameters.ContainsKey("height") ? Convert.ToDouble(parameters["height"]) : 5;
                double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
                int sides = parameters.ContainsKey("sides") ? Convert.ToInt32(parameters["sides"]) : 6;
                double rotation = parameters.ContainsKey("rotation") ? Convert.ToDouble(parameters["rotation"]) : 0;

                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                var createdCurves = new List<int>();

                using (Transaction trans = new Transaction(doc, "Create Symbolic Shape"))
                {
                    trans.Start();

                    try
                    {
                        List<XYZ> shapePoints = GenerateShapePoints(shapeType, center, width, height, radius, sides, rotation);

                        if (shapePoints.Count < 2)
                        {
                            return new { success = false, error = "Invalid shape parameters" };
                        }

                        // Create symbolic curves connecting the points
                        for (int i = 0; i < shapePoints.Count; i++)
                        {
                            XYZ start = shapePoints[i];
                            XYZ end = shapePoints[(i + 1) % shapePoints.Count];
                            
                            Line line = Line.CreateBound(start, end);
                            SymbolicCurve symbolicCurve = doc.FamilyCreate.NewSymbolicCurve(line, sketchPlane);
                            createdCurves.Add(GetElementIdInt(symbolicCurve.Id));
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            shape_type = shapeType,
                            center = new { x = centerX, y = centerY, z = centerZ },
                            curve_count = createdCurves.Count,
                            curve_ids = createdCurves,
                            sketch_plane_id = GetElementIdInt(sketchPlane.Id)
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to create symbolic shape: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateSymbolicShapes error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Generate shape points based on shape type and parameters
        /// </summary>
        private List<XYZ> GenerateShapePoints(string shapeType, XYZ center, double width, double height, double radius, int sides, double rotationDegrees)
        {
            var points = new List<XYZ>();
            double rotationRad = rotationDegrees * Math.PI / 180.0;

            switch (shapeType.ToLower())
            {
                case "rectangle":
                    {
                        XYZ[] corners = new XYZ[]
                        {
                            new XYZ(center.X - width, center.Y - height, center.Z),
                            new XYZ(center.X + width, center.Y - height, center.Z),
                            new XYZ(center.X + width, center.Y + height, center.Z),
                            new XYZ(center.X - width, center.Y + height, center.Z)
                        };

                        foreach (XYZ corner in corners)
                        {
                            XYZ rotated = RotatePointAround(corner, center, rotationRad);
                            points.Add(rotated);
                        }
                        break;
                    }

                case "circle":
                    {
                        int segments = Math.Max(12, sides * 2); // Use more segments for smooth circle
                        for (int i = 0; i < segments; i++)
                        {
                            double angle = (2 * Math.PI * i) / segments + rotationRad;
                            XYZ point = new XYZ(
                                center.X + radius * Math.Cos(angle),
                                center.Y + radius * Math.Sin(angle),
                                center.Z
                            );
                            points.Add(point);
                        }
                        break;
                    }

                case "polygon":
                    {
                        if (sides < 3) sides = 3;
                        for (int i = 0; i < sides; i++)
                        {
                            double angle = (2 * Math.PI * i) / sides + rotationRad;
                            XYZ point = new XYZ(
                                center.X + radius * Math.Cos(angle),
                                center.Y + radius * Math.Sin(angle),
                                center.Z
                            );
                            points.Add(point);
                        }
                        break;
                    }

                default:
                    // Default to rectangle
                    {
                        XYZ[] corners = new XYZ[]
                        {
                            new XYZ(center.X - width, center.Y - height, center.Z),
                            new XYZ(center.X + width, center.Y - height, center.Z),
                            new XYZ(center.X + width, center.Y + height, center.Z),
                            new XYZ(center.X - width, center.Y + height, center.Z)
                        };

                        foreach (XYZ corner in corners)
                        {
                            XYZ rotated = RotatePointAround(corner, center, rotationRad);
                            points.Add(rotated);
                        }
                        break;
                    }
            }

            return points;
        }

        /// <summary>
        /// Rotate a point around a center point by a specified angle
        /// </summary>
        private XYZ RotatePointAround(XYZ point, XYZ center, double angleRadians)
        {
            // Translate to origin
            double x = point.X - center.X;
            double y = point.Y - center.Y;
            double z = point.Z;

            // Rotate around Z axis
            double cosA = Math.Cos(angleRadians);
            double sinA = Math.Sin(angleRadians);
            double newX = x * cosA - y * sinA;
            double newY = x * sinA + y * cosA;

            // Translate back
            return new XYZ(newX + center.X, newY + center.Y, z);
        }

        /// <summary>
        /// Rotate elements around an axis using ElementTransformUtils
        /// </summary>
        private object RotateElements(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Parse element IDs (can be single ID or array)
                List<ElementId> elementIds = new List<ElementId>();
                
                if (parameters.ContainsKey("element_ids") && parameters["element_ids"] != null)
                {
                    var idsObj = parameters["element_ids"];
                    if (idsObj is JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            elementIds.Add(new ElementId(Convert.ToInt32(item)));
                        }
                    }
                    else if (idsObj is IEnumerable<object> idList)
                    {
                        foreach (var id in idList)
                        {
                            elementIds.Add(new ElementId(Convert.ToInt32(id)));
                        }
                    }
                }
                else if (parameters.ContainsKey("element_id") && parameters["element_id"] != null)
                {
                    elementIds.Add(new ElementId(Convert.ToInt32(parameters["element_id"])));
                }
                else
                {
                    return new { success = false, error = "element_id or element_ids is required" };
                }

                if (elementIds.Count == 0)
                {
                    return new { success = false, error = "No valid element IDs provided" };
                }

                // Get the rotation angle in degrees and convert to radians
                if (!parameters.ContainsKey("angle") || parameters["angle"] == null)
                {
                    return new { success = false, error = "angle is required (in degrees)" };
                }
                double angleDegrees = Convert.ToDouble(parameters["angle"]);
                double angleRadians = angleDegrees * Math.PI / 180.0;

                // Determine default Z coordinate for rotation axis
                // In family documents, use Reference Level elevation; in projects, use 0
                double defaultAxisZ = 0;
                string referencePlaneUsed = "origin (0,0,0)";
                
                if (doc.IsFamilyDocument && !parameters.ContainsKey("axis_point_z"))
                {
                    try
                    {
                        // Try to find Reference Level sketch plane in family document
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        var refLevelPlane = collector.OfClass(typeof(SketchPlane))
                            .Cast<SketchPlane>()
                            .FirstOrDefault(sp => sp.Name.Equals("Reference Level", StringComparison.OrdinalIgnoreCase));
                        
                        if (refLevelPlane != null)
                        {
                            defaultAxisZ = refLevelPlane.GetPlane().Origin.Z;
                            referencePlaneUsed = $"Reference Level (Z={defaultAxisZ:F3})";
                        }
                    }
                    catch
                    {
                        // If we can't find Reference Level, use 0
                        defaultAxisZ = 0;
                    }
                }

                // Get axis point (defaults to origin or Reference Level in families)
                double axisX = Convert.ToDouble(parameters.ContainsKey("axis_point_x") ? parameters["axis_point_x"] : 0);
                double axisY = Convert.ToDouble(parameters.ContainsKey("axis_point_y") ? parameters["axis_point_y"] : 0);
                double axisZ = Convert.ToDouble(parameters.ContainsKey("axis_point_z") ? parameters["axis_point_z"] : defaultAxisZ);
                XYZ axisPoint = new XYZ(axisX, axisY, axisZ);

                // Get axis direction (defaults to Z-axis for rotation in XY plane)
                double dirX = Convert.ToDouble(parameters.ContainsKey("axis_direction_x") ? parameters["axis_direction_x"] : 0);
                double dirY = Convert.ToDouble(parameters.ContainsKey("axis_direction_y") ? parameters["axis_direction_y"] : 0);
                double dirZ = Convert.ToDouble(parameters.ContainsKey("axis_direction_z") ? parameters["axis_direction_z"] : 1);
                XYZ axisDirection = new XYZ(dirX, dirY, dirZ).Normalize();

                // Create the rotation axis line
                Line axis = Line.CreateUnbound(axisPoint, axisDirection);

                // Verify elements exist
                var validElements = new List<Element>();
                var invalidIds = new List<int>();
                foreach (var elemId in elementIds)
                {
                    Element elem = doc.GetElement(elemId);
                    if (elem != null)
                    {
                        validElements.Add(elem);
                    }
                    else
                    {
                        invalidIds.Add(GetElementIdInt(elemId));
                    }
                }

                if (validElements.Count == 0)
                {
                    return new { success = false, error = "No valid elements found with the provided IDs" };
                }

                using (Transaction trans = new Transaction(doc, "Rotate Elements"))
                {
                    trans.Start();

                    try
                    {
                        var rotatedIds = new List<int>();
                        var failedIds = new List<int>();

                        foreach (var elem in validElements)
                        {
                            try
                            {
                                ElementTransformUtils.RotateElement(doc, elem.Id, axis, angleRadians);
                                rotatedIds.Add(GetElementIdInt(elem.Id));
                            }
                            catch (Exception ex)
                            {
                                failedIds.Add(GetElementIdInt(elem.Id));
                            }
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            rotated_count = rotatedIds.Count,
                            rotated_element_ids = rotatedIds,
                            failed_element_ids = failedIds,
                            invalid_element_ids = invalidIds,
                            angle_degrees = angleDegrees,
                            angle_radians = angleRadians,
                            axis_point = new { x = axisX, y = axisY, z = axisZ },
                            axis_direction = new { x = dirX, y = dirY, z = dirZ },
                            reference_plane_used = referencePlaneUsed,
                            is_family_document = doc.IsFamilyDocument
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to rotate elements: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in RotateElements: {ex.Message}" };
            }
        }

        /// <summary>
        /// Add a shared parameter to the current family document
        /// </summary>
        private object AddFamilySharedParameter(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Verify this is a family document
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command can only be used in the Family Editor. Open a family document first." };
                }

                FamilyManager familyManager = doc.FamilyManager;
                if (familyManager == null)
                {
                    return new { success = false, error = "Could not access FamilyManager" };
                }

                // Get the shared parameter file path
                if (!parameters.ContainsKey("shared_parameter_file") || parameters["shared_parameter_file"] == null)
                {
                    return new { success = false, error = "shared_parameter_file path is required" };
                }
                string sharedParamFilePath = parameters["shared_parameter_file"].ToString();

                // Get the parameter name to add
                if (!parameters.ContainsKey("parameter_name") || parameters["parameter_name"] == null)
                {
                    return new { success = false, error = "parameter_name is required" };
                }
                string parameterName = parameters["parameter_name"].ToString();

                // Get the parameter group (optional, defaults to General)
                string paramGroupName = parameters.ContainsKey("parameter_group") && parameters["parameter_group"] != null
                    ? parameters["parameter_group"].ToString()
                    : "General";

                // Parse the parameter group using ForgeTypeId (Revit 2024+ API)
                ForgeTypeId paramGroupId = GetParameterGroupTypeId(paramGroupName);

                // Get instance vs type parameter setting (optional, defaults to instance)
                bool isInstance = true;
                if (parameters.ContainsKey("is_instance") && parameters["is_instance"] != null)
                {
                    isInstance = Convert.ToBoolean(parameters["is_instance"]);
                }

                // Open the shared parameter file
                DefinitionFile defFile = null;
                try
                {
                    doc.Application.SharedParametersFilename = sharedParamFilePath;
                    defFile = doc.Application.OpenSharedParameterFile();
                }
                catch (Exception ex)
                {
                    return new { success = false, error = $"Failed to open shared parameter file: {ex.Message}" };
                }

                if (defFile == null)
                {
                    return new { success = false, error = $"Could not open shared parameter file at '{sharedParamFilePath}'" };
                }

                // Find the parameter definition in the shared parameter file
                ExternalDefinition externalDef = null;
                string foundInGroup = null;

                foreach (DefinitionGroup group in defFile.Groups)
                {
                    foreach (Definition def in group.Definitions)
                    {
                        if (def.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            externalDef = def as ExternalDefinition;
                            foundInGroup = group.Name;
                            break;
                        }
                    }
                    if (externalDef != null) break;
                }

                if (externalDef == null)
                {
                    return new { success = false, error = $"Parameter '{parameterName}' not found in shared parameter file" };
                }

                // Check if parameter already exists in the family
                foreach (FamilyParameter fp in familyManager.Parameters)
                {
                    if (fp.Definition.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new {
                            success = false,
                            error = $"Parameter '{parameterName}' already exists in this family",
                            existing_parameter = new {
                                name = fp.Definition.Name,
                                is_instance = fp.IsInstance,
                                is_shared = fp.IsShared
                            }
                        };
                    }
                }

                using (Transaction trans = new Transaction(doc, "Add Shared Parameter"))
                {
                    trans.Start();

                    try
                    {
                        FamilyParameter newParam = familyManager.AddParameter(externalDef, paramGroupId, isInstance);

                        trans.Commit();

                        return new
                        {
                            success = true,
                            parameter_name = newParam.Definition.Name,
                            is_instance = newParam.IsInstance,
                            is_shared = newParam.IsShared,
                            parameter_group = LabelUtils.GetLabelForGroup(paramGroupId),
                            shared_param_group = foundInGroup
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to add parameter: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in AddFamilySharedParameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Remove a parameter from the current family document
        /// </summary>
        private object RemoveFamilyParameter(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Verify this is a family document
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command can only be used in the Family Editor. Open a family document first." };
                }

                FamilyManager familyManager = doc.FamilyManager;
                if (familyManager == null)
                {
                    return new { success = false, error = "Could not access FamilyManager" };
                }

                // Get the parameter name to remove
                if (!parameters.ContainsKey("parameter_name") || parameters["parameter_name"] == null)
                {
                    return new { success = false, error = "parameter_name is required" };
                }
                string parameterName = parameters["parameter_name"].ToString();

                // Find the parameter
                FamilyParameter paramToRemove = null;
                foreach (FamilyParameter fp in familyManager.Parameters)
                {
                    if (fp.Definition.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        paramToRemove = fp;
                        break;
                    }
                }

                if (paramToRemove == null)
                {
                    return new { success = false, error = $"Parameter '{parameterName}' not found in this family" };
                }

                // Store info before removal
                string removedName = paramToRemove.Definition.Name;
                bool wasInstance = paramToRemove.IsInstance;
                bool wasShared = paramToRemove.IsShared;

                using (Transaction trans = new Transaction(doc, "Remove Family Parameter"))
                {
                    trans.Start();

                    try
                    {
                        familyManager.RemoveParameter(paramToRemove);

                        trans.Commit();

                        return new
                        {
                            success = true,
                            removed_parameter = removedName,
                            was_instance = wasInstance,
                            was_shared = wasShared
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to remove parameter: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in RemoveFamilyParameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get all parameters in the current family document
        /// </summary>
        private object GetFamilyParameters(Document doc)
        {
            try
            {
                // Verify this is a family document
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command can only be used in the Family Editor. Open a family document first." };
                }

                FamilyManager familyManager = doc.FamilyManager;
                if (familyManager == null)
                {
                    return new { success = false, error = "Could not access FamilyManager" };
                }

                var parameterList = new List<object>();

                foreach (FamilyParameter fp in familyManager.Parameters)
                {
                    var paramInfo = new Dictionary<string, object>
                    {
                        { "name", fp.Definition.Name },
                        { "is_instance", fp.IsInstance },
                        { "is_shared", fp.IsShared },
                        { "is_read_only", fp.IsReadOnly },
                        { "is_reporting", fp.IsReporting },
                        { "storage_type", fp.StorageType.ToString() },
                        { "can_assign_formula", fp.CanAssignFormula },
                        { "is_determined_by_formula", fp.IsDeterminedByFormula }
                    };

                    // Try to get parameter group
                    try
                    {
                        var groupId = fp.Definition.GetGroupTypeId();
                        paramInfo["parameter_group"] = LabelUtils.GetLabelForGroup(groupId);
                    }
                    catch
                    {
                        paramInfo["parameter_group"] = "Unknown";
                    }

                    // Try to get current value if possible
                    try
                    {
                        if (familyManager.CurrentType != null)
                        {
                            switch (fp.StorageType)
                            {
                                case StorageType.Double:
                                    var dval = familyManager.CurrentType.AsDouble(fp);
                                    paramInfo["current_value"] = dval.HasValue ? dval.Value : (object)null;
                                    break;
                                case StorageType.Integer:
                                    var ival = familyManager.CurrentType.AsInteger(fp);
                                    paramInfo["current_value"] = ival.HasValue ? ival.Value : (object)null;
                                    break;
                                case StorageType.String:
                                    paramInfo["current_value"] = familyManager.CurrentType.AsString(fp);
                                    break;
                                case StorageType.ElementId:
                                    var elemId = familyManager.CurrentType.AsElementId(fp);
                                    paramInfo["current_value"] = elemId != null ? GetElementIdInt(elemId) : (object)null;
                                    break;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore errors getting current value
                    }

                    // Get formula if present
                    try
                    {
                        if (fp.IsDeterminedByFormula)
                        {
                            paramInfo["formula"] = familyManager.CurrentType?.AsValueString(fp);
                        }
                    }
                    catch
                    {
                        // Ignore formula errors
                    }

                    parameterList.Add(paramInfo);
                }

                // Get family types
                var familyTypes = new List<object>();
                foreach (FamilyType ft in familyManager.Types)
                {
                    familyTypes.Add(new {
                        name = ft.Name,
                        is_current = familyManager.CurrentType != null && ft.Name == familyManager.CurrentType.Name
                    });
                }

                return new
                {
                    success = true,
                    family_name = doc.Title,
                    parameter_count = parameterList.Count,
                    family_types = familyTypes,
                    parameters = parameterList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in GetFamilyParameters: {ex.Message}" };
            }
        }

        /// <summary>
        /// Detect whether the current document is a family (.rfa) or a project document (.rvt)
        /// </summary>
        private object DetectDocumentType(Document doc)
        {
            try
            {
                string documentPath = doc.PathName;
                string documentTitle = doc.Title;
                bool isFamilyDocument = doc.IsFamilyDocument;
                bool isProjectDocument = !isFamilyDocument;
                
                // Determine document type
                string documentType = isFamilyDocument ? "Family (.rfa)" : "Project (.rvt)";
                
                // Get additional information
                string documentCategory = "Unknown";
                if (isFamilyDocument)
                {
                    try
                    {
                        // Try to determine family category
                        FamilyManager fm = doc.FamilyManager;
                        if (fm != null && fm.Types.Size > 0)
                        {
                            FamilyType firstType = null;
                            foreach (FamilyType ft in fm.Types)
                            {
                                firstType = ft;
                                break;
                            }
                            if (firstType != null)
                            {
                                documentCategory = "Conceptual Mass";
                            }
                        }
                    }
                    catch
                    {
                        documentCategory = "Family";
                    }
                }
                else
                {
                    documentCategory = "Project";
                }

                return new
                {
                    success = true,
                    is_family_document = isFamilyDocument,
                    is_project_document = isProjectDocument,
                    document_type = documentType,
                    document_category = documentCategory,
                    document_title = documentTitle,
                    document_path = documentPath,
                    can_add_shared_parameters = isProjectDocument,
                    can_edit_family_parameters = isFamilyDocument,
                    can_create_families = isProjectDocument,
                    can_load_families = isProjectDocument
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in DetectDocumentType: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get ForgeTypeId for a parameter group name (Revit 2024+ API)
        /// </summary>
        private ForgeTypeId GetParameterGroupTypeId(string groupName)
        {
            // Normalize the group name
            string normalizedName = groupName.ToLower().Replace("pg_", "").Replace("_", "").Replace(" ", "");

            // Map common parameter group names to GroupTypeId values
            var groupMapping = new Dictionary<string, ForgeTypeId>(StringComparer.OrdinalIgnoreCase)
            {
                { "general", GroupTypeId.General },
                { "geometry", GroupTypeId.Geometry },
                { "identity", GroupTypeId.IdentityData },
                { "identitydata", GroupTypeId.IdentityData },
                { "construction", GroupTypeId.Construction },
                { "materials", GroupTypeId.Materials },
                { "materialandfinishes", GroupTypeId.Materials },
                { "structural", GroupTypeId.Structural },
                { "structuralanalysis", GroupTypeId.StructuralAnalysis },
                { "mechanical", GroupTypeId.Mechanical },
                { "mechanicalairflow", GroupTypeId.MechanicalAirflow },
                { "mechanicalloads", GroupTypeId.MechanicalLoads },
                { "electrical", GroupTypeId.Electrical },
                { "electricalcircuiting", GroupTypeId.ElectricalCircuiting },
                { "electricalengineering", GroupTypeId.ElectricalEngineering },
                { "electricallighting", GroupTypeId.ElectricalLighting },
                { "electricalloads", GroupTypeId.ElectricalLoads },
                { "plumbing", GroupTypeId.Plumbing },
                { "energy", GroupTypeId.EnergyAnalysis },
                { "energyanalysis", GroupTypeId.EnergyAnalysis },
                { "text", GroupTypeId.Text },
                { "graphics", GroupTypeId.Graphics },
                { "constraints", GroupTypeId.Constraints },
                { "phasing", GroupTypeId.Phasing },
                { "green", GroupTypeId.GreenBuilding },
                { "greenbuilding", GroupTypeId.GreenBuilding },
                { "primary", GroupTypeId.PrimaryEnd },
                { "primaryend", GroupTypeId.PrimaryEnd },
                { "secondary", GroupTypeId.SecondaryEnd },
                { "secondaryend", GroupTypeId.SecondaryEnd },
                { "other", GroupTypeId.General },
                { "data", GroupTypeId.Data }
            };

            // Look up in mapping
            if (groupMapping.TryGetValue(normalizedName, out ForgeTypeId result))
            {
                return result;
            }

            // Try exact match from groupName
            foreach (var kvp in groupMapping)
            {
                if (groupName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            // Default to General
            return GroupTypeId.General;
        }

        /// <summary>
        /// Add a shared parameter to a project document (binds to categories)
        /// </summary>
        private object AddProjectSharedParameter(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Verify this is NOT a family document
                if (doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command is for project documents. For family documents, use add_family_shared_parameter." };
                }

                // Get the shared parameter file path
                if (!parameters.ContainsKey("shared_parameter_file") || parameters["shared_parameter_file"] == null)
                {
                    return new { success = false, error = "shared_parameter_file path is required" };
                }
                string sharedParamFilePath = parameters["shared_parameter_file"].ToString();

                // Get the parameter name to add
                if (!parameters.ContainsKey("parameter_name") || parameters["parameter_name"] == null)
                {
                    return new { success = false, error = "parameter_name is required" };
                }
                string parameterName = parameters["parameter_name"].ToString();

                // Get categories to bind to
                if (!parameters.ContainsKey("categories") || parameters["categories"] == null)
                {
                    return new { success = false, error = "categories array is required" };
                }

                var categoriesObj = parameters["categories"];
                List<string> categoryNames = new List<string>();
                if (categoriesObj is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        categoryNames.Add(item.ToString());
                    }
                }
                else if (categoriesObj is IEnumerable<object> catList)
                {
                    foreach (var cat in catList)
                    {
                        categoryNames.Add(cat.ToString());
                    }
                }

                if (categoryNames.Count == 0)
                {
                    return new { success = false, error = "At least one category is required" };
                }

                // Get the parameter group (optional, defaults to General)
                string paramGroupName = parameters.ContainsKey("parameter_group") && parameters["parameter_group"] != null
                    ? parameters["parameter_group"].ToString()
                    : "General";
                ForgeTypeId paramGroupId = GetParameterGroupTypeId(paramGroupName);

                // Get instance vs type parameter setting (optional, defaults to instance)
                bool isInstance = true;
                if (parameters.ContainsKey("is_instance") && parameters["is_instance"] != null)
                {
                    isInstance = Convert.ToBoolean(parameters["is_instance"]);
                }

                // Open the shared parameter file
                DefinitionFile defFile = null;
                try
                {
                    doc.Application.SharedParametersFilename = sharedParamFilePath;
                    defFile = doc.Application.OpenSharedParameterFile();
                }
                catch (Exception ex)
                {
                    return new { success = false, error = $"Failed to open shared parameter file: {ex.Message}" };
                }

                if (defFile == null)
                {
                    return new { success = false, error = $"Could not open shared parameter file at '{sharedParamFilePath}'" };
                }

                // Find the parameter definition in the shared parameter file
                ExternalDefinition externalDef = null;
                string foundInGroup = null;

                foreach (DefinitionGroup group in defFile.Groups)
                {
                    foreach (Definition def in group.Definitions)
                    {
                        if (def.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                        {
                            externalDef = def as ExternalDefinition;
                            foundInGroup = group.Name;
                            break;
                        }
                    }
                    if (externalDef != null) break;
                }

                if (externalDef == null)
                {
                    return new { success = false, error = $"Parameter '{parameterName}' not found in shared parameter file" };
                }

                // Build category set
                CategorySet categorySet = doc.Application.Create.NewCategorySet();
                var addedCategories = new List<string>();
                var failedCategories = new List<string>();

                foreach (string catName in categoryNames)
                {
                    // Try to find category by name
                    Category cat = null;
                    
                    // Try direct name match first
                    foreach (Category c in doc.Settings.Categories)
                    {
                        if (c.Name.Equals(catName, StringComparison.OrdinalIgnoreCase))
                        {
                            cat = c;
                            break;
                        }
                    }

                    // Try with OST_ prefix as BuiltInCategory
                    if (cat == null)
                    {
                        string ostName = catName.StartsWith("OST_") ? catName : "OST_" + catName;
                        if (Enum.TryParse<BuiltInCategory>(ostName, out BuiltInCategory bic))
                        {
                            cat = Category.GetCategory(doc, bic);
                        }
                    }

                    if (cat != null && cat.AllowsBoundParameters)
                    {
                        categorySet.Insert(cat);
                        addedCategories.Add(cat.Name);
                    }
                    else
                    {
                        failedCategories.Add(catName);
                    }
                }

                if (categorySet.Size == 0)
                {
                    return new { success = false, error = "No valid categories found that allow bound parameters", failed_categories = failedCategories };
                }

                // Create the binding
                ElementBinding binding;
                if (isInstance)
                {
                    binding = doc.Application.Create.NewInstanceBinding(categorySet);
                }
                else
                {
                    binding = doc.Application.Create.NewTypeBinding(categorySet);
                }

                using (Transaction trans = new Transaction(doc, "Add Project Shared Parameter"))
                {
                    trans.Start();

                    try
                    {
                        BindingMap bindingMap = doc.ParameterBindings;
                        
                        // Check if parameter already exists
                        Definition existingDef = null;
                        DefinitionBindingMapIterator iter = bindingMap.ForwardIterator();
                        while (iter.MoveNext())
                        {
                            if (iter.Key.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                            {
                                existingDef = iter.Key;
                                break;
                            }
                        }

                        bool result;
                        if (existingDef != null)
                        {
                            // Update existing binding
                            result = bindingMap.ReInsert(existingDef, binding, paramGroupId);
                        }
                        else
                        {
                            // Insert new binding
                            result = bindingMap.Insert(externalDef, binding, paramGroupId);
                        }

                        if (!result)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to bind parameter to categories" };
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            parameter_name = parameterName,
                            is_instance = isInstance,
                            parameter_group = LabelUtils.GetLabelForGroup(paramGroupId),
                            shared_param_group = foundInGroup,
                            bound_categories = addedCategories,
                            failed_categories = failedCategories,
                            was_update = existingDef != null
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to add parameter: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in AddProjectSharedParameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Remove a shared parameter from a project document
        /// </summary>
        private object RemoveProjectSharedParameter(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Verify this is NOT a family document
                if (doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command is for project documents. For family documents, use remove_family_parameter." };
                }

                // Get the parameter name to remove
                if (!parameters.ContainsKey("parameter_name") || parameters["parameter_name"] == null)
                {
                    return new { success = false, error = "parameter_name is required" };
                }
                string parameterName = parameters["parameter_name"].ToString();

                // Find the parameter in bindings
                BindingMap bindingMap = doc.ParameterBindings;
                Definition defToRemove = null;
                ElementBinding existingBinding = null;

                DefinitionBindingMapIterator iter = bindingMap.ForwardIterator();
                while (iter.MoveNext())
                {
                    if (iter.Key.Name.Equals(parameterName, StringComparison.OrdinalIgnoreCase))
                    {
                        defToRemove = iter.Key;
                        existingBinding = iter.Current as ElementBinding;
                        break;
                    }
                }

                if (defToRemove == null)
                {
                    return new { success = false, error = $"Parameter '{parameterName}' not found in project bindings" };
                }

                // Store info before removal
                string removedName = defToRemove.Name;
                bool wasInstance = existingBinding is InstanceBinding;
                var boundCategories = new List<string>();
                if (existingBinding != null)
                {
                    foreach (Category cat in existingBinding.Categories)
                    {
                        boundCategories.Add(cat.Name);
                    }
                }

                using (Transaction trans = new Transaction(doc, "Remove Project Shared Parameter"))
                {
                    trans.Start();

                    try
                    {
                        bool result = bindingMap.Remove(defToRemove);

                        if (!result)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Failed to remove parameter binding" };
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            removed_parameter = removedName,
                            was_instance = wasInstance,
                            was_bound_to_categories = boundCategories
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to remove parameter: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in RemoveProjectSharedParameter: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get all shared parameters bound in the project document
        /// </summary>
        private object GetProjectSharedParameters(Document doc)
        {
            try
            {
                // Verify this is NOT a family document
                if (doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This command is for project documents. For family documents, use get_family_parameters." };
                }

                var parameterList = new List<object>();
                BindingMap bindingMap = doc.ParameterBindings;

                DefinitionBindingMapIterator iter = bindingMap.ForwardIterator();
                while (iter.MoveNext())
                {
                    Definition def = iter.Key;
                    ElementBinding binding = iter.Current as ElementBinding;

                    var boundCategories = new List<string>();
                    if (binding != null)
                    {
                        foreach (Category cat in binding.Categories)
                        {
                            boundCategories.Add(cat.Name);
                        }
                    }

                    var paramInfo = new Dictionary<string, object>
                    {
                        { "name", def.Name },
                        { "is_instance", binding is InstanceBinding },
                        { "bound_categories", boundCategories },
                        { "category_count", boundCategories.Count }
                    };

                    // Try to get parameter group
                    try
                    {
                        var groupId = def.GetGroupTypeId();
                        paramInfo["parameter_group"] = LabelUtils.GetLabelForGroup(groupId);
                    }
                    catch
                    {
                        paramInfo["parameter_group"] = "Unknown";
                    }

                    // Check if it's an external (shared) definition
                    if (def is ExternalDefinition extDef)
                    {
                        paramInfo["is_shared"] = true;
                        paramInfo["guid"] = extDef.GUID.ToString();
                    }
                    else
                    {
                        paramInfo["is_shared"] = false;
                    }

                    parameterList.Add(paramInfo);
                }

                return new
                {
                    success = true,
                    project_name = doc.Title,
                    parameter_count = parameterList.Count,
                    parameters = parameterList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in GetProjectSharedParameters: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get the last placed element in the document (element with highest ElementId)
        /// </summary>
        private object GetLastPlacedElement(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get optional category filter
                string categoryFilter = null;
                if (parameters != null && parameters.ContainsKey("category") && parameters["category"] != null)
                {
                    categoryFilter = parameters["category"].ToString();
                }

                // Build collector
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                
                // Filter by category if specified
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    string ostName = categoryFilter.StartsWith("OST_") ? categoryFilter : "OST_" + categoryFilter;
                    if (Enum.TryParse<BuiltInCategory>(ostName, out BuiltInCategory bic))
                    {
                        collector = collector.OfCategory(bic);
                    }
                    else
                    {
                        // Try to find by name
                        Category cat = null;
                        foreach (Category c in doc.Settings.Categories)
                        {
                            if (c.Name.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase))
                            {
                                cat = c;
                                break;
                            }
                        }
                        if (cat != null)
                        {
                            collector = collector.OfCategoryId(cat.Id);
                        }
                        else
                        {
                            return new { success = false, error = $"Category '{categoryFilter}' not found" };
                        }
                    }
                }

                // Get only non-element-type elements (actual placed instances)
                collector = collector.WhereElementIsNotElementType();

                // Find the element with the highest ElementId (most recently created)
                Element lastElement = null;
                long highestId = long.MinValue;

                foreach (Element elem in collector)
                {
                    long elemIdValue = GetElementIdInt(elem.Id);
                    if (elemIdValue > highestId)
                    {
                        highestId = elemIdValue;
                        lastElement = elem;
                    }
                }

                if (lastElement == null)
                {
                    return new {
                        success = false,
                        error = categoryFilter != null 
                            ? $"No elements found in category '{categoryFilter}'" 
                            : "No elements found in document"
                    };
                }

                // Build result with element details
                var result = new Dictionary<string, object>
                {
                    { "success", true },
                    { "element_id", GetElementIdInt(lastElement.Id) },
                    { "name", GetElementName(lastElement) },
                    { "category", lastElement.Category?.Name },
                    { "type_name", GetElementName(doc.GetElement(lastElement.GetTypeId())) },
                    { "level", GetElementName(doc.GetElement(lastElement.LevelId)) }
                };

                // Try to get location
                try
                {
                    if (lastElement.Location is LocationPoint locPt)
                    {
                        result["location"] = new {
                            type = "point",
                            x = locPt.Point.X,
                            y = locPt.Point.Y,
                            z = locPt.Point.Z
                        };
                    }
                    else if (lastElement.Location is LocationCurve locCrv)
                    {
                        var startPt = locCrv.Curve.GetEndPoint(0);
                        var endPt = locCrv.Curve.GetEndPoint(1);
                        result["location"] = new {
                            type = "curve",
                            start = new { x = startPt.X, y = startPt.Y, z = startPt.Z },
                            end = new { x = endPt.X, y = endPt.Y, z = endPt.Z },
                            length = locCrv.Curve.Length
                        };
                    }
                }
                catch
                {
                    // Location not available
                }

                // Try to get bounding box
                try
                {
                    BoundingBoxXYZ bbox = lastElement.get_BoundingBox(null);
                    if (bbox != null)
                    {
                        result["bounding_box"] = new {
                            min = new { x = bbox.Min.X, y = bbox.Min.Y, z = bbox.Min.Z },
                            max = new { x = bbox.Max.X, y = bbox.Max.Y, z = bbox.Max.Z }
                        };
                    }
                }
                catch
                {
                    // Bounding box not available
                }

                // Add some key parameters
                try
                {
                    var keyParams = new Dictionary<string, object>();
                    foreach (Parameter param in lastElement.Parameters)
                    {
                        if (param.HasValue && !string.IsNullOrEmpty(param.Definition.Name))
                        {
                            string value = param.AsValueString() ?? param.AsString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                keyParams[param.Definition.Name] = value;
                            }
                        }
                    }
                    if (keyParams.Count > 0)
                    {
                        result["parameters"] = keyParams;
                    }
                }
                catch
                {
                    // Parameters not available
                }

                return result;
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in GetLastPlacedElement: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a reference plane in a specific view type
        /// </summary>
        private object CreateReferencePlane(Document doc, UIApplication app, Dictionary<string, object> parameters)
        {
            try
            {
                // Get the bubble end point (required)
                if (!HasPoint(parameters, "bubble"))
                {
                    return new { success = false, error = "bubble point (bubble_x, bubble_y, bubble_z) is required" };
                }
                XYZ bubbleEnd = GetPoint(parameters, "bubble");

                // Get the free end point (required)
                if (!HasPoint(parameters, "free"))
                {
                    return new { success = false, error = "free point (free_x, free_y, free_z) is required" };
                }
                XYZ freeEnd = GetPoint(parameters, "free");

                // Get the cut vector (direction perpendicular to the plane)
                XYZ cutVector;
                if (HasPoint(parameters, "cut_vector"))
                {
                    cutVector = GetPoint(parameters, "cut_vector").Normalize();
                }
                else
                {
                    // Default: calculate based on bubble-free direction, using Z as up
                    XYZ direction = (freeEnd - bubbleEnd).Normalize();
                    cutVector = direction.CrossProduct(XYZ.BasisZ);
                    if (cutVector.IsZeroLength())
                    {
                        cutVector = XYZ.BasisY;
                    }
                    cutVector = cutVector.Normalize();
                }

                // Get optional name
                string name = parameters.ContainsKey("name") && parameters["name"] != null
                    ? parameters["name"].ToString()
                    : null;

                // Get view type filter (optional)
                string viewTypeFilter = parameters.ContainsKey("view_type") && parameters["view_type"] != null
                    ? parameters["view_type"].ToString()
                    : null;

                // Find or use the appropriate view
                View targetView = null;

                if (parameters.ContainsKey("view_id") && parameters["view_id"] != null)
                {
                    // Use specific view by ID
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v && !v.IsTemplate)
                    {
                        targetView = v;
                    }
                    else
                    {
                        return new { success = false, error = $"View with ID {viewId} not found or is a template" };
                    }
                }
                else if (!string.IsNullOrEmpty(viewTypeFilter))
                {
                    // Find a view of the specified type
                    ViewType? targetViewType = null;

                    switch (viewTypeFilter.ToLower())
                    {
                        case "section":
                        case "sections":
                            targetViewType = ViewType.Section;
                            break;
                        case "floorplan":
                        case "floor plan":
                        case "floor":
                            targetViewType = ViewType.FloorPlan;
                            break;
                        case "ceilingplan":
                        case "ceiling plan":
                        case "ceiling":
                        case "ceilings":
                            targetViewType = ViewType.CeilingPlan;
                            break;
                        case "structuralplan":
                        case "structural plan":
                        case "structural":
                            targetViewType = ViewType.EngineeringPlan;
                            break;
                        case "elevation":
                        case "elevations":
                            targetViewType = ViewType.Elevation;
                            break;
                        case "drafting":
                        case "draftingview":
                            targetViewType = ViewType.DraftingView;
                            break;
                        case "detail":
                        case "detailview":
                            targetViewType = ViewType.Detail;
                            break;
                        case "3d":
                        case "threed":
                        case "three dimensional":
                            targetViewType = ViewType.ThreeD;
                            break;
                        default:
                            return new { success = false, error = $"Unknown view type: {viewTypeFilter}. Use: Section, FloorPlan, CeilingPlan, StructuralPlan, Elevation, Drafting, Detail, 3D" };
                    }

                    if (targetViewType.HasValue)
                    {
                        // First try active view if it matches
                        View activeView = doc.ActiveView;
                        if (activeView != null && !activeView.IsTemplate && activeView.ViewType == targetViewType.Value)
                        {
                            targetView = activeView;
                        }
                        else
                        {
                            // Find first matching view
                            var views = new FilteredElementCollector(doc)
                                .OfClass(typeof(View))
                                .Cast<View>()
                                .Where(v => !v.IsTemplate && v.ViewType == targetViewType.Value)
                                .ToList();

                            if (views.Count > 0)
                            {
                                targetView = views[0];
                            }
                            else
                            {
                                return new { success = false, error = $"No {viewTypeFilter} view found in the document" };
                            }
                        }
                    }
                }
                else
                {
                    // Use active view
                    targetView = doc.ActiveView;
                    if (targetView == null || targetView.IsTemplate)
                    {
                        return new { success = false, error = "No valid active view available" };
                    }
                }

                // Check if the view type supports reference planes
                var supportedTypes = new[] {
                    ViewType.FloorPlan, ViewType.CeilingPlan, ViewType.EngineeringPlan,
                    ViewType.Section, ViewType.Elevation, ViewType.Detail,
                    ViewType.DraftingView, ViewType.ThreeD
                };

                if (!supportedTypes.Contains(targetView.ViewType))
                {
                    return new {
                        success = false,
                        error = $"Reference planes cannot be created in {targetView.ViewType} views",
                        view_type = targetView.ViewType.ToString()
                    };
                }

                using (Transaction trans = new Transaction(doc, "Create Reference Plane"))
                {
                    trans.Start();

                    try
                    {
                        ReferencePlane refPlane = doc.Create.NewReferencePlane(bubbleEnd, freeEnd, cutVector, targetView);

                        if (!string.IsNullOrEmpty(name))
                        {
                            refPlane.Name = name;
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            reference_plane_id = GetElementIdInt(refPlane.Id),
                            name = refPlane.Name,
                            view_id = GetElementIdInt(targetView.Id),
                            view_name = targetView.Name,
                            view_type = targetView.ViewType.ToString(),
                            bubble_end = new { x = bubbleEnd.X, y = bubbleEnd.Y, z = bubbleEnd.Z },
                            free_end = new { x = freeEnd.X, y = freeEnd.Y, z = freeEnd.Z },
                            cut_vector = new { x = cutVector.X, y = cutVector.Y, z = cutVector.Z }
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to create reference plane: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in CreateReferencePlane: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get reference planes from the model or from a specific element
        /// </summary>
        private object GetReferencePlanes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get optional filters
                string nameFilter = parameters != null && parameters.ContainsKey("name") && parameters["name"] != null
                    ? parameters["name"].ToString()
                    : null;

                int? elementId = null;
                if (parameters != null && parameters.ContainsKey("element_id") && parameters["element_id"] != null)
                {
                    elementId = Convert.ToInt32(parameters["element_id"]);
                }

                bool includeNonNamed = true;
                if (parameters != null && parameters.ContainsKey("include_unnamed") && parameters["include_unnamed"] != null)
                {
                    includeNonNamed = Convert.ToBoolean(parameters["include_unnamed"]);
                }

                var referencePlanes = new List<object>();

                if (elementId.HasValue)
                {
                    // Get reference planes associated with a specific element
                    Element element = doc.GetElement(new ElementId(elementId.Value));
                    if (element == null)
                    {
                        return new { success = false, error = $"Element with ID {elementId.Value} not found" };
                    }

                    // For family instances, try to get reference planes from the family
                    if (element is FamilyInstance fi)
                    {
                        // Get references from the family instance
                        var refList = new List<object>();
                        
                        // Get named references from family
                        try
                        {
                            // Common reference plane names in families
                            string[] commonRefNames = { "Center (Left/Right)", "Center (Front/Back)", "Reference Plane", "Center", "Left", "Right", "Front", "Back", "Top", "Bottom" };
                            
                            foreach (string refName in commonRefNames)
                            {
                                try
                                {
                                    Reference r = fi.GetReferenceByName(refName);
                                    if (r != null)
                                    {
                                        refList.Add(new {
                                            name = refName,
                                            reference_type = "family_reference",
                                            stable_representation = r.ConvertToStableRepresentation(doc)
                                        });
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }

                        return new
                        {
                            success = true,
                            element_id = elementId.Value,
                            element_name = GetElementName(element),
                            reference_count = refList.Count,
                            references = refList
                        };
                    }
                    else
                    {
                        return new { success = false, error = "Element is not a FamilyInstance. Use without element_id to get all reference planes in the document." };
                    }
                }
                else
                {
                    // Get all reference planes in the document
                    FilteredElementCollector collector = new FilteredElementCollector(doc)
                        .OfClass(typeof(ReferencePlane));

                    foreach (ReferencePlane rp in collector)
                    {
                        // Apply name filter if specified
                        if (!string.IsNullOrEmpty(nameFilter))
                        {
                            if (!rp.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }

                        // Skip unnamed reference planes if requested
                        if (!includeNonNamed && string.IsNullOrWhiteSpace(rp.Name))
                        {
                            continue;
                        }

                        var rpInfo = new Dictionary<string, object>
                        {
                            { "id", GetElementIdInt(rp.Id) },
                            { "name", rp.Name },
                            { "is_named", !string.IsNullOrWhiteSpace(rp.Name) }
                        };

                        // Get bubble and free end points
                        try
                        {
                            XYZ bubbleEnd = rp.BubbleEnd;
                            XYZ freeEnd = rp.FreeEnd;
                            XYZ direction = rp.Direction;
                            XYZ normal = rp.Normal;

                            rpInfo["bubble_end"] = new { x = bubbleEnd.X, y = bubbleEnd.Y, z = bubbleEnd.Z };
                            rpInfo["free_end"] = new { x = freeEnd.X, y = freeEnd.Y, z = freeEnd.Z };
                            rpInfo["direction"] = new { x = direction.X, y = direction.Y, z = direction.Z };
                            rpInfo["normal"] = new { x = normal.X, y = normal.Y, z = normal.Z };
                        }
                        catch { }

                        // Get reference for dimensioning
                        try
                        {
                            Reference reference = rp.GetReference();
                            if (reference != null)
                            {
                                rpInfo["has_reference"] = true;
                                rpInfo["stable_representation"] = reference.ConvertToStableRepresentation(doc);
                            }
                        }
                        catch
                        {
                            rpInfo["has_reference"] = false;
                        }

                        referencePlanes.Add(rpInfo);
                    }
                }

                return new
                {
                    success = true,
                    count = referencePlanes.Count,
                    reference_planes = referencePlanes
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in GetReferencePlanes: {ex.Message}" };
            }
        }

        /// <summary>
        /// Set graphic overrides (halftone, colors, patterns) by category or by element
        /// </summary>
        private object SetGraphicOverrides(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get the view to apply overrides (optional, defaults to active view)
                View targetView = null;
                if (parameters.ContainsKey("view_id") && parameters["view_id"] != null)
                {
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v && !v.IsTemplate)
                    {
                        targetView = v;
                    }
                    else
                    {
                        return new { success = false, error = $"View with ID {viewId} not found or is a template" };
                    }
                }
                else
                {
                    targetView = doc.ActiveView;
                    if (targetView == null || targetView.IsTemplate)
                    {
                        return new { success = false, error = "No valid active view available" };
                    }
                }

                // Check if we're setting by category or by element
                bool byCategory = parameters.ContainsKey("category") && parameters["category"] != null;
                bool byElement = parameters.ContainsKey("element_id") && parameters["element_id"] != null;
                bool byElementIds = parameters.ContainsKey("element_ids") && parameters["element_ids"] != null;

                if (!byCategory && !byElement && !byElementIds)
                {
                    return new { success = false, error = "Either 'category', 'element_id', or 'element_ids' is required" };
                }

                // Build the override settings
                OverrideGraphicSettings ogs = new OverrideGraphicSettings();
                bool hasOverrides = false;

                // Halftone
                if (parameters.ContainsKey("halftone") && parameters["halftone"] != null)
                {
                    ogs.SetHalftone(Convert.ToBoolean(parameters["halftone"]));
                    hasOverrides = true;
                }

                // Transparency (0-100)
                if (parameters.ContainsKey("transparency") && parameters["transparency"] != null)
                {
                    int transparency = Math.Max(0, Math.Min(100, Convert.ToInt32(parameters["transparency"])));
                    ogs.SetSurfaceTransparency(transparency);
                    hasOverrides = true;
                }

                // Visibility
                if (parameters.ContainsKey("visible") && parameters["visible"] != null)
                {
                    // Note: Visibility is handled separately via category/element visibility, not OverrideGraphicSettings
                }

                // Projection line color
                if (parameters.ContainsKey("projection_line_color") && parameters["projection_line_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["projection_line_color"]);
                    if (color != null)
                    {
                        ogs.SetProjectionLineColor(color);
                        hasOverrides = true;
                    }
                }

                // Projection line weight
                if (parameters.ContainsKey("projection_line_weight") && parameters["projection_line_weight"] != null)
                {
                    int weight = Convert.ToInt32(parameters["projection_line_weight"]);
                    ogs.SetProjectionLineWeight(weight);
                    hasOverrides = true;
                }

                // Cut line color
                if (parameters.ContainsKey("cut_line_color") && parameters["cut_line_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["cut_line_color"]);
                    if (color != null)
                    {
                        ogs.SetCutLineColor(color);
                        hasOverrides = true;
                    }
                }

                // Cut line weight
                if (parameters.ContainsKey("cut_line_weight") && parameters["cut_line_weight"] != null)
                {
                    int weight = Convert.ToInt32(parameters["cut_line_weight"]);
                    ogs.SetCutLineWeight(weight);
                    hasOverrides = true;
                }

                // Surface foreground pattern color
                if (parameters.ContainsKey("surface_foreground_color") && parameters["surface_foreground_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["surface_foreground_color"]);
                    if (color != null)
                    {
                        ogs.SetSurfaceForegroundPatternColor(color);
                        hasOverrides = true;
                    }
                }

                // Surface background pattern color
                if (parameters.ContainsKey("surface_background_color") && parameters["surface_background_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["surface_background_color"]);
                    if (color != null)
                    {
                        ogs.SetSurfaceBackgroundPatternColor(color);
                        hasOverrides = true;
                    }
                }

                // Cut foreground pattern color
                if (parameters.ContainsKey("cut_foreground_color") && parameters["cut_foreground_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["cut_foreground_color"]);
                    if (color != null)
                    {
                        ogs.SetCutForegroundPatternColor(color);
                        hasOverrides = true;
                    }
                }

                // Cut background pattern color
                if (parameters.ContainsKey("cut_background_color") && parameters["cut_background_color"] != null)
                {
                    Autodesk.Revit.DB.Color color = ParseColor(parameters["cut_background_color"]);
                    if (color != null)
                    {
                        ogs.SetCutBackgroundPatternColor(color);
                        hasOverrides = true;
                    }
                }

                // Detail level
                if (parameters.ContainsKey("detail_level") && parameters["detail_level"] != null)
                {
                    string detailLevelStr = parameters["detail_level"].ToString().ToLower();
                    ViewDetailLevel detailLevel = ViewDetailLevel.Undefined;
                    switch (detailLevelStr)
                    {
                        case "coarse":
                            detailLevel = ViewDetailLevel.Coarse;
                            break;
                        case "medium":
                            detailLevel = ViewDetailLevel.Medium;
                            break;
                        case "fine":
                            detailLevel = ViewDetailLevel.Fine;
                            break;
                    }
                    if (detailLevel != ViewDetailLevel.Undefined)
                    {
                        ogs.SetDetailLevel(detailLevel);
                        hasOverrides = true;
                    }
                }

                if (!hasOverrides && !parameters.ContainsKey("visible") && !parameters.ContainsKey("reset"))
                {
                    return new { success = false, error = "No override settings specified" };
                }

                // Check if we should reset overrides
                bool resetOverrides = parameters.ContainsKey("reset") && Convert.ToBoolean(parameters["reset"]);
                if (resetOverrides)
                {
                    ogs = new OverrideGraphicSettings(); // Empty = reset to default
                }

                using (Transaction trans = new Transaction(doc, "Set Graphic Overrides"))
                {
                    trans.Start();

                    try
                    {
                        var appliedTo = new List<object>();

                        if (byCategory)
                        {
                            // Apply to category
                            string categoryName = parameters["category"].ToString();
                            Category cat = null;

                            // Try to find category by name
                            foreach (Category c in doc.Settings.Categories)
                            {
                                if (c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                                {
                                    cat = c;
                                    break;
                                }
                            }

                            // Try with OST_ prefix
                            if (cat == null)
                            {
                                string ostName = categoryName.StartsWith("OST_") ? categoryName : "OST_" + categoryName;
                                if (Enum.TryParse<BuiltInCategory>(ostName, out BuiltInCategory bic))
                                {
                                    cat = Category.GetCategory(doc, bic);
                                }
                            }

                            if (cat == null)
                            {
                                trans.RollBack();
                                return new { success = false, error = $"Category '{categoryName}' not found" };
                            }

                            // Handle visibility separately for categories
                            if (parameters.ContainsKey("visible") && parameters["visible"] != null)
                            {
                                bool visible = Convert.ToBoolean(parameters["visible"]);
                                targetView.SetCategoryHidden(cat.Id, !visible);
                            }

                            if (hasOverrides || resetOverrides)
                            {
                                targetView.SetCategoryOverrides(cat.Id, ogs);
                            }

                            appliedTo.Add(new { type = "category", name = cat.Name, id = GetElementIdInt(cat.Id) });
                        }

                        if (byElement || byElementIds)
                        {
                            // Get element IDs
                            List<ElementId> elementIds = new List<ElementId>();

                            if (byElement)
                            {
                                elementIds.Add(new ElementId(Convert.ToInt32(parameters["element_id"])));
                            }

                            if (byElementIds)
                            {
                                var idsObj = parameters["element_ids"];
                                if (idsObj is JArray jArray)
                                {
                                    foreach (var item in jArray)
                                    {
                                        elementIds.Add(new ElementId(Convert.ToInt32(item)));
                                    }
                                }
                                else if (idsObj is IEnumerable<object> idList)
                                {
                                    foreach (var id in idList)
                                    {
                                        elementIds.Add(new ElementId(Convert.ToInt32(id)));
                                    }
                                }
                            }

                            foreach (var elemId in elementIds)
                            {
                                Element elem = doc.GetElement(elemId);
                                if (elem != null)
                                {
                                    // Handle visibility separately for elements
                                    if (parameters.ContainsKey("visible") && parameters["visible"] != null)
                                    {
                                        bool visible = Convert.ToBoolean(parameters["visible"]);
                                        if (visible)
                                        {
                                            targetView.UnhideElements(new List<ElementId> { elemId });
                                        }
                                        else
                                        {
                                            targetView.HideElements(new List<ElementId> { elemId });
                                        }
                                    }

                                    if (hasOverrides || resetOverrides)
                                    {
                                        targetView.SetElementOverrides(elemId, ogs);
                                    }

                                    appliedTo.Add(new {
                                        type = "element",
                                        id = GetElementIdInt(elemId),
                                        name = GetElementName(elem),
                                        category = elem.Category?.Name
                                    });
                                }
                            }
                        }

                        trans.Commit();

                        return new
                        {
                            success = true,
                            view_id = GetElementIdInt(targetView.Id),
                            view_name = targetView.Name,
                            applied_to = appliedTo,
                            settings = new
                            {
                                halftone = parameters.ContainsKey("halftone") ? parameters["halftone"] : null,
                                transparency = parameters.ContainsKey("transparency") ? parameters["transparency"] : null,
                                visible = parameters.ContainsKey("visible") ? parameters["visible"] : null,
                                reset = resetOverrides
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Failed to set graphic overrides: {ex.Message}" };
                    }
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error in SetGraphicOverrides: {ex.Message}" };
            }
        }

        /// <summary>
        /// Parse a color from various formats (hex string, RGB object, or array)
        /// </summary>
        private Autodesk.Revit.DB.Color ParseColor(object colorObj)
        {
            try
            {
                if (colorObj is string colorStr)
                {
                    // Parse hex color like "#FF0000" or "FF0000"
                    colorStr = colorStr.TrimStart('#');
                    if (colorStr.Length == 6)
                    {
                        byte r = Convert.ToByte(colorStr.Substring(0, 2), 16);
                        byte g = Convert.ToByte(colorStr.Substring(2, 2), 16);
                        byte b = Convert.ToByte(colorStr.Substring(4, 2), 16);
                        return new Autodesk.Revit.DB.Color(r, g, b);
                    }
                }
                else if (colorObj is JObject jObj)
                {
                    byte r = jObj["r"]?.ToObject<byte>() ?? 0;
                    byte g = jObj["g"]?.ToObject<byte>() ?? 0;
                    byte b = jObj["b"]?.ToObject<byte>() ?? 0;
                    return new Autodesk.Revit.DB.Color(r, g, b);
                }
                else if (colorObj is JArray jArr && jArr.Count >= 3)
                {
                    byte r = jArr[0].ToObject<byte>();
                    byte g = jArr[1].ToObject<byte>();
                    byte b = jArr[2].ToObject<byte>();
                    return new Autodesk.Revit.DB.Color(r, g, b);
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Create a schedule view for a specific category
        /// </summary>
        private object CreateScheduleView(Document doc, Dictionary<string, object> parameters)
        {
            string categoryName = parameters.ContainsKey("category") ? parameters["category"]?.ToString() : null;
            string scheduleName = parameters.ContainsKey("name") ? parameters["name"]?.ToString() : null;
            bool isKeySchedule = parameters.ContainsKey("is_key_schedule") && Convert.ToBoolean(parameters["is_key_schedule"]);
            string groupByField = parameters.ContainsKey("group_by") ? parameters["group_by"]?.ToString() : null;
            bool itemizeEveryInstance = !parameters.ContainsKey("itemize_every_instance") || Convert.ToBoolean(parameters["itemize_every_instance"]);

            // Parse field names list
            List<string> fieldNames = new List<string>();
            if (parameters.ContainsKey("fields") && parameters["fields"] != null)
            {
                var fieldsObj = parameters["fields"];
                if (fieldsObj is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        fieldNames.Add(item.ToString());
                    }
                }
                else if (fieldsObj is IEnumerable<object> fieldList)
                {
                    foreach (var field in fieldList)
                    {
                        fieldNames.Add(field.ToString());
                    }
                }
            }

            if (string.IsNullOrEmpty(categoryName))
            {
                return new { success = false, error = "Category name is required" };
            }

            // Find the category
            Category targetCategory = null;
            foreach (Category cat in doc.Settings.Categories)
            {
                if (cat.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase))
                {
                    targetCategory = cat;
                    break;
                }
            }

            if (targetCategory == null)
            {
                return new { success = false, error = $"Category '{categoryName}' not found" };
            }

            // Default schedule name if not provided
            if (string.IsNullOrEmpty(scheduleName))
            {
                scheduleName = $"{categoryName} Schedule";
            }

            using (Transaction trans = new Transaction(doc, "Create Schedule View"))
            {
                trans.Start();
                try
                {
                    ViewSchedule schedule;
                    if (isKeySchedule)
                    {
                        schedule = ViewSchedule.CreateKeySchedule(doc, targetCategory.Id);
                    }
                    else
                    {
                        schedule = ViewSchedule.CreateSchedule(doc, targetCategory.Id);
                    }

                    schedule.Name = scheduleName;

                    ScheduleDefinition definition = schedule.Definition;

                    // Set itemize every instance
                    definition.IsItemized = itemizeEveryInstance;

                    // Get available schedulable fields
                    var schedulableFields = definition.GetSchedulableFields();
                    var availableFieldNames = new List<string>();
                    var addedFields = new List<object>();

                    foreach (var sf in schedulableFields)
                    {
                        string fieldName = sf.GetName(doc);
                        availableFieldNames.Add(fieldName);
                    }

                    // Add specified fields or default fields
                    if (fieldNames != null && fieldNames.Count > 0)
                    {
                        foreach (string requestedField in fieldNames)
                        {
                            foreach (var sf in schedulableFields)
                            {
                                string fieldName = sf.GetName(doc);
                                if (fieldName.Equals(requestedField, StringComparison.OrdinalIgnoreCase))
                                {
                                    ScheduleField addedField = definition.AddField(sf);
                                    addedFields.Add(new
                                    {
                                        name = fieldName,
                                        fieldId = addedField.FieldId.ToString()
                                    });
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Add some common default fields if no fields specified
                        int fieldsAdded = 0;
                        string[] defaultFields = { "Family and Type", "Type", "Family", "Count", "Level", "Mark", "Comments" };
                        foreach (string defaultField in defaultFields)
                        {
                            foreach (var sf in schedulableFields)
                            {
                                string fieldName = sf.GetName(doc);
                                if (fieldName.Equals(defaultField, StringComparison.OrdinalIgnoreCase))
                                {
                                    ScheduleField addedField = definition.AddField(sf);
                                    addedFields.Add(new
                                    {
                                        name = fieldName,
                                        fieldId = addedField.FieldId.ToString()
                                    });
                                    fieldsAdded++;
                                    break;
                                }
                            }
                            if (fieldsAdded >= 5) break;
                        }
                    }

                    // Set grouping if specified
                    if (!string.IsNullOrEmpty(groupByField))
                    {
                        // Find the field index in the schedule
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(groupByField, StringComparison.OrdinalIgnoreCase))
                            {
                                // Create a sort/group field
                                ScheduleSortGroupField sortGroup = new ScheduleSortGroupField(field.FieldId);
                                sortGroup.ShowHeader = true;
                                sortGroup.ShowFooter = false;
                                definition.AddSortGroupField(sortGroup);
                                break;
                            }
                        }
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        schedule = new
                        {
                            id = GetElementIdInt(schedule.Id),
                            name = schedule.Name,
                            category = categoryName,
                            isKeySchedule = isKeySchedule,
                            itemized = definition.IsItemized,
                            fieldsAdded = addedFields,
                            availableFields = availableFieldNames
                        }
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = ex.Message };
                }
            }
        }

        /// <summary>
        /// Get table data from a schedule view
        /// </summary>
        private object GetTableData(Document doc, Dictionary<string, object> parameters)
        {
            int? scheduleId = parameters.ContainsKey("schedule_id") && parameters["schedule_id"] != null
                ? Convert.ToInt32(parameters["schedule_id"])
                : (int?)null;
            string scheduleName = parameters.ContainsKey("schedule_name") ? parameters["schedule_name"]?.ToString() : null;
            bool includeHeaders = !parameters.ContainsKey("include_headers") || Convert.ToBoolean(parameters["include_headers"]);
            bool includeHiddenFields = parameters.ContainsKey("include_hidden_fields") && Convert.ToBoolean(parameters["include_hidden_fields"]);
            int? maxRows = parameters.ContainsKey("max_rows") && parameters["max_rows"] != null
                ? Convert.ToInt32(parameters["max_rows"])
                : (int?)null;

            ViewSchedule schedule = null;

            // Find schedule by ID
            if (scheduleId.HasValue)
            {
                Element elem = doc.GetElement(new ElementId(scheduleId.Value));
                schedule = elem as ViewSchedule;
                if (schedule == null)
                {
                    return new { success = false, error = $"Element with ID {scheduleId} is not a schedule view" };
                }
            }
            // Find schedule by name
            else if (!string.IsNullOrEmpty(scheduleName))
            {
                var schedules = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .Cast<ViewSchedule>()
                    .Where(s => s.Name.Equals(scheduleName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (schedules.Count == 0)
                {
                    return new { success = false, error = $"Schedule '{scheduleName}' not found" };
                }
                schedule = schedules.First();
            }
            else
            {
                return new { success = false, error = "Either schedule_id or schedule_name is required" };
            }

            try
            {
                ScheduleDefinition definition = schedule.Definition;
                TableSectionData bodyData = schedule.GetTableData().GetSectionData(SectionType.Body);
                TableSectionData headerData = schedule.GetTableData().GetSectionData(SectionType.Header);

                int rowCount = bodyData.NumberOfRows;
                int colCount = bodyData.NumberOfColumns;

                // Get column info
                var columns = new List<object>();
                for (int col = 0; col < definition.GetFieldCount(); col++)
                {
                    ScheduleField field = definition.GetField(col);
                    bool isHidden = field.IsHidden;
                    
                    if (isHidden && !includeHiddenFields)
                        continue;

                    columns.Add(new
                    {
                        index = col,
                        name = field.GetName(),
                        columnHeading = field.ColumnHeading,
                        isHidden = isHidden,
                        fieldType = field.FieldType.ToString()
                    });
                }

                // Get headers if requested
                var headers = new List<string>();
                if (includeHeaders && headerData != null && headerData.NumberOfRows > 0)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        try
                        {
                            string headerText = schedule.GetCellText(SectionType.Header, 0, col);
                            headers.Add(headerText);
                        }
                        catch
                        {
                            headers.Add($"Column {col}");
                        }
                    }
                }

                // Get row data
                var rows = new List<List<string>>();
                int rowsToProcess = maxRows.HasValue ? Math.Min(maxRows.Value, rowCount) : rowCount;

                for (int row = 0; row < rowsToProcess; row++)
                {
                    var rowData = new List<string>();
                    for (int col = 0; col < colCount; col++)
                    {
                        try
                        {
                            string cellText = schedule.GetCellText(SectionType.Body, row, col);
                            rowData.Add(cellText);
                        }
                        catch
                        {
                            rowData.Add("");
                        }
                    }
                    rows.Add(rowData);
                }

                return new
                {
                    success = true,
                    schedule = new
                    {
                        id = GetElementIdInt(schedule.Id),
                        name = schedule.Name,
                        totalRows = rowCount,
                        totalColumns = colCount,
                        columns = columns,
                        headers = headers,
                        rows = rows,
                        rowsReturned = rows.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Modify schedule settings including filters, itemization, and sorting
        /// </summary>
        private object ModifySchedule(Document doc, Dictionary<string, object> parameters)
        {
            int? scheduleId = parameters.ContainsKey("schedule_id") && parameters["schedule_id"] != null
                ? Convert.ToInt32(parameters["schedule_id"])
                : (int?)null;
            string scheduleName = parameters.ContainsKey("schedule_name") ? parameters["schedule_name"]?.ToString() : null;

            ViewSchedule schedule = null;

            // Find schedule by ID
            if (scheduleId.HasValue)
            {
                Element elem = doc.GetElement(new ElementId(scheduleId.Value));
                schedule = elem as ViewSchedule;
                if (schedule == null)
                {
                    return new { success = false, error = $"Element with ID {scheduleId} is not a schedule view" };
                }
            }
            // Find schedule by name
            else if (!string.IsNullOrEmpty(scheduleName))
            {
                var schedules = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewSchedule))
                    .Cast<ViewSchedule>()
                    .Where(s => s.Name.Equals(scheduleName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (schedules.Count == 0)
                {
                    return new { success = false, error = $"Schedule '{scheduleName}' not found" };
                }
                schedule = schedules.First();
            }
            else
            {
                return new { success = false, error = "Either schedule_id or schedule_name is required" };
            }

            var modifications = new List<string>();

            using (Transaction trans = new Transaction(doc, "Modify Schedule"))
            {
                trans.Start();
                try
                {
                    ScheduleDefinition definition = schedule.Definition;

                    // Handle itemization setting
                    if (parameters.ContainsKey("itemize_every_instance") && parameters["itemize_every_instance"] != null)
                    {
                        bool itemize = Convert.ToBoolean(parameters["itemize_every_instance"]);
                        definition.IsItemized = itemize;
                        modifications.Add($"Set itemize every instance to {itemize}");
                    }

                    // Clear existing filters if requested
                    if (parameters.ContainsKey("clear_filters") && Convert.ToBoolean(parameters["clear_filters"]))
                    {
                        int filterCount = definition.GetFilterCount();
                        for (int i = filterCount - 1; i >= 0; i--)
                        {
                            definition.RemoveFilter(i);
                        }
                        modifications.Add($"Cleared {filterCount} existing filters");
                    }

                    // Add new filter
                    if (parameters.ContainsKey("add_filter") && parameters["add_filter"] != null)
                    {
                        var filterObj = parameters["add_filter"];
                        JObject filterParams = filterObj is JObject jobj ? jobj : JObject.FromObject(filterObj);

                        string fieldName = filterParams["field_name"]?.ToString();
                        string filterTypeStr = filterParams["filter_type"]?.ToString();
                        string filterValue = filterParams["value"]?.ToString();

                        if (string.IsNullOrEmpty(fieldName))
                        {
                            trans.RollBack();
                            return new { success = false, error = "field_name is required for add_filter" };
                        }

                        // Find the field
                        ScheduleFieldId targetFieldId = null;
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetFieldId = field.FieldId;
                                break;
                            }
                        }

                        if (targetFieldId == null)
                        {
                            // Try schedulable fields
                            foreach (var sf in definition.GetSchedulableFields())
                            {
                                if (sf.GetName(doc).Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Add the field first
                                    ScheduleField addedField = definition.AddField(sf);
                                    targetFieldId = addedField.FieldId;
                                    modifications.Add($"Added field '{fieldName}' to schedule");
                                    break;
                                }
                            }
                        }

                        if (targetFieldId == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Field '{fieldName}' not found in schedule" };
                        }

                        // Parse filter type
                        ScheduleFilterType filterType = ScheduleFilterType.Equal;
                        if (!string.IsNullOrEmpty(filterTypeStr))
                        {
                            switch (filterTypeStr.ToLower())
                            {
                                case "equal": filterType = ScheduleFilterType.Equal; break;
                                case "notequal": filterType = ScheduleFilterType.NotEqual; break;
                                case "greaterthan": filterType = ScheduleFilterType.GreaterThan; break;
                                case "greaterthanorequal": filterType = ScheduleFilterType.GreaterThanOrEqual; break;
                                case "lessthan": filterType = ScheduleFilterType.LessThan; break;
                                case "lessthanorequal": filterType = ScheduleFilterType.LessThanOrEqual; break;
                                case "contains": filterType = ScheduleFilterType.Contains; break;
                                case "notcontains": filterType = ScheduleFilterType.NotContains; break;
                                case "beginswith": filterType = ScheduleFilterType.BeginsWith; break;
                                case "endswith": filterType = ScheduleFilterType.EndsWith; break;
                                case "hasvalue": filterType = ScheduleFilterType.HasValue; break;
                                case "hasnovalue": filterType = ScheduleFilterType.HasNoValue; break;
                            }
                        }

                        // Create and add filter
                        ScheduleFilter newFilter = new ScheduleFilter(targetFieldId, filterType, filterValue ?? "");
                        definition.AddFilter(newFilter);
                        modifications.Add($"Added filter: {fieldName} {filterTypeStr} '{filterValue}'");
                    }

                    // Clear existing sort/group fields if requested
                    if (parameters.ContainsKey("clear_sort_groups") && Convert.ToBoolean(parameters["clear_sort_groups"]))
                    {
                        int sortCount = definition.GetSortGroupFieldCount();
                        for (int i = sortCount - 1; i >= 0; i--)
                        {
                            definition.RemoveSortGroupField(i);
                        }
                        modifications.Add($"Cleared {sortCount} existing sort/group fields");
                    }

                    // Add sort/group field
                    if (parameters.ContainsKey("add_sort_group") && parameters["add_sort_group"] != null)
                    {
                        var sortObj = parameters["add_sort_group"];
                        JObject sortParams = sortObj is JObject jobj ? jobj : JObject.FromObject(sortObj);

                        string fieldName = sortParams["field_name"]?.ToString();
                        bool ascending = sortParams["ascending"]?.ToObject<bool>() ?? true;
                        bool showHeader = sortParams["show_header"]?.ToObject<bool>() ?? true;
                        bool showFooter = sortParams["show_footer"]?.ToObject<bool>() ?? false;
                        bool showCount = sortParams["show_count"]?.ToObject<bool>() ?? true;

                        if (string.IsNullOrEmpty(fieldName))
                        {
                            trans.RollBack();
                            return new { success = false, error = "field_name is required for add_sort_group" };
                        }

                        // Find the field
                        ScheduleFieldId targetFieldId = null;
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetFieldId = field.FieldId;
                                break;
                            }
                        }

                        if (targetFieldId == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Field '{fieldName}' not found in schedule. Add the field first." };
                        }

                        ScheduleSortGroupField sortGroup = new ScheduleSortGroupField(targetFieldId);
                        sortGroup.SortOrder = ascending ? ScheduleSortOrder.Ascending : ScheduleSortOrder.Descending;
                        sortGroup.ShowHeader = showHeader;
                        sortGroup.ShowFooter = showFooter;
                        sortGroup.ShowBlankLine = false;
                        if (showCount)
                        {
                            sortGroup.ShowFooter = true; // Count typically shown in footer
                        }
                        definition.AddSortGroupField(sortGroup);
                        modifications.Add($"Added sort/group by '{fieldName}' ({(ascending ? "ascending" : "descending")})");
                    }

                    // Remove specific sort/group field by name
                    if (parameters.ContainsKey("remove_sort_group") && parameters["remove_sort_group"] != null)
                    {
                        string fieldNameToRemove = parameters["remove_sort_group"].ToString();
                        for (int i = definition.GetSortGroupFieldCount() - 1; i >= 0; i--)
                        {
                            ScheduleSortGroupField sg = definition.GetSortGroupField(i);
                            ScheduleField sgField = definition.GetField(definition.GetFieldIndex(sg.FieldId));
                            if (sgField.GetName().Equals(fieldNameToRemove, StringComparison.OrdinalIgnoreCase))
                            {
                                definition.RemoveSortGroupField(i);
                                modifications.Add($"Removed sort/group field '{fieldNameToRemove}'");
                                break;
                            }
                        }
                    }

                    // Format field (alignment, width, heading, visibility, totals)
                    if (parameters.ContainsKey("format_field") && parameters["format_field"] != null)
                    {
                        var formatObj = parameters["format_field"];
                        JObject formatParams = formatObj is JObject jobj ? jobj : JObject.FromObject(formatObj);

                        string fieldName = formatParams["field_name"]?.ToString();
                        if (string.IsNullOrEmpty(fieldName))
                        {
                            trans.RollBack();
                            return new { success = false, error = "field_name is required for format_field" };
                        }

                        // Find the field
                        ScheduleField targetField = null;
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetField = field;
                                break;
                            }
                        }

                        if (targetField == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Field '{fieldName}' not found in schedule" };
                        }

                        // Set column heading
                        if (formatParams.ContainsKey("heading") && formatParams["heading"] != null)
                        {
                            string heading = formatParams["heading"].ToString();
                            targetField.ColumnHeading = heading;
                            modifications.Add($"Set heading for '{fieldName}' to '{heading}'");
                        }

                        // Set horizontal alignment
                        if (formatParams.ContainsKey("alignment") && formatParams["alignment"] != null)
                        {
                            string alignmentStr = formatParams["alignment"].ToString().ToLower();
                            ScheduleHorizontalAlignment alignment = ScheduleHorizontalAlignment.Left;
                            switch (alignmentStr)
                            {
                                case "left": alignment = ScheduleHorizontalAlignment.Left; break;
                                case "center": alignment = ScheduleHorizontalAlignment.Center; break;
                                case "right": alignment = ScheduleHorizontalAlignment.Right; break;
                            }
                            targetField.HorizontalAlignment = alignment;
                            modifications.Add($"Set alignment for '{fieldName}' to {alignmentStr}");
                        }

                        // Set column width
                        if (formatParams.ContainsKey("width") && formatParams["width"] != null)
                        {
                            double width = Convert.ToDouble(formatParams["width"]);
                            targetField.SheetColumnWidth = width / 12.0; // Convert to feet
                            modifications.Add($"Set width for '{fieldName}' to {width} inches");
                        }

                        // Show/hide field
                        if (formatParams.ContainsKey("hidden") && formatParams["hidden"] != null)
                        {
                            bool hidden = Convert.ToBoolean(formatParams["hidden"]);
                            targetField.IsHidden = hidden;
                            modifications.Add($"Set '{fieldName}' to {(hidden ? "hidden" : "visible")}");
                        }

                        // Calculate totals
                        if (formatParams.ContainsKey("calculate_totals") && formatParams["calculate_totals"] != null)
                        {
                            bool calculateTotals = Convert.ToBoolean(formatParams["calculate_totals"]);
                            if (targetField.CanTotal())
                            {
                                targetField.DisplayType = calculateTotals ? ScheduleFieldDisplayType.Totals : ScheduleFieldDisplayType.Standard;
                                modifications.Add($"Set calculate totals for '{fieldName}' to {calculateTotals}");
                            }
                            else
                            {
                                modifications.Add($"Warning: Field '{fieldName}' does not support totals");
                            }
                        }
                    }

                    // Add field calculation (for combined fields or formulas)
                    if (parameters.ContainsKey("add_calculated_field") && parameters["add_calculated_field"] != null)
                    {
                        var calcObj = parameters["add_calculated_field"];
                        JObject calcParams = calcObj is JObject jobj ? jobj : JObject.FromObject(calcObj);

                        string fieldName = calcParams["field_name"]?.ToString();
                        string calculationType = calcParams["calculation_type"]?.ToString()?.ToLower();

                        if (string.IsNullOrEmpty(fieldName))
                        {
                            trans.RollBack();
                            return new { success = false, error = "field_name is required for add_calculated_field" };
                        }

                        // Find the field
                        ScheduleField targetField = null;
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetField = field;
                                break;
                            }
                        }

                        if (targetField == null)
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Field '{fieldName}' not found in schedule" };
                        }

                        // Set calculation type based on what the field supports
                        if (!string.IsNullOrEmpty(calculationType))
                        {
                            switch (calculationType)
                            {
                                case "sum":
                                case "total":
                                case "totals":
                                    if (targetField.CanTotal())
                                    {
                                        targetField.DisplayType = ScheduleFieldDisplayType.Totals;
                                        modifications.Add($"Set '{fieldName}' to calculate totals (sum)");
                                    }
                                    break;
                                    
                                case "minimum":
                                case "min":
                                    if (targetField.FieldType == ScheduleFieldType.Instance || 
                                        targetField.FieldType == ScheduleFieldType.ElementType)
                                    {
                                        // Minimum calculation - show minimum value
                                        modifications.Add($"Note: Minimum calculation set for '{fieldName}' - configure in UI");
                                    }
                                    break;
                                    
                                case "maximum":
                                case "max":
                                    if (targetField.FieldType == ScheduleFieldType.Instance || 
                                        targetField.FieldType == ScheduleFieldType.ElementType)
                                    {
                                        modifications.Add($"Note: Maximum calculation set for '{fieldName}' - configure in UI");
                                    }
                                    break;
                            }
                        }
                    }

                    // Set field order/position
                    if (parameters.ContainsKey("reorder_field") && parameters["reorder_field"] != null)
                    {
                        var reorderObj = parameters["reorder_field"];
                        JObject reorderParams = reorderObj is JObject jobj ? jobj : JObject.FromObject(reorderObj);

                        string fieldName = reorderParams["field_name"]?.ToString();
                        int newPosition = reorderParams["position"]?.ToObject<int>() ?? 0;

                        if (string.IsNullOrEmpty(fieldName))
                        {
                            trans.RollBack();
                            return new { success = false, error = "field_name is required for reorder_field" };
                        }

                        // Find the field
                        int currentIndex = -1;
                        for (int i = 0; i < definition.GetFieldCount(); i++)
                        {
                            ScheduleField field = definition.GetField(i);
                            if (field.GetName().Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                            {
                                currentIndex = i;
                                break;
                            }
                        }

                        if (currentIndex == -1)
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Field '{fieldName}' not found in schedule" };
                        }

                        // Move field to new position
                        int targetPosition = Math.Max(0, Math.Min(newPosition, definition.GetFieldCount() - 1));
                        if (currentIndex != targetPosition)
                        {
                            trans.RollBack();
                            return new { success = false, error = "Field reordering is not currently supported in the Revit API. Fields must be manually reordered in the UI." };
                        }
                    }

                    trans.Commit();

                    // Get current schedule info
                    var currentFilters = new List<object>();
                    for (int i = 0; i < definition.GetFilterCount(); i++)
                    {
                        ScheduleFilter filter = definition.GetFilter(i);
                        ScheduleField filterField = definition.GetField(definition.GetFieldIndex(filter.FieldId));
                        currentFilters.Add(new
                        {
                            fieldName = filterField?.GetName() ?? "Unknown",
                            filterType = filter.FilterType.ToString(),
                            value = filter.GetStringValue()
                        });
                    }

                    var currentSortGroups = new List<object>();
                    for (int i = 0; i < definition.GetSortGroupFieldCount(); i++)
                    {
                        ScheduleSortGroupField sg = definition.GetSortGroupField(i);
                        ScheduleField sgField = definition.GetField(definition.GetFieldIndex(sg.FieldId));
                        currentSortGroups.Add(new
                        {
                            fieldName = sgField?.GetName() ?? "Unknown",
                            sortOrder = sg.SortOrder.ToString(),
                            showHeader = sg.ShowHeader,
                            showFooter = sg.ShowFooter
                        });
                    }

                    return new
                    {
                        success = true,
                        schedule = new
                        {
                            id = GetElementIdInt(schedule.Id),
                            name = schedule.Name,
                            isItemized = definition.IsItemized,
                            filterCount = definition.GetFilterCount(),
                            filters = currentFilters,
                            sortGroupCount = definition.GetSortGroupFieldCount(),
                            sortGroups = currentSortGroups
                        },
                        modifications = modifications
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = ex.Message };
                }
            }
        }

        /// <summary>
        /// Modify an element by changing parameters, location, rotation, or other properties
        /// </summary>
        private object ModifyElement(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("element_id") || parameters["element_id"] == null)
            {
                return new { success = false, error = "element_id is required" };
            }

            int elementId = Convert.ToInt32(parameters["element_id"]);
            Element element = doc.GetElement(new ElementId(elementId));

            if (element == null)
            {
                return new { success = false, error = $"Element with ID {elementId} not found" };
            }

            var modifications = new List<string>();

            using (Transaction trans = new Transaction(doc, "Modify Element"))
            {
                trans.Start();
                try
                {
                    // Set parameters
                    if (parameters.ContainsKey("parameters") && parameters["parameters"] != null)
                    {
                        var paramObj = parameters["parameters"];
                        JObject paramDict = paramObj is JObject jobj ? jobj : JObject.FromObject(paramObj);

                        foreach (var prop in paramDict.Properties())
                        {
                            string paramName = prop.Name;
                            string paramValue = prop.Value?.ToString();

                            Parameter param = element.LookupParameter(paramName);
                            if (param != null && !param.IsReadOnly)
                            {
                                bool success = SetParameterValueInternal(param, paramValue);
                                if (success)
                                {
                                    modifications.Add($"Set parameter '{paramName}' to '{paramValue}'");
                                }
                                else
                                {
                                    modifications.Add($"Failed to set parameter '{paramName}'");
                                }
                            }
                            else if (param == null)
                            {
                                modifications.Add($"Parameter '{paramName}' not found");
                            }
                            else
                            {
                                modifications.Add($"Parameter '{paramName}' is read-only");
                            }
                        }
                    }

                    // Handle move
                    if (parameters.ContainsKey("move") && parameters["move"] != null)
                    {
                        var moveObj = parameters["move"];
                        JObject moveParams = moveObj is JObject jobj ? jobj : JObject.FromObject(moveObj);

                        double x = moveParams["x"]?.ToObject<double>() ?? 0;
                        double y = moveParams["y"]?.ToObject<double>() ?? 0;
                        double z = moveParams["z"]?.ToObject<double>() ?? 0;
                        bool absolute = moveParams["absolute"]?.ToObject<bool>() ?? false;

                        if (absolute)
                        {
                            // Move to absolute location
                            LocationPoint locPoint = element.Location as LocationPoint;
                            LocationCurve locCurve = element.Location as LocationCurve;

                            if (locPoint != null)
                            {
                                XYZ currentPos = locPoint.Point;
                                XYZ newPos = new XYZ(x, y, z);
                                XYZ translation = newPos - currentPos;
                                ElementTransformUtils.MoveElement(doc, element.Id, translation);
                                modifications.Add($"Moved to absolute position ({x}, {y}, {z})");
                            }
                            else if (locCurve != null)
                            {
                                // For curve-based elements, move the start point
                                XYZ currentStart = locCurve.Curve.GetEndPoint(0);
                                XYZ newPos = new XYZ(x, y, z);
                                XYZ translation = newPos - currentStart;
                                ElementTransformUtils.MoveElement(doc, element.Id, translation);
                                modifications.Add($"Moved to absolute position ({x}, {y}, {z})");
                            }
                            else
                            {
                                modifications.Add("Element does not support absolute positioning");
                            }
                        }
                        else
                        {
                            // Relative translation
                            XYZ translation = new XYZ(x, y, z);
                            ElementTransformUtils.MoveElement(doc, element.Id, translation);
                            modifications.Add($"Translated by ({x}, {y}, {z})");
                        }
                    }

                    // Handle rotation
                    if (parameters.ContainsKey("rotate") && parameters["rotate"] != null)
                    {
                        var rotateObj = parameters["rotate"];
                        JObject rotateParams = rotateObj is JObject jobj ? jobj : JObject.FromObject(rotateObj);

                        double angle = rotateParams["angle"]?.ToObject<double>() ?? 0;
                        double angleRad = angle * Math.PI / 180.0;

                        // Get axis point
                        double axisX = rotateParams["axis_x"]?.ToObject<double>() ?? 0;
                        double axisY = rotateParams["axis_y"]?.ToObject<double>() ?? 0;
                        double axisZ = rotateParams["axis_z"]?.ToObject<double>() ?? 0;

                        // If no axis point specified, use element's location
                        if (!rotateParams.ContainsKey("axis_x"))
                        {
                            LocationPoint locPoint = element.Location as LocationPoint;
                            LocationCurve locCurve = element.Location as LocationCurve;
                            if (locPoint != null)
                            {
                                axisX = locPoint.Point.X;
                                axisY = locPoint.Point.Y;
                                axisZ = locPoint.Point.Z;
                            }
                            else if (locCurve != null)
                            {
                                XYZ midpoint = locCurve.Curve.Evaluate(0.5, true);
                                axisX = midpoint.X;
                                axisY = midpoint.Y;
                                axisZ = midpoint.Z;
                            }
                        }

                        XYZ axisPoint = new XYZ(axisX, axisY, axisZ);

                        // Get axis direction
                        string axisDir = rotateParams["axis_direction"]?.ToString() ?? "Z";
                        XYZ axisDirection;
                        switch (axisDir.ToUpper())
                        {
                            case "X": axisDirection = XYZ.BasisX; break;
                            case "Y": axisDirection = XYZ.BasisY; break;
                            default: axisDirection = XYZ.BasisZ; break;
                        }

                        Line axis = Line.CreateBound(axisPoint, axisPoint + axisDirection * 10);
                        ElementTransformUtils.RotateElement(doc, element.Id, axis, angleRad);
                        modifications.Add($"Rotated {angle}° around {axisDir} axis at ({axisX}, {axisY}, {axisZ})");
                    }

                    // Handle flip facing (for FamilyInstance)
                    if (parameters.ContainsKey("flip_facing") && parameters["flip_facing"] != null)
                    {
                        bool doFlip = Convert.ToBoolean(parameters["flip_facing"]);
                        if (doFlip && element is FamilyInstance fi)
                        {
                            if (fi.CanFlipFacing)
                            {
                                fi.flipFacing();
                                modifications.Add("Flipped facing orientation");
                            }
                            else
                            {
                                modifications.Add("Element cannot flip facing");
                            }
                        }
                    }

                    // Handle flip hand (for FamilyInstance)
                    if (parameters.ContainsKey("flip_hand") && parameters["flip_hand"] != null)
                    {
                        bool doFlip = Convert.ToBoolean(parameters["flip_hand"]);
                        if (doFlip && element is FamilyInstance fi)
                        {
                            if (fi.CanFlipHand)
                            {
                                fi.flipHand();
                                modifications.Add("Flipped hand orientation");
                            }
                            else
                            {
                                modifications.Add("Element cannot flip hand");
                            }
                        }
                    }

                    // Handle flip workplane - Note: Work plane flipping via Location.Rotation
                    if (parameters.ContainsKey("flip_workplane") && parameters["flip_workplane"] != null)
                    {
                        bool doFlip = Convert.ToBoolean(parameters["flip_workplane"]);
                        if (doFlip && element is FamilyInstance fi)
                        {
                            // Work plane flip can be achieved by rotating 180 degrees around facing direction
                            // or through location rotation adjustment
                            LocationPoint loc = fi.Location as LocationPoint;
                            if (loc != null)
                            {
                                // Rotate 180 degrees around Z-axis as an approximation
                                XYZ center = loc.Point;
                                Line axis = Line.CreateBound(center, center + XYZ.BasisZ);
                                ElementTransformUtils.RotateElement(doc, fi.Id, axis, Math.PI);
                                modifications.Add("Flipped about work plane (rotated 180°)");
                            }
                            else
                            {
                                modifications.Add("Element does not support work plane flip");
                            }
                        }
                    }

                    // Handle mirror
                    if (parameters.ContainsKey("mirror") && parameters["mirror"] != null)
                    {
                        var mirrorObj = parameters["mirror"];
                        JObject mirrorParams = mirrorObj is JObject jobj ? jobj : JObject.FromObject(mirrorObj);

                        double originX = mirrorParams["plane_origin_x"]?.ToObject<double>() ?? 0;
                        double originY = mirrorParams["plane_origin_y"]?.ToObject<double>() ?? 0;
                        double originZ = mirrorParams["plane_origin_z"]?.ToObject<double>() ?? 0;
                        double normalX = mirrorParams["plane_normal_x"]?.ToObject<double>() ?? 1;
                        double normalY = mirrorParams["plane_normal_y"]?.ToObject<double>() ?? 0;
                        double normalZ = mirrorParams["plane_normal_z"]?.ToObject<double>() ?? 0;

                        XYZ origin = new XYZ(originX, originY, originZ);
                        XYZ normal = new XYZ(normalX, normalY, normalZ).Normalize();
                        Plane mirrorPlane = Plane.CreateByNormalAndOrigin(normal, origin);

                        ElementTransformUtils.MirrorElement(doc, element.Id, mirrorPlane);
                        modifications.Add($"Mirrored across plane at ({originX}, {originY}, {originZ})");
                    }

                    // Handle pin/unpin
                    if (parameters.ContainsKey("pin") && parameters["pin"] != null)
                    {
                        bool pinState = Convert.ToBoolean(parameters["pin"]);
                        element.Pinned = pinState;
                        modifications.Add(pinState ? "Element pinned" : "Element unpinned");
                    }

                    trans.Commit();

                    // Get updated element info
                    LocationPoint finalLocPoint = element.Location as LocationPoint;
                    LocationCurve finalLocCurve = element.Location as LocationCurve;
                    object location = null;

                    if (finalLocPoint != null)
                    {
                        location = new
                        {
                            type = "point",
                            x = finalLocPoint.Point.X,
                            y = finalLocPoint.Point.Y,
                            z = finalLocPoint.Point.Z,
                            rotation = finalLocPoint.Rotation * 180.0 / Math.PI
                        };
                    }
                    else if (finalLocCurve != null)
                    {
                        location = new
                        {
                            type = "curve",
                            startX = finalLocCurve.Curve.GetEndPoint(0).X,
                            startY = finalLocCurve.Curve.GetEndPoint(0).Y,
                            startZ = finalLocCurve.Curve.GetEndPoint(0).Z,
                            endX = finalLocCurve.Curve.GetEndPoint(1).X,
                            endY = finalLocCurve.Curve.GetEndPoint(1).Y,
                            endZ = finalLocCurve.Curve.GetEndPoint(1).Z
                        };
                    }

                    return new
                    {
                        success = true,
                        element = new
                        {
                            id = GetElementIdInt(element.Id),
                            name = GetElementName(element),
                            category = element.Category?.Name,
                            pinned = element.Pinned,
                            location = location
                        },
                        modifications = modifications
                    };
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = ex.Message };
                }
            }
        }

        /// <summary>
        /// Helper to set parameter value with type handling
        /// </summary>
        private bool SetParameterValueInternal(Parameter param, string value)
        {
            if (param == null || param.IsReadOnly || value == null)
                return false;

            try
            {
                switch (param.StorageType)
                {
                    case StorageType.String:
                        param.Set(value);
                        return true;

                    case StorageType.Integer:
                        if (int.TryParse(value, out int intVal))
                        {
                            param.Set(intVal);
                            return true;
                        }
                        // Try boolean
                        if (bool.TryParse(value, out bool boolVal))
                        {
                            param.Set(boolVal ? 1 : 0);
                            return true;
                        }
                        break;

                    case StorageType.Double:
                        if (double.TryParse(value, out double doubleVal))
                        {
                            param.Set(doubleVal);
                            return true;
                        }
                        break;

                    case StorageType.ElementId:
                        if (int.TryParse(value, out int idVal))
                        {
                            param.Set(new ElementId(idVal));
                            return true;
                        }
                        break;
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Transform elements using ElementTransformUtils methods
        /// </summary>
        private object TransformElements(Document doc, Dictionary<string, object> parameters)
        {
            // Parse element IDs
            List<ElementId> elementIds = new List<ElementId>();

            if (parameters.ContainsKey("element_ids") && parameters["element_ids"] != null)
            {
                var idsObj = parameters["element_ids"];
                if (idsObj is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        elementIds.Add(new ElementId(Convert.ToInt32(item)));
                    }
                }
                else if (idsObj is IEnumerable<object> idList)
                {
                    foreach (var id in idList)
                    {
                        elementIds.Add(new ElementId(Convert.ToInt32(id)));
                    }
                }
            }
            else if (parameters.ContainsKey("element_id") && parameters["element_id"] != null)
            {
                elementIds.Add(new ElementId(Convert.ToInt32(parameters["element_id"])));
            }

            if (elementIds.Count == 0)
            {
                return new { success = false, error = "element_id or element_ids is required" };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : "move";

            using (Transaction trans = new Transaction(doc, $"Transform Elements - {operation}"))
            {
                trans.Start();
                try
                {
                    List<int> resultIds = new List<int>();
                    List<int> newElementIds = new List<int>();

                    switch (operation)
                    {
                        case "move":
                            {
                                XYZ translation = GetTranslationVector(parameters);
                                ElementTransformUtils.MoveElements(doc, elementIds, translation);
                                
                                // Apply rotation if specified
                                if (parameters.ContainsKey("rotation") && parameters["rotation"] != null)
                                {
                                    ApplyRotation(doc, elementIds, parameters);
                                }

                                foreach (var id in elementIds)
                                    resultIds.Add(GetElementIdInt(id));

                                trans.Commit();
                                return new
                                {
                                    success = true,
                                    operation = "move",
                                    elementsTransformed = resultIds.Count,
                                    elementIds = resultIds,
                                    translation = new { x = translation.X, y = translation.Y, z = translation.Z }
                                };
                            }

                        case "copy":
                            {
                                XYZ translation = GetTranslationVector(parameters);
                                ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElements(doc, elementIds, translation);

                                foreach (var id in copiedIds)
                                    newElementIds.Add(GetElementIdInt(id));

                                // Apply rotation to copies if specified
                                if (parameters.ContainsKey("rotation") && parameters["rotation"] != null)
                                {
                                    ApplyRotation(doc, copiedIds.ToList(), parameters);
                                }

                                trans.Commit();
                                return new
                                {
                                    success = true,
                                    operation = "copy",
                                    copiesCreated = newElementIds.Count,
                                    newElementIds = newElementIds,
                                    translation = new { x = translation.X, y = translation.Y, z = translation.Z }
                                };
                            }

                        case "mirror":
                            {
                                Plane mirrorPlane = GetMirrorPlane(parameters);
                                if (mirrorPlane == null)
                                {
                                    trans.RollBack();
                                    return new { success = false, error = "mirror_plane is required for mirror operation" };
                                }

                                // Mirror creates copies by default
                                ICollection<ElementId> mirroredIds = ElementTransformUtils.MirrorElements(doc, elementIds, mirrorPlane, true);

                                foreach (var id in mirroredIds)
                                    newElementIds.Add(GetElementIdInt(id));

                                trans.Commit();
                                return new
                                {
                                    success = true,
                                    operation = "mirror",
                                    mirroredCopiesCreated = newElementIds.Count,
                                    newElementIds = newElementIds
                                };
                            }

                        case "linear_array":
                            {
                                int arrayCount = parameters.ContainsKey("array_count") ? Convert.ToInt32(parameters["array_count"]) : 3;
                                XYZ spacing = GetArraySpacing(parameters);

                                if (arrayCount < 2)
                                {
                                    trans.RollBack();
                                    return new { success = false, error = "array_count must be at least 2" };
                                }

                                // Create copies at incremental positions
                                for (int i = 1; i < arrayCount; i++)
                                {
                                    XYZ offset = new XYZ(spacing.X * i, spacing.Y * i, spacing.Z * i);
                                    ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElements(doc, elementIds, offset);
                                    foreach (var id in copiedIds)
                                        newElementIds.Add(GetElementIdInt(id));
                                }

                                trans.Commit();
                                return new
                                {
                                    success = true,
                                    operation = "linear_array",
                                    arrayCount = arrayCount,
                                    copiesCreated = newElementIds.Count,
                                    newElementIds = newElementIds,
                                    spacing = new { x = spacing.X, y = spacing.Y, z = spacing.Z }
                                };
                            }

                        case "radial_array":
                            {
                                int arrayCount = parameters.ContainsKey("array_count") ? Convert.ToInt32(parameters["array_count"]) : 4;
                                double totalAngle = parameters.ContainsKey("radial_angle") ? Convert.ToDouble(parameters["radial_angle"]) : 360.0;
                                XYZ center = GetRadialCenter(parameters, doc, elementIds);

                                if (arrayCount < 2)
                                {
                                    trans.RollBack();
                                    return new { success = false, error = "array_count must be at least 2" };
                                }

                                double angleIncrement = (totalAngle / arrayCount) * Math.PI / 180.0;
                                Line axis = Line.CreateBound(center, center + XYZ.BasisZ * 10);

                                for (int i = 1; i < arrayCount; i++)
                                {
                                    // Copy elements
                                    ICollection<ElementId> copiedIds = ElementTransformUtils.CopyElements(doc, elementIds, XYZ.Zero);
                                    
                                    // Rotate the copies
                                    double angle = angleIncrement * i;
                                    ElementTransformUtils.RotateElements(doc, copiedIds, axis, angle);

                                    foreach (var id in copiedIds)
                                        newElementIds.Add(GetElementIdInt(id));
                                }

                                trans.Commit();
                                return new
                                {
                                    success = true,
                                    operation = "radial_array",
                                    arrayCount = arrayCount,
                                    copiesCreated = newElementIds.Count,
                                    newElementIds = newElementIds,
                                    totalAngleDegrees = totalAngle,
                                    center = new { x = center.X, y = center.Y, z = center.Z }
                                };
                            }

                        default:
                            trans.RollBack();
                            return new { success = false, error = $"Unknown operation: {operation}. Use move, copy, mirror, linear_array, or radial_array." };
                    }
                }
                catch (Exception ex)
                {
                    trans.RollBack();
                    return new { success = false, error = ex.Message };
                }
            }
        }

        private XYZ GetTranslationVector(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("translation") && parameters["translation"] != null)
            {
                var transObj = parameters["translation"];
                JObject transParams = transObj is JObject jobj ? jobj : JObject.FromObject(transObj);
                double x = transParams["x"]?.ToObject<double>() ?? 0;
                double y = transParams["y"]?.ToObject<double>() ?? 0;
                double z = transParams["z"]?.ToObject<double>() ?? 0;
                return new XYZ(x, y, z);
            }
            return XYZ.Zero;
        }

        private XYZ GetArraySpacing(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("array_spacing") && parameters["array_spacing"] != null)
            {
                var spacingObj = parameters["array_spacing"];
                JObject spacingParams = spacingObj is JObject jobj ? jobj : JObject.FromObject(spacingObj);
                double x = spacingParams["x"]?.ToObject<double>() ?? 0;
                double y = spacingParams["y"]?.ToObject<double>() ?? 0;
                double z = spacingParams["z"]?.ToObject<double>() ?? 0;
                return new XYZ(x, y, z);
            }
            return new XYZ(10, 0, 0); // Default spacing
        }

        private XYZ GetRadialCenter(Dictionary<string, object> parameters, Document doc, List<ElementId> elementIds)
        {
            if (parameters.ContainsKey("radial_center") && parameters["radial_center"] != null)
            {
                var centerObj = parameters["radial_center"];
                JObject centerParams = centerObj is JObject jobj ? jobj : JObject.FromObject(centerObj);
                double x = centerParams["x"]?.ToObject<double>() ?? 0;
                double y = centerParams["y"]?.ToObject<double>() ?? 0;
                double z = centerParams["z"]?.ToObject<double>() ?? 0;
                return new XYZ(x, y, z);
            }
            
            // Default: use first element's location
            if (elementIds.Count > 0)
            {
                Element elem = doc.GetElement(elementIds[0]);
                if (elem?.Location is LocationPoint lp)
                    return lp.Point;
                if (elem?.Location is LocationCurve lc)
                    return lc.Curve.Evaluate(0.5, true);
            }
            return XYZ.Zero;
        }

        private Plane GetMirrorPlane(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("mirror_plane") && parameters["mirror_plane"] != null)
            {
                var planeObj = parameters["mirror_plane"];
                JObject planeParams = planeObj is JObject jobj ? jobj : JObject.FromObject(planeObj);
                
                double originX = planeParams["origin_x"]?.ToObject<double>() ?? 0;
                double originY = planeParams["origin_y"]?.ToObject<double>() ?? 0;
                double originZ = planeParams["origin_z"]?.ToObject<double>() ?? 0;
                double normalX = planeParams["normal_x"]?.ToObject<double>() ?? 1;
                double normalY = planeParams["normal_y"]?.ToObject<double>() ?? 0;
                double normalZ = planeParams["normal_z"]?.ToObject<double>() ?? 0;

                XYZ origin = new XYZ(originX, originY, originZ);
                XYZ normal = new XYZ(normalX, normalY, normalZ).Normalize();
                return Plane.CreateByNormalAndOrigin(normal, origin);
            }
            return null;
        }

        private void ApplyRotation(Document doc, List<ElementId> elementIds, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("rotation") || parameters["rotation"] == null) return;

            var rotateObj = parameters["rotation"];
            JObject rotateParams = rotateObj is JObject jobj ? jobj : JObject.FromObject(rotateObj);

            double angle = rotateParams["angle"]?.ToObject<double>() ?? 0;
            double angleRad = angle * Math.PI / 180.0;

            double centerX = rotateParams["center_x"]?.ToObject<double>() ?? 0;
            double centerY = rotateParams["center_y"]?.ToObject<double>() ?? 0;
            double centerZ = rotateParams["center_z"]?.ToObject<double>() ?? 0;
            string axisStr = rotateParams["axis"]?.ToString() ?? "Z";

            XYZ center = new XYZ(centerX, centerY, centerZ);
            XYZ axisDir;
            switch (axisStr.ToUpper())
            {
                case "X": axisDir = XYZ.BasisX; break;
                case "Y": axisDir = XYZ.BasisY; break;
                default: axisDir = XYZ.BasisZ; break;
            }

            Line axis = Line.CreateBound(center, center + axisDir * 10);
            ElementTransformUtils.RotateElements(doc, elementIds, axis, angleRad);
        }

        /// <summary>
        /// Get all family types (FamilySymbols) with optional filtering
        /// </summary>
        private object GetFamilyTypes(Document doc, Dictionary<string, object> parameters)
        {
            string familyNameFilter = parameters.ContainsKey("family_name") ? parameters["family_name"]?.ToString() : null;
            string categoryFilter = parameters.ContainsKey("category") ? parameters["category"]?.ToString() : null;
            bool includeParameters = parameters.ContainsKey("include_parameters") && Convert.ToBoolean(parameters["include_parameters"]);
            int? maxResults = parameters.ContainsKey("max_results") && parameters["max_results"] != null
                ? Convert.ToInt32(parameters["max_results"])
                : (int?)null;

            try
            {
                var collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>();

                // Apply category filter
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    collector = collector.Where(fs => fs.Category != null &&
                        fs.Category.Name.IndexOf(categoryFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                // Apply family name filter
                if (!string.IsNullOrEmpty(familyNameFilter))
                {
                    collector = collector.Where(fs => fs.Family != null &&
                        fs.Family.Name.IndexOf(familyNameFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var results = new List<object>();
                int count = 0;

                foreach (FamilySymbol fs in collector)
                {
                    if (maxResults.HasValue && count >= maxResults.Value)
                        break;

                    var typeInfo = new Dictionary<string, object>
                    {
                        { "id", GetElementIdInt(fs.Id) },
                        { "typeName", fs.Name },
                        { "familyName", fs.Family?.Name ?? "Unknown" },
                        { "category", fs.Category?.Name ?? "Unknown" },
                        { "isActive", fs.IsActive }
                    };

                    if (includeParameters)
                    {
                        var paramList = new List<object>();
                        foreach (Parameter param in fs.Parameters)
                        {
                            if (param.HasValue)
                            {
                                paramList.Add(new
                                {
                                    name = param.Definition.Name,
                                    value = GetParameterValueAsString(param),
                                    isReadOnly = param.IsReadOnly
                                });
                            }
                        }
                        typeInfo["parameters"] = paramList;
                    }

                    results.Add(typeInfo);
                    count++;
                }

                return new
                {
                    success = true,
                    count = results.Count,
                    familyTypes = results
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Get instances of a specific family, type, category, or element class
        /// </summary>
        private object GetInstances(Document doc, Dictionary<string, object> parameters)
        {
            string categoryFilter = parameters.ContainsKey("category") ? parameters["category"]?.ToString() : null;
            string familyNameFilter = parameters.ContainsKey("family_name") ? parameters["family_name"]?.ToString() : null;
            string typeNameFilter = parameters.ContainsKey("type_name") ? parameters["type_name"]?.ToString() : null;
            string elementClassFilter = parameters.ContainsKey("element_class") ? parameters["element_class"]?.ToString() : null;
            bool includeLocation = !parameters.ContainsKey("include_location") || Convert.ToBoolean(parameters["include_location"]);
            bool includeParameters = parameters.ContainsKey("include_parameters") && Convert.ToBoolean(parameters["include_parameters"]);
            int? maxResults = parameters.ContainsKey("max_results") && parameters["max_results"] != null
                ? Convert.ToInt32(parameters["max_results"])
                : (int?)null;

            try
            {
                FilteredElementCollector collector;

                // Start with element class if specified
                if (!string.IsNullOrEmpty(elementClassFilter))
                {
                    Type elementType = GetElementTypeByName(elementClassFilter);
                    if (elementType != null)
                    {
                        collector = new FilteredElementCollector(doc).OfClass(elementType);
                    }
                    else
                    {
                        return new { success = false, error = $"Unknown element class: {elementClassFilter}" };
                    }
                }
                else if (!string.IsNullOrEmpty(categoryFilter))
                {
                    // Find category
                    BuiltInCategory? bic = GetBuiltInCategory(categoryFilter);
                    if (bic.HasValue)
                    {
                        collector = new FilteredElementCollector(doc).OfCategory(bic.Value).WhereElementIsNotElementType();
                    }
                    else
                    {
                        return new { success = false, error = $"Unknown category: {categoryFilter}" };
                    }
                }
                else
                {
                    // Get all FamilyInstances by default
                    collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance));
                }

                var elements = collector.ToElements();
                var results = new List<object>();
                int count = 0;

                foreach (Element elem in elements)
                {
                    if (maxResults.HasValue && count >= maxResults.Value)
                        break;

                    // Apply family name filter
                    if (!string.IsNullOrEmpty(familyNameFilter))
                    {
                        if (elem is FamilyInstance fi)
                        {
                            if (fi.Symbol?.Family?.Name == null ||
                                fi.Symbol.Family.Name.IndexOf(familyNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                        }
                        else continue;
                    }

                    // Apply type name filter
                    if (!string.IsNullOrEmpty(typeNameFilter))
                    {
                        string elemTypeName = doc.GetElement(elem.GetTypeId())?.Name;
                        if (elemTypeName == null || elemTypeName.IndexOf(typeNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                    }

                    var instanceInfo = new Dictionary<string, object>
                    {
                        { "id", GetElementIdInt(elem.Id) },
                        { "name", GetElementName(elem) ?? elem.Name },
                        { "category", elem.Category?.Name ?? "Unknown" }
                    };

                    if (elem is FamilyInstance familyInst)
                    {
                        instanceInfo["familyName"] = familyInst.Symbol?.Family?.Name;
                        instanceInfo["typeName"] = familyInst.Symbol?.Name;
                    }
                    else
                    {
                        Element typeElem = doc.GetElement(elem.GetTypeId());
                        instanceInfo["typeName"] = typeElem?.Name;
                    }

                    if (includeLocation)
                    {
                        instanceInfo["location"] = GetElementLocation(elem);
                    }

                    if (includeParameters)
                    {
                        var paramList = new List<object>();
                        foreach (Parameter param in elem.Parameters)
                        {
                            if (param.HasValue)
                            {
                                paramList.Add(new
                                {
                                    name = param.Definition.Name,
                                    value = GetParameterValueAsString(param)
                                });
                            }
                        }
                        instanceInfo["parameters"] = paramList;
                    }

                    results.Add(instanceInfo);
                    count++;
                }

                return new
                {
                    success = true,
                    count = results.Count,
                    instances = results
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Find a specific family type by name
        /// </summary>
        private object FindFamilyType(Document doc, Dictionary<string, object> parameters)
        {
            string familyNameFilter = parameters.ContainsKey("family_name") ? parameters["family_name"]?.ToString() : null;
            string typeNameFilter = parameters.ContainsKey("type_name") ? parameters["type_name"]?.ToString() : null;
            string categoryFilter = parameters.ContainsKey("category") ? parameters["category"]?.ToString() : null;
            bool exactMatch = parameters.ContainsKey("exact_match") && Convert.ToBoolean(parameters["exact_match"]);

            try
            {
                var collector = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>();

                // Apply filters
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    if (exactMatch)
                        collector = collector.Where(fs => fs.Category?.Name?.Equals(categoryFilter, StringComparison.OrdinalIgnoreCase) == true);
                    else
                        collector = collector.Where(fs => fs.Category?.Name?.IndexOf(categoryFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(familyNameFilter))
                {
                    if (exactMatch)
                        collector = collector.Where(fs => fs.Family?.Name?.Equals(familyNameFilter, StringComparison.OrdinalIgnoreCase) == true);
                    else
                        collector = collector.Where(fs => fs.Family?.Name?.IndexOf(familyNameFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(typeNameFilter))
                {
                    if (exactMatch)
                        collector = collector.Where(fs => fs.Name?.Equals(typeNameFilter, StringComparison.OrdinalIgnoreCase) == true);
                    else
                        collector = collector.Where(fs => fs.Name?.IndexOf(typeNameFilter, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                var results = new List<object>();
                foreach (FamilySymbol fs in collector.Take(50)) // Limit to 50 results
                {
                    var paramList = new List<object>();
                    foreach (Parameter param in fs.Parameters)
                    {
                        if (param.HasValue)
                        {
                            paramList.Add(new
                            {
                                name = param.Definition.Name,
                                value = GetParameterValueAsString(param),
                                storageType = param.StorageType.ToString(),
                                isReadOnly = param.IsReadOnly
                            });
                        }
                    }

                    results.Add(new
                    {
                        id = GetElementIdInt(fs.Id),
                        typeName = fs.Name,
                        familyName = fs.Family?.Name ?? "Unknown",
                        category = fs.Category?.Name ?? "Unknown",
                        familyId = fs.Family != null ? GetElementIdInt(fs.Family.Id) : -1,
                        isActive = fs.IsActive,
                        parameters = paramList
                    });
                }

                return new
                {
                    success = true,
                    count = results.Count,
                    familyTypes = results
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Find specific element instances with advanced filtering
        /// </summary>
        private object FindElements(Document doc, Dictionary<string, object> parameters)
        {
            string categoryFilter = parameters.ContainsKey("category") ? parameters["category"]?.ToString() : null;
            string familyNameFilter = parameters.ContainsKey("family_name") ? parameters["family_name"]?.ToString() : null;
            string typeNameFilter = parameters.ContainsKey("type_name") ? parameters["type_name"]?.ToString() : null;
            string levelNameFilter = parameters.ContainsKey("level_name") ? parameters["level_name"]?.ToString() : null;
            bool viewSpecific = parameters.ContainsKey("view_specific") && Convert.ToBoolean(parameters["view_specific"]);
            bool includeLocation = !parameters.ContainsKey("include_location") || Convert.ToBoolean(parameters["include_location"]);
            int? maxResults = parameters.ContainsKey("max_results") && parameters["max_results"] != null
                ? Convert.ToInt32(parameters["max_results"])
                : (int?)null;

            try
            {
                FilteredElementCollector collector;

                if (viewSpecific && doc.ActiveView != null)
                {
                    collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                }
                else
                {
                    collector = new FilteredElementCollector(doc);
                }

                // Apply category filter
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    BuiltInCategory? bic = GetBuiltInCategory(categoryFilter);
                    if (bic.HasValue)
                    {
                        collector = collector.OfCategory(bic.Value);
                    }
                }

                collector = collector.WhereElementIsNotElementType();

                // Apply bounding box filter if specified
                if (parameters.ContainsKey("bounding_box") && parameters["bounding_box"] != null)
                {
                    var bbObj = parameters["bounding_box"];
                    JObject bbParams = bbObj is JObject jobj ? jobj : JObject.FromObject(bbObj);

                    double minX = bbParams["min_x"]?.ToObject<double>() ?? double.MinValue;
                    double minY = bbParams["min_y"]?.ToObject<double>() ?? double.MinValue;
                    double minZ = bbParams["min_z"]?.ToObject<double>() ?? double.MinValue;
                    double maxX = bbParams["max_x"]?.ToObject<double>() ?? double.MaxValue;
                    double maxY = bbParams["max_y"]?.ToObject<double>() ?? double.MaxValue;
                    double maxZ = bbParams["max_z"]?.ToObject<double>() ?? double.MaxValue;

                    Outline outline = new Outline(new XYZ(minX, minY, minZ), new XYZ(maxX, maxY, maxZ));
                    BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(outline);
                    collector = collector.WherePasses(bbFilter);
                }

                var elements = collector.ToElements();
                var results = new List<object>();
                int count = 0;

                // Find level ID if filtering by level
                ElementId levelId = null;
                if (!string.IsNullOrEmpty(levelNameFilter))
                {
                    var level = new FilteredElementCollector(doc)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault(l => l.Name.Equals(levelNameFilter, StringComparison.OrdinalIgnoreCase));
                    levelId = level?.Id;
                }

                // Parameter filter info
                JObject paramFilter = null;
                if (parameters.ContainsKey("parameter_filter") && parameters["parameter_filter"] != null)
                {
                    var pfObj = parameters["parameter_filter"];
                    paramFilter = pfObj is JObject jobj ? jobj : JObject.FromObject(pfObj);
                }

                foreach (Element elem in elements)
                {
                    if (maxResults.HasValue && count >= maxResults.Value)
                        break;

                    // Apply family name filter
                    if (!string.IsNullOrEmpty(familyNameFilter))
                    {
                        if (elem is FamilyInstance fi)
                        {
                            if (fi.Symbol?.Family?.Name?.IndexOf(familyNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                                continue;
                        }
                        else continue;
                    }

                    // Apply type name filter
                    if (!string.IsNullOrEmpty(typeNameFilter))
                    {
                        Element typeElem = doc.GetElement(elem.GetTypeId());
                        if (typeElem?.Name?.IndexOf(typeNameFilter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                    }

                    // Apply level filter
                    if (levelId != null)
                    {
                        Parameter levelParam = elem.get_Parameter(BuiltInParameter.LEVEL_PARAM) ??
                                               elem.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM) ??
                                               elem.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
                        if (levelParam == null || levelParam.AsElementId() != levelId)
                            continue;
                    }

                    // Apply parameter filter
                    if (paramFilter != null)
                    {
                        string paramName = paramFilter["name"]?.ToString();
                        string paramValue = paramFilter["value"]?.ToString();
                        string op = paramFilter["operator"]?.ToString() ?? "equals";

                        if (!string.IsNullOrEmpty(paramName))
                        {
                            Parameter param = elem.LookupParameter(paramName);
                            if (param == null || !param.HasValue)
                                continue;

                            string actualValue = GetParameterValueAsString(param);
                            bool match = false;

                            switch (op.ToLower())
                            {
                                case "equals":
                                    match = actualValue.Equals(paramValue, StringComparison.OrdinalIgnoreCase);
                                    break;
                                case "contains":
                                    match = actualValue.IndexOf(paramValue, StringComparison.OrdinalIgnoreCase) >= 0;
                                    break;
                                case "startswith":
                                    match = actualValue.StartsWith(paramValue, StringComparison.OrdinalIgnoreCase);
                                    break;
                                case "endswith":
                                    match = actualValue.EndsWith(paramValue, StringComparison.OrdinalIgnoreCase);
                                    break;
                                case "greater":
                                    if (double.TryParse(actualValue, out double av) && double.TryParse(paramValue, out double pv))
                                        match = av > pv;
                                    break;
                                case "less":
                                    if (double.TryParse(actualValue, out double av2) && double.TryParse(paramValue, out double pv2))
                                        match = av2 < pv2;
                                    break;
                            }

                            if (!match) continue;
                        }
                    }

                    var instanceInfo = new Dictionary<string, object>
                    {
                        { "id", GetElementIdInt(elem.Id) },
                        { "name", GetElementName(elem) ?? elem.Name },
                        { "category", elem.Category?.Name ?? "Unknown" }
                    };

                    if (elem is FamilyInstance familyInst)
                    {
                        instanceInfo["familyName"] = familyInst.Symbol?.Family?.Name;
                        instanceInfo["typeName"] = familyInst.Symbol?.Name;
                    }
                    else
                    {
                        Element typeElem = doc.GetElement(elem.GetTypeId());
                        instanceInfo["typeName"] = typeElem?.Name;
                    }

                    if (includeLocation)
                    {
                        instanceInfo["location"] = GetElementLocation(elem);
                    }

                    results.Add(instanceInfo);
                    count++;
                }

                return new
                {
                    success = true,
                    count = results.Count,
                    elements = results
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private string GetParameterValueAsString(Parameter param)
        {
            if (param == null || !param.HasValue) return "";

            switch (param.StorageType)
            {
                case StorageType.String:
                    return param.AsString() ?? "";
                case StorageType.Integer:
                    return param.AsInteger().ToString();
                case StorageType.Double:
                    return param.AsDouble().ToString("F4");
                case StorageType.ElementId:
                    return param.AsElementId().ToString();
                default:
                    return param.AsValueString() ?? "";
            }
        }

        private object GetElementLocation(Element elem)
        {
            if (elem.Location is LocationPoint lp)
            {
                return new
                {
                    type = "point",
                    x = lp.Point.X,
                    y = lp.Point.Y,
                    z = lp.Point.Z,
                    rotation = lp.Rotation * 180.0 / Math.PI
                };
            }
            else if (elem.Location is LocationCurve lc)
            {
                return new
                {
                    type = "curve",
                    startX = lc.Curve.GetEndPoint(0).X,
                    startY = lc.Curve.GetEndPoint(0).Y,
                    startZ = lc.Curve.GetEndPoint(0).Z,
                    endX = lc.Curve.GetEndPoint(1).X,
                    endY = lc.Curve.GetEndPoint(1).Y,
                    endZ = lc.Curve.GetEndPoint(1).Z
                };
            }
            return null;
        }

        private Type GetElementTypeByName(string className)
        {
            var typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Wall", typeof(Wall) },
                { "Floor", typeof(Floor) },
                { "Ceiling", typeof(Ceiling) },
                { "Roof", typeof(RoofBase) },
                { "FamilyInstance", typeof(FamilyInstance) },
                { "Room", typeof(SpatialElement) },
                { "Area", typeof(Area) },
                { "Level", typeof(Level) },
                { "Grid", typeof(Grid) },
                { "View", typeof(View) },
                { "ViewPlan", typeof(ViewPlan) },
                { "ViewSection", typeof(ViewSection) },
                { "View3D", typeof(View3D) },
                { "ViewSheet", typeof(ViewSheet) },
                { "Group", typeof(Group) },
                { "ReferencePlane", typeof(ReferencePlane) },
                { "CurveElement", typeof(CurveElement) },
                { "ModelCurve", typeof(ModelCurve) },
                { "DetailCurve", typeof(DetailCurve) },
                { "TextNote", typeof(TextNote) },
                { "Dimension", typeof(Dimension) }
            };

            return typeMap.TryGetValue(className, out Type result) ? result : null;
        }

        private BuiltInCategory? GetBuiltInCategory(string categoryName)
        {
            var categoryMap = new Dictionary<string, BuiltInCategory>(StringComparer.OrdinalIgnoreCase)
            {
                { "Walls", BuiltInCategory.OST_Walls },
                { "Wall", BuiltInCategory.OST_Walls },
                { "Doors", BuiltInCategory.OST_Doors },
                { "Door", BuiltInCategory.OST_Doors },
                { "Windows", BuiltInCategory.OST_Windows },
                { "Window", BuiltInCategory.OST_Windows },
                { "Floors", BuiltInCategory.OST_Floors },
                { "Floor", BuiltInCategory.OST_Floors },
                { "Ceilings", BuiltInCategory.OST_Ceilings },
                { "Ceiling", BuiltInCategory.OST_Ceilings },
                { "Roofs", BuiltInCategory.OST_Roofs },
                { "Roof", BuiltInCategory.OST_Roofs },
                { "Furniture", BuiltInCategory.OST_Furniture },
                { "Columns", BuiltInCategory.OST_Columns },
                { "Column", BuiltInCategory.OST_Columns },
                { "StructuralColumns", BuiltInCategory.OST_StructuralColumns },
                { "StructuralFraming", BuiltInCategory.OST_StructuralFraming },
                { "Rooms", BuiltInCategory.OST_Rooms },
                { "Room", BuiltInCategory.OST_Rooms },
                { "Areas", BuiltInCategory.OST_Areas },
                { "Area", BuiltInCategory.OST_Areas },
                { "Stairs", BuiltInCategory.OST_Stairs },
                { "Railings", BuiltInCategory.OST_StairsRailing },
                { "Casework", BuiltInCategory.OST_Casework },
                { "GenericModels", BuiltInCategory.OST_GenericModel },
                { "GenericModel", BuiltInCategory.OST_GenericModel },
                { "Plumbing", BuiltInCategory.OST_PlumbingFixtures },
                { "PlumbingFixtures", BuiltInCategory.OST_PlumbingFixtures },
                { "MechanicalEquipment", BuiltInCategory.OST_MechanicalEquipment },
                { "ElectricalEquipment", BuiltInCategory.OST_ElectricalEquipment },
                { "ElectricalFixtures", BuiltInCategory.OST_ElectricalFixtures },
                { "LightingFixtures", BuiltInCategory.OST_LightingFixtures },
                { "Parking", BuiltInCategory.OST_Parking },
                { "Entourage", BuiltInCategory.OST_Entourage },
                { "Planting", BuiltInCategory.OST_Planting },
                { "Site", BuiltInCategory.OST_Site },
                { "Topography", BuiltInCategory.OST_Topography },
                { "CurtainPanels", BuiltInCategory.OST_CurtainWallPanels },
                { "CurtainWallMullions", BuiltInCategory.OST_CurtainWallMullions },
                { "Grids", BuiltInCategory.OST_Grids },
                { "Grid", BuiltInCategory.OST_Grids },
                { "Levels", BuiltInCategory.OST_Levels },
                { "Level", BuiltInCategory.OST_Levels }
            };

            return categoryMap.TryGetValue(categoryName, out BuiltInCategory result) ? result : (BuiltInCategory?)null;
        }

        /// <summary>
        /// Selection tool operations
        /// </summary>
        private object SelectionTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            UIDocument uidoc = uiApp.ActiveUIDocument;
            if (uidoc == null)
            {
                return new { success = false, error = "No active UI document" };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required" };
            }

            bool includeLocation = !parameters.ContainsKey("include_location") || Convert.ToBoolean(parameters["include_location"]);
            bool includeParameters = parameters.ContainsKey("include_parameters") && Convert.ToBoolean(parameters["include_parameters"]);
            string prompt = parameters.ContainsKey("prompt") ? parameters["prompt"]?.ToString() : null;
            string filterCategory = parameters.ContainsKey("filter_category") ? parameters["filter_category"]?.ToString() : null;
            string filterClass = parameters.ContainsKey("filter_class") ? parameters["filter_class"]?.ToString() : null;

            try
            {
                switch (operation)
                {
                    case "get_selection":
                        return GetCurrentSelection(uidoc, doc, includeLocation, includeParameters);

                    case "set_selection":
                        return SetSelection(uidoc, doc, parameters);

                    case "clear_selection":
                        return ClearSelection(uidoc);

                    case "pick_object":
                        return PickSingleObject(uidoc, doc, prompt, filterCategory, filterClass, includeLocation, includeParameters);

                    case "pick_objects":
                        return PickMultipleObjects(uidoc, doc, prompt, filterCategory, filterClass, includeLocation, includeParameters);

                    case "pick_point":
                        return PickPointOperation(uidoc, prompt);

                    case "pick_face":
                        return PickFaceOperation(uidoc, doc, prompt, filterCategory, includeLocation);

                    case "pick_edge":
                        return PickEdgeOperation(uidoc, doc, prompt, filterCategory, includeLocation);

                    default:
                        return new { success = false, error = $"Unknown selection operation: {operation}" };
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return new { success = false, cancelled = true, message = "Selection was cancelled by user" };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private object GetCurrentSelection(UIDocument uidoc, Document doc, bool includeLocation, bool includeParameters)
        {
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            var elements = new List<object>();

            foreach (ElementId id in selectedIds)
            {
                Element elem = doc.GetElement(id);
                if (elem == null) continue;

                var elemInfo = BuildElementInfo(elem, doc, includeLocation, includeParameters);
                elements.Add(elemInfo);
            }

            return new
            {
                success = true,
                count = elements.Count,
                selectedElements = elements
            };
        }

        private object SetSelection(UIDocument uidoc, Document doc, Dictionary<string, object> parameters)
        {
            List<ElementId> elementIds = new List<ElementId>();

            if (parameters.ContainsKey("element_ids") && parameters["element_ids"] != null)
            {
                var idsObj = parameters["element_ids"];
                if (idsObj is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        elementIds.Add(new ElementId(Convert.ToInt32(item)));
                    }
                }
                else if (idsObj is IEnumerable<object> idList)
                {
                    foreach (var id in idList)
                    {
                        elementIds.Add(new ElementId(Convert.ToInt32(id)));
                    }
                }
            }

            if (elementIds.Count == 0)
            {
                return new { success = false, error = "element_ids array is required" };
            }

            // Validate elements exist
            var validIds = new List<ElementId>();
            foreach (var id in elementIds)
            {
                if (doc.GetElement(id) != null)
                    validIds.Add(id);
            }

            uidoc.Selection.SetElementIds(validIds);

            return new
            {
                success = true,
                selectedCount = validIds.Count,
                invalidCount = elementIds.Count - validIds.Count
            };
        }

        private object ClearSelection(UIDocument uidoc)
        {
            uidoc.Selection.SetElementIds(new List<ElementId>());
            return new { success = true, message = "Selection cleared" };
        }

        private object PickSingleObject(UIDocument uidoc, Document doc, string prompt, string filterCategory, string filterClass, bool includeLocation, bool includeParameters)
        {
            ISelectionFilter filter = CreateSelectionFilter(doc, filterCategory, filterClass);

            Reference reference;
            if (filter != null)
            {
                reference = uidoc.Selection.PickObject(ObjectType.Element, filter, prompt ?? "Select an element");
            }
            else
            {
                reference = uidoc.Selection.PickObject(ObjectType.Element, prompt ?? "Select an element");
            }

            Element elem = doc.GetElement(reference.ElementId);
            var elemInfo = BuildElementInfo(elem, doc, includeLocation, includeParameters);

            return new
            {
                success = true,
                pickedElement = elemInfo
            };
        }

        private object PickMultipleObjects(UIDocument uidoc, Document doc, string prompt, string filterCategory, string filterClass, bool includeLocation, bool includeParameters)
        {
            ISelectionFilter filter = CreateSelectionFilter(doc, filterCategory, filterClass);

            IList<Reference> references;
            if (filter != null)
            {
                references = uidoc.Selection.PickObjects(ObjectType.Element, filter, prompt ?? "Select elements (click Finish when done)");
            }
            else
            {
                references = uidoc.Selection.PickObjects(ObjectType.Element, prompt ?? "Select elements (click Finish when done)");
            }

            var elements = new List<object>();
            foreach (Reference reference in references)
            {
                Element elem = doc.GetElement(reference.ElementId);
                if (elem != null)
                {
                    elements.Add(BuildElementInfo(elem, doc, includeLocation, includeParameters));
                }
            }

            return new
            {
                success = true,
                count = elements.Count,
                pickedElements = elements
            };
        }

        private object PickPointOperation(UIDocument uidoc, string prompt)
        {
            XYZ point = uidoc.Selection.PickPoint(prompt ?? "Pick a point");

            return new
            {
                success = true,
                point = new
                {
                    x = point.X,
                    y = point.Y,
                    z = point.Z
                }
            };
        }

        private object PickFaceOperation(UIDocument uidoc, Document doc, string prompt, string filterCategory, bool includeLocation)
        {
            ISelectionFilter filter = CreateSelectionFilter(doc, filterCategory, null);

            Reference reference;
            if (filter != null)
            {
                reference = uidoc.Selection.PickObject(ObjectType.Face, filter, prompt ?? "Select a face");
            }
            else
            {
                reference = uidoc.Selection.PickObject(ObjectType.Face, prompt ?? "Select a face");
            }

            Element elem = doc.GetElement(reference.ElementId);
            GeometryObject geoObj = elem.GetGeometryObjectFromReference(reference);
            Face face = geoObj as Face;

            var faceInfo = new Dictionary<string, object>
            {
                { "elementId", GetElementIdInt(reference.ElementId) },
                { "elementName", GetElementName(elem) },
                { "elementCategory", elem.Category?.Name }
            };

            if (face != null)
            {
                faceInfo["area"] = face.Area;
                faceInfo["materialId"] = face.MaterialElementId != null ? GetElementIdInt(face.MaterialElementId) : -1;
                
                // Get face normal at center
                BoundingBoxUV bb = face.GetBoundingBox();
                UV center = new UV((bb.Min.U + bb.Max.U) / 2, (bb.Min.V + bb.Max.V) / 2);
                XYZ normal = face.ComputeNormal(center);
                faceInfo["normal"] = new { x = normal.X, y = normal.Y, z = normal.Z };
            }

            if (includeLocation)
            {
                faceInfo["location"] = GetElementLocation(elem);
            }

            return new
            {
                success = true,
                face = faceInfo
            };
        }

        private object PickEdgeOperation(UIDocument uidoc, Document doc, string prompt, string filterCategory, bool includeLocation)
        {
            ISelectionFilter filter = CreateSelectionFilter(doc, filterCategory, null);

            Reference reference;
            if (filter != null)
            {
                reference = uidoc.Selection.PickObject(ObjectType.Edge, filter, prompt ?? "Select an edge");
            }
            else
            {
                reference = uidoc.Selection.PickObject(ObjectType.Edge, prompt ?? "Select an edge");
            }

            Element elem = doc.GetElement(reference.ElementId);
            GeometryObject geoObj = elem.GetGeometryObjectFromReference(reference);
            Edge edge = geoObj as Edge;

            var edgeInfo = new Dictionary<string, object>
            {
                { "elementId", GetElementIdInt(reference.ElementId) },
                { "elementName", GetElementName(elem) },
                { "elementCategory", elem.Category?.Name }
            };

            if (edge != null)
            {
                Curve curve = edge.AsCurve();
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);
                edgeInfo["length"] = curve.Length;
                edgeInfo["startPoint"] = new { x = start.X, y = start.Y, z = start.Z };
                edgeInfo["endPoint"] = new { x = end.X, y = end.Y, z = end.Z };
            }

            if (includeLocation)
            {
                edgeInfo["location"] = GetElementLocation(elem);
            }

            return new
            {
                success = true,
                edge = edgeInfo
            };
        }

        private Dictionary<string, object> BuildElementInfo(Element elem, Document doc, bool includeLocation, bool includeParameters)
        {
            var elemInfo = new Dictionary<string, object>
            {
                { "id", GetElementIdInt(elem.Id) },
                { "name", GetElementName(elem) ?? elem.Name },
                { "category", elem.Category?.Name ?? "Unknown" }
            };

            if (elem is FamilyInstance fi)
            {
                elemInfo["familyName"] = fi.Symbol?.Family?.Name;
                elemInfo["typeName"] = fi.Symbol?.Name;
            }
            else
            {
                Element typeElem = doc.GetElement(elem.GetTypeId());
                elemInfo["typeName"] = typeElem?.Name;
            }

            if (includeLocation)
            {
                elemInfo["location"] = GetElementLocation(elem);
            }

            if (includeParameters)
            {
                var paramList = new List<object>();
                foreach (Parameter param in elem.Parameters)
                {
                    if (param.HasValue)
                    {
                        paramList.Add(new
                        {
                            name = param.Definition.Name,
                            value = GetParameterValueAsString(param)
                        });
                    }
                }
                elemInfo["parameters"] = paramList;
            }

            return elemInfo;
        }

        private ISelectionFilter CreateSelectionFilter(Document doc, string filterCategory, string filterClass)
        {
            if (string.IsNullOrEmpty(filterCategory) && string.IsNullOrEmpty(filterClass))
                return null;

            return new CustomSelectionFilter(doc, filterCategory, filterClass, GetBuiltInCategory, GetElementTypeByName);
        }

        /// <summary>
        /// TaskDialog tool - display dialogs in Revit
        /// </summary>
        private object TaskDialogTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            string mode = parameters.ContainsKey("mode") ? parameters["mode"]?.ToString()?.ToLower() : "simple";

            try
            {
                if (mode == "simple")
                {
                    return ShowSimpleDialog(parameters);
                }
                else if (mode == "custom")
                {
                    return ShowCustomDialog(parameters);
                }
                else
                {
                    return new { success = false, error = $"Unknown mode: {mode}. Use 'simple' or 'custom'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private object ShowSimpleDialog(Dictionary<string, object> parameters)
        {
            string title = parameters.ContainsKey("title") ? parameters["title"]?.ToString() : "Revit MCP";
            string message = parameters.ContainsKey("message") ? parameters["message"]?.ToString() : "";

            if (string.IsNullOrEmpty(message))
            {
                return new { success = false, error = "message is required for simple mode" };
            }

            TaskDialog.Show(title, message);

            return new
            {
                success = true,
                mode = "simple",
                title = title,
                message = message,
                result = "Ok"
            };
        }

        private object ShowCustomDialog(Dictionary<string, object> parameters)
        {
            string title = parameters.ContainsKey("title") ? parameters["title"]?.ToString() : "Revit MCP";
            TaskDialog dialog = new TaskDialog(title);

            // Main instruction (large text at top)
            if (parameters.ContainsKey("main_instruction") && parameters["main_instruction"] != null)
            {
                dialog.MainInstruction = parameters["main_instruction"].ToString();
            }

            // Main content
            if (parameters.ContainsKey("main_content") && parameters["main_content"] != null)
            {
                dialog.MainContent = parameters["main_content"].ToString();
            }

            // Expanded content
            if (parameters.ContainsKey("expanded_content") && parameters["expanded_content"] != null)
            {
                dialog.ExpandedContent = parameters["expanded_content"].ToString();
            }

            // Footer text
            if (parameters.ContainsKey("footer_text") && parameters["footer_text"] != null)
            {
                dialog.FooterText = parameters["footer_text"].ToString();
            }

            // Verification text (checkbox)
            if (parameters.ContainsKey("verification_text") && parameters["verification_text"] != null)
            {
                dialog.VerificationText = parameters["verification_text"].ToString();
            }

            // Allow cancellation
            if (parameters.ContainsKey("allow_cancellation"))
            {
                dialog.AllowCancellation = Convert.ToBoolean(parameters["allow_cancellation"]);
            }

            // Main icon
            if (parameters.ContainsKey("main_icon") && parameters["main_icon"] != null)
            {
                dialog.MainIcon = ParseTaskDialogIcon(parameters["main_icon"].ToString());
            }

            // Common buttons
            if (parameters.ContainsKey("common_buttons") && parameters["common_buttons"] != null)
            {
                dialog.CommonButtons = ParseCommonButtons(parameters["common_buttons"].ToString());
            }

            // Default button
            if (parameters.ContainsKey("default_button") && parameters["default_button"] != null)
            {
                dialog.DefaultButton = ParseTaskDialogResult(parameters["default_button"].ToString());
            }

            // Command links
            if (parameters.ContainsKey("command_links") && parameters["command_links"] != null)
            {
                var commandLinks = parameters["command_links"];
                if (commandLinks is JArray jArray)
                {
                    int linkIndex = 1;
                    foreach (var link in jArray)
                    {
                        if (linkIndex > 4) break; // Max 4 command links

                        string text = link["text"]?.ToString() ?? "";
                        string subtext = link["subtext"]?.ToString();

                        switch (linkIndex)
                        {
                            case 1:
                                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, text, subtext);
                                break;
                            case 2:
                                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, text, subtext);
                                break;
                            case 3:
                                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, text, subtext);
                                break;
                            case 4:
                                dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink4, text, subtext);
                                break;
                        }
                        linkIndex++;
                    }
                }
            }

            // Show dialog and get result
            TaskDialogResult result = dialog.Show();
            bool verificationChecked = dialog.WasVerificationChecked();

            return new
            {
                success = true,
                mode = "custom",
                title = title,
                result = result.ToString(),
                verificationChecked = verificationChecked
            };
        }

        private TaskDialogIcon ParseTaskDialogIcon(string iconName)
        {
            switch (iconName?.ToLower())
            {
                case "warning": return TaskDialogIcon.TaskDialogIconWarning;
                case "error": return TaskDialogIcon.TaskDialogIconError;
                case "information": return TaskDialogIcon.TaskDialogIconInformation;
                case "none":
                default: return TaskDialogIcon.TaskDialogIconNone;
            }
        }

        private TaskDialogCommonButtons ParseCommonButtons(string buttonName)
        {
            switch (buttonName?.ToLower())
            {
                case "ok": return TaskDialogCommonButtons.Ok;
                case "cancel": return TaskDialogCommonButtons.Cancel;
                case "yes": return TaskDialogCommonButtons.Yes;
                case "no": return TaskDialogCommonButtons.No;
                case "retry": return TaskDialogCommonButtons.Retry;
                case "close": return TaskDialogCommonButtons.Close;
                case "none":
                default: return TaskDialogCommonButtons.None;
            }
        }

        private TaskDialogResult ParseTaskDialogResult(string resultName)
        {
            switch (resultName?.ToLower())
            {
                case "ok": return TaskDialogResult.Ok;
                case "cancel": return TaskDialogResult.Cancel;
                case "yes": return TaskDialogResult.Yes;
                case "no": return TaskDialogResult.No;
                case "retry": return TaskDialogResult.Retry;
                case "close": return TaskDialogResult.Close;
                case "commandlink1": return TaskDialogResult.CommandLink1;
                case "commandlink2": return TaskDialogResult.CommandLink2;
                case "commandlink3": return TaskDialogResult.CommandLink3;
                case "commandlink4": return TaskDialogResult.CommandLink4;
                case "none":
                default: return TaskDialogResult.None;
            }
        }

        /// <summary>
        /// Ribbon tool - create and manage Revit Ribbon UI elements
        /// </summary>
        private object RibbonTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            UIControlledApplication uiControlledApp = Application.UIControlledApp;
            if (uiControlledApp == null)
            {
                return new { success = false, error = "UIControlledApplication not available. Ribbon operations require application startup context." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required" };
            }

            try
            {
                switch (operation)
                {
                    case "create_tab":
                        return CreateRibbonTab(uiControlledApp, parameters);

                    case "create_panel":
                        return CreateRibbonPanel(uiControlledApp, parameters);

                    case "create_push_button":
                        return CreatePushButton(uiControlledApp, parameters);

                    case "create_split_button":
                        return CreateSplitButton(uiControlledApp, parameters);

                    case "create_pulldown_button":
                        return CreatePulldownButton(uiControlledApp, parameters);

                    case "create_combo_box":
                        return CreateComboBox(uiControlledApp, parameters);

                    case "create_text_box":
                        return CreateTextBox(uiControlledApp, parameters);

                    case "create_stacked_items":
                        return CreateStackedItems(uiControlledApp, parameters);

                    case "list_tabs":
                        return ListRibbonTabs(uiControlledApp);

                    case "list_panels":
                        return ListRibbonPanels(uiControlledApp, parameters);

                    case "get_panel_items":
                        return GetPanelItems(uiControlledApp, parameters);

                    case "get_image_folder":
                        return GetImageFolderInfo(parameters);

                    case "list_images":
                        return ListImagesInFolder(parameters);

                    default:
                        return new { success = false, error = $"Unknown ribbon operation: {operation}" };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private object CreateRibbonTab(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            if (string.IsNullOrEmpty(tabName))
            {
                return new { success = false, error = "tab_name is required" };
            }

            try
            {
                app.CreateRibbonTab(tabName);
                return new { success = true, message = $"Tab '{tabName}' created successfully" };
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
                return new { success = false, error = $"Tab '{tabName}' already exists" };
            }
        }

        private object CreateRibbonPanel(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName))
            {
                return new { success = false, error = "panel_name is required" };
            }

            try
            {
                RibbonPanel panel;
                if (!string.IsNullOrEmpty(tabName))
                {
                    panel = app.CreateRibbonPanel(tabName, panelName);
                }
                else
                {
                    panel = app.CreateRibbonPanel(panelName);
                }

                return new
                {
                    success = true,
                    message = $"Panel '{panelName}' created successfully",
                    panelName = panel.Name,
                    tabName = tabName ?? "Add-Ins"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private RibbonPanel GetRibbonPanel(UIControlledApplication app, string tabName, string panelName)
        {
            try
            {
                IList<RibbonPanel> panels;
                if (!string.IsNullOrEmpty(tabName))
                {
                    panels = app.GetRibbonPanels(tabName);
                }
                else
                {
                    panels = app.GetRibbonPanels();
                }

                return panels?.FirstOrDefault(p => p.Name == panelName);
            }
            catch
            {
                return null;
            }
        }

        private object CreatePushButton(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;
            string buttonName = parameters.ContainsKey("button_name") ? parameters["button_name"]?.ToString() : null;
            string buttonText = parameters.ContainsKey("button_text") ? parameters["button_text"]?.ToString() : null;
            string className = parameters.ContainsKey("class_name") ? parameters["class_name"]?.ToString() : null;
            string tooltip = parameters.ContainsKey("tooltip") ? parameters["tooltip"]?.ToString() : null;
            string longDescription = parameters.ContainsKey("long_description") ? parameters["long_description"]?.ToString() : null;
            string largeImagePath = parameters.ContainsKey("large_image") ? parameters["large_image"]?.ToString() : null;
            string smallImagePath = parameters.ContainsKey("small_image") ? parameters["small_image"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName) || string.IsNullOrEmpty(buttonName) || string.IsNullOrEmpty(buttonText))
            {
                return new { success = false, error = "panel_name, button_name, and button_text are required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            // Use current assembly if no class specified
            string assemblyPath = typeof(Application).Assembly.Location;
            string commandClass = className ?? typeof(MCPStatusCommand).FullName;

            PushButtonData buttonData = new PushButtonData(buttonName, buttonText, assemblyPath, commandClass);

            PushButton button = panel.AddItem(buttonData) as PushButton;
            if (button != null)
            {
                if (!string.IsNullOrEmpty(tooltip))
                    button.ToolTip = tooltip;
                if (!string.IsNullOrEmpty(longDescription))
                    button.LongDescription = longDescription;
                
                // Set large image (32x32)
                if (!string.IsNullOrEmpty(largeImagePath))
                {
                    BitmapImage largeImage = GetBitmapImageFromFolder(largeImagePath) ?? GetBitmapImage(largeImagePath);
                    if (largeImage != null)
                        button.LargeImage = largeImage;
                }
                
                // Set small image (16x16)
                if (!string.IsNullOrEmpty(smallImagePath))
                {
                    BitmapImage smallImage = GetBitmapImageFromFolder(smallImagePath) ?? GetBitmapImage(smallImagePath);
                    if (smallImage != null)
                        button.Image = smallImage;
                }
            }

            return new
            {
                success = true,
                message = $"Push button '{buttonText}' created successfully",
                buttonName = buttonName,
                panelName = panelName
            };
        }

        private object CreateSplitButton(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;
            string buttonName = parameters.ContainsKey("button_name") ? parameters["button_name"]?.ToString() : null;
            string buttonText = parameters.ContainsKey("button_text") ? parameters["button_text"]?.ToString() : null;
            string tooltip = parameters.ContainsKey("tooltip") ? parameters["tooltip"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName) || string.IsNullOrEmpty(buttonName) || string.IsNullOrEmpty(buttonText))
            {
                return new { success = false, error = "panel_name, button_name, and button_text are required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            SplitButtonData splitData = new SplitButtonData(buttonName, buttonText);
            SplitButton splitButton = panel.AddItem(splitData) as SplitButton;

            if (splitButton != null && !string.IsNullOrEmpty(tooltip))
            {
                splitButton.ToolTip = tooltip;
            }

            // Add sub-buttons
            int subButtonCount = 0;
            if (parameters.ContainsKey("sub_buttons") && parameters["sub_buttons"] != null)
            {
                var subButtons = parameters["sub_buttons"];
                if (subButtons is JArray jArray)
                {
                    string assemblyPath = typeof(Application).Assembly.Location;
                    foreach (var subBtn in jArray)
                    {
                        string subName = subBtn["name"]?.ToString();
                        string subText = subBtn["text"]?.ToString();
                        string subClass = subBtn["class_name"]?.ToString() ?? typeof(MCPStatusCommand).FullName;
                        string subTooltip = subBtn["tooltip"]?.ToString();

                        if (!string.IsNullOrEmpty(subName) && !string.IsNullOrEmpty(subText))
                        {
                            PushButtonData pbData = new PushButtonData(subName, subText, assemblyPath, subClass);
                            PushButton pb = splitButton.AddPushButton(pbData);
                            if (pb != null && !string.IsNullOrEmpty(subTooltip))
                            {
                                pb.ToolTip = subTooltip;
                            }
                            subButtonCount++;
                        }
                    }
                }
            }

            return new
            {
                success = true,
                message = $"Split button '{buttonText}' created with {subButtonCount} sub-buttons",
                buttonName = buttonName,
                subButtonCount = subButtonCount
            };
        }

        private object CreatePulldownButton(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;
            string buttonName = parameters.ContainsKey("button_name") ? parameters["button_name"]?.ToString() : null;
            string buttonText = parameters.ContainsKey("button_text") ? parameters["button_text"]?.ToString() : null;
            string tooltip = parameters.ContainsKey("tooltip") ? parameters["tooltip"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName) || string.IsNullOrEmpty(buttonName) || string.IsNullOrEmpty(buttonText))
            {
                return new { success = false, error = "panel_name, button_name, and button_text are required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            PulldownButtonData pulldownData = new PulldownButtonData(buttonName, buttonText);
            PulldownButton pulldownButton = panel.AddItem(pulldownData) as PulldownButton;

            if (pulldownButton != null && !string.IsNullOrEmpty(tooltip))
            {
                pulldownButton.ToolTip = tooltip;
            }

            // Add sub-buttons
            int subButtonCount = 0;
            if (parameters.ContainsKey("sub_buttons") && parameters["sub_buttons"] != null)
            {
                var subButtons = parameters["sub_buttons"];
                if (subButtons is JArray jArray)
                {
                    string assemblyPath = typeof(Application).Assembly.Location;
                    foreach (var subBtn in jArray)
                    {
                        string subName = subBtn["name"]?.ToString();
                        string subText = subBtn["text"]?.ToString();
                        string subClass = subBtn["class_name"]?.ToString() ?? typeof(MCPStatusCommand).FullName;
                        string subTooltip = subBtn["tooltip"]?.ToString();

                        if (!string.IsNullOrEmpty(subName) && !string.IsNullOrEmpty(subText))
                        {
                            PushButtonData pbData = new PushButtonData(subName, subText, assemblyPath, subClass);
                            PushButton pb = pulldownButton.AddPushButton(pbData);
                            if (pb != null && !string.IsNullOrEmpty(subTooltip))
                            {
                                pb.ToolTip = subTooltip;
                            }
                            subButtonCount++;
                        }
                    }
                }
            }

            return new
            {
                success = true,
                message = $"Pulldown button '{buttonText}' created with {subButtonCount} items",
                buttonName = buttonName,
                subButtonCount = subButtonCount
            };
        }

        private object CreateComboBox(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;
            string buttonName = parameters.ContainsKey("button_name") ? parameters["button_name"]?.ToString() : null;
            string tooltip = parameters.ContainsKey("tooltip") ? parameters["tooltip"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName) || string.IsNullOrEmpty(buttonName))
            {
                return new { success = false, error = "panel_name and button_name are required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            ComboBoxData comboData = new ComboBoxData(buttonName);
            ComboBox comboBox = panel.AddItem(comboData) as ComboBox;

            if (comboBox != null)
            {
                if (!string.IsNullOrEmpty(tooltip))
                    comboBox.ToolTip = tooltip;

                // Add combo items
                int itemCount = 0;
                if (parameters.ContainsKey("combo_items") && parameters["combo_items"] != null)
                {
                    var items = parameters["combo_items"];
                    if (items is JArray jArray)
                    {
                        foreach (var item in jArray)
                        {
                            string itemName = item["name"]?.ToString();
                            string itemText = item["text"]?.ToString();
                            string groupName = item["group_name"]?.ToString();

                            if (!string.IsNullOrEmpty(itemName) && !string.IsNullOrEmpty(itemText))
                            {
                                if (!string.IsNullOrEmpty(groupName))
                                {
                                    comboBox.AddItem(new ComboBoxMemberData(itemName, itemText) { GroupName = groupName });
                                }
                                else
                                {
                                    comboBox.AddItem(new ComboBoxMemberData(itemName, itemText));
                                }
                                itemCount++;
                            }
                        }
                    }
                }

                return new
                {
                    success = true,
                    message = $"Combo box '{buttonName}' created with {itemCount} items",
                    comboBoxName = buttonName,
                    itemCount = itemCount
                };
            }

            return new { success = false, error = "Failed to create combo box" };
        }

        private object CreateTextBox(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;
            string buttonName = parameters.ContainsKey("button_name") ? parameters["button_name"]?.ToString() : null;
            string tooltip = parameters.ContainsKey("tooltip") ? parameters["tooltip"]?.ToString() : null;
            string prompt = parameters.ContainsKey("text_box_prompt") ? parameters["text_box_prompt"]?.ToString() : null;
            double width = parameters.ContainsKey("text_box_width") ? Convert.ToDouble(parameters["text_box_width"]) : 150;

            if (string.IsNullOrEmpty(panelName) || string.IsNullOrEmpty(buttonName))
            {
                return new { success = false, error = "panel_name and button_name are required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            TextBoxData textBoxData = new TextBoxData(buttonName);
            TextBox textBox = panel.AddItem(textBoxData) as TextBox;

            if (textBox != null)
            {
                textBox.Width = width;
                if (!string.IsNullOrEmpty(tooltip))
                    textBox.ToolTip = tooltip;
                if (!string.IsNullOrEmpty(prompt))
                    textBox.PromptText = prompt;

                return new
                {
                    success = true,
                    message = $"Text box '{buttonName}' created successfully",
                    textBoxName = buttonName
                };
            }

            return new { success = false, error = "Failed to create text box" };
        }

        private object CreateStackedItems(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName))
            {
                return new { success = false, error = "panel_name is required" };
            }

            if (!parameters.ContainsKey("stacked_items") || parameters["stacked_items"] == null)
            {
                return new { success = false, error = "stacked_items array is required (2-3 items)" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            var stackedItems = parameters["stacked_items"];
            if (!(stackedItems is JArray jArray) || jArray.Count < 2 || jArray.Count > 3)
            {
                return new { success = false, error = "stacked_items must contain 2-3 items" };
            }

            string assemblyPath = typeof(Application).Assembly.Location;
            var itemDataList = new List<RibbonItemData>();

            foreach (var item in jArray)
            {
                string itemType = item["type"]?.ToString()?.ToLower();
                string itemName = item["name"]?.ToString();
                string itemText = item["text"]?.ToString();
                string itemClass = item["class_name"]?.ToString() ?? typeof(MCPStatusCommand).FullName;

                if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(itemText))
                    continue;

                switch (itemType)
                {
                    case "push_button":
                        itemDataList.Add(new PushButtonData(itemName, itemText, assemblyPath, itemClass));
                        break;
                    case "pulldown_button":
                        itemDataList.Add(new PulldownButtonData(itemName, itemText));
                        break;
                    case "combo_box":
                        itemDataList.Add(new ComboBoxData(itemName));
                        break;
                    case "text_box":
                        itemDataList.Add(new TextBoxData(itemName));
                        break;
                }
            }

            if (itemDataList.Count < 2)
            {
                return new { success = false, error = "At least 2 valid stacked items are required" };
            }

            IList<RibbonItem> createdItems;
            if (itemDataList.Count == 2)
            {
                createdItems = panel.AddStackedItems(itemDataList[0], itemDataList[1]);
            }
            else
            {
                createdItems = panel.AddStackedItems(itemDataList[0], itemDataList[1], itemDataList[2]);
            }

            return new
            {
                success = true,
                message = $"Created {createdItems.Count} stacked items",
                itemCount = createdItems.Count
            };
        }

        private object ListRibbonTabs(UIControlledApplication app)
        {
            // Note: Revit API doesn't provide a direct way to list all tabs
            // We can only list panels, which gives us tab information indirectly
            var tabsInfo = new List<string> { "Add-Ins" }; // Default tab always exists
            
            return new
            {
                success = true,
                message = "Note: Only custom tabs created by this add-in can be reliably tracked",
                knownTabs = tabsInfo
            };
        }

        private object ListRibbonPanels(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;

            try
            {
                IList<RibbonPanel> panels;
                if (!string.IsNullOrEmpty(tabName))
                {
                    panels = app.GetRibbonPanels(tabName);
                }
                else
                {
                    panels = app.GetRibbonPanels();
                }

                var panelNames = panels?.Select(p => new { name = p.Name, visible = p.Visible }).ToList();

                return new
                {
                    success = true,
                    tabName = tabName ?? "Add-Ins",
                    panelCount = panelNames?.Count ?? 0,
                    panels = panelNames
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private object GetPanelItems(UIControlledApplication app, Dictionary<string, object> parameters)
        {
            string tabName = parameters.ContainsKey("tab_name") ? parameters["tab_name"]?.ToString() : null;
            string panelName = parameters.ContainsKey("panel_name") ? parameters["panel_name"]?.ToString() : null;

            if (string.IsNullOrEmpty(panelName))
            {
                return new { success = false, error = "panel_name is required" };
            }

            RibbonPanel panel = GetRibbonPanel(app, tabName, panelName);
            if (panel == null)
            {
                return new { success = false, error = $"Panel '{panelName}' not found" };
            }

            var items = panel.GetItems();
            var itemInfoList = items.Select(item => new
            {
                name = item.Name,
                itemType = item.ItemType.ToString(),
                visible = item.Visible,
                enabled = item.Enabled,
                toolTip = item.ToolTip
            }).ToList();

            return new
            {
                success = true,
                panelName = panelName,
                itemCount = itemInfoList.Count,
                items = itemInfoList
            };
        }

        private object GetImageFolderInfo(Dictionary<string, object> parameters)
        {
            string folderName = parameters.ContainsKey("image_folder") ? parameters["image_folder"]?.ToString() : "Images";
            string imageFolderPath = GetImageFolder(folderName);
            
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

            return new
            {
                success = true,
                assemblyLocation = assemblyPath,
                assemblyDirectory = assemblyDir,
                imageFolderName = folderName,
                imageFolderPath = imageFolderPath,
                imageFolderExists = imageFolderPath != null
            };
        }

        private object ListImagesInFolder(Dictionary<string, object> parameters)
        {
            string folderName = parameters.ContainsKey("image_folder") ? parameters["image_folder"]?.ToString() : "Images";
            string[] images = ListImageFiles(folderName);
            string imageFolderPath = GetImageFolder(folderName);

            return new
            {
                success = true,
                folderName = folderName,
                folderPath = imageFolderPath,
                imageCount = images.Length,
                images = images
            };
        }

        #region Image Utility Functions

        /// <summary>
        /// Get the path to the images folder relative to the add-in assembly location
        /// </summary>
        /// <param name="folderName">Name of the images folder (default: "Images")</param>
        /// <returns>Full path to the images folder, or null if not found</returns>
        public static string GetImageFolder(string folderName = "Images")
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            
            // Check in the same directory as the assembly
            string imagePath = Path.Combine(assemblyDir, folderName);
            if (Directory.Exists(imagePath))
                return imagePath;

            // Check in parent directory
            string parentDir = Directory.GetParent(assemblyDir)?.FullName;
            if (parentDir != null)
            {
                imagePath = Path.Combine(parentDir, folderName);
                if (Directory.Exists(imagePath))
                    return imagePath;
            }

            // Check in Resources subfolder
            imagePath = Path.Combine(assemblyDir, "Resources", folderName);
            if (Directory.Exists(imagePath))
                return imagePath;

            return null;
        }

        /// <summary>
        /// Create a BitmapImage from an image file path
        /// </summary>
        /// <param name="imagePath">Full path to the image file</param>
        /// <returns>BitmapImage object, or null if the file doesn't exist</returns>
        public static BitmapImage GetBitmapImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
                return null;

            try
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Makes it cross-thread accessible
                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Create a BitmapImage from an image file in the images folder
        /// </summary>
        /// <param name="fileName">Image file name (e.g., "icon.png")</param>
        /// <param name="folderName">Name of the images folder (default: "Images")</param>
        /// <returns>BitmapImage object, or null if not found</returns>
        public static BitmapImage GetBitmapImageFromFolder(string fileName, string folderName = "Images")
        {
            string imageFolder = GetImageFolder(folderName);
            if (imageFolder == null)
                return null;

            string imagePath = Path.Combine(imageFolder, fileName);
            return GetBitmapImage(imagePath);
        }

        /// <summary>
        /// Create a BitmapImage from an embedded resource
        /// </summary>
        /// <param name="resourceName">Resource name in format "Namespace.Folder.FileName.ext"</param>
        /// <returns>BitmapImage object, or null if resource not found</returns>
        public static BitmapImage GetBitmapImageFromResource(string resourceName)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        return null;

                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// List all image files in the images folder
        /// </summary>
        /// <param name="folderName">Name of the images folder (default: "Images")</param>
        /// <param name="searchPattern">Search pattern (default: "*.png")</param>
        /// <returns>Array of image file names</returns>
        public static string[] ListImageFiles(string folderName = "Images", string searchPattern = "*.*")
        {
            string imageFolder = GetImageFolder(folderName);
            if (imageFolder == null || !Directory.Exists(imageFolder))
                return new string[0];

            try
            {
                string[] extensions = { ".png", ".ico", ".jpg", ".jpeg", ".bmp", ".gif" };
                return Directory.GetFiles(imageFolder, searchPattern)
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()))
                    .Select(Path.GetFileName)
                    .ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        #endregion

        #region Family Points Tool

        /// <summary>
        /// Family Points Tool - create reference points in conceptual mass/adaptive families
        /// </summary>
        private object FamilyPointsTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required" };
            }

            try
            {
                switch (operation)
                {
                    case "create_single_point":
                        return CreateSingleReferencePoint(doc, parameters);

                    case "create_point_row":
                        return CreatePointRow(doc, parameters);

                    case "create_point_grid":
                        return CreatePointGrid(doc, parameters);

                    case "create_point_grid_formula":
                        return CreatePointGridWithFormula(doc, parameters);

                    case "get_reference_points":
                        return GetReferencePoints(doc);

                    case "delete_reference_points":
                        return DeleteReferencePoints(doc, parameters);

                    case "create_curve_by_points":
                        return CreateCurveByPoints(doc, parameters);

                    case "create_curves_from_grid":
                        return CreateCurvesFromGrid(doc, parameters);

                    case "get_curves_by_points":
                        return GetCurvesByPoints(doc);

                    case "create_loft_form":
                        return CreateLoftForm(doc, parameters);

                    case "get_forms":
                        return GetForms(doc);

                    case "create_revolve_axis":
                        return CreateRevolveAxis(doc, parameters);

                    case "create_revolve_profile":
                        return CreateRevolveProfile(doc, parameters);

                    case "create_revolve_form":
                        return CreateRevolveForm(doc, parameters);

                    default:
                        return new { success = false, error = $"Unknown operation: {operation}" };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private object CreateSingleReferencePoint(Document doc, Dictionary<string, object> parameters)
        {
            double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

            XYZ point = new XYZ(x, y, z);

            using (Transaction trans = new Transaction(doc, "Create Reference Point"))
            {
                trans.Start();
                
                ReferencePoint refPoint = doc.FamilyCreate.NewReferencePoint(point);
                
                trans.Commit();

                return new
                {
                    success = true,
                    message = "Reference point created",
                    pointId = GetElementIdInt(refPoint.Id),
                    location = new { x = x, y = y, z = z }
                };
            }
        }

        private object CreatePointRow(Document doc, Dictionary<string, object> parameters)
        {
            double startX = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double startY = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double startZ = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;
            int count = parameters.ContainsKey("count_x") ? Convert.ToInt32(parameters["count_x"]) : 5;
            double spacing = parameters.ContainsKey("spacing_x") ? Convert.ToDouble(parameters["spacing_x"]) : 1.0;
            string direction = parameters.ContainsKey("direction") ? parameters["direction"]?.ToString()?.ToLower() : "x";

            if (count < 1 || count > 1000)
            {
                return new { success = false, error = "count_x must be between 1 and 1000" };
            }

            var createdPoints = new List<object>();

            using (Transaction trans = new Transaction(doc, "Create Point Row"))
            {
                trans.Start();

                for (int i = 0; i < count; i++)
                {
                    double x = startX;
                    double y = startY;
                    double z = startZ;

                    switch (direction)
                    {
                        case "x":
                            x = startX + (i * spacing);
                            break;
                        case "y":
                            y = startY + (i * spacing);
                            break;
                        case "z":
                            z = startZ + (i * spacing);
                            break;
                    }

                    XYZ point = new XYZ(x, y, z);
                    ReferencePoint refPoint = doc.FamilyCreate.NewReferencePoint(point);
                    
                    createdPoints.Add(new
                    {
                        id = GetElementIdInt(refPoint.Id),
                        index = i,
                        x = x,
                        y = y,
                        z = z
                    });
                }

                trans.Commit();
            }

            return new
            {
                success = true,
                message = $"Created {createdPoints.Count} reference points in a row",
                direction = direction,
                count = createdPoints.Count,
                spacing = spacing,
                points = createdPoints
            };
        }

        private object CreatePointGrid(Document doc, Dictionary<string, object> parameters)
        {
            double startX = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double startY = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double startZ = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;
            int countX = parameters.ContainsKey("count_x") ? Convert.ToInt32(parameters["count_x"]) : 5;
            int countY = parameters.ContainsKey("count_y") ? Convert.ToInt32(parameters["count_y"]) : 5;
            double spacingX = parameters.ContainsKey("spacing_x") ? Convert.ToDouble(parameters["spacing_x"]) : 1.0;
            double spacingY = parameters.ContainsKey("spacing_y") ? Convert.ToDouble(parameters["spacing_y"]) : 1.0;

            if (countX < 1 || countX > 100 || countY < 1 || countY > 100)
            {
                return new { success = false, error = "count_x and count_y must be between 1 and 100" };
            }

            var createdPoints = new List<object>();

            using (Transaction trans = new Transaction(doc, "Create Point Grid"))
            {
                trans.Start();

                for (int i = 0; i < countX; i++)
                {
                    for (int j = 0; j < countY; j++)
                    {
                        double x = startX + (i * spacingX);
                        double y = startY + (j * spacingY);
                        double z = startZ;

                        XYZ point = new XYZ(x, y, z);
                        ReferencePoint refPoint = doc.FamilyCreate.NewReferencePoint(point);

                        createdPoints.Add(new
                        {
                            id = GetElementIdInt(refPoint.Id),
                            indexX = i,
                            indexY = j,
                            x = x,
                            y = y,
                            z = z
                        });
                    }
                }

                trans.Commit();
            }

            return new
            {
                success = true,
                message = $"Created {createdPoints.Count} reference points in a {countX}x{countY} grid",
                gridSizeX = countX,
                gridSizeY = countY,
                spacingX = spacingX,
                spacingY = spacingY,
                totalPoints = createdPoints.Count,
                points = createdPoints
            };
        }

        private object CreatePointGridWithFormula(Document doc, Dictionary<string, object> parameters)
        {
            double startX = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double startY = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            int countX = parameters.ContainsKey("count_x") ? Convert.ToInt32(parameters["count_x"]) : 5;
            int countY = parameters.ContainsKey("count_y") ? Convert.ToInt32(parameters["count_y"]) : 5;
            double spacingX = parameters.ContainsKey("spacing_x") ? Convert.ToDouble(parameters["spacing_x"]) : 1.0;
            double spacingY = parameters.ContainsKey("spacing_y") ? Convert.ToDouble(parameters["spacing_y"]) : 1.0;
            string zFormula = parameters.ContainsKey("z_formula") ? parameters["z_formula"]?.ToString() : null;

            if (string.IsNullOrEmpty(zFormula))
            {
                return new { success = false, error = "z_formula is required. Example: '10*cos(x)+10*sin(y)'" };
            }

            if (countX < 1 || countX > 100 || countY < 1 || countY > 100)
            {
                return new { success = false, error = "count_x and count_y must be between 1 and 100" };
            }

            var createdPoints = new List<object>();

            using (Transaction trans = new Transaction(doc, "Create Point Grid with Formula"))
            {
                trans.Start();

                for (int i = 0; i < countX; i++)
                {
                    for (int j = 0; j < countY; j++)
                    {
                        double x = startX + (i * spacingX);
                        double y = startY + (j * spacingY);
                        double z = EvaluateZFormula(zFormula, x, y);

                        XYZ point = new XYZ(x, y, z);
                        ReferencePoint refPoint = doc.FamilyCreate.NewReferencePoint(point);

                        createdPoints.Add(new
                        {
                            id = GetElementIdInt(refPoint.Id),
                            indexX = i,
                            indexY = j,
                            x = x,
                            y = y,
                            z = z
                        });
                    }
                }

                trans.Commit();
            }

            return new
            {
                success = true,
                message = $"Created {createdPoints.Count} reference points in a {countX}x{countY} grid with formula",
                formula = zFormula,
                gridSizeX = countX,
                gridSizeY = countY,
                spacingX = spacingX,
                spacingY = spacingY,
                totalPoints = createdPoints.Count,
                points = createdPoints
            };
        }

        /// <summary>
        /// Evaluate z = f(x, y) formula. Supports: sin, cos, tan, sqrt, abs, pow, exp, log, pi, +, -, *, /, (, )
        /// Example formulas: "10*cos(x)+10*sin(y)", "sqrt(x*x+y*y)", "5*sin(x*0.5)*cos(y*0.5)"
        /// </summary>
        private double EvaluateZFormula(string formula, double x, double y)
        {
            // Replace variables and constants
            string expr = formula.ToLower()
                .Replace("pi", Math.PI.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .Replace(" ", "");

            // Replace x and y with actual values (use markers to avoid partial replacement)
            expr = ReplaceVariable(expr, "x", x);
            expr = ReplaceVariable(expr, "y", y);

            // Evaluate the expression
            return EvaluateMathExpression(expr);
        }

        private string ReplaceVariable(string expr, string varName, double value)
        {
            // Use regex to replace standalone variable names
            string pattern = $@"(?<![a-z]){varName}(?![a-z])";
            return System.Text.RegularExpressions.Regex.Replace(
                expr, 
                pattern, 
                value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
        }

        private double EvaluateMathExpression(string expr)
        {
            // Process functions first (innermost to outermost)
            while (true)
            {
                int funcIndex = -1;
                string funcName = null;
                string[] functions = { "sin", "cos", "tan", "sqrt", "abs", "exp", "log", "asin", "acos", "atan" };

                foreach (var func in functions)
                {
                    int idx = expr.LastIndexOf(func + "(");
                    if (idx > funcIndex)
                    {
                        funcIndex = idx;
                        funcName = func;
                    }
                }

                if (funcIndex == -1) break;

                // Find matching parenthesis
                int parenStart = funcIndex + funcName.Length;
                int parenEnd = FindMatchingParen(expr, parenStart);
                if (parenEnd == -1) throw new Exception($"Unmatched parenthesis in formula");

                string innerExpr = expr.Substring(parenStart + 1, parenEnd - parenStart - 1);
                double innerVal = EvaluateMathExpression(innerExpr);
                double result = ApplyFunction(funcName, innerVal);

                expr = expr.Substring(0, funcIndex) + 
                       result.ToString(System.Globalization.CultureInfo.InvariantCulture) + 
                       expr.Substring(parenEnd + 1);
            }

            // Handle pow(a,b) separately
            while (expr.Contains("pow("))
            {
                int idx = expr.LastIndexOf("pow(");
                int parenEnd = FindMatchingParen(expr, idx + 3);
                if (parenEnd == -1) throw new Exception("Unmatched parenthesis in pow()");

                string innerExpr = expr.Substring(idx + 4, parenEnd - idx - 4);
                string[] parts = SplitByComma(innerExpr);
                if (parts.Length != 2) throw new Exception("pow() requires two arguments");

                double baseVal = EvaluateMathExpression(parts[0]);
                double expVal = EvaluateMathExpression(parts[1]);
                double result = Math.Pow(baseVal, expVal);

                expr = expr.Substring(0, idx) + 
                       result.ToString(System.Globalization.CultureInfo.InvariantCulture) + 
                       expr.Substring(parenEnd + 1);
            }

            // Process remaining parentheses
            while (expr.Contains("("))
            {
                int parenStart = expr.LastIndexOf("(");
                int parenEnd = FindMatchingParen(expr, parenStart);
                if (parenEnd == -1) throw new Exception("Unmatched parenthesis");

                string innerExpr = expr.Substring(parenStart + 1, parenEnd - parenStart - 1);
                double result = EvaluateSimpleExpression(innerExpr);

                expr = expr.Substring(0, parenStart) + 
                       result.ToString(System.Globalization.CultureInfo.InvariantCulture) + 
                       expr.Substring(parenEnd + 1);
            }

            return EvaluateSimpleExpression(expr);
        }

        private int FindMatchingParen(string expr, int openIndex)
        {
            if (openIndex >= expr.Length || expr[openIndex] != '(') return -1;
            int depth = 1;
            for (int i = openIndex + 1; i < expr.Length; i++)
            {
                if (expr[i] == '(') depth++;
                else if (expr[i] == ')') depth--;
                if (depth == 0) return i;
            }
            return -1;
        }

        private string[] SplitByComma(string expr)
        {
            var parts = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i < expr.Length; i++)
            {
                if (expr[i] == '(') depth++;
                else if (expr[i] == ')') depth--;
                else if (expr[i] == ',' && depth == 0)
                {
                    parts.Add(expr.Substring(start, i - start));
                    start = i + 1;
                }
            }
            parts.Add(expr.Substring(start));
            return parts.ToArray();
        }

        private double ApplyFunction(string funcName, double value)
        {
            switch (funcName)
            {
                case "sin": return Math.Sin(value);
                case "cos": return Math.Cos(value);
                case "tan": return Math.Tan(value);
                case "sqrt": return Math.Sqrt(value);
                case "abs": return Math.Abs(value);
                case "exp": return Math.Exp(value);
                case "log": return Math.Log(value);
                case "asin": return Math.Asin(value);
                case "acos": return Math.Acos(value);
                case "atan": return Math.Atan(value);
                default: throw new Exception($"Unknown function: {funcName}");
            }
        }

        private double EvaluateSimpleExpression(string expr)
        {
            // Tokenize: numbers and operators
            var tokens = new List<object>();
            int i = 0;
            while (i < expr.Length)
            {
                if (char.IsDigit(expr[i]) || expr[i] == '.')
                {
                    int start = i;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                        i++;
                    tokens.Add(double.Parse(expr.Substring(start, i - start), System.Globalization.CultureInfo.InvariantCulture));
                }
                else if (expr[i] == '+' || expr[i] == '*' || expr[i] == '/')
                {
                    tokens.Add(expr[i]);
                    i++;
                }
                else if (expr[i] == '-')
                {
                    // Check if this is a negative sign or subtraction
                    if (tokens.Count == 0 || tokens[tokens.Count - 1] is char)
                    {
                        // Negative number
                        i++;
                        int start = i;
                        while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                            i++;
                        if (i > start)
                            tokens.Add(-double.Parse(expr.Substring(start, i - start), System.Globalization.CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        tokens.Add('-');
                        i++;
                    }
                }
                else
                {
                    i++;
                }
            }

            if (tokens.Count == 0) return 0;
            if (tokens.Count == 1 && tokens[0] is double) return (double)tokens[0];

            // First pass: handle * and /
            var simplified = new List<object>();
            i = 0;
            while (i < tokens.Count)
            {
                if (tokens[i] is char op && (op == '*' || op == '/'))
                {
                    double left = (double)simplified[simplified.Count - 1];
                    double right = (double)tokens[i + 1];
                    simplified[simplified.Count - 1] = (op == '*') ? left * right : left / right;
                    i += 2;
                }
                else
                {
                    simplified.Add(tokens[i]);
                    i++;
                }
            }

            // Second pass: handle + and -
            double result = (double)simplified[0];
            i = 1;
            while (i < simplified.Count)
            {
                if (simplified[i] is char op)
                {
                    double right = (double)simplified[i + 1];
                    result = (op == '+') ? result + right : result - right;
                    i += 2;
                }
                else
                {
                    i++;
                }
            }

            return result;
        }

        private object GetReferencePoints(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<Element> refPoints = collector.OfClass(typeof(ReferencePoint)).ToElements();

            var pointsList = new List<object>();

            foreach (Element elem in refPoints)
            {
                ReferencePoint refPoint = elem as ReferencePoint;
                if (refPoint != null)
                {
                    XYZ position = refPoint.Position;
                    pointsList.Add(new
                    {
                        id = GetElementIdInt(refPoint.Id),
                        name = refPoint.Name,
                        x = position.X,
                        y = position.Y,
                        z = position.Z
                    });
                }
            }

            return new
            {
                success = true,
                count = pointsList.Count,
                referencePoints = pointsList
            };
        }

        private object DeleteReferencePoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("point_ids") || parameters["point_ids"] == null)
            {
                return new { success = false, error = "point_ids array is required" };
            }

            var pointIds = new List<ElementId>();
            var idsObj = parameters["point_ids"];

            if (idsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    pointIds.Add(new ElementId(Convert.ToInt32(item)));
                }
            }
            else if (idsObj is IEnumerable<object> idList)
            {
                foreach (var id in idList)
                {
                    pointIds.Add(new ElementId(Convert.ToInt32(id)));
                }
            }

            if (pointIds.Count == 0)
            {
                return new { success = false, error = "No valid point IDs provided" };
            }

            int deletedCount = 0;
            var failedIds = new List<int>();

            using (Transaction trans = new Transaction(doc, "Delete Reference Points"))
            {
                trans.Start();

                foreach (ElementId id in pointIds)
                {
                    Element elem = doc.GetElement(id);
                    if (elem != null && elem is ReferencePoint)
                    {
                        doc.Delete(id);
                        deletedCount++;
                    }
                    else
                    {
                        failedIds.Add(GetElementIdInt(id));
                    }
                }

                trans.Commit();
            }

            return new
            {
                success = true,
                deletedCount = deletedCount,
                failedCount = failedIds.Count,
                failedIds = failedIds
            };
        }

        private object CreateCurveByPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("point_ids") || parameters["point_ids"] == null)
            {
                return new { success = false, error = "point_ids array is required (minimum 2 points)" };
            }

            var pointIds = new List<ElementId>();
            var idsObj = parameters["point_ids"];

            if (idsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    pointIds.Add(new ElementId(Convert.ToInt32(item)));
                }
            }
            else if (idsObj is IEnumerable<object> idList)
            {
                foreach (var id in idList)
                {
                    pointIds.Add(new ElementId(Convert.ToInt32(id)));
                }
            }

            if (pointIds.Count < 2)
            {
                return new { success = false, error = "At least 2 point IDs are required to create a curve" };
            }

            // Get reference points
            var refPointArray = new ReferencePointArray();
            foreach (var id in pointIds)
            {
                Element elem = doc.GetElement(id);
                if (elem is ReferencePoint refPoint)
                {
                    refPointArray.Append(refPoint);
                }
            }

            if (refPointArray.Size < 2)
            {
                return new { success = false, error = "Could not find at least 2 valid reference points" };
            }

            bool isReferenceLine = parameters.ContainsKey("is_reference_line") && Convert.ToBoolean(parameters["is_reference_line"]);

            using (Transaction trans = new Transaction(doc, "Create Curve By Points"))
            {
                trans.Start();

                CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(refPointArray);
                if (curve != null && isReferenceLine)
                {
                    curve.IsReferenceLine = true;
                }

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Curve by points created",
                    curveId = GetElementIdInt(curve.Id),
                    pointCount = refPointArray.Size,
                    isReferenceLine = curve.IsReferenceLine
                };
            }
        }

        private object CreateCurvesFromGrid(Document doc, Dictionary<string, object> parameters)
        {
            // This creates curves from a grid of reference points
            // Either creates curves along rows (X direction) or columns (Y direction)

            string curveDirection = parameters.ContainsKey("curve_direction") ? parameters["curve_direction"]?.ToString()?.ToLower() : "rows";
            bool isReferenceLine = parameters.ContainsKey("is_reference_line") && Convert.ToBoolean(parameters["is_reference_line"]);

            // Get all reference points
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var allRefPoints = collector.OfClass(typeof(ReferencePoint)).Cast<ReferencePoint>().ToList();

            if (allRefPoints.Count < 2)
            {
                return new { success = false, error = "Need at least 2 reference points in the document" };
            }

            // Sort points by position to form a grid
            // Get unique X and Y values to determine grid structure
            var xValues = allRefPoints.Select(p => Math.Round(p.Position.X, 6)).Distinct().OrderBy(x => x).ToList();
            var yValues = allRefPoints.Select(p => Math.Round(p.Position.Y, 6)).Distinct().OrderBy(y => y).ToList();

            // Create a 2D lookup of points
            var pointGrid = new Dictionary<string, ReferencePoint>();
            foreach (var refPoint in allRefPoints)
            {
                double roundedX = Math.Round(refPoint.Position.X, 6);
                double roundedY = Math.Round(refPoint.Position.Y, 6);
                string key = $"{roundedX},{roundedY}";
                pointGrid[key] = refPoint;
            }

            var createdCurves = new List<object>();

            using (Transaction trans = new Transaction(doc, "Create Curves From Grid"))
            {
                trans.Start();

                if (curveDirection == "rows" || curveDirection == "x")
                {
                    // Create curves along rows (constant Y, varying X)
                    foreach (var yVal in yValues)
                    {
                        var rowPoints = new ReferencePointArray();
                        foreach (var xVal in xValues)
                        {
                            string key = $"{xVal},{yVal}";
                            if (pointGrid.ContainsKey(key))
                            {
                                rowPoints.Append(pointGrid[key]);
                            }
                        }

                        if (rowPoints.Size >= 2)
                        {
                            CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(rowPoints);
                            if (curve != null && isReferenceLine)
                            {
                                curve.IsReferenceLine = true;
                            }
                            createdCurves.Add(new
                            {
                                id = GetElementIdInt(curve.Id),
                                direction = "row",
                                yValue = yVal,
                                pointCount = rowPoints.Size
                            });
                        }
                    }
                }
                else if (curveDirection == "columns" || curveDirection == "y")
                {
                    // Create curves along columns (constant X, varying Y)
                    foreach (var xVal in xValues)
                    {
                        var colPoints = new ReferencePointArray();
                        foreach (var yVal in yValues)
                        {
                            string key = $"{xVal},{yVal}";
                            if (pointGrid.ContainsKey(key))
                            {
                                colPoints.Append(pointGrid[key]);
                            }
                        }

                        if (colPoints.Size >= 2)
                        {
                            CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(colPoints);
                            if (curve != null && isReferenceLine)
                            {
                                curve.IsReferenceLine = true;
                            }
                            createdCurves.Add(new
                            {
                                id = GetElementIdInt(curve.Id),
                                direction = "column",
                                xValue = xVal,
                                pointCount = colPoints.Size
                            });
                        }
                    }
                }
                else if (curveDirection == "both")
                {
                    // Create curves in both directions
                    // Rows
                    foreach (var yVal in yValues)
                    {
                        var rowPoints = new ReferencePointArray();
                        foreach (var xVal in xValues)
                        {
                            string key = $"{xVal},{yVal}";
                            if (pointGrid.ContainsKey(key))
                            {
                                rowPoints.Append(pointGrid[key]);
                            }
                        }
                        if (rowPoints.Size >= 2)
                        {
                            CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(rowPoints);
                            if (curve != null && isReferenceLine) curve.IsReferenceLine = true;
                            createdCurves.Add(new { id = GetElementIdInt(curve.Id), direction = "row", yValue = yVal, pointCount = rowPoints.Size });
                        }
                    }
                    // Columns
                    foreach (var xVal in xValues)
                    {
                        var colPoints = new ReferencePointArray();
                        foreach (var yVal in yValues)
                        {
                            string key = $"{xVal},{yVal}";
                            if (pointGrid.ContainsKey(key))
                            {
                                colPoints.Append(pointGrid[key]);
                            }
                        }
                        if (colPoints.Size >= 2)
                        {
                            CurveByPoints curve = doc.FamilyCreate.NewCurveByPoints(colPoints);
                            if (curve != null && isReferenceLine) curve.IsReferenceLine = true;
                            createdCurves.Add(new { id = GetElementIdInt(curve.Id), direction = "column", xValue = xVal, pointCount = colPoints.Size });
                        }
                    }
                }

                trans.Commit();
            }

            return new
            {
                success = true,
                message = $"Created {createdCurves.Count} curves from grid",
                curveDirection = curveDirection,
                gridSizeX = xValues.Count,
                gridSizeY = yValues.Count,
                curveCount = createdCurves.Count,
                curves = createdCurves
            };
        }

        private object GetCurvesByPoints(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var curves = collector.OfClass(typeof(CurveByPoints)).Cast<CurveByPoints>().ToList();

            var curvesList = new List<object>();

            foreach (var curve in curves)
            {
                var pointRefs = curve.GetPoints();
                var pointIds = new List<int>();
                foreach (ReferencePoint rp in pointRefs)
                {
                    pointIds.Add(GetElementIdInt(rp.Id));
                }

                curvesList.Add(new
                {
                    id = GetElementIdInt(curve.Id),
                    isReferenceLine = curve.IsReferenceLine,
                    pointCount = pointIds.Count,
                    pointIds = pointIds
                });
            }

            return new
            {
                success = true,
                count = curvesList.Count,
                curvesByPoints = curvesList
            };
        }

        private object CreateLoftForm(Document doc, Dictionary<string, object> parameters)
        {
            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);
            bool useAllCurves = !parameters.ContainsKey("curve_ids") || parameters["curve_ids"] == null;

            // NewLoftForm requires ReferenceArrayArray - each profile curve in its own ReferenceArray
            var profileArrays = new ReferenceArrayArray();
            List<CurveByPoints> curvesToUse = new List<CurveByPoints>();

            if (useAllCurves)
            {
                // Use all CurveByPoints in the document
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var curves = collector.OfClass(typeof(CurveByPoints)).Cast<CurveByPoints>().ToList();

                if (curves.Count < 2)
                {
                    return new { success = false, error = "Need at least 2 curves to create a loft. Create curves first using create_curves_from_grid." };
                }

                // Sort curves by their first point's position to ensure proper loft order
                curvesToUse = curves.OrderBy(c => {
                    var pts = c.GetPoints();
                    if (pts.Size > 0)
                    {
                        ReferencePoint firstPt = pts.get_Item(0);
                        return firstPt.Position.Y; // Sort by Y position for row curves
                    }
                    return 0.0;
                }).ToList();
            }
            else
            {
                // Use specific curve IDs
                var curveIds = new List<ElementId>();
                var idsObj = parameters["curve_ids"];

                if (idsObj is JArray jArray)
                {
                    foreach (var item in jArray)
                    {
                        curveIds.Add(new ElementId(Convert.ToInt32(item)));
                    }
                }
                else if (idsObj is IEnumerable<object> idList)
                {
                    foreach (var id in idList)
                    {
                        curveIds.Add(new ElementId(Convert.ToInt32(id)));
                    }
                }

                if (curveIds.Count < 2)
                {
                    return new { success = false, error = "Need at least 2 curve IDs to create a loft" };
                }

                foreach (var id in curveIds)
                {
                    Element elem = doc.GetElement(id);
                    if (elem is CurveByPoints cbp)
                    {
                        curvesToUse.Add(cbp);
                    }
                }
            }

            if (curvesToUse.Count < 2)
            {
                return new { success = false, error = "Could not get at least 2 valid CurveByPoints elements" };
            }

            // Create ReferenceArrayArray: each profile curve goes into its own ReferenceArray
            foreach (var curve in curvesToUse)
            {
                var refArray = new ReferenceArray();
                refArray.Append(curve.GeometryCurve.Reference);
                profileArrays.Append(refArray);
            }

            using (Transaction trans = new Transaction(doc, "Create Loft Form"))
            {
                trans.Start();

                Form form = doc.FamilyCreate.NewLoftForm(isSolid, profileArrays);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Loft form created successfully",
                    formId = GetElementIdInt(form.Id),
                    isSolid = isSolid,
                    profileCount = profileArrays.Size
                };
            }
        }

        private object GetForms(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var forms = collector.OfClass(typeof(Form)).Cast<Form>().ToList();

            var formsList = new List<object>();

            foreach (var form in forms)
            {
                formsList.Add(new
                {
                    id = GetElementIdInt(form.Id),
                    name = form.Name,
                    isSolid = !form.get_Parameter(BuiltInParameter.FAMILY_ELEM_SUBCATEGORY)?.AsString()?.Contains("Void")
                });
            }

            return new
            {
                success = true,
                count = formsList.Count,
                forms = formsList
            };
        }

        private object CreateRevolveAxis(Document doc, Dictionary<string, object> parameters)
        {
            double startX = parameters.ContainsKey("axis_start_x") ? Convert.ToDouble(parameters["axis_start_x"]) : 0;
            double startY = parameters.ContainsKey("axis_start_y") ? Convert.ToDouble(parameters["axis_start_y"]) : 0;
            double startZ = parameters.ContainsKey("axis_start_z") ? Convert.ToDouble(parameters["axis_start_z"]) : 0;
            double endX = parameters.ContainsKey("axis_end_x") ? Convert.ToDouble(parameters["axis_end_x"]) : 0;
            double endY = parameters.ContainsKey("axis_end_y") ? Convert.ToDouble(parameters["axis_end_y"]) : 0;
            double endZ = parameters.ContainsKey("axis_end_z") ? Convert.ToDouble(parameters["axis_end_z"]) : 10;

            XYZ startPoint = new XYZ(startX, startY, startZ);
            XYZ endPoint = new XYZ(endX, endY, endZ);

            if (startPoint.DistanceTo(endPoint) < 0.001)
            {
                return new { success = false, error = "Axis start and end points must be different" };
            }

            using (Transaction trans = new Transaction(doc, "Create Revolve Axis"))
            {
                trans.Start();

                // Create a line for the axis
                Line axisLine = Line.CreateBound(startPoint, endPoint);

                // Determine sketch plane - use a plane containing the axis
                XYZ axisDir = (endPoint - startPoint).Normalize();
                XYZ perpDir;
                
                // Find a perpendicular direction
                if (Math.Abs(axisDir.Z) < 0.9)
                    perpDir = axisDir.CrossProduct(XYZ.BasisZ).Normalize();
                else
                    perpDir = axisDir.CrossProduct(XYZ.BasisX).Normalize();

                XYZ normal = axisDir.CrossProduct(perpDir).Normalize();
                Plane plane = Plane.CreateByNormalAndOrigin(normal, startPoint);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                // Create model curve as reference line for axis
                ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(axisLine, sketchPlane);

                // Make it a reference line so we can use it as axis
                modelCurve.ChangeToReferenceLine();

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Axis line created",
                    axisLineId = GetElementIdInt(modelCurve.Id),
                    startPoint = new { x = startX, y = startY, z = startZ },
                    endPoint = new { x = endX, y = endY, z = endZ }
                };
            }
        }

        private object CreateRevolveProfile(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("profile_points") || parameters["profile_points"] == null)
            {
                return new { success = false, error = "profile_points is required" };
            }

            var points = new List<XYZ>();
            var pointsObj = parameters["profile_points"];

            if (pointsObj is JArray jArray)
            {
                foreach (JObject pt in jArray)
                {
                    double x = pt.ContainsKey("x") ? Convert.ToDouble(pt["x"]) : 0;
                    double y = pt.ContainsKey("y") ? Convert.ToDouble(pt["y"]) : 0;
                    double z = pt.ContainsKey("z") ? Convert.ToDouble(pt["z"]) : 0;
                    points.Add(new XYZ(x, y, z));
                }
            }
            else if (pointsObj is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        double x = dict.ContainsKey("x") ? Convert.ToDouble(dict["x"]) : 0;
                        double y = dict.ContainsKey("y") ? Convert.ToDouble(dict["y"]) : 0;
                        double z = dict.ContainsKey("z") ? Convert.ToDouble(dict["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }

            if (points.Count < 2)
            {
                return new { success = false, error = "At least 2 points are required for the profile curve" };
            }

            using (Transaction trans = new Transaction(doc, "Create Profile Curve"))
            {
                trans.Start();

                // Create reference points
                var refPoints = new ReferencePointArray();
                foreach (var pt in points)
                {
                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(pt);
                    refPoints.Append(refPt);
                }

                // Create CurveByPoints through the reference points
                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPoints);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Profile curve created",
                    profileCurveId = GetElementIdInt(curveByPoints.Id),
                    pointCount = points.Count
                };
            }
        }

        private object CreateRevolveForm(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("axis_line_id"))
            {
                return new { success = false, error = "axis_line_id is required. Create an axis first with create_axis_line." };
            }
            if (!parameters.ContainsKey("profile_curve_id"))
            {
                return new { success = false, error = "profile_curve_id is required. Create a profile first with create_profile_curve." };
            }

            int axisId = Convert.ToInt32(parameters["axis_line_id"]);
            int profileId = Convert.ToInt32(parameters["profile_curve_id"]);
            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);
            double startAngle = parameters.ContainsKey("start_angle") ? Convert.ToDouble(parameters["start_angle"]) : 0;
            double endAngle = parameters.ContainsKey("end_angle") ? Convert.ToDouble(parameters["end_angle"]) : 360;

            // Convert degrees to radians
            startAngle = startAngle * Math.PI / 180.0;
            endAngle = endAngle * Math.PI / 180.0;

            Element axisElement = doc.GetElement(new ElementId(axisId));
            Element profileElement = doc.GetElement(new ElementId(profileId));

            if (axisElement == null)
            {
                return new { success = false, error = $"Axis element with ID {axisId} not found" };
            }
            if (profileElement == null)
            {
                return new { success = false, error = $"Profile element with ID {profileId} not found" };
            }

            // Get axis reference
            Reference axisRef = null;
            if (axisElement is ModelCurve modelCurve)
            {
                axisRef = modelCurve.GeometryCurve.Reference;
            }
            else
            {
                return new { success = false, error = "Axis element must be a ModelCurve" };
            }

            // Get profile reference
            ReferenceArray profileRefs = new ReferenceArray();
            if (profileElement is CurveByPoints cbp)
            {
                profileRefs.Append(cbp.GeometryCurve.Reference);
            }
            else
            {
                return new { success = false, error = "Profile element must be a CurveByPoints" };
            }

            using (Transaction trans = new Transaction(doc, "Create Revolve Form"))
            {
                trans.Start();

                FormArray forms = doc.FamilyCreate.NewRevolveForms(isSolid, profileRefs, axisRef, startAngle, endAngle);

                trans.Commit();

                var formIds = new List<int>();
                foreach (Form f in forms)
                {
                    formIds.Add(GetElementIdInt(f.Id));
                }

                return new
                {
                    success = true,
                    message = "Revolve form created successfully",
                    formIds = formIds,
                    formCount = forms.Size,
                    isSolid = isSolid,
                    startAngleDegrees = startAngle * 180.0 / Math.PI,
                    endAngleDegrees = endAngle * 180.0 / Math.PI
                };
            }
        }

        #endregion

        #region Revolve Tool

        private object RevolveTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required" };
            }

            try
            {
                switch (operation)
                {
                    case "create_axis_line":
                        return CreateRevolveAxis(doc, parameters);

                    case "create_profile_curve":
                        return CreateRevolveProfile(doc, parameters);

                    case "create_revolve":
                        return CreateRevolveForm(doc, parameters);

                    case "get_revolve_forms":
                        return GetForms(doc);

                    default:
                        return new { success = false, error = $"Unknown revolve operation: {operation}" };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        #endregion

        #region Cap Tool

        /// <summary>
        /// Tool for creating cap forms using FamilyCreate.NewFormByCap
        /// Supports creating caps from points or from existing model lines
        /// </summary>
        private object CapTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required. Use 'create_cap_from_points' or 'create_cap_from_lines'." };
            }

            try
            {
                switch (operation)
                {
                    case "create_cap_from_points":
                        return CreateCapFromPoints(doc, parameters);

                    case "create_cap_from_lines":
                        return CreateCapFromLines(doc, parameters);

                    case "get_cap_forms":
                        return GetForms(doc);

                    default:
                        return new { success = false, error = $"Unknown cap operation: {operation}. Use 'create_cap_from_points' or 'create_cap_from_lines'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Creates a cap form from a list of points.
        /// Points should form a closed polygon (the method will close it automatically).
        /// </summary>
        private object CreateCapFromPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points is required. Provide a list of {x, y, z} coordinates forming a closed profile." };
            }

            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);
            bool closedLoop = !parameters.ContainsKey("closed_loop") || Convert.ToBoolean(parameters["closed_loop"]);

            var points = new List<XYZ>();
            var pointsObj = parameters["points"];

            // Parse points array
            if (pointsObj is JArray jArray)
            {
                foreach (JObject pt in jArray)
                {
                    double x = pt.ContainsKey("x") ? Convert.ToDouble(pt["x"]) : 0;
                    double y = pt.ContainsKey("y") ? Convert.ToDouble(pt["y"]) : 0;
                    double z = pt.ContainsKey("z") ? Convert.ToDouble(pt["z"]) : 0;
                    points.Add(new XYZ(x, y, z));
                }
            }
            else if (pointsObj is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        double x = dict.ContainsKey("x") ? Convert.ToDouble(dict["x"]) : 0;
                        double y = dict.ContainsKey("y") ? Convert.ToDouble(dict["y"]) : 0;
                        double z = dict.ContainsKey("z") ? Convert.ToDouble(dict["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }

            if (points.Count < 3)
            {
                return new { success = false, error = "At least 3 points are required to create a cap form." };
            }

            using (Transaction trans = new Transaction(doc, "Create Cap From Points"))
            {
                trans.Start();

                // Create reference points
                var refPointsList = new List<ReferencePoint>();
                foreach (var pt in points)
                {
                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(pt);
                    refPointsList.Add(refPt);
                }

                // Create model lines connecting the points to form a closed loop
                var modelLines = new List<ModelCurve>();
                ReferenceArray profileRefs = new ReferenceArray();

                for (int i = 0; i < refPointsList.Count; i++)
                {
                    int nextIndex = (i + 1) % refPointsList.Count;
                    if (!closedLoop && i == refPointsList.Count - 1)
                    {
                        break; // Skip last segment if not closed loop
                    }

                    XYZ startPt = refPointsList[i].Position;
                    XYZ endPt = refPointsList[nextIndex].Position;

                    if (startPt.DistanceTo(endPt) < 0.001)
                    {
                        continue; // Skip zero-length segments
                    }

                    // Create line geometry
                    Line line = Line.CreateBound(startPt, endPt);

                    // Create sketch plane for the line
                    XYZ lineDir = (endPt - startPt).Normalize();
                    XYZ perpDir;
                    if (Math.Abs(lineDir.Z) < 0.9)
                        perpDir = lineDir.CrossProduct(XYZ.BasisZ).Normalize();
                    else
                        perpDir = lineDir.CrossProduct(XYZ.BasisX).Normalize();
                    XYZ normal = lineDir.CrossProduct(perpDir).Normalize();
                    
                    Plane plane = Plane.CreateByNormalAndOrigin(normal, startPt);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    // Create model curve
                    ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                    modelLines.Add(modelCurve);

                    // Add reference to profile array
                    profileRefs.Append(modelCurve.GeometryCurve.Reference);
                }

                if (profileRefs.Size < 3)
                {
                    trans.RollBack();
                    return new { success = false, error = "Could not create enough valid line segments for the cap profile." };
                }

                // Create the cap form
                Form capForm = doc.FamilyCreate.NewFormByCap(isSolid, profileRefs);

                trans.Commit();

                var lineIds = modelLines.Select(l => GetElementIdInt(l.Id)).ToList();
                var pointIds = refPointsList.Select(p => GetElementIdInt(p.Id)).ToList();

                return new
                {
                    success = true,
                    message = "Cap form created successfully from points",
                    formId = GetElementIdInt(capForm.Id),
                    isSolid = isSolid,
                    pointCount = points.Count,
                    lineCount = modelLines.Count,
                    pointIds = pointIds,
                    lineIds = lineIds
                };
            }
        }

        /// <summary>
        /// Creates a cap form from existing model lines or curves.
        /// The lines should form a closed loop profile.
        /// </summary>
        private object CreateCapFromLines(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("line_ids") || parameters["line_ids"] == null)
            {
                return new { success = false, error = "line_ids is required. Provide a list of element IDs of model lines forming a closed profile." };
            }

            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);

            // Parse line IDs
            var lineIds = new List<ElementId>();
            var idsObj = parameters["line_ids"];

            if (idsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    lineIds.Add(new ElementId(Convert.ToInt32(item)));
                }
            }
            else if (idsObj is IEnumerable<object> idList)
            {
                foreach (var id in idList)
                {
                    lineIds.Add(new ElementId(Convert.ToInt32(id)));
                }
            }

            if (lineIds.Count < 3)
            {
                return new { success = false, error = "At least 3 line element IDs are required to create a cap form." };
            }

            // Collect references from the line elements
            ReferenceArray profileRefs = new ReferenceArray();
            var validElements = new List<Element>();

            foreach (var id in lineIds)
            {
                Element elem = doc.GetElement(id);
                if (elem == null)
                {
                    return new { success = false, error = $"Element with ID {GetElementIdInt(id)} not found." };
                }

                Reference curveRef = null;

                // Handle different curve element types
                if (elem is ModelCurve modelCurve)
                {
                    curveRef = modelCurve.GeometryCurve.Reference;
                }
                else if (elem is CurveByPoints curveByPoints)
                {
                    curveRef = curveByPoints.GeometryCurve.Reference;
                }
                else if (elem is ModelLine modelLine)
                {
                    curveRef = modelLine.GeometryCurve.Reference;
                }
                else
                {
                    return new { success = false, error = $"Element {GetElementIdInt(id)} is not a valid curve element (ModelCurve, CurveByPoints, or ModelLine). Type: {elem.GetType().Name}" };
                }

                if (curveRef != null)
                {
                    profileRefs.Append(curveRef);
                    validElements.Add(elem);
                }
            }

            if (profileRefs.Size < 3)
            {
                return new { success = false, error = "Could not get valid references from at least 3 curve elements." };
            }

            using (Transaction trans = new Transaction(doc, "Create Cap From Lines"))
            {
                trans.Start();

                // Create the cap form
                Form capForm = doc.FamilyCreate.NewFormByCap(isSolid, profileRefs);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Cap form created successfully from lines",
                    formId = GetElementIdInt(capForm.Id),
                    isSolid = isSolid,
                    lineCount = validElements.Count,
                    usedLineIds = lineIds.Select(id => GetElementIdInt(id)).ToList()
                };
            }
        }

        #endregion

        #region Extrusion Tool

        /// <summary>
        /// Tool for creating extrusion forms using FamilyCreate.NewExtrusionForm
        /// Supports creating extrusions from points or from existing model lines
        /// </summary>
        private object ExtrusionTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required. Use 'create_extrusion_from_points' or 'create_extrusion_from_lines'." };
            }

            try
            {
                switch (operation)
                {
                    case "create_extrusion_from_points":
                        return CreateExtrusionFromPoints(doc, parameters);

                    case "create_extrusion_from_lines":
                        return CreateExtrusionFromLines(doc, parameters);

                    case "create_box_row_from_curves":
                        return CreateBoxRowFromCurves(doc, parameters);

                    case "get_extrusion_forms":
                        return GetForms(doc);

                    default:
                        return new { success = false, error = $"Unknown extrusion operation: {operation}. Use 'create_extrusion_from_points', 'create_extrusion_from_lines', or 'create_box_row_from_curves'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Creates an extrusion form from a list of points.
        /// Points should form a closed polygon profile that will be extruded in the specified direction.
        /// </summary>
        private object CreateExtrusionFromPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points is required. Provide a list of {x, y, z} coordinates forming a closed profile." };
            }

            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);
            bool closedLoop = !parameters.ContainsKey("closed_loop") || Convert.ToBoolean(parameters["closed_loop"]);

            // Parse extrusion direction (default is Z-up)
            double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 0;
            double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
            double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 10;

            XYZ direction = new XYZ(dirX, dirY, dirZ);
            if (direction.GetLength() < 0.001)
            {
                return new { success = false, error = "Extrusion direction cannot be zero. Provide valid direction_x, direction_y, or direction_z values." };
            }

            var points = new List<XYZ>();
            var pointsObj = parameters["points"];

            // Parse points array
            if (pointsObj is JArray jArray)
            {
                foreach (JObject pt in jArray)
                {
                    double x = pt.ContainsKey("x") ? Convert.ToDouble(pt["x"]) : 0;
                    double y = pt.ContainsKey("y") ? Convert.ToDouble(pt["y"]) : 0;
                    double z = pt.ContainsKey("z") ? Convert.ToDouble(pt["z"]) : 0;
                    points.Add(new XYZ(x, y, z));
                }
            }
            else if (pointsObj is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        double x = dict.ContainsKey("x") ? Convert.ToDouble(dict["x"]) : 0;
                        double y = dict.ContainsKey("y") ? Convert.ToDouble(dict["y"]) : 0;
                        double z = dict.ContainsKey("z") ? Convert.ToDouble(dict["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }

            if (points.Count < 3)
            {
                return new { success = false, error = "At least 3 points are required to create an extrusion profile." };
            }

            using (Transaction trans = new Transaction(doc, "Create Extrusion From Points"))
            {
                trans.Start();

                // Create reference points
                var refPointsList = new List<ReferencePoint>();
                foreach (var pt in points)
                {
                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(pt);
                    refPointsList.Add(refPt);
                }

                // Create model lines connecting the points to form a closed loop
                var modelLines = new List<ModelCurve>();
                ReferenceArray profileRefs = new ReferenceArray();

                for (int i = 0; i < refPointsList.Count; i++)
                {
                    int nextIndex = (i + 1) % refPointsList.Count;
                    if (!closedLoop && i == refPointsList.Count - 1)
                    {
                        break; // Skip last segment if not closed loop
                    }

                    XYZ startPt = refPointsList[i].Position;
                    XYZ endPt = refPointsList[nextIndex].Position;

                    if (startPt.DistanceTo(endPt) < 0.001)
                    {
                        continue; // Skip zero-length segments
                    }

                    // Create line geometry
                    Line line = Line.CreateBound(startPt, endPt);

                    // Create sketch plane for the line
                    XYZ lineDir = (endPt - startPt).Normalize();
                    XYZ perpDir;
                    if (Math.Abs(lineDir.Z) < 0.9)
                        perpDir = lineDir.CrossProduct(XYZ.BasisZ).Normalize();
                    else
                        perpDir = lineDir.CrossProduct(XYZ.BasisX).Normalize();
                    XYZ normal = lineDir.CrossProduct(perpDir).Normalize();
                    
                    Plane plane = Plane.CreateByNormalAndOrigin(normal, startPt);
                    SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                    // Create model curve
                    ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                    modelLines.Add(modelCurve);

                    // Add reference to profile array
                    profileRefs.Append(modelCurve.GeometryCurve.Reference);
                }

                if (profileRefs.Size < 3)
                {
                    trans.RollBack();
                    return new { success = false, error = "Could not create enough valid line segments for the extrusion profile." };
                }

                // Create the extrusion form
                Form extrusionForm = doc.FamilyCreate.NewExtrusionForm(isSolid, profileRefs, direction);

                trans.Commit();

                var lineIds = modelLines.Select(l => GetElementIdInt(l.Id)).ToList();
                var pointIds = refPointsList.Select(p => GetElementIdInt(p.Id)).ToList();

                return new
                {
                    success = true,
                    message = "Extrusion form created successfully from points",
                    formId = GetElementIdInt(extrusionForm.Id),
                    isSolid = isSolid,
                    pointCount = points.Count,
                    lineCount = modelLines.Count,
                    direction = new { x = dirX, y = dirY, z = dirZ },
                    pointIds = pointIds,
                    lineIds = lineIds
                };
            }
        }

        /// <summary>
        /// Creates an extrusion form from existing model lines or curves.
        /// The lines should form a closed loop profile that will be extruded in the specified direction.
        /// </summary>
        private object CreateExtrusionFromLines(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("line_ids") || parameters["line_ids"] == null)
            {
                return new { success = false, error = "line_ids is required. Provide a list of element IDs of model lines forming a closed profile." };
            }

            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);

            // Parse extrusion direction (default is Z-up)
            double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 0;
            double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
            double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 10;

            XYZ direction = new XYZ(dirX, dirY, dirZ);
            if (direction.GetLength() < 0.001)
            {
                return new { success = false, error = "Extrusion direction cannot be zero. Provide valid direction_x, direction_y, or direction_z values." };
            }

            // Parse line IDs
            var lineIds = new List<ElementId>();
            var idsObj = parameters["line_ids"];

            if (idsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    lineIds.Add(new ElementId(Convert.ToInt32(item)));
                }
            }
            else if (idsObj is IEnumerable<object> idList)
            {
                foreach (var id in idList)
                {
                    lineIds.Add(new ElementId(Convert.ToInt32(id)));
                }
            }

            if (lineIds.Count < 3)
            {
                return new { success = false, error = "At least 3 line element IDs are required to create an extrusion profile." };
            }

            // Collect references from the line elements
            ReferenceArray profileRefs = new ReferenceArray();
            var validElements = new List<Element>();

            foreach (var id in lineIds)
            {
                Element elem = doc.GetElement(id);
                if (elem == null)
                {
                    return new { success = false, error = $"Element with ID {GetElementIdInt(id)} not found." };
                }

                Reference curveRef = null;

                // Handle different curve element types
                if (elem is ModelCurve modelCurve)
                {
                    curveRef = modelCurve.GeometryCurve.Reference;
                }
                else if (elem is CurveByPoints curveByPoints)
                {
                    curveRef = curveByPoints.GeometryCurve.Reference;
                }
                else if (elem is ModelLine modelLine)
                {
                    curveRef = modelLine.GeometryCurve.Reference;
                }
                else
                {
                    return new { success = false, error = $"Element {GetElementIdInt(id)} is not a valid curve element (ModelCurve, CurveByPoints, or ModelLine). Type: {elem.GetType().Name}" };
                }

                if (curveRef != null)
                {
                    profileRefs.Append(curveRef);
                    validElements.Add(elem);
                }
            }

            if (profileRefs.Size < 3)
            {
                return new { success = false, error = "Could not get valid references from at least 3 curve elements." };
            }

            using (Transaction trans = new Transaction(doc, "Create Extrusion From Lines"))
            {
                trans.Start();

                // Create the extrusion form
                Form extrusionForm = doc.FamilyCreate.NewExtrusionForm(isSolid, profileRefs, direction);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Extrusion form created successfully from lines",
                    formId = GetElementIdInt(extrusionForm.Id),
                    isSolid = isSolid,
                    lineCount = validElements.Count,
                    direction = new { x = dirX, y = dirY, z = dirZ },
                    usedLineIds = lineIds.Select(id => GetElementIdInt(id)).ToList()
                };
            }
        }

        /// <summary>
        /// Creates a row of boxes (extrusions) with varying heights.
        /// Heights are determined by sampling points from two curves and calculating the Z-difference.
        /// </summary>
        private object CreateBoxRowFromCurves(Document doc, Dictionary<string, object> parameters)
        {
            // Validate required parameters
            if (!parameters.ContainsKey("bottom_curve_id") || parameters["bottom_curve_id"] == null)
            {
                return new { success = false, error = "bottom_curve_id is required. Provide the element ID of the bottom curve (CurveByPoints or ModelCurve)." };
            }
            if (!parameters.ContainsKey("top_curve_id") || parameters["top_curve_id"] == null)
            {
                return new { success = false, error = "top_curve_id is required. Provide the element ID of the top curve (CurveByPoints or ModelCurve)." };
            }

            int bottomCurveId = Convert.ToInt32(parameters["bottom_curve_id"]);
            int topCurveId = Convert.ToInt32(parameters["top_curve_id"]);

            // Number of boxes to create
            int boxCount = parameters.ContainsKey("box_count") ? Convert.ToInt32(parameters["box_count"]) : 5;
            if (boxCount < 1 || boxCount > 100)
            {
                return new { success = false, error = "box_count must be between 1 and 100." };
            }

            // Separation between boxes (in feet)
            double separation = parameters.ContainsKey("separation") ? Convert.ToDouble(parameters["separation"]) : 1.0;
            if (separation < 0)
            {
                return new { success = false, error = "separation must be a positive value." };
            }

            // Box width and depth (default 1 foot x 1 foot)
            double boxWidth = parameters.ContainsKey("box_width") ? Convert.ToDouble(parameters["box_width"]) : 1.0;
            double boxDepth = parameters.ContainsKey("box_depth") ? Convert.ToDouble(parameters["box_depth"]) : 1.0;

            bool isSolid = !parameters.ContainsKey("is_solid") || Convert.ToBoolean(parameters["is_solid"]);

            // Get the curve elements
            Element bottomElem = doc.GetElement(new ElementId(bottomCurveId));
            Element topElem = doc.GetElement(new ElementId(topCurveId));

            if (bottomElem == null)
            {
                return new { success = false, error = $"Bottom curve element with ID {bottomCurveId} not found." };
            }
            if (topElem == null)
            {
                return new { success = false, error = $"Top curve element with ID {topCurveId} not found." };
            }

            // Get the geometry curves
            Curve bottomCurve = null;
            Curve topCurve = null;

            if (bottomElem is CurveByPoints bottomCbp)
            {
                bottomCurve = bottomCbp.GeometryCurve;
            }
            else if (bottomElem is ModelCurve bottomMc)
            {
                bottomCurve = bottomMc.GeometryCurve;
            }
            else
            {
                return new { success = false, error = $"Bottom element {bottomCurveId} is not a valid curve (CurveByPoints or ModelCurve). Type: {bottomElem.GetType().Name}" };
            }

            if (topElem is CurveByPoints topCbp)
            {
                topCurve = topCbp.GeometryCurve;
            }
            else if (topElem is ModelCurve topMc)
            {
                topCurve = topMc.GeometryCurve;
            }
            else
            {
                return new { success = false, error = $"Top element {topCurveId} is not a valid curve (CurveByPoints or ModelCurve). Type: {topElem.GetType().Name}" };
            }

            if (bottomCurve == null || topCurve == null)
            {
                return new { success = false, error = "Could not retrieve geometry from curve elements." };
            }

            using (Transaction trans = new Transaction(doc, "Create Box Row From Curves"))
            {
                trans.Start();

                var createdBoxes = new List<object>();
                var allCreatedElements = new List<ElementId>();

                for (int i = 0; i < boxCount; i++)
                {
                    // Calculate normalized parameter (0 to 1) along the curves
                    double param = boxCount > 1 ? (double)i / (boxCount - 1) : 0.5;

                    // Get points on both curves at this parameter
                    XYZ bottomPoint = bottomCurve.Evaluate(param, true);
                    XYZ topPoint = topCurve.Evaluate(param, true);

                    // Calculate height from Z difference
                    double height = topPoint.Z - bottomPoint.Z;
                    if (Math.Abs(height) < 0.001)
                    {
                        height = 1.0; // Minimum height of 1 foot
                    }

                    // Calculate the box center X position based on separation
                    // Start from the bottom curve's X position
                    double centerX = bottomPoint.X + (i * (boxWidth + separation));
                    double centerY = bottomPoint.Y;
                    double baseZ = bottomPoint.Z;

                    // Create the 4 corner points of the box profile (at base Z)
                    double halfWidth = boxWidth / 2.0;
                    double halfDepth = boxDepth / 2.0;

                    XYZ pt1 = new XYZ(centerX - halfWidth, centerY - halfDepth, baseZ);
                    XYZ pt2 = new XYZ(centerX + halfWidth, centerY - halfDepth, baseZ);
                    XYZ pt3 = new XYZ(centerX + halfWidth, centerY + halfDepth, baseZ);
                    XYZ pt4 = new XYZ(centerX - halfWidth, centerY + halfDepth, baseZ);

                    // Create reference points for the profile corners
                    ReferencePoint refPt1 = doc.FamilyCreate.NewReferencePoint(pt1);
                    ReferencePoint refPt2 = doc.FamilyCreate.NewReferencePoint(pt2);
                    ReferencePoint refPt3 = doc.FamilyCreate.NewReferencePoint(pt3);
                    ReferencePoint refPt4 = doc.FamilyCreate.NewReferencePoint(pt4);

                    allCreatedElements.Add(refPt1.Id);
                    allCreatedElements.Add(refPt2.Id);
                    allCreatedElements.Add(refPt3.Id);
                    allCreatedElements.Add(refPt4.Id);

                    // Create model lines for the box profile
                    var profileLines = new List<ModelCurve>();
                    ReferenceArray profileRefs = new ReferenceArray();

                    // Helper to create a model line between two points
                    Func<XYZ, XYZ, ModelCurve> createModelLine = (start, end) =>
                    {
                        Line line = Line.CreateBound(start, end);
                        XYZ lineDir = (end - start).Normalize();
                        XYZ perpDir;
                        if (Math.Abs(lineDir.Z) < 0.9)
                            perpDir = lineDir.CrossProduct(XYZ.BasisZ).Normalize();
                        else
                            perpDir = lineDir.CrossProduct(XYZ.BasisX).Normalize();
                        XYZ normal = lineDir.CrossProduct(perpDir).Normalize();
                        Plane plane = Plane.CreateByNormalAndOrigin(normal, start);
                        SketchPlane sketchPlane = SketchPlane.Create(doc, plane);
                        return doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                    };

                    ModelCurve line1 = createModelLine(pt1, pt2);
                    ModelCurve line2 = createModelLine(pt2, pt3);
                    ModelCurve line3 = createModelLine(pt3, pt4);
                    ModelCurve line4 = createModelLine(pt4, pt1);

                    profileLines.Add(line1);
                    profileLines.Add(line2);
                    profileLines.Add(line3);
                    profileLines.Add(line4);

                    foreach (var l in profileLines)
                    {
                        allCreatedElements.Add(l.Id);
                        profileRefs.Append(l.GeometryCurve.Reference);
                    }

                    // Create extrusion direction (height in Z)
                    XYZ extrusionDir = new XYZ(0, 0, height);

                    // Create the extrusion form (box)
                    Form boxForm = doc.FamilyCreate.NewExtrusionForm(isSolid, profileRefs, extrusionDir);

                    createdBoxes.Add(new
                    {
                        index = i,
                        formId = GetElementIdInt(boxForm.Id),
                        centerX = centerX,
                        centerY = centerY,
                        baseZ = baseZ,
                        height = height,
                        bottomCurvePoint = new { x = bottomPoint.X, y = bottomPoint.Y, z = bottomPoint.Z },
                        topCurvePoint = new { x = topPoint.X, y = topPoint.Y, z = topPoint.Z }
                    });
                }

                trans.Commit();

                return new
                {
                    success = true,
                    message = $"Created {boxCount} boxes with varying heights from curves",
                    boxCount = boxCount,
                    separation = separation,
                    boxWidth = boxWidth,
                    boxDepth = boxDepth,
                    isSolid = isSolid,
                    bottomCurveId = bottomCurveId,
                    topCurveId = topCurveId,
                    boxes = createdBoxes
                };
            }
        }

        #endregion

        #region Plane Tool

        /// <summary>
        /// Tool for creating planes using World XYZ coordinates in generic adaptive families.
        /// Supports creating planes from origin + normal, from 3 points, or standard XY/XZ/YZ planes.
        /// </summary>
        private object PlaneTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required. Use 'create_plane_by_normal', 'create_plane_by_three_points', 'create_xy_plane', 'create_xz_plane', 'create_yz_plane', or 'get_sketch_planes'." };
            }

            try
            {
                switch (operation)
                {
                    case "create_plane_by_normal":
                        return CreatePlaneByNormal(doc, parameters);

                    case "create_plane_by_three_points":
                        return CreatePlaneByThreePoints(doc, parameters);

                    case "create_xy_plane":
                        return CreateStandardPlane(doc, parameters, "XY");

                    case "create_xz_plane":
                        return CreateStandardPlane(doc, parameters, "XZ");

                    case "create_yz_plane":
                        return CreateStandardPlane(doc, parameters, "YZ");

                    case "get_sketch_planes":
                        return GetSketchPlanes(doc);

                    default:
                        return new { success = false, error = $"Unknown plane operation: {operation}. Use 'create_plane_by_normal', 'create_plane_by_three_points', 'create_xy_plane', 'create_xz_plane', 'create_yz_plane', or 'get_sketch_planes'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Creates a SketchPlane defined by an origin point and a normal vector using World XYZ coordinates.
        /// </summary>
        private object CreatePlaneByNormal(Document doc, Dictionary<string, object> parameters)
        {
            // Parse origin point (default is world origin)
            double originX = parameters.ContainsKey("origin_x") ? Convert.ToDouble(parameters["origin_x"]) : 0;
            double originY = parameters.ContainsKey("origin_y") ? Convert.ToDouble(parameters["origin_y"]) : 0;
            double originZ = parameters.ContainsKey("origin_z") ? Convert.ToDouble(parameters["origin_z"]) : 0;

            // Parse normal vector (default is Z-up)
            double normalX = parameters.ContainsKey("normal_x") ? Convert.ToDouble(parameters["normal_x"]) : 0;
            double normalY = parameters.ContainsKey("normal_y") ? Convert.ToDouble(parameters["normal_y"]) : 0;
            double normalZ = parameters.ContainsKey("normal_z") ? Convert.ToDouble(parameters["normal_z"]) : 1;

            XYZ origin = new XYZ(originX, originY, originZ);
            XYZ normal = new XYZ(normalX, normalY, normalZ);

            if (normal.GetLength() < 0.001)
            {
                return new { success = false, error = "Normal vector cannot be zero. Provide valid normal_x, normal_y, or normal_z values." };
            }

            normal = normal.Normalize();

            using (Transaction trans = new Transaction(doc, "Create Plane By Normal"))
            {
                trans.Start();

                Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "SketchPlane created successfully using World XYZ coordinates",
                    sketchPlaneId = GetElementIdInt(sketchPlane.Id),
                    origin = new { x = originX, y = originY, z = originZ },
                    normal = new { x = normal.X, y = normal.Y, z = normal.Z }
                };
            }
        }

        /// <summary>
        /// Creates a SketchPlane defined by three points in World XYZ coordinates.
        /// The plane passes through all three points.
        /// </summary>
        private object CreatePlaneByThreePoints(Document doc, Dictionary<string, object> parameters)
        {
            // Parse first point
            double p1x = parameters.ContainsKey("point1_x") ? Convert.ToDouble(parameters["point1_x"]) : 0;
            double p1y = parameters.ContainsKey("point1_y") ? Convert.ToDouble(parameters["point1_y"]) : 0;
            double p1z = parameters.ContainsKey("point1_z") ? Convert.ToDouble(parameters["point1_z"]) : 0;

            // Parse second point
            double p2x = parameters.ContainsKey("point2_x") ? Convert.ToDouble(parameters["point2_x"]) : 10;
            double p2y = parameters.ContainsKey("point2_y") ? Convert.ToDouble(parameters["point2_y"]) : 0;
            double p2z = parameters.ContainsKey("point2_z") ? Convert.ToDouble(parameters["point2_z"]) : 0;

            // Parse third point
            double p3x = parameters.ContainsKey("point3_x") ? Convert.ToDouble(parameters["point3_x"]) : 0;
            double p3y = parameters.ContainsKey("point3_y") ? Convert.ToDouble(parameters["point3_y"]) : 10;
            double p3z = parameters.ContainsKey("point3_z") ? Convert.ToDouble(parameters["point3_z"]) : 0;

            XYZ point1 = new XYZ(p1x, p1y, p1z);
            XYZ point2 = new XYZ(p2x, p2y, p2z);
            XYZ point3 = new XYZ(p3x, p3y, p3z);

            // Calculate vectors from point1 to point2 and point3
            XYZ v1 = point2 - point1;
            XYZ v2 = point3 - point1;

            if (v1.GetLength() < 0.001 || v2.GetLength() < 0.001)
            {
                return new { success = false, error = "Points must be distinct. Ensure all three points are different." };
            }

            // Calculate normal as cross product
            XYZ normal = v1.CrossProduct(v2);
            if (normal.GetLength() < 0.001)
            {
                return new { success = false, error = "Points are collinear. Provide three non-collinear points." };
            }

            normal = normal.Normalize();

            using (Transaction trans = new Transaction(doc, "Create Plane By Three Points"))
            {
                trans.Start();

                Plane plane = Plane.CreateByNormalAndOrigin(normal, point1);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "SketchPlane created successfully from three points",
                    sketchPlaneId = GetElementIdInt(sketchPlane.Id),
                    point1 = new { x = p1x, y = p1y, z = p1z },
                    point2 = new { x = p2x, y = p2y, z = p2z },
                    point3 = new { x = p3x, y = p3y, z = p3z },
                    calculatedNormal = new { x = normal.X, y = normal.Y, z = normal.Z }
                };
            }
        }

        /// <summary>
        /// Creates a standard XY, XZ, or YZ plane at a specified offset.
        /// </summary>
        private object CreateStandardPlane(Document doc, Dictionary<string, object> parameters, string planeType)
        {
            // Parse offset (distance along the perpendicular axis)
            double offset = parameters.ContainsKey("offset") ? Convert.ToDouble(parameters["offset"]) : 0;

            XYZ origin;
            XYZ normal;
            string description;

            switch (planeType)
            {
                case "XY":
                    origin = new XYZ(0, 0, offset);
                    normal = XYZ.BasisZ;
                    description = $"XY plane at Z={offset}";
                    break;
                case "XZ":
                    origin = new XYZ(0, offset, 0);
                    normal = XYZ.BasisY;
                    description = $"XZ plane at Y={offset}";
                    break;
                case "YZ":
                    origin = new XYZ(offset, 0, 0);
                    normal = XYZ.BasisX;
                    description = $"YZ plane at X={offset}";
                    break;
                default:
                    return new { success = false, error = "Invalid plane type." };
            }

            using (Transaction trans = new Transaction(doc, $"Create {planeType} Plane"))
            {
                trans.Start();

                Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
                SketchPlane sketchPlane = SketchPlane.Create(doc, plane);

                trans.Commit();

                return new
                {
                    success = true,
                    message = $"SketchPlane created: {description}",
                    sketchPlaneId = GetElementIdInt(sketchPlane.Id),
                    planeType = planeType,
                    offset = offset,
                    origin = new { x = origin.X, y = origin.Y, z = origin.Z },
                    normal = new { x = normal.X, y = normal.Y, z = normal.Z }
                };
            }
        }

        /// <summary>
        /// Gets all SketchPlanes in the current family document.
        /// </summary>
        private object GetSketchPlanes(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var sketchPlanes = collector.OfClass(typeof(SketchPlane)).Cast<SketchPlane>().ToList();

            var planesList = new List<object>();

            foreach (var sp in sketchPlanes)
            {
                try
                {
                    Plane plane = sp.GetPlane();
                    planesList.Add(new
                    {
                        id = GetElementIdInt(sp.Id),
                        name = sp.Name,
                        origin = new { x = plane.Origin.X, y = plane.Origin.Y, z = plane.Origin.Z },
                        normal = new { x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z }
                    });
                }
                catch
                {
                    planesList.Add(new
                    {
                        id = GetElementIdInt(sp.Id),
                        name = sp.Name,
                        origin = (object)null,
                        normal = (object)null
                    });
                }
            }

            return new
            {
                success = true,
                count = planesList.Count,
                sketchPlanes = planesList
            };
        }

        #endregion

        #region Model Curve Tool

        /// <summary>
        /// Tool for creating model curves in generic adaptive families.
        /// Supports lines, arcs, and spline curves from points (including mathematical formulas).
        /// </summary>
        private object ModelCurveTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required. Use 'draw_line', 'draw_arc', 'draw_curve_by_points', 'draw_sine_wave', 'draw_spiral', 'draw_helix', 'draw_cosine_wave', 'draw_rectangle', 'draw_circle', 'draw_polygon', or 'get_model_curves'." };
            }

            try
            {
                switch (operation)
                {
                    case "draw_line":
                        return DrawModelLine(doc, parameters);

                    case "draw_arc":
                        return DrawModelArc(doc, parameters);

                    case "draw_curve_by_points":
                        return DrawCurveByPoints(doc, parameters);

                    case "draw_sine_wave":
                        return DrawSineWaveCurve(doc, parameters);

                    case "draw_spiral":
                        return DrawSpiralCurve(doc, parameters);

                    case "draw_helix":
                        return DrawHelixCurve(doc, parameters);

                    case "draw_cosine_wave":
                        return DrawCosineWaveCurve(doc, parameters);

                    case "draw_rectangle":
                        return DrawRectangle(doc, parameters);

                    case "draw_circle":
                        return DrawCircle(doc, parameters);

                    case "draw_polygon":
                        return DrawPolygon(doc, parameters);

                    case "get_model_curves":
                        return GetModelCurves(doc);

                    default:
                        return new { success = false, error = $"Unknown model curve operation: {operation}. Use 'draw_line', 'draw_arc', 'draw_curve_by_points', 'draw_sine_wave', 'draw_spiral', 'draw_helix', 'draw_cosine_wave', 'draw_rectangle', 'draw_circle', 'draw_polygon', or 'get_model_curves'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Draws a model line between two points using NewModelCurve.
        /// </summary>
        private object DrawModelLine(Document doc, Dictionary<string, object> parameters)
        {
            // Parse start point
            double startX = parameters.ContainsKey("start_x") ? Convert.ToDouble(parameters["start_x"]) : 0;
            double startY = parameters.ContainsKey("start_y") ? Convert.ToDouble(parameters["start_y"]) : 0;
            double startZ = parameters.ContainsKey("start_z") ? Convert.ToDouble(parameters["start_z"]) : 0;

            // Parse end point
            double endX = parameters.ContainsKey("end_x") ? Convert.ToDouble(parameters["end_x"]) : 10;
            double endY = parameters.ContainsKey("end_y") ? Convert.ToDouble(parameters["end_y"]) : 0;
            double endZ = parameters.ContainsKey("end_z") ? Convert.ToDouble(parameters["end_z"]) : 0;

            XYZ startPoint = new XYZ(startX, startY, startZ);
            XYZ endPoint = new XYZ(endX, endY, endZ);

            if (startPoint.DistanceTo(endPoint) < 0.001)
            {
                return new { success = false, error = "Start and end points must be different." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Model Line"))
            {
                trans.Start();

                // Create line geometry
                Line line = Line.CreateBound(startPoint, endPoint);

                // Get default family sketch plane (Reference Level / horizontal plane by default)
                SketchPlane sketchPlane = GetDefaultFamilySketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    trans.RollBack();
                    return new { success = false, error = "Could not find or create sketch plane for model line." };
                }

                // Create model curve
                ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Model line created successfully on Reference Level plane",
                    modelCurveId = GetElementIdInt(modelCurve.Id),
                    sketchPlaneId = GetElementIdInt(sketchPlane.Id),
                    sketchPlaneName = sketchPlane.Name,
                    startPoint = new { x = startX, y = startY, z = startZ },
                    endPoint = new { x = endX, y = endY, z = endZ },
                    length = startPoint.DistanceTo(endPoint)
                };
            }
        }

        /// <summary>
        /// Draws a model arc using NewModelCurve.
        /// Supports arc by center/radius/angles or by three points.
        /// </summary>
        private object DrawModelArc(Document doc, Dictionary<string, object> parameters)
        {
            string arcMode = parameters.ContainsKey("arc_mode") ? parameters["arc_mode"]?.ToString()?.ToLower() : "center_radius";

            Arc arc;
            XYZ center;

            if (arcMode == "three_points")
            {
                // Arc by three points
                double p1x = parameters.ContainsKey("point1_x") ? Convert.ToDouble(parameters["point1_x"]) : 0;
                double p1y = parameters.ContainsKey("point1_y") ? Convert.ToDouble(parameters["point1_y"]) : 0;
                double p1z = parameters.ContainsKey("point1_z") ? Convert.ToDouble(parameters["point1_z"]) : 0;

                double p2x = parameters.ContainsKey("point2_x") ? Convert.ToDouble(parameters["point2_x"]) : 5;
                double p2y = parameters.ContainsKey("point2_y") ? Convert.ToDouble(parameters["point2_y"]) : 5;
                double p2z = parameters.ContainsKey("point2_z") ? Convert.ToDouble(parameters["point2_z"]) : 0;

                double p3x = parameters.ContainsKey("point3_x") ? Convert.ToDouble(parameters["point3_x"]) : 10;
                double p3y = parameters.ContainsKey("point3_y") ? Convert.ToDouble(parameters["point3_y"]) : 0;
                double p3z = parameters.ContainsKey("point3_z") ? Convert.ToDouble(parameters["point3_z"]) : 0;

                XYZ point1 = new XYZ(p1x, p1y, p1z);
                XYZ point2 = new XYZ(p2x, p2y, p2z);
                XYZ point3 = new XYZ(p3x, p3y, p3z);

                arc = Arc.Create(point1, point3, point2);
                center = arc.Center;
            }
            else
            {
                // Arc by center, radius, and angles
                double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;

                double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
                double startAngle = parameters.ContainsKey("start_angle") ? Convert.ToDouble(parameters["start_angle"]) : 0;
                double endAngle = parameters.ContainsKey("end_angle") ? Convert.ToDouble(parameters["end_angle"]) : 180;

                if (radius <= 0)
                {
                    return new { success = false, error = "Radius must be positive." };
                }

                center = new XYZ(centerX, centerY, centerZ);

                // Convert degrees to radians
                double startRad = startAngle * Math.PI / 180.0;
                double endRad = endAngle * Math.PI / 180.0;

                // Determine the plane normal (default XY plane)
                string planeNormal = parameters.ContainsKey("plane") ? parameters["plane"]?.ToString()?.ToUpper() : "XY";
                XYZ xVec, yVec;

                switch (planeNormal)
                {
                    case "XZ":
                        xVec = XYZ.BasisX;
                        yVec = XYZ.BasisZ;
                        break;
                    case "YZ":
                        xVec = XYZ.BasisY;
                        yVec = XYZ.BasisZ;
                        break;
                    default: // XY
                        xVec = XYZ.BasisX;
                        yVec = XYZ.BasisY;
                        break;
                }

                arc = Arc.Create(center, radius, startRad, endRad, xVec, yVec);
            }

            using (Transaction trans = new Transaction(doc, "Draw Model Arc"))
            {
                trans.Start();

                // Get default family sketch plane (Reference Level / horizontal plane by default)
                SketchPlane sketchPlane = GetDefaultFamilySketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    trans.RollBack();
                    return new { success = false, error = "Could not find or create sketch plane for model arc." };
                }

                // Create model curve
                ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(arc, sketchPlane);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Model arc created successfully on Reference Level plane",
                    modelCurveId = GetElementIdInt(modelCurve.Id),
                    sketchPlaneId = GetElementIdInt(sketchPlane.Id),
                    sketchPlaneName = sketchPlane.Name,
                    center = new { x = center.X, y = center.Y, z = center.Z },
                    radius = arc.Radius,
                    arcLength = arc.Length
                };
            }
        }

        /// <summary>
        /// Draws a curve by points using NewCurveByPoints (creates a spline through reference points).
        /// </summary>
        private object DrawCurveByPoints(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("points") || parameters["points"] == null)
            {
                return new { success = false, error = "points is required. Provide a list of {x, y, z} coordinates." };
            }

            bool isReferenceLine = parameters.ContainsKey("is_reference_line") && Convert.ToBoolean(parameters["is_reference_line"]);

            var points = new List<XYZ>();
            var pointsObj = parameters["points"];

            // Parse points array
            if (pointsObj is JArray jArray)
            {
                foreach (JObject pt in jArray)
                {
                    double x = pt.ContainsKey("x") ? Convert.ToDouble(pt["x"]) : 0;
                    double y = pt.ContainsKey("y") ? Convert.ToDouble(pt["y"]) : 0;
                    double z = pt.ContainsKey("z") ? Convert.ToDouble(pt["z"]) : 0;
                    points.Add(new XYZ(x, y, z));
                }
            }
            else if (pointsObj is IEnumerable<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        double x = dict.ContainsKey("x") ? Convert.ToDouble(dict["x"]) : 0;
                        double y = dict.ContainsKey("y") ? Convert.ToDouble(dict["y"]) : 0;
                        double z = dict.ContainsKey("z") ? Convert.ToDouble(dict["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }

            if (points.Count < 2)
            {
                return new { success = false, error = "At least 2 points are required to create a curve." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Curve By Points"))
            {
                trans.Start();

                // Create reference points
                var refPointArray = new ReferencePointArray();
                var createdPointIds = new List<int>();

                foreach (var pt in points)
                {
                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(pt);
                    refPointArray.Append(refPt);
                    createdPointIds.Add(GetElementIdInt(refPt.Id));
                }

                // Create curve by points
                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPointArray);

                // Optionally convert to reference line
                if (isReferenceLine)
                {
                    curveByPoints.IsReferenceLine = true;
                }

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Curve by points created successfully",
                    curveByPointsId = GetElementIdInt(curveByPoints.Id),
                    isReferenceLine = curveByPoints.IsReferenceLine,
                    pointCount = points.Count,
                    referencePointIds = createdPointIds
                };
            }
        }

        /// <summary>
        /// Draws a sine wave curve using NewCurveByPoints: z = amplitude * sin(frequency * x)
        /// </summary>
        private object DrawSineWaveCurve(Document doc, Dictionary<string, object> parameters)
        {
            double amplitude = parameters.ContainsKey("amplitude") ? Convert.ToDouble(parameters["amplitude"]) : 2;
            double frequency = parameters.ContainsKey("frequency") ? Convert.ToDouble(parameters["frequency"]) : 1;
            double startX = parameters.ContainsKey("start_x") ? Convert.ToDouble(parameters["start_x"]) : 0;
            double endX = parameters.ContainsKey("end_x") ? Convert.ToDouble(parameters["end_x"]) : 20;
            int pointCount = parameters.ContainsKey("point_count") ? Convert.ToInt32(parameters["point_count"]) : 50;
            double baseY = parameters.ContainsKey("base_y") ? Convert.ToDouble(parameters["base_y"]) : 0;
            double baseZ = parameters.ContainsKey("base_z") ? Convert.ToDouble(parameters["base_z"]) : 0;
            string waveAxis = parameters.ContainsKey("wave_axis") ? parameters["wave_axis"]?.ToString()?.ToUpper() : "Z"; // Z or Y
            double phaseShift = parameters.ContainsKey("phase_shift") ? Convert.ToDouble(parameters["phase_shift"]) : 0;

            if (pointCount < 2 || pointCount > 500)
            {
                return new { success = false, error = "point_count must be between 2 and 500." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Sine Wave Curve"))
            {
                trans.Start();

                var refPointArray = new ReferencePointArray();
                var createdPointIds = new List<int>();
                double step = (endX - startX) / (pointCount - 1);

                for (int i = 0; i < pointCount; i++)
                {
                    double x = startX + (i * step);
                    double waveValue = amplitude * Math.Sin(frequency * x + phaseShift);

                    XYZ point;
                    if (waveAxis == "Y")
                    {
                        point = new XYZ(x, baseY + waveValue, baseZ);
                    }
                    else // Z
                    {
                        point = new XYZ(x, baseY, baseZ + waveValue);
                    }

                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(point);
                    refPointArray.Append(refPt);
                    createdPointIds.Add(GetElementIdInt(refPt.Id));
                }

                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPointArray);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Sine wave curve created successfully",
                    curveByPointsId = GetElementIdInt(curveByPoints.Id),
                    formula = $"wave = {amplitude} * sin({frequency} * x + {phaseShift})",
                    amplitude = amplitude,
                    frequency = frequency,
                    phaseShift = phaseShift,
                    waveAxis = waveAxis,
                    pointCount = pointCount,
                    referencePointIds = createdPointIds
                };
            }
        }

        /// <summary>
        /// Draws a cosine wave curve using NewCurveByPoints: z = amplitude * cos(frequency * x)
        /// </summary>
        private object DrawCosineWaveCurve(Document doc, Dictionary<string, object> parameters)
        {
            double amplitude = parameters.ContainsKey("amplitude") ? Convert.ToDouble(parameters["amplitude"]) : 2;
            double frequency = parameters.ContainsKey("frequency") ? Convert.ToDouble(parameters["frequency"]) : 1;
            double startX = parameters.ContainsKey("start_x") ? Convert.ToDouble(parameters["start_x"]) : 0;
            double endX = parameters.ContainsKey("end_x") ? Convert.ToDouble(parameters["end_x"]) : 20;
            int pointCount = parameters.ContainsKey("point_count") ? Convert.ToInt32(parameters["point_count"]) : 50;
            double baseY = parameters.ContainsKey("base_y") ? Convert.ToDouble(parameters["base_y"]) : 0;
            double baseZ = parameters.ContainsKey("base_z") ? Convert.ToDouble(parameters["base_z"]) : 0;
            string waveAxis = parameters.ContainsKey("wave_axis") ? parameters["wave_axis"]?.ToString()?.ToUpper() : "Z";
            double phaseShift = parameters.ContainsKey("phase_shift") ? Convert.ToDouble(parameters["phase_shift"]) : 0;

            if (pointCount < 2 || pointCount > 500)
            {
                return new { success = false, error = "point_count must be between 2 and 500." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Cosine Wave Curve"))
            {
                trans.Start();

                var refPointArray = new ReferencePointArray();
                var createdPointIds = new List<int>();
                double step = (endX - startX) / (pointCount - 1);

                for (int i = 0; i < pointCount; i++)
                {
                    double x = startX + (i * step);
                    double waveValue = amplitude * Math.Cos(frequency * x + phaseShift);

                    XYZ point;
                    if (waveAxis == "Y")
                    {
                        point = new XYZ(x, baseY + waveValue, baseZ);
                    }
                    else
                    {
                        point = new XYZ(x, baseY, baseZ + waveValue);
                    }

                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(point);
                    refPointArray.Append(refPt);
                    createdPointIds.Add(GetElementIdInt(refPt.Id));
                }

                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPointArray);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Cosine wave curve created successfully",
                    curveByPointsId = GetElementIdInt(curveByPoints.Id),
                    formula = $"wave = {amplitude} * cos({frequency} * x + {phaseShift})",
                    amplitude = amplitude,
                    frequency = frequency,
                    phaseShift = phaseShift,
                    waveAxis = waveAxis,
                    pointCount = pointCount,
                    referencePointIds = createdPointIds
                };
            }
        }

        /// <summary>
        /// Draws a 2D spiral curve using NewCurveByPoints: r = a + b*theta
        /// x = r*cos(theta), y = r*sin(theta)
        /// </summary>
        private object DrawSpiralCurve(Document doc, Dictionary<string, object> parameters)
        {
            double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
            double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
            double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
            double initialRadius = parameters.ContainsKey("initial_radius") ? Convert.ToDouble(parameters["initial_radius"]) : 1;
            double growthRate = parameters.ContainsKey("growth_rate") ? Convert.ToDouble(parameters["growth_rate"]) : 0.5;
            double turns = parameters.ContainsKey("turns") ? Convert.ToDouble(parameters["turns"]) : 3;
            int pointCount = parameters.ContainsKey("point_count") ? Convert.ToInt32(parameters["point_count"]) : 100;
            string spiralPlane = parameters.ContainsKey("plane") ? parameters["plane"]?.ToString()?.ToUpper() : "XY";

            if (pointCount < 2 || pointCount > 500)
            {
                return new { success = false, error = "point_count must be between 2 and 500." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Spiral Curve"))
            {
                trans.Start();

                var refPointArray = new ReferencePointArray();
                var createdPointIds = new List<int>();
                double maxAngle = turns * 2 * Math.PI;
                double angleStep = maxAngle / (pointCount - 1);

                for (int i = 0; i < pointCount; i++)
                {
                    double theta = i * angleStep;
                    double r = initialRadius + (growthRate * theta);

                    XYZ point;
                    switch (spiralPlane)
                    {
                        case "XZ":
                            point = new XYZ(centerX + r * Math.Cos(theta), centerY, centerZ + r * Math.Sin(theta));
                            break;
                        case "YZ":
                            point = new XYZ(centerX, centerY + r * Math.Cos(theta), centerZ + r * Math.Sin(theta));
                            break;
                        default: // XY
                            point = new XYZ(centerX + r * Math.Cos(theta), centerY + r * Math.Sin(theta), centerZ);
                            break;
                    }

                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(point);
                    refPointArray.Append(refPt);
                    createdPointIds.Add(GetElementIdInt(refPt.Id));
                }

                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPointArray);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Spiral curve created successfully",
                    curveByPointsId = GetElementIdInt(curveByPoints.Id),
                    formula = $"r = {initialRadius} + {growthRate} * theta",
                    initialRadius = initialRadius,
                    growthRate = growthRate,
                    turns = turns,
                    plane = spiralPlane,
                    pointCount = pointCount,
                    referencePointIds = createdPointIds
                };
            }
        }

        /// <summary>
        /// Draws a 3D helix curve using NewCurveByPoints.
        /// x = radius*cos(theta), y = radius*sin(theta), z = pitch*theta/(2*pi)
        /// </summary>
        private object DrawHelixCurve(Document doc, Dictionary<string, object> parameters)
        {
            double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
            double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
            double baseZ = parameters.ContainsKey("base_z") ? Convert.ToDouble(parameters["base_z"]) : 0;
            double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
            double pitch = parameters.ContainsKey("pitch") ? Convert.ToDouble(parameters["pitch"]) : 3; // height per turn
            double turns = parameters.ContainsKey("turns") ? Convert.ToDouble(parameters["turns"]) : 3;
            int pointCount = parameters.ContainsKey("point_count") ? Convert.ToInt32(parameters["point_count"]) : 100;
            bool clockwise = parameters.ContainsKey("clockwise") && Convert.ToBoolean(parameters["clockwise"]);

            if (pointCount < 2 || pointCount > 500)
            {
                return new { success = false, error = "point_count must be between 2 and 500." };
            }
            if (radius <= 0)
            {
                return new { success = false, error = "radius must be positive." };
            }

            using (Transaction trans = new Transaction(doc, "Draw Helix Curve"))
            {
                trans.Start();

                var refPointArray = new ReferencePointArray();
                var createdPointIds = new List<int>();
                double maxAngle = turns * 2 * Math.PI;
                double angleStep = maxAngle / (pointCount - 1);
                int direction = clockwise ? -1 : 1;

                for (int i = 0; i < pointCount; i++)
                {
                    double theta = i * angleStep;
                    double x = centerX + radius * Math.Cos(direction * theta);
                    double y = centerY + radius * Math.Sin(direction * theta);
                    double z = baseZ + (pitch * theta / (2 * Math.PI));

                    XYZ point = new XYZ(x, y, z);
                    ReferencePoint refPt = doc.FamilyCreate.NewReferencePoint(point);
                    refPointArray.Append(refPt);
                    createdPointIds.Add(GetElementIdInt(refPt.Id));
                }

                CurveByPoints curveByPoints = doc.FamilyCreate.NewCurveByPoints(refPointArray);

                trans.Commit();

                double totalHeight = pitch * turns;

                return new
                {
                    success = true,
                    message = "Helix curve created successfully",
                    curveByPointsId = GetElementIdInt(curveByPoints.Id),
                    radius = radius,
                    pitch = pitch,
                    turns = turns,
                    totalHeight = totalHeight,
                    clockwise = clockwise,
                    pointCount = pointCount,
                    referencePointIds = createdPointIds
                };
            }
        }

        /// <summary>
        /// Gets all model curves in the current family document.
        /// </summary>
        private object GetModelCurves(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var modelCurves = collector.OfClass(typeof(CurveElement)).Cast<CurveElement>().ToList();

            var curvesList = new List<object>();

            foreach (var curve in modelCurves)
            {
                try
                {
                    Curve geomCurve = curve.GeometryCurve;
                    string curveType = geomCurve.GetType().Name;

                    curvesList.Add(new
                    {
                        id = GetElementIdInt(curve.Id),
                        elementType = curve.GetType().Name,
                        curveType = curveType,
                        length = geomCurve.Length,
                        isBound = geomCurve.IsBound,
                        startPoint = geomCurve.IsBound ? new { x = geomCurve.GetEndPoint(0).X, y = geomCurve.GetEndPoint(0).Y, z = geomCurve.GetEndPoint(0).Z } : null,
                        endPoint = geomCurve.IsBound ? new { x = geomCurve.GetEndPoint(1).X, y = geomCurve.GetEndPoint(1).Y, z = geomCurve.GetEndPoint(1).Z } : null
                    });
                }
                catch
                {
                    curvesList.Add(new
                    {
                        id = GetElementIdInt(curve.Id),
                        elementType = curve.GetType().Name,
                        curveType = "Unknown",
                        length = 0.0,
                        isBound = false,
                        startPoint = (object)null,
                        endPoint = (object)null
                    });
                }
            }

            return new
            {
                success = true,
                count = curvesList.Count,
                modelCurves = curvesList
            };
        }

        /// <summary>
        /// Draw rectangle using model curves in a family document
        /// </summary>
        private object DrawRectangle(Document doc, Dictionary<string, object> parameters)
        {
            double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
            double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
            double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
            double width = parameters.ContainsKey("width") ? Convert.ToDouble(parameters["width"]) : 10;
            double height = parameters.ContainsKey("height") ? Convert.ToDouble(parameters["height"]) : 5;

            XYZ center = new XYZ(centerX, centerY, centerZ);
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            // Calculate corner points
            XYZ[] corners = new XYZ[4]
            {
                center + new XYZ(-halfWidth, -halfHeight, 0),
                center + new XYZ(halfWidth, -halfHeight, 0),
                center + new XYZ(halfWidth, halfHeight, 0),
                center + new XYZ(-halfWidth, halfHeight, 0)
            };

            // Get default family sketch plane
            SketchPlane sketchPlane = GetDefaultFamilySketchPlane(doc, parameters);
            return DrawClosedShape(doc, corners, "Rectangle", sketchPlane);
        }

        /// <summary>
        /// Draw circle using model curves in a family document
        /// </summary>
        private object DrawCircle(Document doc, Dictionary<string, object> parameters)
        {
            double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
            double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
            double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
            double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
            int segments = parameters.ContainsKey("segments") ? Convert.ToInt32(parameters["segments"]) : 24;

            if (segments < 4 || segments > 100)
                segments = 24;

            XYZ center = new XYZ(centerX, centerY, centerZ);

            // Generate circle points
            XYZ[] points = new XYZ[segments];
            double angleStep = 2 * Math.PI / segments;

            for (int i = 0; i < segments; i++)
            {
                double angle = i * angleStep;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                points[i] = new XYZ(x, y, centerZ);
            }

            // Get default family sketch plane
            SketchPlane sketchPlane = GetDefaultFamilySketchPlane(doc, parameters);
            return DrawClosedShape(doc, points, $"Circle (r={radius})", sketchPlane);
        }

        /// <summary>
        /// Draw regular polygon (3-12 sides) using model curves in a family document
        /// </summary>
        private object DrawPolygon(Document doc, Dictionary<string, object> parameters)
        {
            double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
            double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
            double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
            double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
            int sides = parameters.ContainsKey("sides") ? Convert.ToInt32(parameters["sides"]) : 6;

            if (sides < 3 || sides > 12)
            {
                return new { success = false, error = "Polygon sides must be between 3 and 12." };
            }

            XYZ center = new XYZ(centerX, centerY, centerZ);

            // Generate polygon points
            XYZ[] points = new XYZ[sides];
            double angleStep = 2 * Math.PI / sides;
            // Start at top (90 degrees offset for symmetry)
            double startAngle = Math.PI / 2;

            for (int i = 0; i < sides; i++)
            {
                double angle = startAngle + i * angleStep;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                points[i] = new XYZ(x, y, centerZ);
            }

            string[] shapeNames = new[] { "", "", "Triangle", "Square", "Pentagon", "Hexagon", "Heptagon", "Octagon", "Nonagon", "Decagon", "Hendecagon", "Dodecagon" };
            string shapeName = sides <= 12 ? shapeNames[sides] : $"{sides}-sided Polygon";

            // Get default family sketch plane
            SketchPlane sketchPlane = GetDefaultFamilySketchPlane(doc, parameters);
            return DrawClosedShape(doc, points, shapeName, sketchPlane);
        }

        /// <summary>
        /// Helper to draw closed shapes using model lines
        /// </summary>
        private object DrawClosedShape(Document doc, XYZ[] points, string shapeName, SketchPlane sketchPlane = null)
        {
            try
            {
                if (points.Length < 2)
                {
                    return new { success = false, error = "At least 2 points required." };
                }

                var createdLineIds = new List<int>();

                using (Transaction trans = new Transaction(doc, $"Draw {shapeName}"))
                {
                    trans.Start();

                    // Use provided sketch plane or create default
                    if (sketchPlane == null)
                    {
                        Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, points[0]);
                        sketchPlane = SketchPlane.Create(doc, plane);
                    }

                    // Draw lines connecting all points, closing the shape
                    for (int i = 0; i < points.Length; i++)
                    {
                        XYZ startPt = points[i];
                        XYZ endPt = points[(i + 1) % points.Length]; // Wrap to first point to close

                        Line line = Line.CreateBound(startPt, endPt);
                        ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(line, sketchPlane);
                        createdLineIds.Add(GetElementIdInt(modelCurve.Id));
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = $"{shapeName} created successfully on Reference Level plane",
                        shape = shapeName,
                        line_count = createdLineIds.Count,
                        line_ids = createdLineIds,
                        point_count = points.Length,
                        sketch_plane_id = GetElementIdInt(sketchPlane.Id),
                        sketch_plane_name = sketchPlane.Name
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"Error drawing {shapeName}: {ex.Message}" };
            }
        }

        #endregion

        #region Divided Surface Tool

        /// <summary>
        /// Tool for creating divided surfaces and pattern systems.
        /// Supports creating divided surfaces from form faces and configuring grid patterns.
        /// </summary>
        private object DividedSurfaceTool(UIApplication uiApp, Document doc, Dictionary<string, object> parameters)
        {
            // Check if we're in a family document
            if (!doc.IsFamilyDocument)
            {
                return new { success = false, error = "This tool requires a family document. Open a conceptual mass or adaptive family." };
            }

            string operation = parameters.ContainsKey("operation") ? parameters["operation"]?.ToString()?.ToLower() : null;
            if (string.IsNullOrEmpty(operation))
            {
                return new { success = false, error = "operation is required. Use 'create_divided_surface', 'set_uv_divisions', 'get_divided_surfaces', or 'get_forms'." };
            }

            try
            {
                switch (operation)
                {
                    case "create_divided_surface":
                        return CreateDividedSurface(doc, parameters);

                    case "set_uv_divisions":
                        return SetUVDivisions(doc, parameters);

                    case "get_divided_surfaces":
                        return GetDividedSurfaces(doc);

                    case "get_forms":
                        return GetFormsForDividedSurface(doc);

                    default:
                        return new { success = false, error = $"Unknown divided surface operation: {operation}. Use 'create_divided_surface', 'set_uv_divisions', 'get_divided_surfaces', or 'get_forms'." };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        /// <summary>
        /// Creates a divided surface from a form element's face using DividedSurface.Create.
        /// </summary>
        private object CreateDividedSurface(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("form_id") || parameters["form_id"] == null)
            {
                return new { success = false, error = "form_id is required. Provide the element ID of a Form element." };
            }

            int formId = Convert.ToInt32(parameters["form_id"]);
            int faceIndex = parameters.ContainsKey("face_index") ? Convert.ToInt32(parameters["face_index"]) : 0;

            Element formElem = doc.GetElement(new ElementId(formId));
            if (formElem == null)
            {
                return new { success = false, error = $"Form element with ID {formId} not found." };
            }

            if (!(formElem is Form form))
            {
                return new { success = false, error = $"Element {formId} is not a Form. Type: {formElem.GetType().Name}" };
            }

            // Get geometry to find faces
            Options geomOptions = new Options();
            geomOptions.ComputeReferences = true;
            GeometryElement geomElem = form.get_Geometry(geomOptions);

            if (geomElem == null)
            {
                return new { success = false, error = "Could not retrieve geometry from the form." };
            }

            // Collect all faces from the form geometry
            var faces = new List<Face>();
            var faceRefs = new List<Reference>();

            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid solid)
                {
                    foreach (Face face in solid.Faces)
                    {
                        if (face.Reference != null)
                        {
                            faces.Add(face);
                            faceRefs.Add(face.Reference);
                        }
                    }
                }
            }

            if (faces.Count == 0)
            {
                return new { success = false, error = "No faces found on the form. Ensure the form has valid geometry." };
            }

            if (faceIndex < 0 || faceIndex >= faces.Count)
            {
                return new { success = false, error = $"face_index {faceIndex} is out of range. The form has {faces.Count} faces (0 to {faces.Count - 1})." };
            }

            Reference selectedFaceRef = faceRefs[faceIndex];

            using (Transaction trans = new Transaction(doc, "Create Divided Surface"))
            {
                trans.Start();

                // Create the divided surface using static Create method
                DividedSurface dividedSurface = DividedSurface.Create(doc, selectedFaceRef);

                trans.Commit();

                return new
                {
                    success = true,
                    message = "Divided surface created successfully",
                    dividedSurfaceId = GetElementIdInt(dividedSurface.Id),
                    formId = formId,
                    faceIndex = faceIndex,
                    totalFacesOnForm = faces.Count
                };
            }
        }

        /// <summary>
        /// Sets the U and V division counts on a divided surface using parameter names.
        /// </summary>
        private object SetUVDivisions(Document doc, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("divided_surface_id") || parameters["divided_surface_id"] == null)
            {
                return new { success = false, error = "divided_surface_id is required." };
            }

            int dividedSurfaceId = Convert.ToInt32(parameters["divided_surface_id"]);
            int uDivisions = parameters.ContainsKey("u_divisions") ? Convert.ToInt32(parameters["u_divisions"]) : -1;
            int vDivisions = parameters.ContainsKey("v_divisions") ? Convert.ToInt32(parameters["v_divisions"]) : -1;

            Element elem = doc.GetElement(new ElementId(dividedSurfaceId));
            if (elem == null)
            {
                return new { success = false, error = $"Element with ID {dividedSurfaceId} not found." };
            }

            if (!(elem is DividedSurface dividedSurface))
            {
                return new { success = false, error = $"Element {dividedSurfaceId} is not a DividedSurface. Type: {elem.GetType().Name}" };
            }

            int actualU = -1;
            int actualV = -1;

            using (Transaction trans = new Transaction(doc, "Set UV Divisions"))
            {
                trans.Start();

                // Try to find and set UV parameters by searching through all parameters
                foreach (Parameter param in dividedSurface.Parameters)
                {
                    string paramName = param.Definition?.Name?.ToLower() ?? "";
                    
                    if (uDivisions > 0 && (paramName.Contains("u") && (paramName.Contains("grid") || paramName.Contains("division") || paramName.Contains("number"))))
                    {
                        if (!param.IsReadOnly && param.StorageType == StorageType.Integer)
                        {
                            param.Set(uDivisions);
                            actualU = uDivisions;
                        }
                    }
                    
                    if (vDivisions > 0 && (paramName.Contains("v") && (paramName.Contains("grid") || paramName.Contains("division") || paramName.Contains("number"))))
                    {
                        if (!param.IsReadOnly && param.StorageType == StorageType.Integer)
                        {
                            param.Set(vDivisions);
                            actualV = vDivisions;
                        }
                    }
                }

                trans.Commit();

                return new
                {
                    success = true,
                    message = "UV divisions updated",
                    dividedSurfaceId = dividedSurfaceId,
                    uDivisions = actualU,
                    vDivisions = actualV,
                    note = "Actual parameter values may vary based on available parameters."
                };
            }
        }

        /// <summary>
        /// Gets all divided surfaces in the current family document.
        /// </summary>
        private object GetDividedSurfaces(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var dividedSurfaces = collector.OfClass(typeof(DividedSurface)).Cast<DividedSurface>().ToList();

            var surfacesList = new List<object>();

            foreach (var ds in dividedSurfaces)
            {
                try
                {
                    // Collect parameter information
                    var paramInfo = new Dictionary<string, object>();
                    foreach (Parameter param in ds.Parameters)
                    {
                        if (param.Definition != null && param.HasValue)
                        {
                            string name = param.Definition.Name;
                            object value = null;
                            switch (param.StorageType)
                            {
                                case StorageType.Integer:
                                    value = param.AsInteger();
                                    break;
                                case StorageType.Double:
                                    value = param.AsDouble();
                                    break;
                                case StorageType.String:
                                    value = param.AsString();
                                    break;
                            }
                            if (value != null)
                            {
                                paramInfo[name] = value;
                            }
                        }
                    }

                    surfacesList.Add(new
                    {
                        id = GetElementIdInt(ds.Id),
                        name = ds.Name,
                        parameters = paramInfo
                    });
                }
                catch
                {
                    surfacesList.Add(new
                    {
                        id = GetElementIdInt(ds.Id),
                        name = ds.Name,
                        parameters = new Dictionary<string, object>()
                    });
                }
            }

            return new
            {
                success = true,
                count = surfacesList.Count,
                dividedSurfaces = surfacesList
            };
        }

        /// <summary>
        /// Gets all Form elements that can be used to create divided surfaces.
        /// </summary>
        private object GetFormsForDividedSurface(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var forms = collector.OfClass(typeof(Form)).Cast<Form>().ToList();

            var formsList = new List<object>();

            foreach (var form in forms)
            {
                // Count faces on this form
                int faceCount = 0;
                try
                {
                    Options geomOptions = new Options();
                    geomOptions.ComputeReferences = true;
                    GeometryElement geomElem = form.get_Geometry(geomOptions);

                    if (geomElem != null)
                    {
                        foreach (GeometryObject geomObj in geomElem)
                        {
                            if (geomObj is Solid solid)
                            {
                                faceCount += solid.Faces.Size;
                            }
                        }
                    }
                }
                catch { }

                formsList.Add(new
                {
                    id = GetElementIdInt(form.Id),
                    name = form.Name,
                    faceCount = faceCount
                });
            }

            return new
            {
                success = true,
                count = formsList.Count,
                forms = formsList,
                hint = "Use form_id and face_index (0 to faceCount-1) with create_divided_surface operation."
            };
        }

        #endregion

        #region Dimension Tool

        /// <summary>
        /// Create linear dimensions in projects or families
        /// </summary>
        private object DimensionTool(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string operation = parameters.ContainsKey("operation") ? parameters["operation"].ToString() : "get_dimension_types";

                switch (operation.ToLower())
                {
                    case "create_linear_dimension":
                        return CreateLinearDimension(doc, parameters);

                    case "create_dimension_from_references":
                        return CreateDimensionFromReferences(doc, parameters);

                    case "create_dimension_between_walls":
                        return CreateDimensionBetweenWalls(doc, parameters);

                    case "create_dimension_between_grids":
                        return CreateDimensionBetweenGrids(doc, parameters);

                    case "create_radial_dimension":
                        return CreateRadialDimension(doc, parameters);

                    case "modify_dimension":
                        return ModifyDimension(doc, parameters);

                    case "get_dimension_types":
                        return GetDimensionTypes(doc);

                    case "get_dimensions":
                        return GetDimensions(doc);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown operation: {operation}",
                            available_operations = new[]
                            {
                                "create_linear_dimension",
                                "create_dimension_from_references",
                                "create_dimension_between_walls",
                                "create_dimension_between_grids",
                                "create_radial_dimension",
                                "modify_dimension",
                                "get_dimension_types",
                                "get_dimensions"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"DimensionTool error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// Create linear dimension between two points with a dimension line
        /// </summary>
        private object CreateLinearDimension(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get start and end points
                double startX = parameters.ContainsKey("start_x") ? Convert.ToDouble(parameters["start_x"]) : 0;
                double startY = parameters.ContainsKey("start_y") ? Convert.ToDouble(parameters["start_y"]) : 0;
                double startZ = parameters.ContainsKey("start_z") ? Convert.ToDouble(parameters["start_z"]) : 0;
                double endX = parameters.ContainsKey("end_x") ? Convert.ToDouble(parameters["end_x"]) : 10;
                double endY = parameters.ContainsKey("end_y") ? Convert.ToDouble(parameters["end_y"]) : 0;
                double endZ = parameters.ContainsKey("end_z") ? Convert.ToDouble(parameters["end_z"]) : 0;

                // Dimension line offset from the line being dimensioned
                double offsetDistance = parameters.ContainsKey("offset") ? Convert.ToDouble(parameters["offset"]) : 2;

                // Optional dimension type
                int? dimTypeId = parameters.ContainsKey("dimension_type_id") ? 
                    Convert.ToInt32(parameters["dimension_type_id"]) : (int?)null;

                XYZ startPoint = new XYZ(startX, startY, startZ);
                XYZ endPoint = new XYZ(endX, endY, endZ);

                // Get active view
                View view = doc.ActiveView;
                if (view == null)
                {
                    return new { success = false, error = "No active view" };
                }

                // Check if we're in a family document
                bool isFamilyDoc = doc.IsFamilyDocument;

                using (Transaction trans = new Transaction(doc, "Create Linear Dimension"))
                {
                    trans.Start();

                    // Create detail lines for the dimension references
                    XYZ direction = (endPoint - startPoint).Normalize();
                    XYZ perpendicular = new XYZ(-direction.Y, direction.X, 0);
                    if (perpendicular.IsZeroLength())
                    {
                        perpendicular = new XYZ(0, 1, 0);
                    }

                    // Create dimension line location (offset from the line)
                    XYZ dimLineStart = startPoint + perpendicular * offsetDistance;
                    XYZ dimLineEnd = endPoint + perpendicular * offsetDistance;
                    Line dimensionLine = Line.CreateBound(dimLineStart, dimLineEnd);

                    // Create reference lines to dimension
                    Line refLine1 = Line.CreateBound(startPoint, startPoint + perpendicular * (offsetDistance + 1));
                    Line refLine2 = Line.CreateBound(endPoint, endPoint + perpendicular * (offsetDistance + 1));

                    DetailCurve dc1, dc2;
                    if (view.ViewType == ViewType.DraftingView || view.ViewType == ViewType.FloorPlan ||
                        view.ViewType == ViewType.CeilingPlan || view.ViewType == ViewType.Section ||
                        view.ViewType == ViewType.Elevation || view.ViewType == ViewType.Detail)
                    {
                        dc1 = doc.Create.NewDetailCurve(view, refLine1);
                        dc2 = doc.Create.NewDetailCurve(view, refLine2);
                    }
                    else
                    {
                        return new { success = false, error = "Active view type does not support detail lines and dimensions. Use a plan, section, elevation, or drafting view." };
                    }

                    // Collect references
                    ReferenceArray refArray = new ReferenceArray();
                    refArray.Append(dc1.GeometryCurve.GetEndPointReference(0));
                    refArray.Append(dc2.GeometryCurve.GetEndPointReference(0));

                    // Get dimension type
                    DimensionType dimType = null;
                    if (dimTypeId.HasValue)
                    {
                        Element typeElem = doc.GetElement(new ElementId(dimTypeId.Value));
                        if (typeElem is DimensionType dt)
                        {
                            dimType = dt;
                        }
                    }

                    // Create the dimension
                    Dimension dimension;
                    if (dimType != null)
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray, dimType);
                    }
                    else
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray);
                    }

                    // Delete the temporary detail lines if requested
                    bool deleteRefLines = parameters.ContainsKey("delete_ref_lines") ? 
                        Convert.ToBoolean(parameters["delete_ref_lines"]) : true;
                    if (deleteRefLines)
                    {
                        doc.Delete(dc1.Id);
                        doc.Delete(dc2.Id);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimension.Id),
                        value = dimension.Value.HasValue ? dimension.Value.Value : 0,
                        value_string = dimension.ValueString,
                        message = $"Created linear dimension measuring {dimension.ValueString}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateLinearDimension error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create dimension from element references (stable representation strings)
        /// </summary>
        private object CreateDimensionFromReferences(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("references"))
                {
                    return new { success = false, error = "Missing required parameter: references (array of element IDs or stable representations)" };
                }

                var referencesList = parameters["references"] as List<object>;
                if (referencesList == null || referencesList.Count < 2)
                {
                    return new { success = false, error = "Need at least 2 references to create a dimension" };
                }

                // Dimension line position
                double lineX1 = parameters.ContainsKey("line_x1") ? Convert.ToDouble(parameters["line_x1"]) : 0;
                double lineY1 = parameters.ContainsKey("line_y1") ? Convert.ToDouble(parameters["line_y1"]) : 0;
                double lineZ1 = parameters.ContainsKey("line_z1") ? Convert.ToDouble(parameters["line_z1"]) : 0;
                double lineX2 = parameters.ContainsKey("line_x2") ? Convert.ToDouble(parameters["line_x2"]) : 10;
                double lineY2 = parameters.ContainsKey("line_y2") ? Convert.ToDouble(parameters["line_y2"]) : 0;
                double lineZ2 = parameters.ContainsKey("line_z2") ? Convert.ToDouble(parameters["line_z2"]) : 0;

                int? dimTypeId = parameters.ContainsKey("dimension_type_id") ? 
                    Convert.ToInt32(parameters["dimension_type_id"]) : (int?)null;

                View view = doc.ActiveView;
                if (view == null)
                {
                    return new { success = false, error = "No active view" };
                }

                using (Transaction trans = new Transaction(doc, "Create Dimension from References"))
                {
                    trans.Start();

                    ReferenceArray refArray = new ReferenceArray();
                    foreach (var refObj in referencesList)
                    {
                        if (refObj is string stableRef)
                        {
                            // Try as stable representation
                            try
                            {
                                Reference reference = Reference.ParseFromStableRepresentation(doc, stableRef);
                                refArray.Append(reference);
                            }
                            catch
                            {
                                return new { success = false, error = $"Invalid stable representation: {stableRef}" };
                            }
                        }
                        else if (refObj is long || refObj is int || refObj is double)
                        {
                            // Element ID - get a reference from the element
                            int elemId = Convert.ToInt32(refObj);
                            Element elem = doc.GetElement(new ElementId(elemId));
                            if (elem == null)
                            {
                                return new { success = false, error = $"Element not found: {elemId}" };
                            }

                            // Try to get reference from element
                            Reference reference = GetReferenceFromElement(doc, elem);
                            if (reference == null)
                            {
                                return new { success = false, error = $"Cannot get reference from element {elemId}" };
                            }
                            refArray.Append(reference);
                        }
                    }

                    if (refArray.Size < 2)
                    {
                        return new { success = false, error = "Need at least 2 valid references" };
                    }

                    Line dimensionLine = Line.CreateBound(new XYZ(lineX1, lineY1, lineZ1), new XYZ(lineX2, lineY2, lineZ2));

                    DimensionType dimType = null;
                    if (dimTypeId.HasValue)
                    {
                        Element typeElem = doc.GetElement(new ElementId(dimTypeId.Value));
                        if (typeElem is DimensionType dt)
                        {
                            dimType = dt;
                        }
                    }

                    Dimension dimension;
                    if (dimType != null)
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray, dimType);
                    }
                    else
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimension.Id),
                        value_string = dimension.ValueString,
                        segments = dimension.NumberOfSegments
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateDimensionFromReferences error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create dimension between two walls
        /// </summary>
        private object CreateDimensionBetweenWalls(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("wall_id_1") || !parameters.ContainsKey("wall_id_2"))
                {
                    return new { success = false, error = "Missing required parameters: wall_id_1 and wall_id_2" };
                }

                int wallId1 = Convert.ToInt32(parameters["wall_id_1"]);
                int wallId2 = Convert.ToInt32(parameters["wall_id_2"]);

                Element elem1 = doc.GetElement(new ElementId(wallId1));
                Element elem2 = doc.GetElement(new ElementId(wallId2));

                if (!(elem1 is Wall wall1) || !(elem2 is Wall wall2))
                {
                    return new { success = false, error = "Both elements must be walls" };
                }

                // Get which face to use
                string faceType = parameters.ContainsKey("face") ? parameters["face"].ToString().ToLower() : "center";
                // center, interior, exterior

                // Offset for dimension line
                double offset = parameters.ContainsKey("offset") ? Convert.ToDouble(parameters["offset"]) : 3;

                int? dimTypeId = parameters.ContainsKey("dimension_type_id") ? 
                    Convert.ToInt32(parameters["dimension_type_id"]) : (int?)null;

                View view = doc.ActiveView;
                if (view == null)
                {
                    return new { success = false, error = "No active view" };
                }

                using (Transaction trans = new Transaction(doc, "Create Dimension Between Walls"))
                {
                    trans.Start();

                    ReferenceArray refArray = new ReferenceArray();

                    // Get wall references
                    Reference ref1 = GetWallFaceReference(doc, wall1, faceType);
                    Reference ref2 = GetWallFaceReference(doc, wall2, faceType);

                    if (ref1 == null || ref2 == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not get face references from walls. Try using get_element_references operation first." };
                    }

                    refArray.Append(ref1);
                    refArray.Append(ref2);

                    // Calculate dimension line
                    LocationCurve loc1 = wall1.Location as LocationCurve;
                    LocationCurve loc2 = wall2.Location as LocationCurve;

                    if (loc1 == null || loc2 == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not get wall locations" };
                    }

                    XYZ mid1 = (loc1.Curve.GetEndPoint(0) + loc1.Curve.GetEndPoint(1)) / 2;
                    XYZ mid2 = (loc2.Curve.GetEndPoint(0) + loc2.Curve.GetEndPoint(1)) / 2;

                    // Create dimension line perpendicular to walls direction
                    XYZ direction = (mid2 - mid1).Normalize();
                    XYZ dimStart = mid1 + new XYZ(0, 0, offset);
                    XYZ dimEnd = mid2 + new XYZ(0, 0, offset);

                    Line dimensionLine = Line.CreateBound(dimStart, dimEnd);

                    DimensionType dimType = null;
                    if (dimTypeId.HasValue)
                    {
                        Element typeElem = doc.GetElement(new ElementId(dimTypeId.Value));
                        if (typeElem is DimensionType dt)
                        {
                            dimType = dt;
                        }
                    }

                    Dimension dimension;
                    if (dimType != null)
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray, dimType);
                    }
                    else
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimension.Id),
                        value_string = dimension.ValueString,
                        wall_1 = wallId1,
                        wall_2 = wallId2
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateDimensionBetweenWalls error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create dimension between grid lines
        /// </summary>
        private object CreateDimensionBetweenGrids(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("grid_ids"))
                {
                    return new { success = false, error = "Missing required parameter: grid_ids (array of grid element IDs)" };
                }

                var gridIdsList = parameters["grid_ids"] as List<object>;
                if (gridIdsList == null || gridIdsList.Count < 2)
                {
                    return new { success = false, error = "Need at least 2 grid IDs" };
                }

                // Offset for dimension line position
                double offset = parameters.ContainsKey("offset") ? Convert.ToDouble(parameters["offset"]) : 5;

                int? dimTypeId = parameters.ContainsKey("dimension_type_id") ? 
                    Convert.ToInt32(parameters["dimension_type_id"]) : (int?)null;

                View view = doc.ActiveView;
                if (view == null)
                {
                    return new { success = false, error = "No active view" };
                }

                using (Transaction trans = new Transaction(doc, "Create Dimension Between Grids"))
                {
                    trans.Start();

                    ReferenceArray refArray = new ReferenceArray();
                    List<XYZ> gridPoints = new List<XYZ>();

                    foreach (var gridIdObj in gridIdsList)
                    {
                        int gridId = Convert.ToInt32(gridIdObj);
                        Element elem = doc.GetElement(new ElementId(gridId));

                        if (!(elem is Grid grid))
                        {
                            trans.RollBack();
                            return new { success = false, error = $"Element {gridId} is not a grid" };
                        }

                        // Get grid curve and its reference
                        Curve curve = grid.Curve;
                        Reference gridRef = new Reference(grid);

                        refArray.Append(gridRef);
                        gridPoints.Add(curve.GetEndPoint(0));
                    }

                    if (refArray.Size < 2)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not get references from grids" };
                    }

                    // Calculate dimension line - offset from grids
                    XYZ firstPoint = gridPoints[0];
                    XYZ lastPoint = gridPoints[gridPoints.Count - 1];

                    // Dimension line perpendicular to grid direction
                    XYZ dimStart = new XYZ(firstPoint.X, firstPoint.Y + offset, firstPoint.Z);
                    XYZ dimEnd = new XYZ(lastPoint.X, lastPoint.Y + offset, lastPoint.Z);

                    Line dimensionLine = Line.CreateBound(dimStart, dimEnd);

                    DimensionType dimType = null;
                    if (dimTypeId.HasValue)
                    {
                        Element typeElem = doc.GetElement(new ElementId(dimTypeId.Value));
                        if (typeElem is DimensionType dt)
                        {
                            dimType = dt;
                        }
                    }

                    Dimension dimension;
                    if (dimType != null)
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray, dimType);
                    }
                    else
                    {
                        dimension = doc.Create.NewDimension(view, dimensionLine, refArray);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimension.Id),
                        value_string = dimension.ValueString,
                        segments = dimension.NumberOfSegments,
                        grid_count = gridIdsList.Count
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateDimensionBetweenGrids error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Modify an existing dimension (text override, leader, etc.)
        /// </summary>
        private object ModifyDimension(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("dimension_id"))
                {
                    return new { success = false, error = "Missing required parameter: dimension_id" };
                }

                int dimId = Convert.ToInt32(parameters["dimension_id"]);
                Element elem = doc.GetElement(new ElementId(dimId));

                if (!(elem is Dimension dimension))
                {
                    return new { success = false, error = $"Element {dimId} is not a dimension" };
                }

                using (Transaction trans = new Transaction(doc, "Modify Dimension"))
                {
                    trans.Start();

                    // Value override (above/below)
                    if (parameters.ContainsKey("value_override"))
                    {
                        string over = parameters["value_override"].ToString();
                        dimension.ValueOverride = over;
                    }

                    // Above text
                    if (parameters.ContainsKey("above"))
                    {
                        dimension.Above = parameters["above"].ToString();
                    }

                    // Below text
                    if (parameters.ContainsKey("below"))
                    {
                        dimension.Below = parameters["below"].ToString();
                    }

                    // Prefix
                    if (parameters.ContainsKey("prefix"))
                    {
                        dimension.Prefix = parameters["prefix"].ToString();
                    }

                    // Suffix
                    if (parameters.ContainsKey("suffix"))
                    {
                        dimension.Suffix = parameters["suffix"].ToString();
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = dimId,
                        value_string = dimension.ValueString,
                        value_override = dimension.ValueOverride,
                        above = dimension.Above,
                        below = dimension.Below,
                        prefix = dimension.Prefix,
                        suffix = dimension.Suffix
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ModifyDimension error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get available dimension types
        /// </summary>
        private object GetDimensionTypes(Document doc)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var dimTypes = collector.OfClass(typeof(DimensionType)).Cast<DimensionType>().ToList();

                var typesList = dimTypes.Select(dt => new
                {
                    id = GetElementIdInt(dt.Id),
                    name = dt.Name,
                    family_name = dt.FamilyName,
                    style_type = dt.StyleType.ToString()
                }).ToList();

                return new
                {
                    success = true,
                    count = typesList.Count,
                    dimension_types = typesList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetDimensionTypes error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get existing dimensions in the document
        /// </summary>
        private object GetDimensions(Document doc)
        {
            try
            {
                View view = doc.ActiveView;
                FilteredElementCollector collector;

                if (view != null && !view.IsTemplate)
                {
                    collector = new FilteredElementCollector(doc, view.Id);
                }
                else
                {
                    collector = new FilteredElementCollector(doc);
                }

                var dimensions = collector.OfClass(typeof(Dimension)).Cast<Dimension>().ToList();

                var dimList = dimensions.Select(d => new
                {
                    id = GetElementIdInt(d.Id),
                    value_string = d.ValueString,
                    value = d.Value.HasValue ? d.Value.Value : 0,
                    segments = d.NumberOfSegments,
                    dimension_type = d.DimensionType?.Name,
                    value_override = d.ValueOverride,
                    above = d.Above,
                    below = d.Below,
                    prefix = d.Prefix,
                    suffix = d.Suffix
                }).ToList();

                return new
                {
                    success = true,
                    count = dimList.Count,
                    dimensions = dimList,
                    scope = view != null ? "active_view" : "document"
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetDimensions error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create radial dimension on an arc or circle
        /// </summary>
        private object CreateRadialDimension(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get the arc element ID
                if (!parameters.ContainsKey("arc_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: arc_element_id (element ID of arc or circle)" };
                }

                int arcElementId = Convert.ToInt32(parameters["arc_element_id"]);
                Element arcElement = doc.GetElement(new ElementId(arcElementId));

                if (arcElement == null)
                {
                    return new { success = false, error = $"Element not found: {arcElementId}" };
                }

                // Dimension type: radius or diameter
                string dimStyle = parameters.ContainsKey("dimension_style") ? 
                    parameters["dimension_style"].ToString().ToLower() : "radius";

                // Optional dimension type ID
                int? dimTypeId = parameters.ContainsKey("dimension_type_id") ? 
                    Convert.ToInt32(parameters["dimension_type_id"]) : (int?)null;

                // Optional location for dimension text
                double? locX = parameters.ContainsKey("location_x") ? Convert.ToDouble(parameters["location_x"]) : (double?)null;
                double? locY = parameters.ContainsKey("location_y") ? Convert.ToDouble(parameters["location_y"]) : (double?)null;
                double? locZ = parameters.ContainsKey("location_z") ? Convert.ToDouble(parameters["location_z"]) : (double?)null;

                View view = doc.ActiveView;
                if (view == null)
                {
                    return new { success = false, error = "No active view" };
                }

                // Get the arc geometry from the element
                Arc arc = null;
                Reference arcReference = null;

                if (arcElement is ModelCurve modelCurve)
                {
                    if (modelCurve.GeometryCurve is Arc a)
                    {
                        arc = a;
                        arcReference = modelCurve.GeometryCurve.Reference;
                    }
                }
                else if (arcElement is DetailCurve detailCurve)
                {
                    if (detailCurve.GeometryCurve is Arc a)
                    {
                        arc = a;
                        arcReference = detailCurve.GeometryCurve.Reference;
                    }
                }
                else if (arcElement is CurveElement curveElement)
                {
                    if (curveElement.GeometryCurve is Arc a)
                    {
                        arc = a;
                        arcReference = curveElement.GeometryCurve.Reference;
                    }
                }
                else
                {
                    // Try to get arc from element geometry
                    Options options = new Options();
                    options.ComputeReferences = true;
                    GeometryElement geomElem = arcElement.get_Geometry(options);

                    if (geomElem != null)
                    {
                        foreach (GeometryObject geomObj in geomElem)
                        {
                            if (geomObj is Arc a)
                            {
                                arc = a;
                                arcReference = a.Reference;
                                break;
                            }
                            else if (geomObj is Solid solid)
                            {
                                foreach (Edge edge in solid.Edges)
                                {
                                    if (edge.AsCurve() is Arc edgeArc)
                                    {
                                        arc = edgeArc;
                                        arcReference = edge.Reference;
                                        break;
                                    }
                                }
                            }
                            if (arc != null) break;
                        }
                    }
                }

                if (arc == null)
                {
                    return new { success = false, error = "Element does not contain an arc or circle geometry" };
                }

                if (arcReference == null)
                {
                    return new { success = false, error = "Could not get reference from arc element. Make sure the element supports dimensioning." };
                }

                using (Transaction trans = new Transaction(doc, "Create Radial Dimension"))
                {
                    trans.Start();

                    // Calculate location for dimension text if not provided
                    XYZ location;
                    if (locX.HasValue && locY.HasValue)
                    {
                        location = new XYZ(locX.Value, locY.Value, locZ ?? 0);
                    }
                    else
                    {
                        // Default: place at arc center offset
                        XYZ center = arc.Center;
                        double radius = arc.Radius;
                        // Offset dimension text outside the arc
                        XYZ midPoint = arc.Evaluate(0.5, true);
                        XYZ direction = (midPoint - center).Normalize();
                        location = center + direction * (radius * 1.5);
                    }

                    // Get dimension type
                    DimensionType dimType = null;
                    if (dimTypeId.HasValue)
                    {
                        Element typeElem = doc.GetElement(new ElementId(dimTypeId.Value));
                        if (typeElem is DimensionType dt)
                        {
                            dimType = dt;
                        }
                    }

                    // Find appropriate radial dimension type if not specified
                    if (dimType == null)
                    {
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        var dimTypes = collector.OfClass(typeof(DimensionType)).Cast<DimensionType>();

                        foreach (DimensionType dt in dimTypes)
                        {
                            string styleName = dt.StyleType.ToString().ToLower();
                            if (dimStyle == "diameter" && styleName.Contains("diameter"))
                            {
                                dimType = dt;
                                break;
                            }
                            else if (dimStyle == "radius" && styleName.Contains("radial"))
                            {
                                dimType = dt;
                                break;
                            }
                        }
                    }

                    Dimension dimension = null;

                    // NewRadialDimension is only available in family documents via FamilyItemFactory
                    if (doc.IsFamilyDocument)
                    {
                        if (dimType != null)
                        {
                            dimension = doc.FamilyCreate.NewRadialDimension(view, arcReference, location, dimType);
                        }
                        else
                        {
                            dimension = doc.FamilyCreate.NewRadialDimension(view, arcReference, location);
                        }
                    }
                    else
                    {
                        // For project documents, create dimension using arc reference with ReferenceArray
                        // RadialDimension in projects requires using NewDimension with appropriate references
                        ReferenceArray refArray = new ReferenceArray();
                        refArray.Append(arcReference);

                        // Create a line for dimension placement
                        XYZ center = arc.Center;
                        Line dimLine = Line.CreateBound(center, location);

                        if (dimType != null)
                        {
                            dimension = doc.Create.NewDimension(view, dimLine, refArray, dimType);
                        }
                        else
                        {
                            dimension = doc.Create.NewDimension(view, dimLine, refArray);
                        }
                    }

                    if (dimension == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Failed to create radial dimension. The arc may not support radial dimensioning in this view type." };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimension.Id),
                        value_string = dimension.ValueString,
                        value = dimension.Value.HasValue ? dimension.Value.Value : 0,
                        radius = arc.Radius,
                        center = new { x = arc.Center.X, y = arc.Center.Y, z = arc.Center.Z },
                        dimension_style = dimStyle,
                        message = $"Created {dimStyle} dimension: {dimension.ValueString}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateRadialDimension error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Helper: Get a reference from an element for dimensioning
        /// </summary>
        private Reference GetReferenceFromElement(Document doc, Element elem)
        {
            try
            {
                // For reference planes
                if (elem is ReferencePlane rp)
                {
                    return rp.GetReference();
                }

                // For grids
                if (elem is Grid grid)
                {
                    return new Reference(grid);
                }

                // For levels
                if (elem is Level level)
                {
                    return new Reference(level);
                }

                // For model curves
                if (elem is ModelCurve mc)
                {
                    return mc.GeometryCurve.Reference;
                }

                // For detail curves
                if (elem is DetailCurve dc)
                {
                    return dc.GeometryCurve.Reference;
                }

                // For walls - get center line reference
                if (elem is Wall wall)
                {
                    return GetWallFaceReference(doc, wall, "center");
                }

                // For generic elements - try to get geometry reference
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;
                GeometryElement geomElem = elem.get_Geometry(options);

                if (geomElem != null)
                {
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        if (geomObj is Solid solid)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                if (face.Reference != null)
                                {
                                    return face.Reference;
                                }
                            }
                            foreach (Edge edge in solid.Edges)
                            {
                                if (edge.Reference != null)
                                {
                                    return edge.Reference;
                                }
                            }
                        }
                        else if (geomObj is Curve curve)
                        {
                            if (curve.Reference != null)
                            {
                                return curve.Reference;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Helper: Get wall face reference for dimensioning
        /// </summary>
        private Reference GetWallFaceReference(Document doc, Wall wall, string faceType)
        {
            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;

                GeometryElement geomElem = wall.get_Geometry(options);
                if (geomElem == null) return null;

                LocationCurve wallLoc = wall.Location as LocationCurve;
                if (wallLoc == null) return null;

                XYZ wallDirection = (wallLoc.Curve.GetEndPoint(1) - wallLoc.Curve.GetEndPoint(0)).Normalize();
                XYZ wallNormal = new XYZ(-wallDirection.Y, wallDirection.X, 0);

                PlanarFace exteriorFace = null;
                PlanarFace interiorFace = null;
                PlanarFace centerFace = null;

                foreach (GeometryObject geomObj in geomElem)
                {
                    if (geomObj is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace pf)
                            {
                                XYZ faceNormal = pf.FaceNormal;
                                double dot = faceNormal.DotProduct(wallNormal);

                                if (Math.Abs(dot) > 0.9)
                                {
                                    if (dot > 0)
                                    {
                                        exteriorFace = pf;
                                    }
                                    else
                                    {
                                        interiorFace = pf;
                                    }
                                }
                            }
                        }
                    }
                }

                switch (faceType.ToLower())
                {
                    case "exterior":
                    case "external":
                        return exteriorFace?.Reference;
                    case "interior":
                    case "internal":
                        return interiorFace?.Reference;
                    case "center":
                    default:
                        // Return exterior face as default
                        return exteriorFace?.Reference ?? interiorFace?.Reference;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Family Instance Tool

        /// <summary>
        /// Create family instances using various NewFamilyInstance overloads
        /// </summary>
        private object FamilyInstanceTool(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string operation = parameters.ContainsKey("operation") ? parameters["operation"].ToString() : "get_family_symbols";

                switch (operation.ToLower())
                {
                    case "place_at_point":
                        return PlaceFamilyInstanceAtPoint(doc, parameters);

                    case "place_at_point_in_view":
                        return PlaceFamilyInstanceAtPointInView(doc, parameters);

                    case "place_on_host":
                        return PlaceFamilyInstanceOnHost(doc, parameters);

                    case "place_on_host_with_direction":
                        return PlaceFamilyInstanceOnHostWithDirection(doc, parameters);

                    case "place_along_line":
                        return PlaceFamilyInstanceAlongLine(doc, parameters);

                    case "place_along_line_in_view":
                        return PlaceFamilyInstanceAlongLineInView(doc, parameters);

                    case "place_on_face":
                        return PlaceFamilyInstanceOnFace(doc, parameters);

                    case "place_on_face_at_point":
                        return PlaceFamilyInstanceOnFaceAtPoint(doc, parameters);

                    case "place_on_reference":
                        return PlaceFamilyInstanceOnReference(doc, parameters);

                    case "get_family_symbols":
                        return GetFamilySymbols(doc, parameters);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown operation: {operation}",
                            available_operations = new[]
                            {
                                "place_at_point",
                                "place_at_point_in_view",
                                "place_on_host",
                                "place_on_host_with_direction",
                                "place_along_line",
                                "place_along_line_in_view",
                                "place_on_face",
                                "place_on_face_at_point",
                                "place_on_reference",
                                "get_family_symbols"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"FamilyInstanceTool error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// Place family instance at XYZ point with structural type - NewFamilyInstance(XYZ, FamilySymbol, StructuralType)
        /// </summary>
        private object PlaceFamilyInstanceAtPoint(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                string structuralTypeStr = parameters.ContainsKey("structural_type") ?
                    parameters["structural_type"].ToString() : "NonStructural";

                StructuralType structuralType = GetStructuralType(structuralTypeStr);

                XYZ location = new XYZ(x, y, z);

                using (Transaction trans = new Transaction(doc, "Place Family Instance"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, structuralType);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        location = new { x = x, y = y, z = z },
                        structural_type = structuralType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceAtPoint error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance at XYZ point in specific view - NewFamilyInstance(XYZ, FamilySymbol, View)
        /// </summary>
        private object PlaceFamilyInstanceAtPointInView(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                View view = null;
                if (parameters.ContainsKey("view_id"))
                {
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v)
                    {
                        view = v;
                    }
                }
                else
                {
                    view = doc.ActiveView;
                }

                if (view == null)
                {
                    return new { success = false, error = "No valid view specified or active" };
                }

                XYZ location = new XYZ(x, y, z);

                using (Transaction trans = new Transaction(doc, "Place Family Instance in View"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, view);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        location = new { x = x, y = y, z = z },
                        view_id = GetElementIdInt(view.Id),
                        view_name = view.Name
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceAtPointInView error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance on host element - NewFamilyInstance(XYZ, FamilySymbol, Element, StructuralType)
        /// </summary>
        private object PlaceFamilyInstanceOnHost(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("host_id"))
                {
                    return new { success = false, error = "Missing required parameter: host_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int hostId = Convert.ToInt32(parameters["host_id"]);
                Element hostElement = doc.GetElement(new ElementId(hostId));

                if (hostElement == null)
                {
                    return new { success = false, error = $"Host element {hostId} not found" };
                }

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                string structuralTypeStr = parameters.ContainsKey("structural_type") ?
                    parameters["structural_type"].ToString() : "NonStructural";

                StructuralType structuralType = GetStructuralType(structuralTypeStr);

                XYZ location = new XYZ(x, y, z);

                using (Transaction trans = new Transaction(doc, "Place Family Instance on Host"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, hostElement, structuralType);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        host_id = hostId,
                        location = new { x = x, y = y, z = z },
                        structural_type = structuralType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceOnHost error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance on host with direction - NewFamilyInstance(XYZ, FamilySymbol, XYZ, Element, StructuralType)
        /// </summary>
        private object PlaceFamilyInstanceOnHostWithDirection(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("host_id"))
                {
                    return new { success = false, error = "Missing required parameter: host_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int hostId = Convert.ToInt32(parameters["host_id"]);
                Element hostElement = doc.GetElement(new ElementId(hostId));

                if (hostElement == null)
                {
                    return new { success = false, error = $"Host element {hostId} not found" };
                }

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 0;
                double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
                double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 1;

                string structuralTypeStr = parameters.ContainsKey("structural_type") ?
                    parameters["structural_type"].ToString() : "NonStructural";

                StructuralType structuralType = GetStructuralType(structuralTypeStr);

                XYZ location = new XYZ(x, y, z);
                XYZ direction = new XYZ(dirX, dirY, dirZ);

                using (Transaction trans = new Transaction(doc, "Place Family Instance on Host with Direction"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, direction, hostElement, structuralType);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        host_id = hostId,
                        location = new { x = x, y = y, z = z },
                        direction = new { x = dirX, y = dirY, z = dirZ },
                        structural_type = structuralType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceOnHostWithDirection error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance along a line on reference - NewFamilyInstance(Reference, Line, FamilySymbol)
        /// </summary>
        private object PlaceFamilyInstanceAlongLine(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("reference_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: reference_element_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int refElemId = Convert.ToInt32(parameters["reference_element_id"]);
                Element refElement = doc.GetElement(new ElementId(refElemId));

                if (refElement == null)
                {
                    return new { success = false, error = $"Reference element {refElemId} not found" };
                }

                // Get line parameters
                double startX = parameters.ContainsKey("line_start_x") ? Convert.ToDouble(parameters["line_start_x"]) : 0;
                double startY = parameters.ContainsKey("line_start_y") ? Convert.ToDouble(parameters["line_start_y"]) : 0;
                double startZ = parameters.ContainsKey("line_start_z") ? Convert.ToDouble(parameters["line_start_z"]) : 0;
                double endX = parameters.ContainsKey("line_end_x") ? Convert.ToDouble(parameters["line_end_x"]) : 10;
                double endY = parameters.ContainsKey("line_end_y") ? Convert.ToDouble(parameters["line_end_y"]) : 0;
                double endZ = parameters.ContainsKey("line_end_z") ? Convert.ToDouble(parameters["line_end_z"]) : 0;

                XYZ startPoint = new XYZ(startX, startY, startZ);
                XYZ endPoint = new XYZ(endX, endY, endZ);
                Line line = Line.CreateBound(startPoint, endPoint);

                // Get reference from element
                Reference reference = GetReferenceFromElement(doc, refElement);
                if (reference == null)
                {
                    return new { success = false, error = "Could not get reference from element" };
                }

                using (Transaction trans = new Transaction(doc, "Place Family Instance Along Line"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(reference, line, familySymbol);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        reference_element_id = refElemId,
                        line_start = new { x = startX, y = startY, z = startZ },
                        line_end = new { x = endX, y = endY, z = endZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceAlongLine error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance along a line in view - NewFamilyInstance(Line, FamilySymbol, View)
        /// </summary>
        private object PlaceFamilyInstanceAlongLineInView(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                // Get line parameters
                double startX = parameters.ContainsKey("line_start_x") ? Convert.ToDouble(parameters["line_start_x"]) : 0;
                double startY = parameters.ContainsKey("line_start_y") ? Convert.ToDouble(parameters["line_start_y"]) : 0;
                double startZ = parameters.ContainsKey("line_start_z") ? Convert.ToDouble(parameters["line_start_z"]) : 0;
                double endX = parameters.ContainsKey("line_end_x") ? Convert.ToDouble(parameters["line_end_x"]) : 10;
                double endY = parameters.ContainsKey("line_end_y") ? Convert.ToDouble(parameters["line_end_y"]) : 0;
                double endZ = parameters.ContainsKey("line_end_z") ? Convert.ToDouble(parameters["line_end_z"]) : 0;

                XYZ startPoint = new XYZ(startX, startY, startZ);
                XYZ endPoint = new XYZ(endX, endY, endZ);
                Line line = Line.CreateBound(startPoint, endPoint);

                View view = null;
                if (parameters.ContainsKey("view_id"))
                {
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v)
                    {
                        view = v;
                    }
                }
                else
                {
                    view = doc.ActiveView;
                }

                if (view == null)
                {
                    return new { success = false, error = "No valid view specified or active" };
                }

                using (Transaction trans = new Transaction(doc, "Place Family Instance Along Line in View"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(line, familySymbol, view);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        line_start = new { x = startX, y = startY, z = startZ },
                        line_end = new { x = endX, y = endY, z = endZ },
                        view_id = GetElementIdInt(view.Id),
                        view_name = view.Name
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceAlongLineInView error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance on a face with line - NewFamilyInstance(Face, Line, FamilySymbol)
        /// </summary>
        private object PlaceFamilyInstanceOnFace(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("host_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: host_element_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int hostElemId = Convert.ToInt32(parameters["host_element_id"]);
                Element hostElement = doc.GetElement(new ElementId(hostElemId));

                if (hostElement == null)
                {
                    return new { success = false, error = $"Host element {hostElemId} not found" };
                }

                int faceIndex = parameters.ContainsKey("face_index") ? Convert.ToInt32(parameters["face_index"]) : 0;

                // Get line parameters
                double startX = parameters.ContainsKey("line_start_x") ? Convert.ToDouble(parameters["line_start_x"]) : 0;
                double startY = parameters.ContainsKey("line_start_y") ? Convert.ToDouble(parameters["line_start_y"]) : 0;
                double startZ = parameters.ContainsKey("line_start_z") ? Convert.ToDouble(parameters["line_start_z"]) : 0;
                double endX = parameters.ContainsKey("line_end_x") ? Convert.ToDouble(parameters["line_end_x"]) : 10;
                double endY = parameters.ContainsKey("line_end_y") ? Convert.ToDouble(parameters["line_end_y"]) : 0;
                double endZ = parameters.ContainsKey("line_end_z") ? Convert.ToDouble(parameters["line_end_z"]) : 0;

                XYZ startPoint = new XYZ(startX, startY, startZ);
                XYZ endPoint = new XYZ(endX, endY, endZ);
                Line line = Line.CreateBound(startPoint, endPoint);

                // Get face from element
                Face face = GetFaceFromElement(doc, hostElement, faceIndex);
                if (face == null)
                {
                    return new { success = false, error = $"Could not get face at index {faceIndex} from element" };
                }

                using (Transaction trans = new Transaction(doc, "Place Family Instance on Face"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(face, line, familySymbol);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        host_element_id = hostElemId,
                        face_index = faceIndex,
                        line_start = new { x = startX, y = startY, z = startZ },
                        line_end = new { x = endX, y = endY, z = endZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceOnFace error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance on face at point with direction - NewFamilyInstance(Face, XYZ, XYZ, FamilySymbol)
        /// </summary>
        private object PlaceFamilyInstanceOnFaceAtPoint(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("host_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: host_element_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int hostElemId = Convert.ToInt32(parameters["host_element_id"]);
                Element hostElement = doc.GetElement(new ElementId(hostElemId));

                if (hostElement == null)
                {
                    return new { success = false, error = $"Host element {hostElemId} not found" };
                }

                int faceIndex = parameters.ContainsKey("face_index") ? Convert.ToInt32(parameters["face_index"]) : 0;

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 1;
                double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
                double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 0;

                XYZ location = new XYZ(x, y, z);
                XYZ direction = new XYZ(dirX, dirY, dirZ);

                // Get face from element
                Face face = GetFaceFromElement(doc, hostElement, faceIndex);
                if (face == null)
                {
                    return new { success = false, error = $"Could not get face at index {faceIndex} from element" };
                }

                using (Transaction trans = new Transaction(doc, "Place Family Instance on Face at Point"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(face, location, direction, familySymbol);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        host_element_id = hostElemId,
                        face_index = faceIndex,
                        location = new { x = x, y = y, z = z },
                        direction = new { x = dirX, y = dirY, z = dirZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceOnFaceAtPoint error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place family instance on reference at point with direction - NewFamilyInstance(Reference, XYZ, XYZ, FamilySymbol)
        /// </summary>
        private object PlaceFamilyInstanceOnReference(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_symbol_id"))
                {
                    return new { success = false, error = "Missing required parameter: family_symbol_id" };
                }

                if (!parameters.ContainsKey("reference_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: reference_element_id" };
                }

                int symbolId = Convert.ToInt32(parameters["family_symbol_id"]);
                Element symbolElem = doc.GetElement(new ElementId(symbolId));

                if (!(symbolElem is FamilySymbol familySymbol))
                {
                    return new { success = false, error = $"Element {symbolId} is not a FamilySymbol" };
                }

                int refElemId = Convert.ToInt32(parameters["reference_element_id"]);
                Element refElement = doc.GetElement(new ElementId(refElemId));

                if (refElement == null)
                {
                    return new { success = false, error = $"Reference element {refElemId} not found" };
                }

                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

                double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 1;
                double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
                double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 0;

                XYZ location = new XYZ(x, y, z);
                XYZ direction = new XYZ(dirX, dirY, dirZ);

                // Get reference from element
                Reference reference = GetReferenceFromElement(doc, refElement);
                if (reference == null)
                {
                    return new { success = false, error = "Could not get reference from element" };
                }

                using (Transaction trans = new Transaction(doc, "Place Family Instance on Reference"))
                {
                    trans.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                        doc.Regenerate();
                    }

                    FamilyInstance instance = doc.Create.NewFamilyInstance(reference, location, direction, familySymbol);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        instance_id = GetElementIdInt(instance.Id),
                        family_name = familySymbol.Family?.Name,
                        type_name = familySymbol.Name,
                        reference_element_id = refElemId,
                        location = new { x = x, y = y, z = z },
                        direction = new { x = dirX, y = dirY, z = dirZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstanceOnReference error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get available family symbols for placement
        /// </summary>
        private object GetFamilySymbols(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string familyName = parameters.ContainsKey("family_name") ? parameters["family_name"].ToString() : null;
                string categoryName = parameters.ContainsKey("category") ? parameters["category"].ToString() : null;

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var symbols = collector.OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>();

                if (!string.IsNullOrEmpty(familyName))
                {
                    symbols = symbols.Where(s => s.Family != null && 
                        s.Family.Name.IndexOf(familyName, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrEmpty(categoryName))
                {
                    var builtInCat = GetBuiltInCategory(categoryName);
                    if (builtInCat.HasValue)
                    {
                        symbols = symbols.Where(s => s.Category != null && 
                            s.Category.BuiltInCategory == builtInCat.Value);
                    }
                }

                var symbolList = symbols.Select(s => new
                {
                    id = GetElementIdInt(s.Id),
                    family_name = s.Family?.Name,
                    type_name = s.Name,
                    category = s.Category?.Name,
                    is_active = s.IsActive
                }).Take(100).ToList();

                return new
                {
                    success = true,
                    count = symbolList.Count,
                    family_symbols = symbolList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetFamilySymbols error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Helper: Get StructuralType from string
        /// </summary>
        private StructuralType GetStructuralType(string typeStr)
        {
            switch (typeStr.ToLower())
            {
                case "beam":
                    return StructuralType.Beam;
                case "brace":
                    return StructuralType.Brace;
                case "column":
                    return StructuralType.Column;
                case "footing":
                    return StructuralType.Footing;
                case "nonstructural":
                default:
                    return StructuralType.NonStructural;
            }
        }

        #endregion

        #region Family Modeling Tool
        private object LoadAndPlaceFamily(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string operation = parameters.ContainsKey("operation") ? parameters["operation"].ToString() : "list_families";

                switch (operation.ToLower())
                {
                    case "list_families":
                        return ListFamiliesInProject(doc, parameters);

                    case "list_family_types":
                        return ListFamilyTypes(doc, parameters);

                    case "load_family":
                        return LoadFamilyFromFile(doc, parameters);

                    case "place_family":
                        return PlaceFamilyInstance(doc, parameters);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown operation: {operation}",
                            available_operations = new[]
                            {
                                "list_families",
                                "list_family_types",
                                "load_family",
                                "place_family"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"LoadAndPlaceFamily error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// List all families in the project, optionally filtered by category
        /// </summary>
        private object ListFamiliesInProject(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string categoryFilter = parameters.ContainsKey("category") ? parameters["category"].ToString() : null;
                bool includeSystemFamilies = parameters.ContainsKey("include_system_families") && 
                    Convert.ToBoolean(parameters["include_system_families"]);

                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var families = collector.OfClass(typeof(Family)).Cast<Family>();

                // Filter by category if specified
                if (!string.IsNullOrEmpty(categoryFilter))
                {
                    var builtInCat = GetBuiltInCategory(categoryFilter);
                    if (builtInCat.HasValue)
                    {
                        families = families.Where(f => f.FamilyCategory != null && 
                            f.FamilyCategory.BuiltInCategory == builtInCat.Value);
                    }
                }

                // Filter out system families unless requested
                if (!includeSystemFamilies)
                {
                    families = families.Where(f => f.IsEditable);
                }

                var familyList = families.Select(f =>
                {
                    var symbols = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .Where(s => s.Family.Id == f.Id)
                        .ToList();

                    return new
                    {
                        family_id = GetElementIdInt(f.Id),
                        family_name = f.Name,
                        category = f.FamilyCategory?.Name,
                        is_editable = f.IsEditable,
                        is_in_place = f.IsInPlace,
                        type_count = symbols.Count,
                        types = symbols.Select(s => new
                        {
                            type_id = GetElementIdInt(s.Id),
                            type_name = s.Name,
                            is_active = s.IsActive
                        }).ToList()
                    };
                }).ToList();

                return new
                {
                    success = true,
                    count = familyList.Count,
                    category_filter = categoryFilter,
                    families = familyList
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ListFamiliesInProject error: {ex.Message}" };
            }
        }

        /// <summary>
        /// List all types within a specific family
        /// </summary>
        private object ListFamilyTypes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("family_name") && !parameters.ContainsKey("family_id"))
                {
                    return new { success = false, error = "Either family_name or family_id is required" };
                }

                Family family = null;

                // Find by ID
                if (parameters.ContainsKey("family_id"))
                {
                    int familyId = Convert.ToInt32(parameters["family_id"]);
                    Element elem = doc.GetElement(new ElementId(familyId));
                    family = elem as Family;
                }
                // Find by name
                else if (parameters.ContainsKey("family_name"))
                {
                    string familyName = parameters["family_name"].ToString();
                    family = new FilteredElementCollector(doc)
                        .OfClass(typeof(Family))
                        .Cast<Family>()
                        .FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
                }

                if (family == null)
                {
                    return new { success = false, error = "Family not found" };
                }

                var symbols = new FilteredElementCollector(doc)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .Where(s => s.Family.Id == family.Id)
                    .Select(s => new
                    {
                        type_id = GetElementIdInt(s.Id),
                        type_name = s.Name,
                        is_active = s.IsActive,
                        category = s.Category?.Name,
                        family_name = s.Family?.Name
                    })
                    .ToList();

                return new
                {
                    success = true,
                    family_id = GetElementIdInt(family.Id),
                    family_name = family.Name,
                    category = family.FamilyCategory?.Name,
                    type_count = symbols.Count,
                    types = symbols
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ListFamilyTypes error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Load a family from file path
        /// </summary>
        private object LoadFamilyFromFile(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("file_path"))
                {
                    return new { success = false, error = "file_path is required" };
                }

                string filePath = parameters["file_path"].ToString();

                if (!File.Exists(filePath))
                {
                    return new { success = false, error = $"File not found: {filePath}" };
                }

                if (!filePath.EndsWith(".rfa", StringComparison.OrdinalIgnoreCase))
                {
                    return new { success = false, error = "File must be a Revit family file (.rfa)" };
                }

                using (Transaction trans = new Transaction(doc, "Load Family"))
                {
                    trans.Start();

                    Family loadedFamily = null;
                    bool loaded = doc.LoadFamily(filePath, out loadedFamily);

                    if (!loaded || loadedFamily == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Failed to load family" };
                    }

                    trans.Commit();

                    // Get all symbols from the loaded family
                    var symbols = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .Where(s => s.Family.Id == loadedFamily.Id)
                        .Select(s => new
                        {
                            type_id = GetElementIdInt(s.Id),
                            type_name = s.Name,
                            is_active = s.IsActive
                        })
                        .ToList();

                    return new
                    {
                        success = true,
                        message = "Family loaded successfully",
                        family_id = GetElementIdInt(loadedFamily.Id),
                        family_name = loadedFamily.Name,
                        category = loadedFamily.FamilyCategory?.Name,
                        file_path = filePath,
                        type_count = symbols.Count,
                        types = symbols
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"LoadFamilyFromFile error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Place a family instance with various placement methods
        /// </summary>
        private object PlaceFamilyInstance(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!parameters.ContainsKey("type_id") && !parameters.ContainsKey("type_name"))
                {
                    return new { success = false, error = "Either type_id or type_name is required" };
                }

                FamilySymbol familySymbol = null;

                // Find by ID
                if (parameters.ContainsKey("type_id"))
                {
                    int typeId = Convert.ToInt32(parameters["type_id"]);
                    Element elem = doc.GetElement(new ElementId(typeId));
                    familySymbol = elem as FamilySymbol;
                }
                // Find by name
                else if (parameters.ContainsKey("type_name"))
                {
                    string typeName = parameters["type_name"].ToString();
                    string familyName = parameters.ContainsKey("family_name") ? parameters["family_name"].ToString() : null;

                    var query = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .Where(s => s.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

                    if (!string.IsNullOrEmpty(familyName))
                    {
                        query = query.Where(s => s.Family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase));
                    }

                    familySymbol = query.FirstOrDefault();
                }

                if (familySymbol == null)
                {
                    return new { success = false, error = "Family type not found" };
                }

                // Determine placement method
                string placementMethod = parameters.ContainsKey("placement_method") ? 
                    parameters["placement_method"].ToString().ToLower() : "point";

                switch (placementMethod)
                {
                    case "point":
                        return PlaceFamilyAtPoint(doc, familySymbol, parameters);

                    case "point_in_view":
                        return PlaceFamilyAtPointInView(doc, familySymbol, parameters);

                    case "host":
                    case "on_host":
                        return PlaceFamilyOnHost(doc, familySymbol, parameters);

                    case "face":
                    case "on_face":
                        return PlaceFamilyOnFace(doc, familySymbol, parameters);

                    case "line":
                    case "along_line":
                        return PlaceFamilyAlongLine(doc, familySymbol, parameters);

                    default:
                        return new { success = false, error = $"Unknown placement_method: {placementMethod}. Use: point, point_in_view, host, face, or line" };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"PlaceFamilyInstance error: {ex.Message}" };
            }
        }

        private object PlaceFamilyAtPoint(Document doc, FamilySymbol familySymbol, Dictionary<string, object> parameters)
        {
            double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

            string structuralTypeStr = parameters.ContainsKey("structural_type") ?
                parameters["structural_type"].ToString() : "NonStructural";
            StructuralType structuralType = GetStructuralType(structuralTypeStr);

            XYZ location = new XYZ(x, y, z);

            using (Transaction trans = new Transaction(doc, "Place Family at Point"))
            {
                trans.Start();

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, structuralType);

                trans.Commit();

                return new
                {
                    success = true,
                    instance_id = GetElementIdInt(instance.Id),
                    family_name = familySymbol.Family?.Name,
                    type_name = familySymbol.Name,
                    location = new { x, y, z },
                    placement_method = "point"
                };
            }
        }

        private object PlaceFamilyAtPointInView(Document doc, FamilySymbol familySymbol, Dictionary<string, object> parameters)
        {
            double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

            View view = null;
            if (parameters.ContainsKey("view_id"))
            {
                int viewId = Convert.ToInt32(parameters["view_id"]);
                view = doc.GetElement(new ElementId(viewId)) as View;
            }
            else
            {
                view = doc.ActiveView;
            }

            if (view == null)
            {
                return new { success = false, error = "No valid view specified or active" };
            }

            XYZ location = new XYZ(x, y, z);

            using (Transaction trans = new Transaction(doc, "Place Family in View"))
            {
                trans.Start();

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, view);

                trans.Commit();

                return new
                {
                    success = true,
                    instance_id = GetElementIdInt(instance.Id),
                    family_name = familySymbol.Family?.Name,
                    type_name = familySymbol.Name,
                    location = new { x, y, z },
                    view_id = GetElementIdInt(view.Id),
                    placement_method = "point_in_view"
                };
            }
        }

        private object PlaceFamilyOnHost(Document doc, FamilySymbol familySymbol, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("host_id"))
            {
                return new { success = false, error = "host_id is required for host placement" };
            }

            int hostId = Convert.ToInt32(parameters["host_id"]);
            Element hostElement = doc.GetElement(new ElementId(hostId));

            if (hostElement == null)
            {
                return new { success = false, error = $"Host element {hostId} not found" };
            }

            double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;

            string structuralTypeStr = parameters.ContainsKey("structural_type") ?
                parameters["structural_type"].ToString() : "NonStructural";
            StructuralType structuralType = GetStructuralType(structuralTypeStr);

            XYZ location = new XYZ(x, y, z);

            using (Transaction trans = new Transaction(doc, "Place Family on Host"))
            {
                trans.Start();

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                FamilyInstance instance = doc.Create.NewFamilyInstance(location, familySymbol, hostElement, structuralType);

                trans.Commit();

                return new
                {
                    success = true,
                    instance_id = GetElementIdInt(instance.Id),
                    family_name = familySymbol.Family?.Name,
                    type_name = familySymbol.Name,
                    host_id = hostId,
                    location = new { x, y, z },
                    placement_method = "host"
                };
            }
        }

        private object PlaceFamilyOnFace(Document doc, FamilySymbol familySymbol, Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey("face_element_id"))
            {
                return new { success = false, error = "face_element_id is required for face placement" };
            }

            int faceElemId = Convert.ToInt32(parameters["face_element_id"]);
            Element faceElement = doc.GetElement(new ElementId(faceElemId));

            if (faceElement == null)
            {
                return new { success = false, error = $"Face element {faceElemId} not found" };
            }

            int faceIndex = parameters.ContainsKey("face_index") ? Convert.ToInt32(parameters["face_index"]) : 0;
            Face face = GetFaceFromElement(doc, faceElement, faceIndex);

            if (face == null)
            {
                return new { success = false, error = $"Could not get face {faceIndex} from element" };
            }

            double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
            double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
            double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;
            XYZ location = new XYZ(x, y, z);

            using (Transaction trans = new Transaction(doc, "Place Family on Face"))
            {
                trans.Start();

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                FamilyInstance instance = doc.Create.NewFamilyInstance(face, location, XYZ.BasisX, familySymbol);

                trans.Commit();

                return new
                {
                    success = true,
                    instance_id = GetElementIdInt(instance.Id),
                    family_name = familySymbol.Family?.Name,
                    type_name = familySymbol.Name,
                    face_element_id = faceElemId,
                    location = new { x, y, z },
                    placement_method = "face"
                };
            }
        }

        private object PlaceFamilyAlongLine(Document doc, FamilySymbol familySymbol, Dictionary<string, object> parameters)
        {
            double startX = parameters.ContainsKey("line_start_x") ? Convert.ToDouble(parameters["line_start_x"]) : 0;
            double startY = parameters.ContainsKey("line_start_y") ? Convert.ToDouble(parameters["line_start_y"]) : 0;
            double startZ = parameters.ContainsKey("line_start_z") ? Convert.ToDouble(parameters["line_start_z"]) : 0;
            double endX = parameters.ContainsKey("line_end_x") ? Convert.ToDouble(parameters["line_end_x"]) : 10;
            double endY = parameters.ContainsKey("line_end_y") ? Convert.ToDouble(parameters["line_end_y"]) : 0;
            double endZ = parameters.ContainsKey("line_end_z") ? Convert.ToDouble(parameters["line_end_z"]) : 0;

            XYZ startPoint = new XYZ(startX, startY, startZ);
            XYZ endPoint = new XYZ(endX, endY, endZ);
            Line line = Line.CreateBound(startPoint, endPoint);

            View view = null;
            if (parameters.ContainsKey("view_id"))
            {
                int viewId = Convert.ToInt32(parameters["view_id"]);
                view = doc.GetElement(new ElementId(viewId)) as View;
            }
            else
            {
                view = doc.ActiveView;
            }

            if (view == null)
            {
                return new { success = false, error = "No valid view specified or active" };
            }

            using (Transaction trans = new Transaction(doc, "Place Family Along Line"))
            {
                trans.Start();

                if (!familySymbol.IsActive)
                {
                    familySymbol.Activate();
                    doc.Regenerate();
                }

                FamilyInstance instance = doc.Create.NewFamilyInstance(line, familySymbol, view);

                trans.Commit();

                return new
                {
                    success = true,
                    instance_id = GetElementIdInt(instance.Id),
                    family_name = familySymbol.Family?.Name,
                    type_name = familySymbol.Name,
                    line_start = new { x = startX, y = startY, z = startZ },
                    line_end = new { x = endX, y = endY, z = endZ },
                    placement_method = "line"
                };
            }
        }

        #endregion

        #region Family Modeling Tool

        /// <summary>
        /// Helper: Get StructuralType from string (moved from original location)
        /// </summary>
        private Face GetFaceFromElement(Document doc, Element elem, int faceIndex)
        {
            try
            {
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;

                GeometryElement geomElem = elem.get_Geometry(options);
                if (geomElem == null) return null;

                int currentIndex = 0;
                foreach (GeometryObject geomObj in geomElem)
                {
                    if (geomObj is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (currentIndex == faceIndex)
                            {
                                return face;
                            }
                            currentIndex++;
                        }
                    }
                    else if (geomObj is GeometryInstance geomInstance)
                    {
                        GeometryElement instanceGeom = geomInstance.GetInstanceGeometry();
                        foreach (GeometryObject instObj in instanceGeom)
                        {
                            if (instObj is Solid instSolid)
                            {
                                foreach (Face face in instSolid.Faces)
                                {
                                    if (currentIndex == faceIndex)
                                    {
                                        return face;
                                    }
                                    currentIndex++;
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Family Modeling Tool

        /// <summary>
        /// Create geometry in family documents using FamilyCreate methods
        /// </summary>
        private object FamilyModelingTool(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This tool only works in family documents. Open a family (.rfa) file first." };
                }

                string operation = parameters.ContainsKey("operation") ? parameters["operation"].ToString() : "get_forms";

                switch (operation.ToLower())
                {
                    case "new_extrusion":
                        return CreateFamilyExtrusion(doc, parameters);

                    case "new_blend":
                        return CreateFamilyBlend(doc, parameters);

                    case "new_revolution":
                        return CreateFamilyRevolution(doc, parameters);

                    case "new_sweep":
                        return CreateFamilySweep(doc, parameters);

                    case "new_swept_blend":
                        return CreateFamilySweptBlend(doc, parameters);

                    case "new_loft_form":
                        return CreateFamilyLoftForm(doc, parameters);

                    case "new_form_by_cap":
                        return CreateFamilyFormByCap(doc, parameters);

                    case "new_form_by_thicken":
                        return CreateFamilyFormByThicken(doc, parameters);

                    case "new_revolve_form":
                        return CreateFamilyRevolveForm(doc, parameters);

                    case "new_extrusion_form":
                        return CreateFamilyExtrusionForm(doc, parameters);

                    case "new_swept_blend_form":
                        return CreateFamilySweptBlendForm(doc, parameters);

                    case "new_model_text":
                        return CreateFamilyModelText(doc, parameters);

                    case "new_opening":
                        return CreateFamilyOpening(doc, parameters);

                    case "new_symbolic_curve":
                        return CreateFamilySymbolicCurve(doc, parameters);

                    case "new_control":
                        return CreateFamilyControl(doc, parameters);

                    case "new_diameter_dimension":
                        return CreateFamilyDiameterDimension(doc, parameters);

                    case "get_forms":
                        return GetFamilyForms(doc, parameters);

                    case "get_sketch_planes":
                        return GetFamilySketchPlanes(doc, parameters);

                    case "convert_symbolic_to_model":
                        return ConvertSymbolicToModelCurves(doc, parameters);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown operation: {operation}",
                            available_operations = new[]
                            {
                                "new_extrusion",
                                "new_blend",
                                "new_revolution",
                                "new_sweep",
                                "new_swept_blend",
                                "new_loft_form",
                                "new_form_by_cap",
                                "new_form_by_thicken",
                                "new_revolve_form",
                                "new_extrusion_form",
                                "new_swept_blend_form",
                                "new_model_text",
                                "new_opening",
                                "new_symbolic_curve",
                                "new_control",
                                "new_diameter_dimension",
                                "convert_symbolic_to_model",
                                "get_forms",
                                "get_sketch_planes"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"FamilyModelingTool error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// Create an extrusion in a family document
        /// </summary>
        private object CreateFamilyExtrusion(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;
                double extrusionEnd = parameters.ContainsKey("extrusion_end") ? Convert.ToDouble(parameters["extrusion_end"]) : 10;
                double extrusionStart = parameters.ContainsKey("extrusion_start") ? Convert.ToDouble(parameters["extrusion_start"]) : 0;

                // Get or create sketch plane
                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                CurveArrArray curveArrArray = new CurveArrArray();
                CurveArray curveArray = new CurveArray();

                // Check if using existing curve elements (profile_curve_ids) or points (profile_points)
                var profileCurveIds = GetElementIdListFromParam(parameters, "profile_curve_ids");
                
                if (profileCurveIds != null && profileCurveIds.Count > 0)
                {
                    // Using existing curves - collect all curves into a single profile
                    foreach (int curveId in profileCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Curve curve = null;
                            
                            // Extract curve from different element types
                            if (curveElem is ModelCurve modelCurve)
                            {
                                curve = modelCurve.GeometryCurve;
                            }
                            else if (curveElem is CurveByPoints curveByPoints)
                            {
                                curve = curveByPoints.GeometryCurve;
                            }
                            else if (curveElem is ModelLine modelLine)
                            {
                                curve = modelLine.GeometryCurve;
                            }
                            
                            if (curve != null)
                            {
                                curveArray.Append(curve);
                            }
                        }
                    }
                    
                    if (curveArray.Size < 3)
                    {
                        return new { success = false, error = $"At least 3 valid curves required for extrusion profile. Found {curveArray.Size} curves." };
                    }
                }
                else
                {
                    // Using profile points - create curves from points
                    var profilePoints = GetProfilePoints(parameters);
                    if (profilePoints == null || profilePoints.Count < 3)
                    {
                        return new { success = false, error = "Profile requires at least 3 points (use 'profile_points') or 3 curve element IDs (use 'profile_curve_ids')." };
                    }
                    
                    curveArray = CreateCurveArrayFromPoints(profilePoints);
                }

                curveArrArray.Append(curveArray);

                using (Transaction trans = new Transaction(doc, "Create Extrusion"))
                {
                    trans.Start();

                    // Create the extrusion (single call with all curves in the profile)
                    Extrusion extrusion = doc.FamilyCreate.NewExtrusion(isSolid, curveArrArray, sketchPlane, extrusionEnd);

                    // Set start offset if specified - try EXTRUSION_START_PARAM
                    if (extrusionStart != 0)
                    {
                        foreach (Parameter p in extrusion.Parameters)
                        {
                            if (p.Definition?.Name?.ToLower().Contains("start") == true && !p.IsReadOnly)
                            {
                                try { p.Set(extrusionStart); break; } catch { }
                            }
                        }
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = profileCurveIds != null ? 
                            $"Extrusion created from {curveArray.Size} existing curves in a single operation" :
                            $"Extrusion created from {curveArray.Size} curve segments",
                        extrusion_id = GetElementIdInt(extrusion.Id),
                        is_solid = isSolid,
                        extrusion_start = extrusionStart,
                        extrusion_end = extrusionEnd,
                        sketch_plane_id = GetElementIdInt(sketchPlane.Id),
                        profile_curve_count = curveArray.Size,
                        used_existing_curves = profileCurveIds != null && profileCurveIds.Count > 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyExtrusion error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a blend in a family document
        /// </summary>
        private object CreateFamilyBlend(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get bottom profile points
                var bottomPoints = GetProfilePointsFromParam(parameters, "bottom_profile_points");
                if (bottomPoints == null || bottomPoints.Count < 3)
                {
                    return new { success = false, error = "Bottom profile requires at least 3 points. Use 'bottom_profile_points' parameter." };
                }

                // Get top profile points
                var topPoints = GetProfilePointsFromParam(parameters, "top_profile_points");
                if (topPoints == null || topPoints.Count < 3)
                {
                    return new { success = false, error = "Top profile requires at least 3 points. Use 'top_profile_points' parameter." };
                }

                // Get sketch planes
                SketchPlane bottomPlane = GetOrCreateSketchPlane(doc, parameters, "bottom_");
                SketchPlane topPlane = GetOrCreateSketchPlane(doc, parameters, "top_");

                if (bottomPlane == null || topPlane == null)
                {
                    return new { success = false, error = "Could not create sketch planes for blend" };
                }

                using (Transaction trans = new Transaction(doc, "Create Blend"))
                {
                    trans.Start();

                    // Create bottom profile
                    CurveArray bottomCurves = CreateCurveArrayFromPoints(bottomPoints);

                    // Create top profile
                    CurveArray topCurves = CreateCurveArrayFromPoints(topPoints);

                    // Create the blend
                    Blend blend = doc.FamilyCreate.NewBlend(isSolid, topCurves, bottomCurves, bottomPlane);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        blend_id = GetElementIdInt(blend.Id),
                        is_solid = isSolid,
                        bottom_plane_id = GetElementIdInt(bottomPlane.Id),
                        top_plane_id = GetElementIdInt(topPlane.Id)
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyBlend error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a revolution in a family document
        /// </summary>
        private object CreateFamilyRevolution(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;
                double startAngle = parameters.ContainsKey("start_angle") ? Convert.ToDouble(parameters["start_angle"]) : 0;
                double endAngle = parameters.ContainsKey("end_angle") ? Convert.ToDouble(parameters["end_angle"]) : 360;

                // Convert to radians
                double startRad = startAngle * Math.PI / 180.0;
                double endRad = endAngle * Math.PI / 180.0;

                // Get sketch plane
                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                // Get profile points
                var profilePoints = GetProfilePoints(parameters);
                if (profilePoints == null || profilePoints.Count < 2)
                {
                    return new { success = false, error = "Profile requires at least 2 points. Use 'profile_points' parameter." };
                }

                // Get axis line
                double axisStartX = parameters.ContainsKey("axis_start_x") ? Convert.ToDouble(parameters["axis_start_x"]) : 0;
                double axisStartY = parameters.ContainsKey("axis_start_y") ? Convert.ToDouble(parameters["axis_start_y"]) : 0;
                double axisStartZ = parameters.ContainsKey("axis_start_z") ? Convert.ToDouble(parameters["axis_start_z"]) : 0;
                double axisEndX = parameters.ContainsKey("axis_end_x") ? Convert.ToDouble(parameters["axis_end_x"]) : 0;
                double axisEndY = parameters.ContainsKey("axis_end_y") ? Convert.ToDouble(parameters["axis_end_y"]) : 0;
                double axisEndZ = parameters.ContainsKey("axis_end_z") ? Convert.ToDouble(parameters["axis_end_z"]) : 10;

                XYZ axisStart = new XYZ(axisStartX, axisStartY, axisStartZ);
                XYZ axisEnd = new XYZ(axisEndX, axisEndY, axisEndZ);
                Line axisLine = Line.CreateBound(axisStart, axisEnd);

                using (Transaction trans = new Transaction(doc, "Create Revolution"))
                {
                    trans.Start();

                    // Create profile curves
                    CurveArrArray curveArrArray = new CurveArrArray();
                    CurveArray curveArray = CreateCurveArrayFromPoints(profilePoints, false); // Open profile for revolution
                    curveArrArray.Append(curveArray);

                    // Create the revolution
                    Revolution revolution = doc.FamilyCreate.NewRevolution(isSolid, curveArrArray, sketchPlane, axisLine, startRad, endRad);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        revolution_id = GetElementIdInt(revolution.Id),
                        is_solid = isSolid,
                        start_angle = startAngle,
                        end_angle = endAngle,
                        sketch_plane_id = GetElementIdInt(sketchPlane.Id)
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyRevolution error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a sweep in a family document
        /// </summary>
        private object CreateFamilySweep(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get path curve element IDs
                var pathCurveIds = GetElementIdListFromParam(parameters, "path_curve_ids");
                if (pathCurveIds == null || pathCurveIds.Count == 0)
                {
                    return new { success = false, error = "Path curve IDs required. Use 'path_curve_ids' parameter." };
                }

                // Get profile sketch plane
                SketchPlane profilePlane = GetOrCreateSketchPlane(doc, parameters, "profile_");
                if (profilePlane == null)
                {
                    return new { success = false, error = "Could not get or create profile sketch plane" };
                }

                // Get profile points
                var profilePoints = GetProfilePoints(parameters);
                if (profilePoints == null || profilePoints.Count < 3)
                {
                    return new { success = false, error = "Profile requires at least 3 points. Use 'profile_points' parameter." };
                }

                using (Transaction trans = new Transaction(doc, "Create Sweep"))
                {
                    trans.Start();

                    // Get path curves from reference array
                    ReferenceArray pathRefs = new ReferenceArray();
                    foreach (int curveId in pathCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                pathRefs.Append(curveRef);
                            }
                        }
                    }

                    if (pathRefs.Size == 0)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No valid path curves found" };
                    }

                    // Create profile
                    CurveArrArray profileLoops = new CurveArrArray();
                    CurveArray profileCurves = CreateCurveArrayFromPoints(profilePoints);
                    profileLoops.Append(profileCurves);

                    // Create the sweep
                    SweepProfile sweepProfile = doc.Application.Create.NewCurveLoopsProfile(profileLoops);
                    Sweep sweep = doc.FamilyCreate.NewSweep(isSolid, pathRefs, sweepProfile, 0, ProfilePlaneLocation.Start);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        sweep_id = GetElementIdInt(sweep.Id),
                        is_solid = isSolid,
                        path_curve_count = pathCurveIds.Count
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilySweep error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a swept blend in a family document
        /// </summary>
        private object CreateFamilySweptBlend(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get path curve element IDs
                var pathCurveIds = GetElementIdListFromParam(parameters, "path_curve_ids");
                if (pathCurveIds == null || pathCurveIds.Count == 0)
                {
                    return new { success = false, error = "Path curve IDs required. Use 'path_curve_ids' parameter." };
                }

                // Get bottom profile
                var bottomPoints = GetProfilePointsFromParam(parameters, "bottom_profile_points");
                if (bottomPoints == null || bottomPoints.Count < 3)
                {
                    return new { success = false, error = "Bottom profile requires at least 3 points." };
                }

                // Get top profile
                var topPoints = GetProfilePointsFromParam(parameters, "top_profile_points");
                if (topPoints == null || topPoints.Count < 3)
                {
                    return new { success = false, error = "Top profile requires at least 3 points." };
                }

                using (Transaction trans = new Transaction(doc, "Create Swept Blend"))
                {
                    trans.Start();

                    // Get path references
                    ReferenceArray pathRefs = new ReferenceArray();
                    foreach (int curveId in pathCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                pathRefs.Append(curveRef);
                            }
                        }
                    }

                    if (pathRefs.Size == 0)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No valid path curves found" };
                    }

                    // Create profiles
                    CurveArrArray bottomProfile = new CurveArrArray();
                    bottomProfile.Append(CreateCurveArrayFromPoints(bottomPoints));

                    CurveArrArray topProfile = new CurveArrArray();
                    topProfile.Append(CreateCurveArrayFromPoints(topPoints));

                    SweepProfile bottomSweepProfile = doc.Application.Create.NewCurveLoopsProfile(bottomProfile);
                    SweepProfile topSweepProfile = doc.Application.Create.NewCurveLoopsProfile(topProfile);

                    // Create the swept blend - needs single path Reference
                    Reference pathRef = null;
                    if (pathRefs.Size > 0)
                    {
                        ReferenceArrayIterator iter = pathRefs.ForwardIterator();
                        if (iter.MoveNext())
                        {
                            pathRef = iter.Current as Reference;
                        }
                    }

                    if (pathRef == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not get path reference" };
                    }

                    SweptBlend sweptBlend = doc.FamilyCreate.NewSweptBlend(isSolid, pathRef, bottomSweepProfile, topSweepProfile);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        swept_blend_id = GetElementIdInt(sweptBlend.Id),
                        is_solid = isSolid
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilySweptBlend error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a loft form in a family document (conceptual mass)
        /// </summary>
        private object CreateFamilyLoftForm(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get profile curve IDs
                var profileCurveIds = GetElementIdListFromParam(parameters, "profile_curve_ids");
                if (profileCurveIds == null || profileCurveIds.Count < 2)
                {
                    return new { success = false, error = "At least 2 profile curve IDs required. Use 'profile_curve_ids' parameter." };
                }

                using (Transaction trans = new Transaction(doc, "Create Loft Form"))
                {
                    trans.Start();

                    // Get profile references
                    ReferenceArrayArray profileRefs = new ReferenceArrayArray();
                    foreach (int curveId in profileCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            ReferenceArray refArray = new ReferenceArray();
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                refArray.Append(curveRef);
                                profileRefs.Append(refArray);
                            }
                        }
                    }

                    if (profileRefs.Size < 2)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Need at least 2 valid profile curves" };
                    }

                    // Create the loft form
                    Form loftForm = doc.FamilyCreate.NewLoftForm(isSolid, profileRefs);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        form_id = GetElementIdInt(loftForm.Id),
                        is_solid = isSolid,
                        profile_count = profileRefs.Size
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyLoftForm error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a form by cap in a family document
        /// </summary>
        private object CreateFamilyFormByCap(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get profile curve IDs
                var profileCurveIds = GetElementIdListFromParam(parameters, "profile_curve_ids");
                if (profileCurveIds == null || profileCurveIds.Count == 0)
                {
                    return new { success = false, error = "Profile curve IDs required. Use 'profile_curve_ids' parameter." };
                }

                using (Transaction trans = new Transaction(doc, "Create Form By Cap"))
                {
                    trans.Start();

                    // Get profile references
                    ReferenceArray profileRefs = new ReferenceArray();
                    foreach (int curveId in profileCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                profileRefs.Append(curveRef);
                            }
                        }
                    }

                    if (profileRefs.Size == 0)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No valid profile curves found" };
                    }

                    // Create the cap form
                    Form capForm = doc.FamilyCreate.NewFormByCap(isSolid, profileRefs);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        form_id = GetElementIdInt(capForm.Id),
                        is_solid = isSolid
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyFormByCap error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a form by thickening a surface
        /// </summary>
        private object CreateFamilyFormByThicken(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;
                double thickness = parameters.ContainsKey("thickness") ? Convert.ToDouble(parameters["thickness"]) : 1.0;

                // Get surface form ID
                if (!parameters.ContainsKey("surface_form_id"))
                {
                    return new { success = false, error = "Missing required parameter: surface_form_id" };
                }

                int surfaceFormId = Convert.ToInt32(parameters["surface_form_id"]);
                Element surfaceElem = doc.GetElement(new ElementId(surfaceFormId));

                if (!(surfaceElem is Form surfaceForm))
                {
                    return new { success = false, error = $"Element {surfaceFormId} is not a Form" };
                }

                int faceIndex = parameters.ContainsKey("face_index") ? Convert.ToInt32(parameters["face_index"]) : 0;

                using (Transaction trans = new Transaction(doc, "Create Form By Thicken"))
                {
                    trans.Start();

                    // Create thickened form using correct signature:
                    // NewFormByThickenSingleSurface(bool isSolid, Form surfaceForm, XYZ thicknessVector)
                    XYZ thicknessDir = new XYZ(0, 0, thickness);
                    Form thickenedForm = doc.FamilyCreate.NewFormByThickenSingleSurface(isSolid, surfaceForm, thicknessDir);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        form_id = GetElementIdInt(thickenedForm.Id),
                        is_solid = isSolid,
                        thickness = thickness,
                        source_form_id = surfaceFormId
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyFormByThicken error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a revolve form in a conceptual mass family
        /// </summary>
        private object CreateFamilyRevolveForm(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;
                double startAngle = parameters.ContainsKey("start_angle") ? Convert.ToDouble(parameters["start_angle"]) : 0;
                double endAngle = parameters.ContainsKey("end_angle") ? Convert.ToDouble(parameters["end_angle"]) : 360;

                // Get axis line ID
                if (!parameters.ContainsKey("axis_line_id"))
                {
                    return new { success = false, error = "Missing required parameter: axis_line_id" };
                }

                int axisLineId = Convert.ToInt32(parameters["axis_line_id"]);
                Element axisElem = doc.GetElement(new ElementId(axisLineId));
                if (axisElem == null)
                {
                    return new { success = false, error = $"Axis line element {axisLineId} not found" };
                }

                // Get profile curve ID
                if (!parameters.ContainsKey("profile_curve_id"))
                {
                    return new { success = false, error = "Missing required parameter: profile_curve_id" };
                }

                int profileCurveId = Convert.ToInt32(parameters["profile_curve_id"]);
                Element profileElem = doc.GetElement(new ElementId(profileCurveId));
                if (profileElem == null)
                {
                    return new { success = false, error = $"Profile curve element {profileCurveId} not found" };
                }

                using (Transaction trans = new Transaction(doc, "Create Revolve Form"))
                {
                    trans.Start();

                    Reference axisRef = GetReferenceFromElement(doc, axisElem);
                    Reference profileRef = GetReferenceFromElement(doc, profileElem);

                    if (axisRef == null || profileRef == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not get references from elements" };
                    }

                    ReferenceArray profileRefs = new ReferenceArray();
                    profileRefs.Append(profileRef);

                    // Create revolve form using NewRevolveForms
                    // Signature: NewRevolveForms(bool isSolid, ReferenceArray profile, Reference axis, double startAngle, double endAngle)
                    // Note: Some overloads take Reference axis, some take ReferenceArray profiles
                    FormArray revolveFormArray = doc.FamilyCreate.NewRevolveForms(isSolid, profileRefs, axisRef, startAngle * Math.PI / 180.0, endAngle * Math.PI / 180.0);

                    trans.Commit();

                    // Get first form from array
                    Form revolveForm = revolveFormArray.Size > 0 ? revolveFormArray.get_Item(0) : null;
                    int formId = revolveForm != null ? GetElementIdInt(revolveForm.Id) : -1;

                    return new
                    {
                        success = true,
                        form_id = formId,
                        form_count = revolveFormArray.Size,
                        is_solid = isSolid,
                        start_angle = startAngle,
                        end_angle = endAngle
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyRevolveForm error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create an extrusion form in a conceptual mass family
        /// </summary>
        private object CreateFamilyExtrusionForm(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get profile curve IDs
                var profileCurveIds = GetElementIdListFromParam(parameters, "profile_curve_ids");
                if (profileCurveIds == null || profileCurveIds.Count == 0)
                {
                    return new { success = false, error = "Profile curve IDs required. Use 'profile_curve_ids' parameter." };
                }

                // Get direction
                double dirX = parameters.ContainsKey("direction_x") ? Convert.ToDouble(parameters["direction_x"]) : 0;
                double dirY = parameters.ContainsKey("direction_y") ? Convert.ToDouble(parameters["direction_y"]) : 0;
                double dirZ = parameters.ContainsKey("direction_z") ? Convert.ToDouble(parameters["direction_z"]) : 10;

                XYZ direction = new XYZ(dirX, dirY, dirZ);

                using (Transaction trans = new Transaction(doc, "Create Extrusion Form"))
                {
                    trans.Start();

                    // Get profile references
                    ReferenceArray profileRefs = new ReferenceArray();
                    foreach (int curveId in profileCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                profileRefs.Append(curveRef);
                            }
                        }
                    }

                    if (profileRefs.Size == 0)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No valid profile curves found" };
                    }

                    // Create extrusion form
                    Form extrusionForm = doc.FamilyCreate.NewExtrusionForm(isSolid, profileRefs, direction);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        form_id = GetElementIdInt(extrusionForm.Id),
                        is_solid = isSolid,
                        direction = new { x = dirX, y = dirY, z = dirZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyExtrusionForm error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a swept blend form in a conceptual mass family
        /// </summary>
        private object CreateFamilySweptBlendForm(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                bool isSolid = parameters.ContainsKey("is_solid") ? Convert.ToBoolean(parameters["is_solid"]) : true;

                // Get path curve IDs
                var pathCurveIds = GetElementIdListFromParam(parameters, "path_curve_ids");
                if (pathCurveIds == null || pathCurveIds.Count == 0)
                {
                    return new { success = false, error = "Path curve IDs required. Use 'path_curve_ids' parameter." };
                }

                // Get profile curves (array of arrays)
                var profileArrays = GetProfileArraysFromParam(parameters, "profile_curve_ids_array");
                if (profileArrays == null || profileArrays.Count < 2)
                {
                    return new { success = false, error = "At least 2 profile arrays required. Use 'profile_curve_ids_array' parameter." };
                }

                using (Transaction trans = new Transaction(doc, "Create Swept Blend Form"))
                {
                    trans.Start();

                    // Get path references
                    ReferenceArray pathRefs = new ReferenceArray();
                    foreach (int curveId in pathCurveIds)
                    {
                        Element curveElem = doc.GetElement(new ElementId(curveId));
                        if (curveElem != null)
                        {
                            Reference curveRef = GetReferenceFromElement(doc, curveElem);
                            if (curveRef != null)
                            {
                                pathRefs.Append(curveRef);
                            }
                        }
                    }

                    // Get profile references
                    ReferenceArrayArray profileRefsArray = new ReferenceArrayArray();
                    foreach (var profileIds in profileArrays)
                    {
                        ReferenceArray profileRefs = new ReferenceArray();
                        foreach (int curveId in profileIds)
                        {
                            Element curveElem = doc.GetElement(new ElementId(curveId));
                            if (curveElem != null)
                            {
                                Reference curveRef = GetReferenceFromElement(doc, curveElem);
                                if (curveRef != null)
                                {
                                    profileRefs.Append(curveRef);
                                }
                            }
                        }
                        if (profileRefs.Size > 0)
                        {
                            profileRefsArray.Append(profileRefs);
                        }
                    }

                    if (pathRefs.Size == 0)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No valid path curves found" };
                    }

                    if (profileRefsArray.Size < 2)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Need at least 2 valid profile arrays" };
                    }

                    // Create swept blend form
                    Form sweptBlendForm = doc.FamilyCreate.NewSweptBlendForm(isSolid, pathRefs, profileRefsArray);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        form_id = GetElementIdInt(sweptBlendForm.Id),
                        is_solid = isSolid,
                        path_count = pathRefs.Size,
                        profile_count = profileRefsArray.Size
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilySweptBlendForm error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create model text in a family document
        /// </summary>
        private object CreateFamilyModelText(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                string text = parameters.ContainsKey("text") ? parameters["text"].ToString() : "Text";
                double depth = parameters.ContainsKey("depth") ? Convert.ToDouble(parameters["depth"]) : 1.0;

                // Get position
                double x = parameters.ContainsKey("x") ? Convert.ToDouble(parameters["x"]) : 0;
                double y = parameters.ContainsKey("y") ? Convert.ToDouble(parameters["y"]) : 0;
                double z = parameters.ContainsKey("z") ? Convert.ToDouble(parameters["z"]) : 0;
                XYZ position = new XYZ(x, y, z);

                // Get horizontal alignment
                HorizontalAlign hAlign = HorizontalAlign.Left;
                if (parameters.ContainsKey("horizontal_align"))
                {
                    string alignStr = parameters["horizontal_align"].ToString().ToLower();
                    if (alignStr == "center") hAlign = HorizontalAlign.Center;
                    else if (alignStr == "right") hAlign = HorizontalAlign.Right;
                }

                // Get sketch plane
                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                using (Transaction trans = new Transaction(doc, "Create Model Text"))
                {
                    trans.Start();

                    // Get model text type
                    ModelTextType textType = null;
                    if (parameters.ContainsKey("text_type_id"))
                    {
                        int typeId = Convert.ToInt32(parameters["text_type_id"]);
                        Element typeElem = doc.GetElement(new ElementId(typeId));
                        if (typeElem is ModelTextType mtt)
                        {
                            textType = mtt;
                        }
                    }

                    if (textType == null)
                    {
                        // Find first available model text type
                        FilteredElementCollector collector = new FilteredElementCollector(doc);
                        textType = collector.OfClass(typeof(ModelTextType)).FirstOrDefault() as ModelTextType;
                    }

                    if (textType == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "No ModelTextType found in document" };
                    }

                    // Create the model text
                    ModelText modelText = doc.FamilyCreate.NewModelText(text, textType, sketchPlane, position, hAlign, depth);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        model_text_id = GetElementIdInt(modelText.Id),
                        text = text,
                        depth = depth,
                        position = new { x = x, y = y, z = z }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyModelText error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create an opening in a family element
        /// </summary>
        private object CreateFamilyOpening(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get host element
                if (!parameters.ContainsKey("host_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: host_element_id" };
                }

                int hostId = Convert.ToInt32(parameters["host_element_id"]);
                Element hostElem = doc.GetElement(new ElementId(hostId));
                if (hostElem == null)
                {
                    return new { success = false, error = $"Host element {hostId} not found" };
                }

                // Get profile points
                var profilePoints = GetProfilePoints(parameters);
                if (profilePoints == null || profilePoints.Count < 3)
                {
                    return new { success = false, error = "Profile requires at least 3 points. Use 'profile_points' parameter." };
                }

                // Get sketch plane
                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                using (Transaction trans = new Transaction(doc, "Create Opening"))
                {
                    trans.Start();

                    // Create profile curves
                    CurveArray curveArray = CreateCurveArrayFromPoints(profilePoints);

                    // Create the opening based on host element type
                    Opening opening = null;

                    if (hostElem is GenericForm genericForm)
                    {
                        // For extrusions, sweeps, blends etc - use NewOpening(GenericForm, CurveArray)
                        opening = doc.FamilyCreate.NewOpening(genericForm, curveArray);
                    }
                    else if (hostElem is Wall wall)
                    {
                        // For walls, use rectangular opening
                        double minX = profilePoints.Min(p => p.X);
                        double maxX = profilePoints.Max(p => p.X);
                        double minZ = profilePoints.Min(p => p.Z);
                        double maxZ = profilePoints.Max(p => p.Z);

                        XYZ pnt1 = new XYZ(minX, 0, minZ);
                        XYZ pnt2 = new XYZ(maxX, 0, maxZ);
                        opening = doc.Create.NewOpening(wall, pnt1, pnt2);
                    }
                    else if (hostElem is Floor floor)
                    {
                        // For floors
                        opening = doc.Create.NewOpening(floor, curveArray, true);
                    }

                    if (opening == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = "Could not create opening. Host element type may not support family openings. Use GenericForm (Extrusion, Sweep, Blend, etc.), Wall, or Floor." };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        opening_id = GetElementIdInt(opening.Id),
                        host_element_id = hostId
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyOpening error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create symbolic curves in a family document
        /// </summary>
        private object CreateFamilySymbolicCurve(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get sketch plane
                SketchPlane sketchPlane = GetOrCreateSketchPlane(doc, parameters);
                if (sketchPlane == null)
                {
                    return new { success = false, error = "Could not get or create sketch plane" };
                }

                string curveType = parameters.ContainsKey("curve_type") ? parameters["curve_type"].ToString().ToLower() : "line";

                using (Transaction trans = new Transaction(doc, "Create Symbolic Curve"))
                {
                    trans.Start();

                    SymbolicCurve symbolicCurve = null;

                    if (curveType == "line")
                    {
                        double startX = parameters.ContainsKey("start_x") ? Convert.ToDouble(parameters["start_x"]) : 0;
                        double startY = parameters.ContainsKey("start_y") ? Convert.ToDouble(parameters["start_y"]) : 0;
                        double startZ = parameters.ContainsKey("start_z") ? Convert.ToDouble(parameters["start_z"]) : 0;
                        double endX = parameters.ContainsKey("end_x") ? Convert.ToDouble(parameters["end_x"]) : 10;
                        double endY = parameters.ContainsKey("end_y") ? Convert.ToDouble(parameters["end_y"]) : 0;
                        double endZ = parameters.ContainsKey("end_z") ? Convert.ToDouble(parameters["end_z"]) : 0;

                        Line line = Line.CreateBound(new XYZ(startX, startY, startZ), new XYZ(endX, endY, endZ));
                        symbolicCurve = doc.FamilyCreate.NewSymbolicCurve(line, sketchPlane);
                    }
                    else if (curveType == "arc")
                    {
                        double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                        double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                        double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
                        double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;
                        double startAngle = parameters.ContainsKey("start_angle") ? Convert.ToDouble(parameters["start_angle"]) : 0;
                        double endAngle = parameters.ContainsKey("end_angle") ? Convert.ToDouble(parameters["end_angle"]) : 180;

                        XYZ center = new XYZ(centerX, centerY, centerZ);
                        Arc arc = Arc.Create(center, radius, startAngle * Math.PI / 180, endAngle * Math.PI / 180, XYZ.BasisX, XYZ.BasisY);
                        symbolicCurve = doc.FamilyCreate.NewSymbolicCurve(arc, sketchPlane);
                    }
                    else if (curveType == "circle")
                    {
                        double centerX = parameters.ContainsKey("center_x") ? Convert.ToDouble(parameters["center_x"]) : 0;
                        double centerY = parameters.ContainsKey("center_y") ? Convert.ToDouble(parameters["center_y"]) : 0;
                        double centerZ = parameters.ContainsKey("center_z") ? Convert.ToDouble(parameters["center_z"]) : 0;
                        double radius = parameters.ContainsKey("radius") ? Convert.ToDouble(parameters["radius"]) : 5;

                        XYZ center = new XYZ(centerX, centerY, centerZ);
                        Arc circle = Arc.Create(center, radius, 0, 2 * Math.PI, XYZ.BasisX, XYZ.BasisY);
                        symbolicCurve = doc.FamilyCreate.NewSymbolicCurve(circle, sketchPlane);
                    }

                    if (symbolicCurve == null)
                    {
                        trans.RollBack();
                        return new { success = false, error = $"Unknown curve type: {curveType}. Use 'line', 'arc', or 'circle'." };
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        symbolic_curve_id = GetElementIdInt(symbolicCurve.Id),
                        curve_type = curveType,
                        sketch_plane_id = GetElementIdInt(sketchPlane.Id)
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilySymbolicCurve error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Convert symbolic lines to model lines in a family document
        /// </summary>
        private object ConvertSymbolicToModelCurves(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This operation only works in family documents." };
                }

                // Get the element IDs to convert
                List<int> symbolicCurveIds = new List<int>();
                
                if (parameters.ContainsKey("element_ids") && parameters["element_ids"] is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        symbolicCurveIds.Add(Convert.ToInt32(item));
                    }
                }
                else if (parameters.ContainsKey("element_id"))
                {
                    symbolicCurveIds.Add(Convert.ToInt32(parameters["element_id"]));
                }
                else
                {
                    return new { success = false, error = "Please provide 'element_id' or 'element_ids' parameter." };
                }

                if (symbolicCurveIds.Count == 0)
                {
                    return new { success = false, error = "No element IDs provided." };
                }

                List<int> newModelCurveIds = new List<int>();
                List<string> messages = new List<string>();
                int convertedCount = 0;

                using (Transaction trans = new Transaction(doc, "Convert Symbolic to Model Curves"))
                {
                    trans.Start();

                    foreach (int symbolicId in symbolicCurveIds)
                    {
                        try
                        {
                            Element element = doc.GetElement(new ElementId(symbolicId));
                            
                            if (element == null)
                            {
                                messages.Add($"Element {symbolicId} not found.");
                                continue;
                            }

                            if (!(element is SymbolicCurve symbolicCurve))
                            {
                                messages.Add($"Element {symbolicId} is not a symbolic curve. Type: {element.GetType().Name}");
                                continue;
                            }

                            // Get the geometry and sketch plane from the symbolic curve
                            Curve curve = symbolicCurve.GeometryCurve;
                            SketchPlane sketchPlane = symbolicCurve.SketchPlane;

                            if (curve == null || sketchPlane == null)
                            {
                                messages.Add($"Could not get geometry or sketch plane from symbolic curve {symbolicId}.");
                                continue;
                            }

                            // Create a new model curve with the same geometry
                            ModelCurve modelCurve = doc.FamilyCreate.NewModelCurve(curve, sketchPlane);
                            
                            if (modelCurve != null)
                            {
                                newModelCurveIds.Add(GetElementIdInt(modelCurve.Id));
                                convertedCount++;

                                // Delete the original symbolic curve
                                doc.Delete(symbolicCurve.Id);
                                messages.Add($"Successfully converted symbolic curve {symbolicId} to model curve {GetElementIdInt(modelCurve.Id)}.");
                            }
                            else
                            {
                                messages.Add($"Failed to create model curve from symbolic curve {symbolicId}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            messages.Add($"Error converting element {symbolicId}: {ex.Message}");
                        }
                    }

                    trans.Commit();
                }

                return new
                {
                    success = convertedCount > 0,
                    converted_count = convertedCount,
                    new_model_curve_ids = newModelCurveIds,
                    messages = messages
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ConvertSymbolicToModelCurves error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// Create a control in a family document (for parameter manipulation)
        /// Note: Control creation has limited API support in Revit 2026
        /// </summary>
        private object CreateFamilyControl(Document doc, Dictionary<string, object> parameters)
        {
            // Control creation via API is limited in Revit 2026
            // Provide information about manual control creation
            return new
            {
                success = false,
                error = "Control creation is not fully supported via API in Revit 2026. Controls are typically created manually in the Family Editor by adding dimension parameters and associating them with labels.",
                workaround = "To create controls: 1) Add dimensions to your geometry, 2) Select dimension and click 'Label' dropdown, 3) Create or select a parameter to associate with the dimension."
            };
        }

        /// <summary>
        /// Create a diameter dimension in a family document
        /// </summary>
        private object CreateFamilyDiameterDimension(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get view
                View view = doc.ActiveView;
                if (parameters.ContainsKey("view_id"))
                {
                    int viewId = Convert.ToInt32(parameters["view_id"]);
                    Element viewElem = doc.GetElement(new ElementId(viewId));
                    if (viewElem is View v) view = v;
                }

                if (view == null)
                {
                    return new { success = false, error = "No valid view available" };
                }

                // Get arc element
                if (!parameters.ContainsKey("arc_element_id"))
                {
                    return new { success = false, error = "Missing required parameter: arc_element_id" };
                }

                int arcId = Convert.ToInt32(parameters["arc_element_id"]);
                Element arcElem = doc.GetElement(new ElementId(arcId));
                if (arcElem == null)
                {
                    return new { success = false, error = $"Arc element {arcId} not found" };
                }

                Reference arcRef = GetReferenceFromElement(doc, arcElem);
                if (arcRef == null)
                {
                    return new { success = false, error = "Could not get reference from arc element" };
                }

                // Get origin point for dimension
                double originX = parameters.ContainsKey("origin_x") ? Convert.ToDouble(parameters["origin_x"]) : 0;
                double originY = parameters.ContainsKey("origin_y") ? Convert.ToDouble(parameters["origin_y"]) : 5;
                double originZ = parameters.ContainsKey("origin_z") ? Convert.ToDouble(parameters["origin_z"]) : 0;
                XYZ origin = new XYZ(originX, originY, originZ);

                using (Transaction trans = new Transaction(doc, "Create Diameter Dimension"))
                {
                    trans.Start();

                    Dimension dimDiam = doc.FamilyCreate.NewDiameterDimension(view, arcRef, origin);

                    trans.Commit();

                    return new
                    {
                        success = true,
                        dimension_id = GetElementIdInt(dimDiam.Id),
                        arc_element_id = arcId,
                        origin = new { x = originX, y = originY, z = originZ }
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateFamilyDiameterDimension error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get forms in a family document
        /// </summary>
        private object GetFamilyForms(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);

                var forms = collector.OfClass(typeof(GenericForm)).Cast<GenericForm>()
                    .Select(f => new
                    {
                        id = GetElementIdInt(f.Id),
                        name = f.Name,
                        type = f.GetType().Name,
                        is_solid = f.IsSolid,
                        is_visible = f.Visible
                    }).ToList();

                return new
                {
                    success = true,
                    count = forms.Count,
                    forms = forms
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetFamilyForms error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get sketch planes in a family document
        /// </summary>
        private object GetFamilySketchPlanes(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);

                var planes = collector.OfClass(typeof(SketchPlane)).Cast<SketchPlane>()
                    .Select(sp => {
                        Plane plane = sp.GetPlane();
                        return new
                        {
                            id = GetElementIdInt(sp.Id),
                            name = sp.Name,
                            origin = new { x = plane.Origin.X, y = plane.Origin.Y, z = plane.Origin.Z },
                            normal = new { x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z }
                        };
                    }).ToList();

                return new
                {
                    success = true,
                    count = planes.Count,
                    sketch_planes = planes
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetFamilySketchPlanes error: {ex.Message}" };
            }
        }

        #region Family Modeling Helpers

        /// <summary>
        /// Get or create a sketch plane from parameters
        /// </summary>
        private SketchPlane GetOrCreateSketchPlane(Document doc, Dictionary<string, object> parameters, string prefix = "")
        {
            try
            {
                // Check if sketch plane ID is provided
                string planeIdKey = prefix + "sketch_plane_id";
                if (parameters.ContainsKey(planeIdKey))
                {
                    int planeId = Convert.ToInt32(parameters[planeIdKey]);
                    Element planeElem = doc.GetElement(new ElementId(planeId));
                    if (planeElem is SketchPlane sp)
                    {
                        return sp;
                    }
                }

                // Check for plane name
                string planeNameKey = prefix + "sketch_plane_name";
                if (parameters.ContainsKey(planeNameKey))
                {
                    string planeName = parameters[planeNameKey].ToString();
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    var plane = collector.OfClass(typeof(SketchPlane)).Cast<SketchPlane>()
                        .FirstOrDefault(sp => sp.Name.Equals(planeName, StringComparison.OrdinalIgnoreCase));
                    if (plane != null) return plane;
                }

                // Create from normal and origin
                string originXKey = prefix + "plane_origin_x";
                string originYKey = prefix + "plane_origin_y";
                string originZKey = prefix + "plane_origin_z";
                string normalXKey = prefix + "plane_normal_x";
                string normalYKey = prefix + "plane_normal_y";
                string normalZKey = prefix + "plane_normal_z";

                double originX = parameters.ContainsKey(originXKey) ? Convert.ToDouble(parameters[originXKey]) : 0;
                double originY = parameters.ContainsKey(originYKey) ? Convert.ToDouble(parameters[originYKey]) : 0;
                double originZ = parameters.ContainsKey(originZKey) ? Convert.ToDouble(parameters[originZKey]) : 0;
                double normalX = parameters.ContainsKey(normalXKey) ? Convert.ToDouble(parameters[normalXKey]) : 0;
                double normalY = parameters.ContainsKey(normalYKey) ? Convert.ToDouble(parameters[normalYKey]) : 0;
                double normalZ = parameters.ContainsKey(normalZKey) ? Convert.ToDouble(parameters[normalZKey]) : 1;

                XYZ origin = new XYZ(originX, originY, originZ);
                XYZ normal = new XYZ(normalX, normalY, normalZ).Normalize();

                Plane plane2 = Plane.CreateByNormalAndOrigin(normal, origin);
                return SketchPlane.Create(doc, plane2);
            }
            catch
            {
                // If all else fails, try to get a default sketch plane
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                return collector.OfClass(typeof(SketchPlane)).Cast<SketchPlane>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Get default family sketch plane for model curves (Reference Level / horizontal plane at Z=0)
        /// This is the standard floor plan view plane in family editor
        /// </summary>
        private SketchPlane GetDefaultFamilySketchPlane(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Check if sketch plane ID is explicitly provided
                if (parameters.ContainsKey("sketch_plane_id"))
                {
                    int planeId = Convert.ToInt32(parameters["sketch_plane_id"]);
                    Element planeElem = doc.GetElement(new ElementId(planeId));
                    if (planeElem is SketchPlane sp)
                    {
                        return sp;
                    }
                }

                // Check for sketch plane name
                if (parameters.ContainsKey("sketch_plane_name"))
                {
                    string planeName = parameters["sketch_plane_name"].ToString();
                    FilteredElementCollector collector = new FilteredElementCollector(doc);
                    var plane = collector.OfClass(typeof(SketchPlane)).Cast<SketchPlane>()
                        .FirstOrDefault(s => s.Name.Equals(planeName, StringComparison.OrdinalIgnoreCase));
                    if (plane != null) return plane;
                }

                // Try to find "Reference Level" - the standard horizontal plane in family editor
                FilteredElementCollector sketchPlaneCollector = new FilteredElementCollector(doc);
                var refLevelPlane = sketchPlaneCollector.OfClass(typeof(SketchPlane))
                    .Cast<SketchPlane>()
                    .FirstOrDefault(sp => sp.Name.Equals("Reference Level", StringComparison.OrdinalIgnoreCase));
                
                if (refLevelPlane != null)
                {
                    return refLevelPlane;
                }

                // Try to find any horizontal plane at Z=0 or close to it
                var horizontalPlanes = sketchPlaneCollector.OfClass(typeof(SketchPlane))
                    .Cast<SketchPlane>()
                    .Where(sp => 
                    {
                        XYZ normal = sp.GetPlane().Normal;
                        // Check if normal is close to BasisZ (vertical plane with horizontal normal)
                        return Math.Abs(Math.Abs(normal.Z) - 1.0) < 0.001;
                    })
                    .ToList();

                if (horizontalPlanes.Count > 0)
                {
                    // Prefer the one closest to Z=0
                    var closestToZero = horizontalPlanes
                        .OrderBy(sp => Math.Abs(sp.GetPlane().Origin.Z))
                        .FirstOrDefault();
                    return closestToZero;
                }

                // Default: create a horizontal plane at Z=0 (XY plane)
                Plane defaultPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                return SketchPlane.Create(doc, defaultPlane);
            }
            catch
            {
                // Last resort: create a simple XY plane at origin
                try
                {
                    Plane fallbackPlane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
                    return SketchPlane.Create(doc, fallbackPlane);
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Get profile points from parameters
        /// </summary>
        private List<XYZ> GetProfilePoints(Dictionary<string, object> parameters)
        {
            return GetProfilePointsFromParam(parameters, "profile_points");
        }

        /// <summary>
        /// Get profile points from a specific parameter key
        /// </summary>
        private List<XYZ> GetProfilePointsFromParam(Dictionary<string, object> parameters, string paramKey)
        {
            if (!parameters.ContainsKey(paramKey))
            {
                return null;
            }

            var pointsObj = parameters[paramKey];
            List<XYZ> points = new List<XYZ>();

            if (pointsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    if (item is JObject jObj)
                    {
                        double x = jObj.ContainsKey("x") ? Convert.ToDouble(jObj["x"]) : 0;
                        double y = jObj.ContainsKey("y") ? Convert.ToDouble(jObj["y"]) : 0;
                        double z = jObj.ContainsKey("z") ? Convert.ToDouble(jObj["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }
            else if (pointsObj is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        double x = dict.ContainsKey("x") ? Convert.ToDouble(dict["x"]) : 0;
                        double y = dict.ContainsKey("y") ? Convert.ToDouble(dict["y"]) : 0;
                        double z = dict.ContainsKey("z") ? Convert.ToDouble(dict["z"]) : 0;
                        points.Add(new XYZ(x, y, z));
                    }
                }
            }

            return points.Count > 0 ? points : null;
        }

        /// <summary>
        /// Get element ID list from parameter
        /// </summary>
        private List<int> GetElementIdListFromParam(Dictionary<string, object> parameters, string paramKey)
        {
            if (!parameters.ContainsKey(paramKey))
            {
                return null;
            }

            var idsObj = parameters[paramKey];
            List<int> ids = new List<int>();

            if (idsObj is JArray jArray)
            {
                foreach (var item in jArray)
                {
                    ids.Add(Convert.ToInt32(item));
                }
            }
            else if (idsObj is IEnumerable<object> enumerable)
            {
                foreach (var item in enumerable)
                {
                    ids.Add(Convert.ToInt32(item));
                }
            }

            return ids.Count > 0 ? ids : null;
        }

        /// <summary>
        /// Get profile arrays from parameter (array of arrays of element IDs)
        /// </summary>
        private List<List<int>> GetProfileArraysFromParam(Dictionary<string, object> parameters, string paramKey)
        {
            if (!parameters.ContainsKey(paramKey))
            {
                return null;
            }

            var arraysObj = parameters[paramKey];
            List<List<int>> arrays = new List<List<int>>();

            if (arraysObj is JArray outerArray)
            {
                foreach (var innerItem in outerArray)
                {
                    if (innerItem is JArray innerArray)
                    {
                        List<int> innerList = new List<int>();
                        foreach (var item in innerArray)
                        {
                            innerList.Add(Convert.ToInt32(item));
                        }
                        if (innerList.Count > 0)
                        {
                            arrays.Add(innerList);
                        }
                    }
                }
            }

            return arrays.Count > 0 ? arrays : null;
        }

        /// <summary>
        /// Create curve array from points
        /// </summary>
        private CurveArray CreateCurveArrayFromPoints(List<XYZ> points, bool closedLoop = true)
        {
            CurveArray curveArray = new CurveArray();

            for (int i = 0; i < points.Count - 1; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                curveArray.Append(line);
            }

            // Close the loop if requested
            if (closedLoop && points.Count > 2)
            {
                Line closingLine = Line.CreateBound(points[points.Count - 1], points[0]);
                curveArray.Append(closingLine);
            }

            return curveArray;
        }

        #endregion

        #endregion

        #region Connector Tool

        /// <summary>
        /// Connector tool - creates MEP connectors in family documents
        /// </summary>
        private object ConnectorTool(UIApplication app, Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                if (!doc.IsFamilyDocument)
                {
                    return new { success = false, error = "This tool only works in family documents. Open a family (.rfa) file first." };
                }

                string operation = parameters.ContainsKey("operation") ? parameters["operation"].ToString() : "get_connectors";

                switch (operation.ToLower())
                {
                    case "create_duct_connector":
                        return CreateDuctConnector(doc, parameters);

                    case "create_pipe_connector":
                        return CreatePipeConnector(doc, parameters);

                    case "create_electrical_connector":
                        return CreateElectricalConnector(doc, parameters);

                    case "create_cable_tray_connector":
                        return CreateCableTrayConnector(doc, parameters);

                    case "create_conduit_connector":
                        return CreateConduitConnector(doc, parameters);

                    case "change_host_reference":
                        return ChangeConnectorHostReference(doc, parameters);

                    case "get_connectors":
                        return GetConnectors(doc, parameters);

                    default:
                        return new
                        {
                            success = false,
                            error = $"Unknown operation: {operation}",
                            available_operations = new[]
                            {
                                "create_duct_connector",
                                "create_pipe_connector",
                                "create_electrical_connector",
                                "create_cable_tray_connector",
                                "create_conduit_connector",
                                "change_host_reference",
                                "get_connectors"
                            }
                        };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ConnectorTool error: {ex.Message}", stackTrace = ex.StackTrace };
            }
        }

        /// <summary>
        /// Create a duct connector on a face
        /// </summary>
        private object CreateDuctConnector(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get system type
                string systemTypeStr = parameters.ContainsKey("system_type") ? parameters["system_type"].ToString() : "SupplyAir";
                DuctSystemType systemType = DuctSystemType.SupplyAir;
                if (Enum.TryParse(systemTypeStr, true, out DuctSystemType parsedSystemType))
                {
                    systemType = parsedSystemType;
                }

                // Get profile type
                string profileTypeStr = parameters.ContainsKey("profile_type") ? parameters["profile_type"].ToString() : "Round";
                ConnectorProfileType profileType = ConnectorProfileType.Round;
                if (Enum.TryParse(profileTypeStr, true, out ConnectorProfileType parsedProfileType))
                {
                    profileType = parsedProfileType;
                }

                // Get face reference from element
                Reference faceRef = GetFaceReferenceFromParams(doc, parameters);
                if (faceRef == null)
                {
                    return new { success = false, error = "Could not get face reference. Provide element_id and optionally face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters);

                using (Transaction trans = new Transaction(doc, "Create Duct Connector"))
                {
                    trans.Start();

                    ConnectorElement connector;
                    if (edge != null)
                    {
                        connector = ConnectorElement.CreateDuctConnector(doc, systemType, profileType, faceRef, edge);
                    }
                    else
                    {
                        connector = ConnectorElement.CreateDuctConnector(doc, systemType, profileType, faceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Duct connector created successfully",
                        connector_id = GetElementIdInt(connector.Id),
                        system_type = systemType.ToString(),
                        profile_type = profileType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateDuctConnector error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a pipe connector on a face
        /// </summary>
        private object CreatePipeConnector(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get system type
                string systemTypeStr = parameters.ContainsKey("system_type") ? parameters["system_type"].ToString() : "SupplyHydronic";
                PipeSystemType systemType = PipeSystemType.SupplyHydronic;
                if (Enum.TryParse(systemTypeStr, true, out PipeSystemType parsedSystemType))
                {
                    systemType = parsedSystemType;
                }

                // Get face reference from element
                Reference faceRef = GetFaceReferenceFromParams(doc, parameters);
                if (faceRef == null)
                {
                    return new { success = false, error = "Could not get face reference. Provide element_id and optionally face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters);

                using (Transaction trans = new Transaction(doc, "Create Pipe Connector"))
                {
                    trans.Start();

                    ConnectorElement connector;
                    if (edge != null)
                    {
                        connector = ConnectorElement.CreatePipeConnector(doc, systemType, faceRef, edge);
                    }
                    else
                    {
                        connector = ConnectorElement.CreatePipeConnector(doc, systemType, faceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Pipe connector created successfully",
                        connector_id = GetElementIdInt(connector.Id),
                        system_type = systemType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreatePipeConnector error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create an electrical connector on a face
        /// </summary>
        private object CreateElectricalConnector(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get system type
                string systemTypeStr = parameters.ContainsKey("system_type") ? parameters["system_type"].ToString() : "PowerCircuit";
                ElectricalSystemType systemType = ElectricalSystemType.PowerCircuit;
                if (Enum.TryParse(systemTypeStr, true, out ElectricalSystemType parsedSystemType))
                {
                    systemType = parsedSystemType;
                }

                // Get face reference from element
                Reference faceRef = GetFaceReferenceFromParams(doc, parameters);
                if (faceRef == null)
                {
                    return new { success = false, error = "Could not get face reference. Provide element_id and optionally face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters);

                using (Transaction trans = new Transaction(doc, "Create Electrical Connector"))
                {
                    trans.Start();

                    ConnectorElement connector;
                    if (edge != null)
                    {
                        connector = ConnectorElement.CreateElectricalConnector(doc, systemType, faceRef, edge);
                    }
                    else
                    {
                        connector = ConnectorElement.CreateElectricalConnector(doc, systemType, faceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Electrical connector created successfully",
                        connector_id = GetElementIdInt(connector.Id),
                        system_type = systemType.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateElectricalConnector error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a cable tray connector on a face
        /// </summary>
        private object CreateCableTrayConnector(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get face reference from element
                Reference faceRef = GetFaceReferenceFromParams(doc, parameters);
                if (faceRef == null)
                {
                    return new { success = false, error = "Could not get face reference. Provide element_id and optionally face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters);

                using (Transaction trans = new Transaction(doc, "Create Cable Tray Connector"))
                {
                    trans.Start();

                    ConnectorElement connector;
                    if (edge != null)
                    {
                        connector = ConnectorElement.CreateCableTrayConnector(doc, faceRef, edge);
                    }
                    else
                    {
                        connector = ConnectorElement.CreateCableTrayConnector(doc, faceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Cable tray connector created successfully",
                        connector_id = GetElementIdInt(connector.Id)
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateCableTrayConnector error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Create a conduit connector on a face
        /// </summary>
        private object CreateConduitConnector(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get face reference from element
                Reference faceRef = GetFaceReferenceFromParams(doc, parameters);
                if (faceRef == null)
                {
                    return new { success = false, error = "Could not get face reference. Provide element_id and optionally face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters);

                using (Transaction trans = new Transaction(doc, "Create Conduit Connector"))
                {
                    trans.Start();

                    ConnectorElement connector;
                    if (edge != null)
                    {
                        connector = ConnectorElement.CreateConduitConnector(doc, faceRef, edge);
                    }
                    else
                    {
                        connector = ConnectorElement.CreateConduitConnector(doc, faceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Conduit connector created successfully",
                        connector_id = GetElementIdInt(connector.Id)
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"CreateConduitConnector error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Change the host reference of a connector
        /// </summary>
        private object ChangeConnectorHostReference(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                // Get connector element
                if (!parameters.ContainsKey("connector_id"))
                {
                    return new { success = false, error = "connector_id is required" };
                }
                int connectorId = Convert.ToInt32(parameters["connector_id"]);
                Element connectorElem = doc.GetElement(new ElementId(connectorId));
                if (!(connectorElem is ConnectorElement connector))
                {
                    return new { success = false, error = $"Element {connectorId} is not a ConnectorElement" };
                }

                // Get new face reference
                Reference newFaceRef = GetFaceReferenceFromParams(doc, parameters, "new_");
                if (newFaceRef == null)
                {
                    return new { success = false, error = "Could not get new face reference. Provide new_element_id and optionally new_face_index." };
                }

                // Check if edge is provided
                Edge edge = GetEdgeFromParams(doc, parameters, "new_");

                using (Transaction trans = new Transaction(doc, "Change Connector Host Reference"))
                {
                    trans.Start();

                    if (edge != null)
                    {
                        connector.ChangeHostReference(newFaceRef, edge);
                    }
                    else
                    {
                        connector.ChangeHostReference(newFaceRef);
                    }

                    trans.Commit();

                    return new
                    {
                        success = true,
                        message = "Connector host reference changed successfully",
                        connector_id = connectorId
                    };
                }
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"ChangeConnectorHostReference error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Get all connectors in the family document
        /// </summary>
        private object GetConnectors(Document doc, Dictionary<string, object> parameters)
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var connectors = collector.OfClass(typeof(ConnectorElement)).Cast<ConnectorElement>()
                    .Select(c => {
                        string connectorType = "Unknown";
                        try
                        {
                            // Try to determine connector domain/type
                            var domain = c.Domain;
                            connectorType = domain.ToString();
                        }
                        catch { }

                        return new
                        {
                            id = GetElementIdInt(c.Id),
                            name = c.Name,
                            connector_type = connectorType
                        };
                    }).ToList();

                return new
                {
                    success = true,
                    count = connectors.Count,
                    connectors = connectors
                };
            }
            catch (Exception ex)
            {
                return new { success = false, error = $"GetConnectors error: {ex.Message}" };
            }
        }

        #region Connector Helpers

        /// <summary>
        /// Get face reference from parameters
        /// </summary>
        private Reference GetFaceReferenceFromParams(Document doc, Dictionary<string, object> parameters, string prefix = "")
        {
            try
            {
                string elementIdKey = prefix + "element_id";
                if (!parameters.ContainsKey(elementIdKey))
                {
                    return null;
                }

                int elementId = Convert.ToInt32(parameters[elementIdKey]);
                Element elem = doc.GetElement(new ElementId(elementId));
                if (elem == null) return null;

                int faceIndex = 0;
                string faceIndexKey = prefix + "face_index";
                if (parameters.ContainsKey(faceIndexKey))
                {
                    faceIndex = Convert.ToInt32(parameters[faceIndexKey]);
                }

                // Get geometry and find the face
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;
                GeometryElement geomElem = elem.get_Geometry(options);

                if (geomElem != null)
                {
                    int currentIndex = 0;
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        if (geomObj is Solid solid)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                if (face is PlanarFace && face.Reference != null)
                                {
                                    if (currentIndex == faceIndex)
                                    {
                                        return face.Reference;
                                    }
                                    currentIndex++;
                                }
                            }
                        }
                        else if (geomObj is GeometryInstance gi)
                        {
                            GeometryElement instanceGeom = gi.GetInstanceGeometry();
                            foreach (GeometryObject instObj in instanceGeom)
                            {
                                if (instObj is Solid instSolid)
                                {
                                    foreach (Face face in instSolid.Faces)
                                    {
                                        if (face is PlanarFace && face.Reference != null)
                                        {
                                            if (currentIndex == faceIndex)
                                            {
                                                return face.Reference;
                                            }
                                            currentIndex++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // If we couldn't find planar face at index, try any face
                    currentIndex = 0;
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        if (geomObj is Solid solid)
                        {
                            foreach (Face face in solid.Faces)
                            {
                                if (face.Reference != null)
                                {
                                    if (currentIndex == faceIndex)
                                    {
                                        return face.Reference;
                                    }
                                    currentIndex++;
                                }
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get edge from parameters
        /// </summary>
        private Edge GetEdgeFromParams(Document doc, Dictionary<string, object> parameters, string prefix = "")
        {
            try
            {
                string edgeIndexKey = prefix + "edge_index";
                if (!parameters.ContainsKey(edgeIndexKey))
                {
                    return null;
                }

                string elementIdKey = prefix + "element_id";
                if (!parameters.ContainsKey(elementIdKey))
                {
                    return null;
                }

                int elementId = Convert.ToInt32(parameters[elementIdKey]);
                int edgeIndex = Convert.ToInt32(parameters[edgeIndexKey]);
                Element elem = doc.GetElement(new ElementId(elementId));
                if (elem == null) return null;

                // Get geometry and find the edge
                Options options = new Options();
                options.ComputeReferences = true;
                options.IncludeNonVisibleObjects = true;
                GeometryElement geomElem = elem.get_Geometry(options);

                if (geomElem != null)
                {
                    int currentIndex = 0;
                    foreach (GeometryObject geomObj in geomElem)
                    {
                        if (geomObj is Solid solid)
                        {
                            foreach (Edge edge in solid.Edges)
                            {
                                if (currentIndex == edgeIndex)
                                {
                                    return edge;
                                }
                                currentIndex++;
                            }
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// Custom selection filter for category and class filtering
    /// </summary>
    public class CustomSelectionFilter : ISelectionFilter
    {
        private readonly Document _doc;
        private readonly string _categoryName;
        private readonly string _className;
        private readonly Func<string, BuiltInCategory?> _getCategoryFunc;
        private readonly Func<string, Type> _getTypeFunc;
        private readonly BuiltInCategory? _targetCategory;
        private readonly Type _targetType;

        public CustomSelectionFilter(Document doc, string categoryName, string className, 
            Func<string, BuiltInCategory?> getCategoryFunc, Func<string, Type> getTypeFunc)
        {
            _doc = doc;
            _categoryName = categoryName;
            _className = className;
            _getCategoryFunc = getCategoryFunc;
            _getTypeFunc = getTypeFunc;

            if (!string.IsNullOrEmpty(categoryName))
            {
                _targetCategory = _getCategoryFunc(categoryName);
            }

            if (!string.IsNullOrEmpty(className))
            {
                _targetType = _getTypeFunc(className);
            }
        }

        public bool AllowElement(Element elem)
        {
            if (elem == null) return false;

            // Check category filter
            if (_targetCategory.HasValue)
            {
                if (elem.Category == null) return false;
                if (elem.Category.BuiltInCategory != _targetCategory.Value) return false;
            }

            // Check class filter
            if (_targetType != null)
            {
                if (!_targetType.IsAssignableFrom(elem.GetType())) return false;
            }

            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            // For faces/edges, check the element
            Element elem = _doc.GetElement(reference.ElementId);
            return AllowElement(elem);
        }
    }
}
