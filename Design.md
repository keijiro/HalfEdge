# Half-Edge Mesh Library for Unity

## Project Overview

This project implements a half-edge-based mesh data structure and procedural
geometry library in C# for Unity. It is designed for real-time topology editing,
procedural mesh generation, and geometry modification. The library:

- Uses [`Unity.Mathematics`] for math types (`float3`, etc.)
- Targets **in-memory mesh editing**
- Generates/updates `UnityEngine.Mesh` for rendering

[`Unity.Mathematics`]: https://docs.unity3d.com/Packages/com.unity.mathematics@latest

## Design Principles

| Principle        | Description                                         |
|------------------|-----------------------------------------------------|
| **Unity-native** | Uses Unity.Mathematics, integrates with Unity Mesh  |
| **Modular**      | Each generator and modifier is isolated and testable|
| **Extensible**   | Designed for UVs, normals, attributes, selections   |
| **Serializable** | Prepared for export, scene storage, or editor tools |

## Core Architecture

### Data Structures

| Class      | Description                                                      |
|------------|------------------------------------------------------------------|
| `MeshData` | Main container for vertices, faces, half-edges, and edges.       |
|            | Provides mesh editing and export methods                         |
| `Vertex`   | Stores a `float3` position and a reference to one outgoing       |
|            | half-edge                                                        |
| `HalfEdge` | Represents a directed edge. Links to `next`, `twin`, `face`, and |
|            | `origin vertex`                                                  |
| `Face`     | Stores a reference to a loop of half-edges defining a polygon    |

### Generators

Generators are classes that create new `MeshData` objects from scratch. They are
used to generate procedural geometry such as boxes, cylinders, and spheres.

### Modifiers

Modifiers are classes that take a `MeshData` object as input and modify it in
some way. They are used to apply transformations to existing meshes, such as
extrusion, twisting, and scaling.

### Math Dependency

- All math uses types from `Unity.Mathematics`
  - `float3`, `math.normalize`, `math.dot`, etc.
- No usage of `UnityEngine.Vector3`