#!/usr/bin/env python3
"""
Test script to verify Revit MCP pipe connection
"""

import win32pipe
import win32file
import pywintypes
import json
import time

def test_pipe_connection():
    """Test connection to Revit MCP named pipe"""
    pipe_name = r'\\.\pipe\RevitMCP'

    print("Testing connection to Revit MCP pipe...")

    try:
        # Try to connect to the pipe
        pipe_handle = win32file.CreateFile(
            pipe_name,
            win32file.GENERIC_READ | win32file.GENERIC_WRITE,
            0, None,
            win32file.OPEN_EXISTING,
            0, None
        )

        print("✅ Successfully connected to Revit MCP pipe")

        # Test a simple command
        test_command = {
            "Command": "get_project_info",
            "Parameters": {}
        }

        # Send command
        command_json = json.dumps(test_command) + "\n"
        win32file.WriteFile(pipe_handle, command_json.encode('utf-8'))
        print(f"Sent command: {command_json.strip()}")

        # Read response
        response_data = b""
        start_time = time.time()

        while time.time() - start_time < 5:  # 5 second timeout
            try:
                hr, data = win32file.ReadFile(pipe_handle, 4096)
                response_data += data
                if b'\n' in response_data:
                    break
            except pywintypes.error:
                time.sleep(0.1)  # Wait a bit
                continue

        if response_data:
            response_str = response_data.decode('utf-8').strip()
            print(f"Received response: {response_str}")

            try:
                response = json.loads(response_str)
                print("✅ Valid JSON response received")
                return True
            except json.JSONDecodeError as e:
                print(f"❌ Invalid JSON response: {e}")
                return False
        else:
            print("❌ No response received within timeout")
            return False

    except pywintypes.error as e:
        print(f"❌ Failed to connect to pipe: {e}")
        return False
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return False
    finally:
        try:
            if 'pipe_handle' in locals():
                win32file.CloseHandle(pipe_handle)
        except:
            pass

if __name__ == "__main__":
    success = test_pipe_connection()
    if success:
        print("\n🎉 Pipe connection test PASSED - Revit MCP is working!")
    else:
        print("\n❌ Pipe connection test FAILED - Check Revit and add-in status")