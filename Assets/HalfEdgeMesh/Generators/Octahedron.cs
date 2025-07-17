using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Octahedron
    {
        float size;

        public Octahedron(float size)
        {
            this.size = size;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            // Regular octahedron vertices (6 vertices at unit distance along axes)
            var s = size * 0.5f;
            
            var vertices = new float3[]
            {
                new float3( s,  0,  0), // +X
                new float3(-s,  0,  0), // -X
                new float3( 0,  s,  0), // +Y
                new float3( 0, -s,  0), // -Y
                new float3( 0,  0,  s), // +Z
                new float3( 0,  0, -s)  // -Z
            };

            // Octahedron faces (8 triangular faces)
            var faces = new int[][]
            {
                // Top half (around +Y vertex)
                new int[] {2, 4, 0}, // +Y, +Z, +X
                new int[] {2, 1, 4}, // +Y, -X, +Z
                new int[] {2, 5, 1}, // +Y, -Z, -X
                new int[] {2, 0, 5}, // +Y, +X, -Z
                
                // Bottom half (around -Y vertex)
                new int[] {3, 0, 4}, // -Y, +X, +Z
                new int[] {3, 4, 1}, // -Y, +Z, -X
                new int[] {3, 1, 5}, // -Y, -X, -Z
                new int[] {3, 5, 0}  // -Y, -Z, +X
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}