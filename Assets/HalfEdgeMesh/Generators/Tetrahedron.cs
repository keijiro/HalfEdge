using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Tetrahedron
    {
        float size;

        public Tetrahedron(float size)
        {
            this.size = size;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            // Regular tetrahedron vertices
            var s = size / math.sqrt(2.0f);
            
            var vertices = new float3[]
            {
                new float3( s,  s,  s),
                new float3( s, -s, -s),
                new float3(-s,  s, -s),
                new float3(-s, -s,  s)
            };

            // Tetrahedron faces
            var faces = new int[][]
            {
                new int[] {0, 1, 2},
                new int[] {0, 3, 1},
                new int[] {0, 2, 3},
                new int[] {1, 3, 2}
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}