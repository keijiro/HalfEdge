# HalfEdgeMesh2 Design Document

## Overview

HalfEdgeMesh2 is a new implementation of the half-edge mesh data structure designed for high performance with Unity's Burst Compiler. Unlike the original HalfEdgeMesh, this implementation prioritizes unmanaged types and Burst compatibility while maintaining a clean API.

## Design Goals

1. **Burst Compiler Compatibility**: All core operations can be compiled with Burst
2. **Unmanaged Types**: Use only blittable types in core data structures
3. **Method-level Burst**: Focus on Burst-compiled methods rather than Job System integration
4. **Performance**: Minimize GC allocations and maximize cache efficiency
5. **Simplicity**: Start with core functionality only

## Architecture

### Core Data Structures

All core structures are unmanaged structs using indices instead of references:

```csharp
public struct Vertex
{
    public float3 position;
    public int halfEdge; // Index to first outgoing half-edge (-1 if none)
}

public struct HalfEdge
{
    public int next;     // Index to next half-edge in face
    public int twin;     // Index to opposite half-edge (-1 if boundary)
    public int vertex;   // Index to origin vertex
    public int face;     // Index to face (-1 if boundary)
}

public struct Face
{
    public int halfEdge; // Index to one half-edge of this face
}
```

### Mesh Container

The mesh data is stored in native arrays for Burst compatibility:

```csharp
public struct MeshData : IDisposable
{
    public NativeArray<Vertex> vertices;
    public NativeArray<HalfEdge> halfEdges;
    public NativeArray<Face> faces;
    
    // Allocation info
    public int vertexCount;
    public int halfEdgeCount;
    public int faceCount;
}
```

### Builder Pattern

A builder class manages mesh construction with a managed API:

```csharp
public class MeshBuilder
{
    private List<Vertex> vertices;
    private List<HalfEdge> halfEdges;
    private List<Face> faces;
    
    public MeshData Build(Allocator allocator);
}
```

### Burst-Compiled Operations

Core operations are implemented as static methods with BurstCompile attribute:

```csharp
[BurstCompile]
public static class MeshOperations
{
    [BurstCompile]
    public static void ComputeNormals(ref MeshData mesh, NativeArray<float3> normals);
    
    [BurstCompile]
    public static void ComputeBounds(ref MeshData mesh, out Bounds bounds);
    
    [BurstCompile]
    public static bool ValidateMesh(ref MeshData mesh);
}
```

## Implementation Phases

### Phase 1: Core Implementation (Week 1-2)
- Basic data structures
- MeshData container with allocation/deallocation
- MeshBuilder for construction
- Basic validation

### Phase 2: Mesh Operations (Week 3)
- Normal calculation
- Bounds calculation
- Face area and perimeter
- Vertex valence

### Phase 3: Mesh Queries (Week 4)
- Vertex neighbors iteration
- Face vertices iteration
- Boundary detection
- Edge iteration

### Phase 4: Mesh Generation (Week 5)
- Box generator
- Sphere generator
- Plane generator

### Phase 5: Unity Integration (Week 6)
- Convert to/from Unity Mesh
- Gizmo visualization
- Inspector support

## Usage Example

```csharp
// Building a mesh
var builder = new MeshBuilder();
// Add vertices and faces...
var meshData = builder.Build(Allocator.Persistent);

// Using Burst-compiled operations
var normals = new NativeArray<float3>(meshData.vertexCount, Allocator.Temp);
MeshOperations.ComputeNormals(ref meshData, normals);

// Cleanup
meshData.Dispose();
normals.Dispose();
```

## Benefits

1. **Zero GC Pressure**: All core operations work with unmanaged memory
2. **Burst Performance**: Critical paths can be Burst-compiled
3. **Cache Friendly**: Contiguous memory layout
4. **Predictable Performance**: No hidden allocations or boxing

## Limitations

1. **Manual Memory Management**: Users must dispose NativeArrays
2. **No Direct Modification**: Mesh topology changes require rebuilding
3. **Index-based Access**: Less convenient than reference-based API
4. **Limited Dynamism**: Optimized for static meshes

## Migration Path

Existing HalfEdgeMesh users can gradually adopt HalfEdgeMesh2:
1. Use HalfEdgeMesh2 for performance-critical operations
2. Convert between formats as needed
3. Maintain both implementations during transition