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
| `Mesh`     | Main container for vertices, faces, half-edges, and edges.       |
| `Vertex`   | Stores a `float3` position and a reference to one outgoing       |
|            | half-edge                                                        |
| `HalfEdge` | Represents a directed edge. Links to `next`, `twin`, `face`, and |
|            | `origin vertex`                                                  |
| `Face`     | Stores a reference to a loop of half-edges defining a polygon    |

### Generators

Generators are classes that create new `Mesh` objects from scratch. They are
used to generate procedural geometry such as boxes, cylinders, and spheres. All
generators reside in the `Generators` sub-namespace.

The following generators are included:

#### Basic Primitives

- `BoxGenerator(float width, float height, float depth, int subdivisions)`
- `PlaneGenerator(int widthSegments, int heightSegments, float size)`
- `SphereGenerator(float radius, int resolution)`
- `IcosphereGenerator(float radius, int subdivisions)`
- `CylinderGenerator(float radius, float height, int segments)`
- `ConeGenerator(float radius, float height, int segments)`
- `TorusGenerator(float majorRadius, float minorRadius, int majorSegments, int minorSegments)`
- `TetrahedronGenerator(float size)`
- `OctahedronGenerator(float size)`
- `IcosahedronGenerator(float size)`
- `DodecahedronGenerator(float size)`

#### Revolved and Extruded Shapes

- `ExtrusionGenerator(List<float3> profile, float height)`
- `LatheGenerator(List<float2> profile, int segments)`

These generators allow for flexible shape creation and parameterized geometry
construction.

### Modifiers

Modifiers are classes that take a `Mesh` object as input and modify it in
some way. They are used to apply transformations to existing meshes, such as
extrusion, twisting, and scaling. All modifiers reside in the `Modifiers`
sub-namespace.

The following modifiers are included:

- `ChamferVertices(Mesh mesh, float distance)`
- `ChamferEdges(Mesh mesh, float distance)`
- `ExtrudeFaces(Mesh mesh, float distance)`
- `CreateLattice(Mesh mesh, float spacing)`
- `SplitFaces(Mesh mesh, float3 planeNormal, float3 planePoint)`
- `SkewMesh(Mesh mesh, float angle, float3 direction)`
- `SmoothVertices(Mesh mesh, int iterations)`
- `StretchMesh(Mesh mesh, float3 scale)`
- `TwistMesh(Mesh mesh, float angle, float3 axis)`
- `ExpandVertices(Mesh mesh, float distance)`

Each modifier provides a clear and consistent API for applying a specific type
of geometric or topological operation.

### Math Dependency

- All math uses types from `Unity.Mathematics`
  - `float3`, `math.normalize`, `math.dot`, etc.
- No usage of `UnityEngine.Vector3`

### Mesh API

The `Mesh` class exposes the following core methods:

- `Translate(float3 offset)`
- `Rotate(float angle, float3 axis, float3 center)`
- `Scale(float3 factors, float3 center)`
- `Transform(float4x4 matrix)`
- `ToUnityMesh(NormalGenerationMode mode = NormalGenerationMode.Smooth)`
- `UpdateUnityMesh(Mesh unityMesh, NormalGenerationMode mode = NormalGenerationMode.Smooth)`

These methods support full transformation control and Unity Mesh generation.
The normal generation mode determines whether shared vertex normals (smooth shading)
or face normals (flat shading with duplicated vertices) are used.

#### NormalGenerationMode enum

```csharp
public enum NormalGenerationMode
{
    Smooth, // Shared-vertex normals (Gouraud shading)
    Flat    // Per-face normals (duplicated vertices)
}
```

This allows clear and extensible specification of shading behavior when converting
to Unity meshes.

### Testing

All core components, generators, and modifiers should be covered by automated
tests using the Unity Test Framework. Each feature must include corresponding
tests to ensure correctness, robustness, and future refactor safety. Testing
against known inputs and mesh topology invariants is strongly recommended.