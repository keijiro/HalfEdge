# TODO

This document lists discrepancies between the current implementation and the design outlined in `Design.md`. These items focus on aligning the existing code with the design document without adding new features.

- **Rename `MeshData` class to `Mesh`**:
  - The core data structure is named `MeshData` in the implementation, but `Design.md` specifies it as `Mesh`. The class and the corresponding file (`MeshData.cs`) should be renamed to `Mesh.cs` to match the design.

- **Rename `ShadingMode` enum to `NormalGenerationMode`**:
  - The enum used for controlling shading behavior in `ToUnityMesh` is currently named `ShadingMode`. This should be renamed to `NormalGenerationMode` as defined in `Design.md`.

- **Adjust `ToUnityMesh` method signature**:
  - The current implementation has an overloaded `ToUnityMesh` method. This should be consolidated into a single method with a default parameter, as specified in `Design.md`: `ToUnityMesh(NormalGenerationMode mode = NormalGenerationMode.Smooth)`.

- **Replace `UnityEngine.Vector3` with `Unity.Mathematics.float3` in calculations**:
  - `Design.md` states that `Unity.Mathematics` types should be used for all math operations. The `ToUnityMesh` method currently uses `UnityEngine.Vector3` for intermediate calculations (e.g., `Vector3.Cross`). These should be replaced with their `Unity.Mathematics` equivalents (e.g., `math.cross`). The final assignment to `mesh.vertices` and `mesh.normals` will still require conversion to `Vector3[]`.

- **Rename Generator classes**:
  - `BoxGenerator` should be renamed to `Box`.
  - `CylinderGenerator` should be renamed to `Cylinder`.
  - `SphereGenerator` should be renamed to `Sphere`.
  - `IndexedMeshBuilder` should be renamed to `IndexedMesh`.

- **Rename Modifier classes**:
  - `ScaleModifier` should be renamed to `StretchMesh`.
  - `SmoothModifier` should be renamed to `SmoothVertices`.
  - `TwistModifier` should be renamed to `TwistMesh`.
  - `ExtrudeModifier` should be renamed to `ExtrudeFaces`.

- **Adjust `Sphere` constructor**:
  - The constructor should be `Sphere(float radius, int resolution)` as specified in `Design.md`. The current implementation with `SphereType` should be simplified, and a separate `Icosphere` class should be created later as a new feature. The `subdivisions` parameter should be renamed to `resolution`.

- **Review Modifier class constructors**:
  - `SmoothModifier` has a `smoothingFactor` parameter not present in the `Design.md` signature for `SmoothVertices`.
  - `TwistModifier` has `center` and `falloffDistance` parameters not present in the `Design.md` signature for `TwistMesh`.
  - These extra parameters could be considered feature additions. For now, we will leave them, but they should be reviewed for consistency with the design goals.

- **Update Samples and Tests**:
  - After applying the class and method renames, all relevant sample scripts and tests must be updated to reflect these changes. This ensures that the project remains fully functional and that all tests pass.