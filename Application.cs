using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace RevitMCPAddin
{
    /// <summary>
    /// Main application class for the Revit MCP add-in
    /// Implements IExternalApplication for Revit integration
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Application : IExternalApplication
    {
        private static UIControlledApplication _uiApp;
        private static ExternalEvent _externalEvent;
        private static MCPCommandHandler _commandHandler;
        private NamedPipeServerStream _pipeServer;
        private bool _isRunning;

        /// <summary>
        /// Public accessor for UIControlledApplication for ribbon operations
        /// </summary>
        public static UIControlledApplication UIControlledApp => _uiApp;

        /// <summary>
        /// Called when Revit starts
        /// </summary>
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                _uiApp = application;
                
                // Create external event handler for executing commands
                _commandHandler = new MCPCommandHandler();
                _externalEvent = ExternalEvent.Create(_commandHandler);
                
                // Create ribbon panel
                CreateRibbonPanel(application);
                
                // Start pipe server for MCP communication
                Task.Run(() => StartPipeServer());
                
                TaskDialog.Show("MCP Server", "Revit MCP Server initialized successfully.");
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to initialize MCP Server: {ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Called when Revit shuts down
        /// </summary>
        public Result OnShutdown(UIControlledApplication application)
        {
            try
            {
                _isRunning = false;
                _pipeServer?.Dispose();
                _externalEvent?.Dispose();
                
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Error during shutdown: {ex.Message}");
                return Result.Failed;
            }
        }

        /// <summary>
        /// Create ribbon panel for MCP server controls
        /// </summary>
        private void CreateRibbonPanel(UIControlledApplication app)
        {
            // Create ribbon panel safely: ensure tab exists and reuse existing panel if present
            string tabName = "Add-Ins";
            string panelName = "MCP Server";

            try
            {
                // Create the tab if it doesn't exist (CreateRibbonTab throws if it already exists)
                try
                {
                    app.CreateRibbonTab(tabName);
                }
                catch (Autodesk.Revit.Exceptions.ArgumentException)
                {
                    // Tab already exists — ignore
                }
                catch
                {
                    // Ignore errors creating tab; some Revit builds may manage tabs differently
                }

                // Try to find an existing panel first
                RibbonPanel panel = null;
                try
                {
                    var panels = app.GetRibbonPanels(tabName);
                    panel = panels?.FirstOrDefault(p => p?.Name == panelName);
                }
                catch
                {
                    // Fall back to creating the panel if GetRibbonPanels is not available
                }

                if (panel == null)
                {
                    try
                    {
                        panel = app.CreateRibbonPanel(tabName, panelName);
                    }
                    catch
                    {
                        // Some Revit builds may not allow creating panels this way.
                        // Try the simpler overload that only takes a panel name.
                        try
                        {
                            panel = app.CreateRibbonPanel(panelName);
                        }
                        catch (Exception exInner)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to create panel with both overloads: {exInner.Message}");
                            panel = null;
                        }
                    }
                }

                // Add status button
                PushButtonData buttonData = new PushButtonData(
                    "MCPStatus",
                    "MCP Status",
                    typeof(Application).Assembly.Location,
                    typeof(MCPStatusCommand).FullName
                );

                PushButton button = panel.AddItem(buttonData) as PushButton;
                if (button != null)
                {
                    button.ToolTip = "Show MCP Server status";
                }
            }
            catch (Exception ex)
            {
                // Don't show a modal dialog during startup; log instead.
                System.Diagnostics.Debug.WriteLine($"Failed to create ribbon panel: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Start named pipe server for communication with Python MCP server
        /// </summary>
        private async Task StartPipeServer()
        {
            _isRunning = true;
            
            while (_isRunning)
            {
                NamedPipeServerStream pipeServer = null;
                try
                {
                    pipeServer = new NamedPipeServerStream(
                        "RevitMCP",
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous
                    );
                    
                    System.Diagnostics.Debug.WriteLine("Pipe server created, waiting for connection...");
                    await pipeServer.WaitForConnectionAsync();
                    System.Diagnostics.Debug.WriteLine("Client connected to pipe");
                    
                    // Handle client connection
                    await HandleClient(pipeServer);
                    System.Diagnostics.Debug.WriteLine("Client handling completed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Pipe server error: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                }
                finally
                {
                    pipeServer?.Dispose();
                }
                
                // Small delay to prevent tight loop
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Handle client requests
        /// </summary>
        private async Task HandleClient(NamedPipeServerStream pipe)
        {
            try
            {
                using (StreamReader reader = new StreamReader(pipe, Encoding.UTF8, false, 1024, true))
                using (StreamWriter writer = new StreamWriter(pipe, new UTF8Encoding(false), 1024, true))
                {
                    while (pipe.IsConnected)
                    {
                        string request = await reader.ReadLineAsync();
                        
                        if (string.IsNullOrEmpty(request))
                            break;
                        
                        // Process request
                        string response = ProcessRequest(request);
                        
                        // Send response
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Client handling error: {ex.Message}");
            }
        }

        /// <summary>
        /// Process incoming MCP request
        /// </summary>
        private string ProcessRequest(string requestJson)
        {
            try
            {
                var request = JsonConvert.DeserializeObject<MCPRequest>(requestJson);
                
                // Queue command for execution in Revit context
                _commandHandler.SetCommand(request);
                _externalEvent.Raise();
                
                // Wait for result (with timeout)
                var result = _commandHandler.WaitForResult(5000);
                
                return JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get active document
        /// Note: `UIControlledApplication` does not provide `ActiveUIDocument`.
        /// The active `Document` is available only from a `UIApplication` instance
        /// (for example inside an ExternalEvent handler). This helper returns null
        /// when a `UIApplication` is not available. Use `MCPCommandHandler.Execute(UIApplication)`
        /// to access the active document in Revit's UI context.
        /// </summary>
        public static Document GetActiveDocument()
        {
            return null;
        }
    }

    /// <summary>
    /// MCP request structure
    /// </summary>
    public class MCPRequest
    {
        public string Command { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }

    /// <summary>
    /// Status command for ribbon button
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class MCPStatusCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("MCP Server Status", "MCP Server is running and ready for connections.");
            return Result.Succeeded;
        }
    }
}
