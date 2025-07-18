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

- `Box(float width, float height, float depth, int subdivisions)`
- `Plane(int widthSegments, int heightSegments, float size)`
- `Sphere(float radius, int resolution)`
- `Icosphere(float radius, int subdivisions)`
- `Cylinder(float radius, float height, int segments)`
- `Cone(float radius, float height, int segments)`
- `Torus(float majorRadius, float minorRadius, int majorSegments, int minorSegments)`
- `Tetrahedron(float size)`
- `Octahedron(float size)`
- `Dodecahedron(float size)`

#### Revolved and Extruded Shapes

- `Extrusion(List<float3> profile, float height)`
- `Lathe(List<float2> profile, int segments)`

#### From Existing Data

- `IndexedMesh(float3[] vertices, int[][] faces)`

These generators allow for flexible shape creation and parameterized geometry
construction.

### Modifiers

Modifiers are classes that take parameters in their constructor and modify
a `Mesh` object in-place through their `Apply(Mesh mesh)` method. They are
used to apply transformations to existing meshes, such as extrusion, twisting,
and scaling. All modifiers reside in the `Modifiers` sub-namespace.

The following modifiers are included:

#### Constructor + Apply Pattern (In-place Modifications)
- `ExtrudeFaces(float distance, bool useNormalDirection = true)` - Extrudes faces along normals
- `SmoothVertices(float smoothingFactor = 0.5f, int iterations = 1)` - Smooths vertex positions
- `StretchMesh(float3 scale, float3 center = default)` - Scales mesh around center point
- `TwistMesh(float3 axis, float3 center, float angle)` - Twists mesh around axis
- `SkewMesh(float angle, float3 direction)` - Applies skew transformation
- `ExpandVertices(float distance)` - Moves vertices outward along their normals

#### Static Methods (Complex Topology Changes)
- `ChamferVertices.Apply(Mesh mesh, float distance)` - Replaces vertices with chamfered corners
- `ChamferEdges.Apply(Mesh mesh, float distance)` - Replaces edges with chamfered faces
- `SplitFaces.Apply(Mesh mesh, float3 planeNormal, float3 planePoint)` - Splits faces with plane

**Usage Pattern:**
```csharp
// Constructor + Apply pattern (in-place modification)
new ExtrudeFaces(0.1f).Apply(mesh);
new SmoothVertices(0.5f, 3).Apply(mesh);

// Static method pattern (returns new mesh)
mesh = ChamferVertices.Apply(mesh, 0.1f);
mesh = ChamferEdges.Apply(mesh, 0.1f);
mesh = CreateLattice.Apply(mesh, 0.2f);
```

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