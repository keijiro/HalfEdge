using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;

namespace HalfEdgeMesh2.Generators
{
    public static class Box
    {
        static readonly ProfilerMarker s_CreateVertexGridMarker = new ProfilerMarker("Box.CreateVertexGrid");
        static readonly ProfilerMarker s_CreateFacesMarker = new ProfilerMarker("Box.CreateFaces");
        static readonly ProfilerMarker s_BuildMeshMarker = new ProfilerMarker("Box.BuildMesh");
        public static MeshData Generate(float3 size, int3 segments, Allocator allocator)
        {
            var clampedSegments = math.max(segments, 1);

            // Calculate estimated capacity more accurately
            // For a box mesh, we need to estimate the number of unique edges
            // Each face contributes edges, but adjacent faces share edges
            var vertexCount = (clampedSegments.x + 1) * (clampedSegments.y + 1) * (clampedSegments.z + 1);

            // Estimate unique edges: internal edges + boundary edges
            var internalEdgesX = clampedSegments.x * (clampedSegments.y + 1) * (clampedSegments.z + 1);
            var internalEdgesY = (clampedSegments.x + 1) * clampedSegments.y * (clampedSegments.z + 1);
            var internalEdgesZ = (clampedSegments.x + 1) * (clampedSegments.y + 1) * clampedSegments.z;
            var uniqueEdges = internalEdgesX + internalEdgesY + internalEdgesZ;

            var builder = new MeshBuilder(Allocator.Temp, uniqueEdges);

            var hw = size.x * 0.5f;
            var hh = size.y * 0.5f;
            var hd = size.z * 0.5f;

            // Create vertices grid for the entire box
            VertexGrid vertexGrid;
            using (s_CreateVertexGridMarker.Auto())
            {
                vertexGrid = CreateVertexGrid(ref builder, size, clampedSegments);
            }

            // Create faces using shared vertices
            using (s_CreateFacesMarker.Auto())
            {
                CreateFrontFace(ref builder, ref vertexGrid, clampedSegments);
                CreateBackFace(ref builder, ref vertexGrid, clampedSegments);
                CreateLeftFace(ref builder, ref vertexGrid, clampedSegments);
                CreateRightFace(ref builder, ref vertexGrid, clampedSegments);
                CreateTopFace(ref builder, ref vertexGrid, clampedSegments);
                CreateBottomFace(ref builder, ref vertexGrid, clampedSegments);
            }

            MeshData result;
            using (s_BuildMeshMarker.Auto())
            {
                result = builder.Build(allocator);
            }

            builder.Dispose();
            vertexGrid.Dispose();
            return result;
        }

        struct VertexGrid
        {
            public NativeArray<int> vertices;
            public int xSize, ySize, zSize;

            public VertexGrid(int xSize, int ySize, int zSize, Allocator allocator)
            {
                this.xSize = xSize;
                this.ySize = ySize;
                this.zSize = zSize;
                this.vertices = new NativeArray<int>(xSize * ySize * zSize, allocator);
            }

            public int GetIndex(int x, int y, int z) => z * (xSize * ySize) + y * xSize + x;
            public int GetVertex(int x, int y, int z) => vertices[GetIndex(x, y, z)];
            public void SetVertex(int x, int y, int z, int vertexIndex) => vertices[GetIndex(x, y, z)] = vertexIndex;
            public void Dispose() => vertices.Dispose();
        }

        static VertexGrid CreateVertexGrid(ref MeshBuilder builder, float3 size, int3 segments)
        {
            var grid = new VertexGrid(segments.x + 1, segments.y + 1, segments.z + 1, Allocator.Temp);
            var hw = size.x * 0.5f;
            var hh = size.y * 0.5f;
            var hd = size.z * 0.5f;

            for (var z = 0; z <= segments.z; z++)
            {
                for (var y = 0; y <= segments.y; y++)
                {
                    for (var x = 0; x <= segments.x; x++)
                    {
                        var position = new float3(
                            math.lerp(-hw, hw, x / (float)segments.x),
                            math.lerp(-hh, hh, y / (float)segments.y),
                            math.lerp(-hd, hd, z / (float)segments.z)
                        );
                        var vertexIndex = builder.AddVertex(position);
                        grid.SetVertex(x, y, z, vertexIndex);
                    }
                }
            }

            return grid;
        }

