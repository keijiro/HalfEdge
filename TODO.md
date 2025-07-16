# TODO

This document lists discrepancies between the current implementation and the design outlined in `Design.md`. These items focus on aligning the existing code with the design document without adding new features.

- **Rename `Mesh` class to `Mesh`**:
  - The core data structure is named `MeshData` in the implementation, but `Design.md` specifies it as `Mesh`. The class and the corresponding file (`MeshData.cs`) should be renamed to `Mesh.cs` to match the design.

- **Rename `NormalGenerationMode` enum to `NormalGenerationMode`**:
  - The enum used for controlling shading behavior in `ToUnityMesh` is currently named `NormalGenerationMode`. This should be renamed to `NormalGenerationMode` as defined in `Design.md`.

- **Adjust `ToUnityMesh` method signature**:
  - The current implementation has an overloaded `ToUnityMesh` method. This should be consolidated into a single method with a default parameter, as specified in `Design.md`: `ToUnityMesh(NormalGenerationMode mode = NormalGenerationMode.Smooth)`.

- **Replace `UnityEngine.Vector3` with `Unity.Mathematics.float3` in calculations**:
  - `Design.md` states that `Unity.Mathematics` types should be used for all math operations. The `ToUnityMesh` method currently uses `UnityEngine.Vector3` for intermediate calculations (e.g., `Vector3.Cross`). These should be replaced with their `Unity.Mathematics` equivalents (e.g., `math.cross`). The final assignment to `mesh.vertices` and `mesh.normals` will still require conversion to `Vector3[]`.

- **Rename Generator classes**:
  - `Box` should be renamed to `Box`.
  - `Cylinder` should be renamed to `Cylinder`.
  - `Sphere` should be renamed to `Sphere`.
  - `IndexedMesh` should be renamed to `IndexedMesh`.

- **Rename Modifier classes**:
  - `StretchMesh` should be renamed to `StretchMesh`.
  - `SmoothVertices` should be renamed to `SmoothVertices`.
  - `TwistMesh` should be renamed to `TwistMesh`.
  - `ExtrudeFaces` should be renamed to `ExtrudeFaces`.

- **Adjust `Sphere` constructor**:
  - The constructor is now `Sphere(float radius, int resolution)` as specified in `Design.md`.

- **Review Modifier class constructors**:
  - `SmoothVertices` has a `smoothingFactor` parameter not present in the `Design.md` signature for `SmoothVertices`.
  - `TwistMesh` has `center` and `falloffDistance` parameters not present in the `Design.md` signature for `TwistMesh`.
  - These extra parameters could be considered feature additions. For now, we will leave them, but they should be reviewed for consistency with the design goals.

- **Update Samples and Tests**:
  - After applying the class and method renames, all relevant sample scripts and tests must be updated to reflect these changes. This ensures that the project remains fully functional and that all tests pass.