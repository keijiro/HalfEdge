using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Icosahedron
    {
        float size;

        public Icosahedron(float size)
        {
            this.size = size;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            // Golden ratio
            var t = (1.0f + math.sqrt(5.0f)) / 2.0f;
            var scale = size / (2.0f * t);

            // Regular icosahedron vertices
            var vertices = new float3[]
            {
                math.normalize(new float3(-1,  t,  0)) * scale,
                math.normalize(new float3( 1,  t,  0)) * scale,
                math.normalize(new float3(-1, -t,  0)) * scale,
                math.normalize(new float3( 1, -t,  0)) * scale,
                math.normalize(new float3( 0, -1,  t)) * scale,
                math.normalize(new float3( 0,  1,  t)) * scale,
                math.normalize(new float3( 0, -1, -t)) * scale,
                math.normalize(new float3( 0,  1, -t)) * scale,
                math.normalize(new float3( t,  0, -1)) * scale,
                math.normalize(new float3( t,  0,  1)) * scale,
                math.normalize(new float3(-t,  0, -1)) * scale,
                math.normalize(new float3(-t,  0,  1)) * scale
            };

            // Icosahedron faces (20 triangular faces)
            var faces = new int[][]
            {
                new int[] {0, 11, 5},
                new int[] {0, 5, 1},
                new int[] {0, 1, 7},
                new int[] {0, 7, 10},
                new int[] {0, 10, 11},
                new int[] {1, 5, 9},
                new int[] {5, 11, 4},
                new int[] {11, 10, 2},
                new int[] {10, 7, 6},
                new int[] {7, 1, 8},
                new int[] {3, 9, 4},
                new int[] {3, 4, 2},
                new int[] {3, 2, 6},
                new int[] {3, 6, 8},
                new int[] {3, 8, 9},
                new int[] {4, 9, 5},
                new int[] {2, 4, 11},
                new int[] {6, 2, 10},
                new int[] {8, 6, 7},
                new int[] {9, 8, 1}
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}