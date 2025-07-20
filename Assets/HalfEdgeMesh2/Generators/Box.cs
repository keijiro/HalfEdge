using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2.Generators
{
    public static class Box
    {
        public static MeshData Generate(float3 size, int3 segments, Allocator allocator)
        {
            var clampedSegments = math.max(segments, 1);

            // Calculate unique edges for initial capacity estimation
            var internalEdgesX = clampedSegments.x * (clampedSegments.y + 1) * (clampedSegments.z + 1);
            var internalEdgesY = (clampedSegments.x + 1) * clampedSegments.y * (clampedSegments.z + 1);
            var internalEdgesZ = (clampedSegments.x + 1) * (clampedSegments.y + 1) * clampedSegments.z;
            var uniqueEdges = internalEdgesX + internalEdgesY + internalEdgesZ;

            var builder = new MeshBuilder(Allocator.TempJob, uniqueEdges);
            var vertexGrid = CreateVertexGrid(ref builder, size, clampedSegments);

            CreateAllFaces(ref builder, ref vertexGrid, clampedSegments);

            var result = builder.Build(allocator);

            builder.Dispose();
            vertexGrid.Dispose();
            return result;
        }

        struct VertexGrid : System.IDisposable
        {
            public NativeArray<int> vertices;
            public int xSize, ySize, zSize;

            public VertexGrid(int xSize, int ySize, int zSize, Allocator allocator)
            {
                this.xSize = xSize;
                this.ySize = ySize;
                this.zSize = zSize;
                vertices = new NativeArray<int>(xSize * ySize * zSize, allocator);
            }

            public int GetVertex(int x, int y, int z) => vertices[z * (xSize * ySize) + y * xSize + x];
            public void SetVertex(int x, int y, int z, int vertexIndex) => vertices[z * (xSize * ySize) + y * xSize + x] = vertexIndex;
            public void Dispose() => vertices.Dispose();
        }

        static VertexGrid CreateVertexGrid(ref MeshBuilder builder, float3 size, int3 segments)
        {
            var grid = new VertexGrid(segments.x + 1, segments.y + 1, segments.z + 1, Allocator.TempJob);
            var halfSize = size * 0.5f;

            for (var z = 0; z <= segments.z; z++)
            for (var y = 0; y <= segments.y; y++)
            for (var x = 0; x <= segments.x; x++)
            {
                var position = new float3(
                    math.lerp(-halfSize.x, halfSize.x, x / (float)segments.x),
                    math.lerp(-halfSize.y, halfSize.y, y / (float)segments.y),
                    math.lerp(-halfSize.z, halfSize.z, z / (float)segments.z)
                );
                var vertexIndex = builder.AddVertex(position);
                grid.SetVertex(x, y, z, vertexIndex);
            }

            return grid;
        }

        static void CreateAllFaces(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            CreateFrontFace(ref builder, ref grid, segments);
            CreateBackFace(ref builder, ref grid, segments);
            CreateLeftFace(ref builder, ref grid, segments);
            CreateRightFace(ref builder, ref grid, segments);
            CreateTopFace(ref builder, ref grid, segments);
            CreateBottomFace(ref builder, ref grid, segments);
        }

        static void CreateFrontFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Front face (z = 0)
            for (var y = 0; y < segments.y; y++)
            for (var x = 0; x < segments.x; x++)
            {
                var i0 = grid.GetVertex(x, y, 0);
                var i1 = grid.GetVertex(x, y + 1, 0);
                var i2 = grid.GetVertex(x + 1, y + 1, 0);
                var i3 = grid.GetVertex(x + 1, y, 0);
                builder.AddFace(i0, i1, i2, i3);
            }
        }

        static void CreateBackFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Back face (z = segments.z)
            for (var y = 0; y < segments.y; y++)
            for (var x = 0; x < segments.x; x++)
            {
                var i0 = grid.GetVertex(x + 1, y, segments.z);
                var i1 = grid.GetVertex(x + 1, y + 1, segments.z);
                var i2 = grid.GetVertex(x, y + 1, segments.z);
                var i3 = grid.GetVertex(x, y, segments.z);
                builder.AddFace(i0, i1, i2, i3);
            }
        }

        static void CreateLeftFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Left face (x = 0)
            for (var y = 0; y < segments.y; y++)
            for (var z = 0; z < segments.z; z++)
            {
                var i0 = grid.GetVertex(0, y, z + 1);
                var i1 = grid.GetVertex(0, y + 1, z + 1);
                var i2 = grid.GetVertex(0, y + 1, z);
                var i3 = grid.GetVertex(0, y, z);
                builder.AddFace(i0, i1, i2, i3);
            }
        }

        static void CreateRightFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Right face (x = segments.x)
            for (var y = 0; y < segments.y; y++)
            for (var z = 0; z < segments.z; z++)
            {
                var i0 = grid.GetVertex(segments.x, y, z);
                var i1 = grid.GetVertex(segments.x, y + 1, z);
                var i2 = grid.GetVertex(segments.x, y + 1, z + 1);
                var i3 = grid.GetVertex(segments.x, y, z + 1);
                builder.AddFace(i0, i1, i2, i3);
            }
        }

        static void CreateTopFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Top face (y = segments.y)
            for (var z = 0; z < segments.z; z++)
            for (var x = 0; x < segments.x; x++)
            {
                var i0 = grid.GetVertex(x, segments.y, z);
                var i1 = grid.GetVertex(x, segments.y, z + 1);
                var i2 = grid.GetVertex(x + 1, segments.y, z + 1);
                var i3 = grid.GetVertex(x + 1, segments.y, z);
                builder.AddFace(i0, i1, i2, i3);
            }
        }

        static void CreateBottomFace(ref MeshBuilder builder, ref VertexGrid grid, int3 segments)
        {
            // Bottom face (y = 0)
            for (var z = 0; z < segments.z; z++)
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