        static void CreateFrontFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Front face (-Z): z = 0
            // Corner order: (-hw,-hh,-hd), (-hw,hh,-hd), (hw,hh,-hd), (hw,-hh,-hd)
            for (var y = 0; y < segments.y; y++)
            {
                for (var x = 0; x < segments.x; x++)
                {
                    var i0 = grid.GetVertex(x, y, 0);
                    var i1 = grid.GetVertex(x, y + 1, 0);
                    var i2 = grid.GetVertex(x + 1, y + 1, 0);
                    var i3 = grid.GetVertex(x + 1, y, 0);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }

        static void CreateBackFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Back face (+Z): z = segments.z
            // Corner order: (hw,-hh,hd), (hw,hh,hd), (-hw,hh,hd), (-hw,-hh,hd)
            for (var y = 0; y < segments.y; y++)
            {
                for (var x = 0; x < segments.x; x++)
                {
                    var i0 = grid.GetVertex(x + 1, y, segments.z);
                    var i1 = grid.GetVertex(x + 1, y + 1, segments.z);
                    var i2 = grid.GetVertex(x, y + 1, segments.z);
                    var i3 = grid.GetVertex(x, y, segments.z);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }

        static void CreateLeftFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Left face (-X): x = 0
            // Corner order: (-hw,-hh,hd), (-hw,hh,hd), (-hw,hh,-hd), (-hw,-hh,-hd)
            for (var y = 0; y < segments.y; y++)
            {
                for (var z = 0; z < segments.z; z++)
                {
                    var i0 = grid.GetVertex(0, y, z + 1);
                    var i1 = grid.GetVertex(0, y + 1, z + 1);
                    var i2 = grid.GetVertex(0, y + 1, z);
                    var i3 = grid.GetVertex(0, y, z);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }

        static void CreateRightFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Right face (+X): x = segments.x
            // Corner order: (hw,-hh,-hd), (hw,hh,-hd), (hw,hh,hd), (hw,-hh,hd)
            for (var y = 0; y < segments.y; y++)
            {
                for (var z = 0; z < segments.z; z++)
                {
                    var i0 = grid.GetVertex(segments.x, y, z);
                    var i1 = grid.GetVertex(segments.x, y + 1, z);
                    var i2 = grid.GetVertex(segments.x, y + 1, z + 1);
                    var i3 = grid.GetVertex(segments.x, y, z + 1);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }

        static void CreateTopFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Top face (+Y): y = segments.y
            // Corner order: (-hw,hh,-hd), (-hw,hh,hd), (hw,hh,hd), (hw,hh,-hd)
            for (var z = 0; z < segments.z; z++)
            {
                for (var x = 0; x < segments.x; x++)
                {
                    var i0 = grid.GetVertex(x, segments.y, z);
                    var i1 = grid.GetVertex(x, segments.y, z + 1);
                    var i2 = grid.GetVertex(x + 1, segments.y, z + 1);
                    var i3 = grid.GetVertex(x + 1, segments.y, z);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }

        static void CreateBottomFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Bottom face (-Y): y = 0
            // Corner order: (-hw,-hh,hd), (-hw,-hh,-hd), (hw,-hh,-hd), (hw,-hh,hd)
            for (var z = 0; z < segments.z; z++)
            {
                for (var x = 0; x < segments.x; x++)
                {
                    var i0 = grid.GetVertex(x, 0, z + 1);
                    var i1 = grid.GetVertex(x, 0, z);
                    var i2 = grid.GetVertex(x + 1, 0, z);
                    var i3 = grid.GetVertex(x + 1, 0, z + 1);
                    builder.AddFace(i0, i1, i2, i3);
                }
            }
        }
    }
}