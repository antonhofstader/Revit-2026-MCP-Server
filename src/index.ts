#!/usr/bin/env node

import { Server } from "@modelcontextprotocol/sdk/server/index.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import {
  CallToolRequestSchema,
  ListToolsRequestSchema,
  ListResourcesRequestSchema,
  ReadResourceRequestSchema,
  ListPromptsRequestSchema,
  GetPromptRequestSchema,
} from "@modelcontextprotocol/sdk/types.js";

// Simulated Revit API connection
// In production, this would connect to the actual Revit API via a bridge
class RevitAPIBridge {
  async getElements(category?: string): Promise<any[]> {
    // Simulate getting elements from Revit
    return [
      { id: "elem-001", name: "Wall-001", category: "Walls", level: "Level 1" },
      { id: "elem-002", name: "Door-001", category: "Doors", level: "Level 1" },
      { id: "elem-003", name: "Window-001", category: "Windows", level: "Level 1" },
    ].filter(e => !category || e.category === category);
  }

  async getElementProperties(elementId: string): Promise<any> {
    // Simulate getting element properties
    return {
      id: elementId,
      name: `Element-${elementId}`,
      category: "Walls",
      parameters: {
        "Base Constraint": "Level 1",
        "Top Constraint": "Level 2",
        "Height": "4000mm",
        "Width": "200mm",
      },
    };
  }

  async createElement(category: string, properties: any): Promise<string> {
    // Simulate creating an element
    const newId = `elem-${Date.now()}`;
    return newId;
  }

  async modifyElement(elementId: string, properties: any): Promise<boolean> {
    // Simulate modifying an element
    return true;
  }

  async getProjectInfo(): Promise<any> {
    // Simulate getting project information
    return {
      name: "Sample Project",
      author: "Revit User",
      address: "123 Main Street",
      clientName: "Sample Client",
      projectNumber: "2026-001",
    };
  }

  async getLevels(): Promise<any[]> {
    // Simulate getting levels
    return [
      { id: "level-001", name: "Level 1", elevation: 0 },
      { id: "level-002", name: "Level 2", elevation: 4000 },
      { id: "level-003", name: "Level 3", elevation: 8000 },
    ];
  }

  async getViews(): Promise<any[]> {
    // Simulate getting views
    return [
      { id: "view-001", name: "Floor Plan - Level 1", viewType: "FloorPlan" },
      { id: "view-002", name: "3D View", viewType: "ThreeD" },
      { id: "view-003", name: "Section A", viewType: "Section" },
    ];
  }
}

const revitBridge = new RevitAPIBridge();

// Create server instance
const server = new Server(
  {
    name: "revit-2026-mcp-server",
    version: "1.0.0",
  },
  {
    capabilities: {
      tools: {},
      resources: {},
      prompts: {},
    },
  }
);

// List available tools
server.setRequestHandler(ListToolsRequestSchema, async () => {
  return {
    tools: [
      {
        name: "get_elements",
        description: "Get elements from the Revit project, optionally filtered by category",
        inputSchema: {
          type: "object",
          properties: {
            category: {
              type: "string",
              description: "Optional category to filter elements (e.g., 'Walls', 'Doors', 'Windows')",
            },
          },
        },
      },
      {
        name: "get_element_properties",
        description: "Get detailed properties of a specific element by its ID",
        inputSchema: {
          type: "object",
          properties: {
            elementId: {
              type: "string",
              description: "The unique identifier of the element",
            },
          },
          required: ["elementId"],
        },
      },
      {
        name: "create_element",
        description: "Create a new element in the Revit project",
        inputSchema: {
          type: "object",
          properties: {
            category: {
              type: "string",
              description: "The category of the element to create (e.g., 'Walls', 'Doors')",
            },
            properties: {
              type: "object",
              description: "Properties for the new element",
            },
          },
          required: ["category", "properties"],
        },
      },
      {
        name: "modify_element",
        description: "Modify properties of an existing element",
        inputSchema: {
          type: "object",
          properties: {
            elementId: {
              type: "string",
              description: "The unique identifier of the element to modify",
            },
            properties: {
              type: "object",
              description: "Properties to update on the element",
            },
          },
          required: ["elementId", "properties"],
        },
      },
      {
        name: "get_levels",
        description: "Get all levels in the Revit project",
        inputSchema: {
          type: "object",
          properties: {},
        },
      },
      {
        name: "get_views",
        description: "Get all views in the Revit project",
        inputSchema: {
          type: "object",
          properties: {},
        },
      },
    ],
  };
});

