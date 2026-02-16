# Contributing to Revit 2026 MCP Server

Thank you for your interest in contributing to the Revit 2026 MCP Server! This document provides guidelines for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/Revit-2026-MCP-Server.git`
3. Install dependencies: `npm install`
4. Build the project: `npm run build`

## Development Workflow

1. Create a new branch for your feature or bugfix: `git checkout -b feature/my-new-feature`
2. Make your changes
3. Build and test your changes: `npm run build`
4. Commit your changes with a descriptive message: `git commit -m "Add new feature"`
5. Push to your fork: `git push origin feature/my-new-feature`
6. Create a Pull Request

## Code Style

- Use TypeScript for all source code
- Follow existing code formatting conventions
- Use meaningful variable and function names
- Add comments for complex logic
- Keep functions focused and concise

## Testing

Before submitting a PR, please:
- Build the project without errors: `npm run build`
- Test the server manually to ensure it works correctly
- Verify that existing functionality hasn't broken

## Pull Request Guidelines

- Provide a clear description of the changes
- Reference any related issues
- Keep PRs focused on a single feature or fix
- Update documentation if needed

## Areas for Contribution

We welcome contributions in the following areas:

1. **Revit API Integration**: Implement real Revit API connections to replace simulated data
2. **Additional Tools**: Add more MCP tools for Revit operations
3. **Resources**: Expose more Revit project data through resources
4. **Prompts**: Create helpful prompts for common Revit workflows
5. **Documentation**: Improve setup guides, examples, and API documentation
6. **Testing**: Add automated tests for the server
7. **Error Handling**: Improve error messages and handling

## Questions?

If you have questions about contributing, please open an issue for discussion.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
