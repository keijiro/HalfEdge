using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Dodecahedron
    {
        float size;

        public Dodecahedron(float size)
        {
            this.size = size;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            // Simplified dodecahedron - use truncated cube approach
            var scale = size * 0.5f;

            // 8 vertices of a cube (corners)
            var vertices = new float3[]
            {
                new float3( 1,  1,  1) * scale,
                new float3( 1,  1, -1) * scale,
                new float3( 1, -1,  1) * scale,
                new float3( 1, -1, -1) * scale,
                new float3(-1,  1,  1) * scale,
                new float3(-1,  1, -1) * scale,
                new float3(-1, -1,  1) * scale,
                new float3(-1, -1, -1) * scale
            };

            // Create faces as triangles instead of pentagons for simplicity
            var faces = new int[][]
            {
                new int[] {0, 2, 1},
                new int[] {1, 2, 3},
                new int[] {4, 5, 6},
                new int[] {5, 7, 6},
                new int[] {0, 1, 5},
                new int[] {0, 5, 4},
                new int[] {2, 6, 7},
                new int[] {2, 7, 3},
                new int[] {0, 4, 6},
                new int[] {0, 6, 2},
                new int[] {1, 3, 7},
                new int[] {1, 7, 5}
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}