// Handle tool calls
server.setRequestHandler(CallToolRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  try {
    switch (name) {
      case "get_elements": {
        const elements = await revitBridge.getElements(args?.category as string);
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(elements, null, 2),
            },
          ],
        };
      }

      case "get_element_properties": {
        if (!args?.elementId) {
          throw new Error("elementId is required");
        }
        const properties = await revitBridge.getElementProperties(args.elementId as string);
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(properties, null, 2),
            },
          ],
        };
      }

      case "create_element": {
        if (!args?.category || !args?.properties) {
          throw new Error("category and properties are required");
        }
        const elementId = await revitBridge.createElement(
          args.category as string,
          args.properties
        );
        return {
          content: [
            {
              type: "text",
              text: `Successfully created element with ID: ${elementId}`,
            },
          ],
        };
      }

      case "modify_element": {
        if (!args?.elementId || !args?.properties) {
          throw new Error("elementId and properties are required");
        }
        await revitBridge.modifyElement(args.elementId as string, args.properties);
        return {
          content: [
            {
              type: "text",
              text: `Successfully modified element ${args.elementId}`,
            },
          ],
        };
      }

      case "get_levels": {
        const levels = await revitBridge.getLevels();
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(levels, null, 2),
            },
          ],
        };
      }

      case "get_views": {
        const views = await revitBridge.getViews();
        return {
          content: [
            {
              type: "text",
              text: JSON.stringify(views, null, 2),
            },
          ],
        };
      }

      default:
        throw new Error(`Unknown tool: ${name}`);
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    return {
      content: [
        {
          type: "text",
          text: `Error: ${errorMessage}`,
        },
      ],
      isError: true,
    };
  }
});

// List available resources
server.setRequestHandler(ListResourcesRequestSchema, async () => {
  return {
    resources: [
      {
        uri: "revit://project/info",
        name: "Project Information",
        description: "Basic information about the current Revit project",
        mimeType: "application/json",
      },
      {
        uri: "revit://project/elements",
        name: "All Elements",
        description: "List of all elements in the project",
        mimeType: "application/json",
      },
      {
        uri: "revit://project/levels",
        name: "Project Levels",
        description: "All levels in the project",
        mimeType: "application/json",
      },
      {
        uri: "revit://project/views",
        name: "Project Views",
        description: "All views in the project",
        mimeType: "application/json",
      },
    ],
  };
});

// Read resource content
server.setRequestHandler(ReadResourceRequestSchema, async (request) => {
  const { uri } = request.params;

  try {
    switch (uri) {
      case "revit://project/info": {
        const info = await revitBridge.getProjectInfo();
        return {
          contents: [
            {
              uri,
              mimeType: "application/json",
              text: JSON.stringify(info, null, 2),
            },
          ],
        };
      }

      case "revit://project/elements": {
        const elements = await revitBridge.getElements();
        return {
          contents: [
            {
              uri,
              mimeType: "application/json",
              text: JSON.stringify(elements, null, 2),
            },
          ],
        };
      }

      case "revit://project/levels": {
        const levels = await revitBridge.getLevels();
        return {
          contents: [
            {
              uri,
              mimeType: "application/json",
              text: JSON.stringify(levels, null, 2),
            },
          ],
        };
      }

      case "revit://project/views": {
        const views = await revitBridge.getViews();
        return {
          contents: [
            {
              uri,
              mimeType: "application/json",
              text: JSON.stringify(views, null, 2),
            },
          ],
        };
      }

      default:
        throw new Error(`Unknown resource: ${uri}`);
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    throw new Error(`Failed to read resource ${uri}: ${errorMessage}`);
  }
});

// List available prompts
server.setRequestHandler(ListPromptsRequestSchema, async () => {
  return {
    prompts: [
      {
        name: "revit_project_overview",
        description: "Get a comprehensive overview of the Revit project",
      },
      {
        name: "element_analysis",
        description: "Analyze specific elements in the project",
        arguments: [
          {
            name: "category",
            description: "Element category to analyze (e.g., 'Walls', 'Doors')",
            required: true,
          },
        ],
      },
      {
        name: "create_wall",
        description: "Guide for creating a new wall in the project",
        arguments: [
          {
            name: "level",
            description: "Level where the wall should be created",
            required: true,
          },
        ],
      },
    ],
  };
});

// Get prompt content
server.setRequestHandler(GetPromptRequestSchema, async (request) => {
  const { name, arguments: args } = request.params;

  switch (name) {
    case "revit_project_overview":
      return {
        messages: [
          {
            role: "user",
            content: {
              type: "text",
              text: "Please provide a comprehensive overview of the current Revit project, including project information, number of elements by category, levels, and available views.",
            },
          },
        ],
      };

    case "element_analysis": {
      const category = args?.category || "all";
      return {
        messages: [
          {
            role: "user",
            content: {
              type: "text",
              text: `Please analyze the ${category} elements in the Revit project. Provide statistics, common properties, and any notable patterns or issues.`,
            },
          },
        ],
      };
    }

    case "create_wall": {
      const level = args?.level || "Level 1";
      return {
        messages: [
          {
            role: "user",
            content: {
              type: "text",
              text: `Guide me through creating a new wall on ${level}. What properties should I set and what are the best practices?`,
            },
          },
        ],
      };
    }

    default:
      throw new Error(`Unknown prompt: ${name}`);
  }
});

// Start the server
async function main() {
  const transport = new StdioServerTransport();
  await server.connect(transport);
  console.error("Revit 2026 MCP Server running on stdio");
}

main().catch((error) => {
  console.error("Fatal error in main():", error);
  process.exit(1);
});
