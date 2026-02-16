#!/usr/bin/env python3
"""
Setup and installation script for Revit MCP Server
"""

import os
import sys
import shutil
import subprocess
from pathlib import Path


def check_python_version():
    """Verify Python version is 3.10 or higher"""
    if sys.version_info < (3, 10):
        print("Error: Python 3.10 or higher is required")
        print(f"Current version: {sys.version}")
        return False
    print(f"✓ Python version: {sys.version}")
    return True


def check_windows():
    """Verify running on Windows"""
    if sys.platform != "win32":
        print("Warning: This server requires Windows for Revit API access")
        return False
    print("✓ Running on Windows")
    return True


def create_virtual_environment():
    """Create Python virtual environment"""
    print("\nCreating virtual environment...")
    try:
        subprocess.run([sys.executable, "-m", "venv", "venv"], check=True)
        print("✓ Virtual environment created")
        return True
    except subprocess.CalledProcessError as e:
        print(f"Error creating virtual environment: {e}")
        return False


def install_dependencies():
    """Install Python dependencies"""
    print("\nInstalling dependencies...")
    
    # Determine pip path based on platform
    if sys.platform == "win32":
        pip_path = Path("venv/Scripts/pip.exe")
    else:
        pip_path = Path("venv/bin/pip")
    
    if not pip_path.exists():
        print("Error: pip not found in virtual environment")
        return False
    
    try:
        # Upgrade pip
        subprocess.run([str(pip_path), "install", "--upgrade", "pip"], check=True)
        
        # Install package in development mode
        subprocess.run([str(pip_path), "install", "-e", "."], check=True)
        
        # Install dev dependencies
        subprocess.run([str(pip_path), "install", "-e", ".[dev]"], check=True)
        
        print("✓ Dependencies installed")
        return True
    except subprocess.CalledProcessError as e:
        print(f"Error installing dependencies: {e}")
        return False


def copy_revit_addin():
    """Copy Revit add-in files to Revit addins folder"""
    print("\nInstalling Revit add-in...")
    
    # Get Revit addins path
    appdata = os.getenv("APPDATA")
    if not appdata:
        print("Error: APPDATA environment variable not found")
        return False
    
    revit_addins_path = Path(appdata) / "Autodesk" / "Revit" / "Addins" / "2026"
    
    if not revit_addins_path.exists():
        print(f"Creating Revit addins directory: {revit_addins_path}")
        revit_addins_path.mkdir(parents=True, exist_ok=True)
    
    # Copy .addin file
    addin_file = Path("RevitMCPAddin.addin")
    if addin_file.exists():
        shutil.copy(addin_file, revit_addins_path)
        print(f"✓ Copied {addin_file.name} to {revit_addins_path}")
    else:
        print(f"Warning: {addin_file} not found")
    
    print("\nNote: You will need to build the C# project to generate the DLL")
    print("Instructions:")
    print("1. Open RevitMCPAddin.csproj in Visual Studio")
    print("2. Build the solution (F7)")
    print("3. The DLL will be automatically copied to the Revit addins folder")
    
    return True


def create_config_example():
    """Create example MCP configuration file"""
    print("\nCreating example configuration...")
    
    config = {
        "mcpServers": {
            "revit": {
                "command": "python",
                "args": [
                    str(Path.cwd() / "revit_mcp_server.py")
                ]
            }
        }
    }
    
    import json
    
    config_path = Path("claude_desktop_config.example.json")
    with open(config_path, "w") as f:
        json.dump(config, f, indent=2)
    
    print(f"✓ Created {config_path}")
    print("\nTo use with Claude Desktop:")
    print(f"1. Copy the configuration from {config_path}")
    print("2. Edit %APPDATA%\\Claude\\claude_desktop_config.json")
    print("3. Add the configuration to the 'mcpServers' section")
    
    return True


def main():
    """Main setup function"""
    print("=" * 60)
    print("Revit MCP Server - Setup")
    print("=" * 60)
    
    # Check prerequisites
    if not check_python_version():
        return 1
    
    if not check_windows():
        response = input("Continue anyway? (y/n): ")
        if response.lower() != 'y':
            return 1
    
    # Setup steps
    steps = [
        ("Creating virtual environment", create_virtual_environment),
        ("Installing dependencies", install_dependencies),
        ("Installing Revit add-in", copy_revit_addin),
        ("Creating configuration example", create_config_example),
    ]
    
    for description, func in steps:
        if not func():
            print(f"\n✗ Failed: {description}")
            return 1
    
    print("\n" + "=" * 60)
    print("Setup completed successfully!")
    print("=" * 60)
    print("\nNext steps:")
    print("1. Build the C# Revit add-in using Visual Studio")
    print("2. Start Revit 2026")
    print("3. Configure Claude Desktop with the MCP server")
    print("4. Run the server: venv\\Scripts\\python revit_mcp_server.py")
    print("\nFor more information, see README.md")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())
