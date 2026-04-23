# P5 Designer

An interactive design tool built with Unity that converts visual designs into p5.js code.
It features a full undo/redo system, intuitive camera controls, and a modular architecture designed for extensibility.

---
## Why I Built This

I originally built this project to support my *Introduction to Programming 1* module at the University of London.
What started as a simple tool to help with coursework gradually evolved into a more complete and polished application.

I decided to expand it into a fully usable design tool, focusing on clean architecture, usability, and extensibility.

---

## Demo

Showcase GIF:
![Showcase](media/Showcase.gif)

Eiffel Tower designed using tool:
![Eiffel Tower](media/Eiffel-Tower-Design.png)

> The tool is included in the `Builds/` folder.
> If you just want to use the tool (not modify it), download the contents of that folder and run the executable.

---

## Features

* Main menu for managing saved designs
* Auto-save system
* Built-in color picker
* Full shape customization (stroke, fill, etc.)
* Layer system (reorder, duplicate, delete)
* Import system to reuse designs across projects
* Full undo/redo system
* Precise control over shape scaling, vertices, and positioning
* Intuitive camera (zooming and panning)
* One-click export to p5.js (copied directly to clipboard)
* Clean and easy-to-use UI
* 10x10 pixel grid for structured design

---

## Architecture

The project is built around a modular, action-based architecture:

* **ActionLogger**
  Each user interaction is encapsulated as an object with its own undo and redo logic.

* **CanvasManager**
  A central static manager responsible for tracking and managing all design elements.

* **DesignEditor**
  Handles rendering and drawing logic for the design scene.

* **UIManager**
  Controls all UI elements and ensures they stay in sync with the system state.

* **SaveSystem**
  A static system responsible for saving and loading designs.

* **CameraMovement**
  Handles zooming and panning independently from rendering logic.

---

## Getting Started

### Option 1: Use the Tool

1. Navigate to the `Builds/` folder
2. Download its contents
3. Run the executable

---

### Option 2: Edit the Project

1. Clone the repository
2. Open the project using Unity `2022.3.62f2` (use other versions at your own risk)
3. Start the project from the editor

---

## Controls

### Navigation

* **Mouse Scroll Wheel** – Zoom in/out
* **Right Mouse Button Drag** – Pan the canvas

### Editing

* **Left Click in Layers Tab** - Select layer
* **Left Click Drag** – Move or modify points in layers
* **Delete** – Remove selected layer

### Shortcuts

* **Ctrl + Z** – Undo
* **Ctrl + Shift + Z** – Redo


The interface is designed to be intuitive, so most interactions are directly accessible through the UI.

---

## Project Structure

* `Assets/` – Core project files
* `ProjectSettings/` – Unity configuration
* `Packages/` – Project dependencies
* `Builds/` – Compiled application for end users

---

## Built With

* Unity `2022.3.62f2`
* C#
* p5.js (export target)

---

## Future Improvements

* Additional shapes (e.g. stars)
* Background color customization
* Light mode UI

---

## Contributing

Feel free to fork the project and submit pull requests.

---

## License

This project is licensed under the MIT License.
You are free to use, modify, and build upon it.
