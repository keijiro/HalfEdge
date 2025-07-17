using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Icosphere
    {
        float radius;
        int subdivisions;

        public Icosphere(float radius, int subdivisions)
        {
            this.radius = radius;
            this.subdivisions = subdivisions;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            // Create initial icosahedron vertices
            var t = (1.0f + math.sqrt(5.0f)) / 2.0f;
            
            var vertices = new List<float3>
            {
                math.normalize(new float3(-1,  t,  0)) * radius,
                math.normalize(new float3( 1,  t,  0)) * radius,
                math.normalize(new float3(-1, -t,  0)) * radius,
                math.normalize(new float3( 1, -t,  0)) * radius,
                math.normalize(new float3( 0, -1,  t)) * radius,
                math.normalize(new float3( 0,  1,  t)) * radius,
                math.normalize(new float3( 0, -1, -t)) * radius,
                math.normalize(new float3( 0,  1, -t)) * radius,
                math.normalize(new float3( t,  0, -1)) * radius,
                math.normalize(new float3( t,  0,  1)) * radius,
                math.normalize(new float3(-t,  0, -1)) * radius,
                math.normalize(new float3(-t,  0,  1)) * radius
            };

            // Create initial icosahedron faces
            var faces = new List<int[]>
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

            // Apply subdivisions
            for (int i = 0; i < subdivisions; i++)
            {
                var newFaces = new List<int[]>();
                var midpointCache = new Dictionary<(int, int), int>();

                foreach (var face in faces)
                {
                    int a = face[0];
                    int b = face[1];
                    int c = face[2];

                    // Get midpoints
                    int ab = GetMidpoint(a, b, vertices, midpointCache);
                    int bc = GetMidpoint(b, c, vertices, midpointCache);
                    int ca = GetMidpoint(c, a, vertices, midpointCache);

                    // Create 4 new triangular faces
                    newFaces.Add(new int[] {a, ab, ca});
                    newFaces.Add(new int[] {b, bc, ab});
                    newFaces.Add(new int[] {c, ca, bc});
                    newFaces.Add(new int[] {ab, bc, ca});
                }

                faces = newFaces;
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }

        int GetMidpoint(int v1, int v2, List<float3> vertices, Dictionary<(int, int), int> cache)
        {
            var key = v1 < v2 ? (v1, v2) : (v2, v1);
            
            if (cache.TryGetValue(key, out int existing))
                return existing;

            var point1 = vertices[v1];
            var point2 = vertices[v2];
            var middle = math.normalize((point1 + point2) * 0.5f) * radius;

            vertices.Add(middle);
            var index = vertices.Count - 1;
            cache[key] = index;

            return index;
        }
    }
}