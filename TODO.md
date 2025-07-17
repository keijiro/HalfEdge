# TODO

This file tracks the remaining tasks to complete the Half-Edge Mesh library based on the original design document.

## Generators

- [x] `Plane(int widthSegments, int heightSegments, float size)`
- [x] `Icosphere(float radius, int subdivisions)`
- [x] `Cone(float radius, float height, int segments)`
- [x] `Torus(float majorRadius, float minorRadius, int majorSegments, int minorSegments)`
- [x] `Tetrahedron(float size)`
- [x] `Octahedron(float size)`
- [x] `Icosahedron(float size)`
- [x] `Dodecahedron(float size)` (simplified implementation)
- [x] `Extrusion(List<float3> profile, float height)`
- [x] `Lathe(List<float2> profile, int segments)`

## Modifiers

- [x] `ChamferVertices(Mesh mesh, float distance)` (basic implementation)
- [x] `ChamferEdges(Mesh mesh, float distance)` (simplified implementation)
- [x] `CreateLattice(Mesh mesh, float spacing)` (basic lattice structure)
- [x] `SplitFaces(Mesh mesh, float3 planeNormal, float3 planePoint)` (plane-based face splitting)
- [x] `SkewMesh(Mesh mesh, float angle, float3 direction)` (mesh skewing transformation)
- [x] `ExpandVertices(Mesh mesh, float distance)`

## Completed

All generators and modifiers from the original design document have been implemented and tested. The modifier implementations use simplified algorithms suitable for real-time applications:

- **ChamferEdges**: Creates chamfered edges by shrinking faces toward their centers
- **CreateLattice**: Generates lattice structures with cylindrical struts and spherical joints
- **SplitFaces**: Splits faces that intersect with a specified plane using polygon clipping
- **SkewMesh**: Applies skew transformations based on mesh bounds and primary axis
- **ChamferVertices & ExpandVertices**: Move vertices along averaged normals

The library now provides a comprehensive set of 13 generators and 6 modifiers with full test coverage.
