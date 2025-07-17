using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Cone
    {
        float radius;
        float height;
        int segments;

        public Cone(float radius, float height, int segments)
        {
            this.radius = radius;
            this.height = height;
            this.segments = segments;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            // Add apex vertex
            vertices.Add(new float3(0, height * 0.5f, 0));

            // Add base center vertex
            vertices.Add(new float3(0, -height * 0.5f, 0));

            // Add base rim vertices
            var angleStep = math.PI * 2.0f / segments;
            for (int i = 0; i < segments; i++)
            {
                var angle = i * angleStep;
                var x = math.cos(angle) * radius;
                var z = math.sin(angle) * radius;
                vertices.Add(new float3(x, -height * 0.5f, z));
            }

            // Create side faces (triangles from apex to base rim)
            for (int i = 0; i < segments; i++)
            {
                var next = (i + 1) % segments;
                faces.Add(new int[] { 0, 2 + next, 2 + i });
            }

            // Create base face (triangle fan from center to rim)
            for (int i = 0; i < segments; i++)
            {
                var next = (i + 1) % segments;
                faces.Add(new int[] { 1, 2 + next, 2 + i });
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }
    }
}