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

            // Golden ratio for dodecahedron construction
            var phi = (1.0f + math.sqrt(5.0f)) / 2.0f; // Golden ratio ≈ 1.618
            var invPhi = 1.0f / phi; // ≈ 0.618
            var scale = size * 0.5f;

            // 20 vertices of a regular dodecahedron using standard icosahedral coordinates
            var vertices = new float3[]
            {
                // Cube vertices (8 vertices)
                new float3( 1,  1,  1) * scale,   // 0
                new float3( 1,  1, -1) * scale,   // 1
                new float3( 1, -1,  1) * scale,   // 2
                new float3( 1, -1, -1) * scale,   // 3
                new float3(-1,  1,  1) * scale,   // 4
                new float3(-1,  1, -1) * scale,   // 5
                new float3(-1, -1,  1) * scale,   // 6
                new float3(-1, -1, -1) * scale,   // 7
                
                // Golden ratio rectangles in YZ plane (4 vertices)
                new float3( 0,  phi,  invPhi) * scale,   // 8
                new float3( 0,  phi, -invPhi) * scale,   // 9
                new float3( 0, -phi,  invPhi) * scale,   // 10
                new float3( 0, -phi, -invPhi) * scale,   // 11
                
                // Golden ratio rectangles in XZ plane (4 vertices)
                new float3( invPhi,  0,  phi) * scale,   // 12
                new float3(-invPhi,  0,  phi) * scale,   // 13
                new float3( invPhi,  0, -phi) * scale,   // 14
                new float3(-invPhi,  0, -phi) * scale,   // 15
                
                // Golden ratio rectangles in XY plane (4 vertices)
                new float3( phi,  invPhi,  0) * scale,   // 16
                new float3( phi, -invPhi,  0) * scale,   // 17
                new float3(-phi,  invPhi,  0) * scale,   // 18
                new float3(-phi, -invPhi,  0) * scale    // 19
            };

            // 12 pentagonal faces with proper winding order and connectivity
            var faces = new int[][]
            {
                new int[] {0, 8, 4, 13, 12},     // Face 0
                new int[] {0, 12, 2, 17, 16},    // Face 1
                new int[] {0, 16, 1, 9, 8},      // Face 2
                new int[] {1, 14, 15, 5, 9},     // Face 3 (reversed)
                new int[] {1, 16, 17, 3, 14},    // Face 4 (reversed)
                new int[] {2, 12, 13, 6, 10},    // Face 5
                new int[] {2, 10, 11, 3, 17},    // Face 6
                new int[] {3, 11, 7, 15, 14},    // Face 7
                new int[] {4, 8, 9, 5, 18},      // Face 8
                new int[] {4, 18, 19, 6, 13},    // Face 9
                new int[] {5, 15, 7, 19, 18},    // Face 10 (reversed)
                new int[] {6, 19, 7, 11, 10}     // Face 11 (reversed)
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}