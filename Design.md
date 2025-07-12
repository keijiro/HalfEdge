# Half-Edge Mesh Library for Unity

## Project Overview

This project implements a half-edge-based mesh data structure and procedural
geometry library in C# for Unity. It is designed for real-time topology editing,
procedural mesh generation, and geometry modification. The library:

- Uses [`Unity.Mathematics`] for math types (`float3`, etc.)
- Targets **in-memory mesh editing**
- Generates/updates `UnityEngine.Mesh` for rendering

[`Unity.Mathematics`]: https://docs.unity3d.com/Packages/com.unity.mathematics@latest

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

### Math Dependency

- All math uses types from `Unity.Mathematics`
  - `float3`, `math.normalize`, `math.dot`, etc.
- No usage of `UnityEngine.Vector3`

## Phase 1 Scope

### 1. Half-Edge Topology Support

- Initialize mesh from indexed face list
- Traverse:
  - face boundary (`face → loop of half-edges`)
  - vertex neighborhood (`vertex → outgoing half-edges`)
- Operations:
  - Split edge
  - Extrude face
  - Triangulate polygonal face

### 2. Procedural Mesh Generators

| Class               | Description                          |
|---------------------|--------------------------------------|
| `BoxGenerator`      | Generates a subdivided box mesh      |
| `CylinderGenerator` | Generates a capped cylinder          |
| `SphereGenerator`   | Generates a UV sphere or icosphere   |
| `IndexedMeshBuilder`| Builds mesh from vertex/face arrays  |

### 3. Geometry Modifiers

| Class             | Description                                   |
|-------------------|-----------------------------------------------|
| `ExtrudeModifier` | Extrudes selected faces in normal direction   |
| `TwistModifier`   | Applies twist around a defined axis           |
| `ScaleModifier`   | Uniform or axis-aligned scale                 |
| `SmoothModifier`  | Laplacian smoothing on vertex positions       |

Modifiers are applied via:

```csharp
modifier.Apply(MeshData mesh);
```

### 4. Unity Mesh Generation

#### `MeshData.ToUnityMesh()`

Converts internal structure to a `UnityEngine.Mesh`.

- Triangulates polygonal faces (fan method for n-gons)
- Generates:
  - Vertex positions
  - Triangle indices
  - Optionally: normals, UVs, etc.

#### Example usage:

```csharp
Mesh unityMesh = meshData.ToUnityMesh();
GetComponent<MeshFilter>().mesh = unityMesh;
```

## Testing Strategy

### Framework

- **Unity Test Framework (NUnit)** in `Tests/Editor`
- Tested via PlayMode and EditMode tests

### Coverage

| Test Type        | Examples                                           |
|------------------|----------------------------------------------------|
| Topology         | Loop consistency, twin-next integrity              |
| Geometry         | Vertex counts, triangle counts                     |
| Modifier results | Check vertex displacements or mesh changes         |
| Export validation| Ensure `MeshData.ToUnityMesh()` outputs valid data |

#### Sample Test:

```csharp
[Test]
public void BoxGenerator_GeneratesExpectedVertexCount()
{
    var generator = new BoxGenerator(1f, 1f, 1f, 1);
    var mesh = generator.Generate();
    Assert.AreEqual(8, mesh.Vertices.Count);
}
```

## Suggested File Layout

```
/HalfEdgeMesh
  /Core
    MeshData.cs
    Vertex.cs
    HalfEdge.cs
    Edge.cs
    Face.cs
  /Generators
    BoxGenerator.cs
    CylinderGenerator.cs
    SphereGenerator.cs
    IndexedMeshBuilder.cs
  /Modifiers
    ExtrudeModifier.cs
    TwistModifier.cs
    ScaleModifier.cs
    SmoothModifier.cs
  /Tests
    MeshExportTests.cs
    MeshGenerationTests.cs
    ModifierTests.cs
```

## Design Principles

| Principle        | Description                                         |
|------------------|-----------------------------------------------------|
| **Unity-native** | Uses Unity.Mathematics, integrates with Unity Mesh  |
| **Modular**      | Each generator and modifier is isolated and testable|
| **Extensible**   | Designed for UVs, normals, attributes, selections   |
| **Serializable** | Prepared for export, scene storage, or editor tools |
