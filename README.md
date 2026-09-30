# README for CursorSwitcher

Small Windows WPF utility for switching between two cursor styles using `.ani` files and a global hotkey.

Features:
- Select one custom cursor for Style 1 and one for Style 2.
- Use the standard Windows 11 cursor as Style 2 by default.
- Mount a custom `.ani` file or import a folder containing `.ani` files.
- Bind a hotkey that toggles between the custom cursor and the default Windows cursor.
- Run in the system tray while remaining hidden in the background.
- Save the configuration next to the executable.

Important:
- The app swaps the main pointer cursor (`IDC_ARROW`) to the chosen `.ani` file.
- This is best suited to animated `.ani` cursor files designed for the default arrow pointer.

Build:
1. Open the solution in Visual Studio 2026.
2. Restore NuGet packages.
3. Build the project in Release x64 or Any CPU.
4. Run the generated EXE.

Requirements:
- Windows 10/11
- .NET 8 Desktop runtime
- WPF support in Visual Studio

This project is intentionally simple and designed for fast testing and iteration.
