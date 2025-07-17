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
- [ ] `ChamferEdges(Mesh mesh, float distance)`
- [ ] `CreateLattice(Mesh mesh, float spacing)`
- [ ] `SplitFaces(Mesh mesh, float3 planeNormal, float3 planePoint)`
- [ ] `SkewMesh(Mesh mesh, float angle, float3 direction)`
- [x] `ExpandVertices(Mesh mesh, float distance)`

## Completed

All major generators have been implemented and tested. Basic modifier implementations for ChamferVertices and ExpandVertices are complete. The remaining modifiers (ChamferEdges, CreateLattice, SplitFaces, SkewMesh) require more complex half-edge mesh manipulation and can be implemented as future enhancements.
