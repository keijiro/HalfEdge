using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class CylinderGenerator
    {
        float radius;
        float height;
        int segments;
        bool capped;

        public CylinderGenerator(float radius, float height, int segments, bool capped = true)
        {
            this.radius = radius;
            this.height = height;
            this.segments = math.max(3, segments);
            this.capped = capped;
        }

        public MeshData Generate()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var halfHeight = height * 0.5f;
            var angleStep = math.PI * 2f / segments;

            for (int i = 0; i < segments; i++)
            {
                var angle = i * angleStep;
                var x = math.cos(angle) * radius;
                var z = math.sin(angle) * radius;

                vertices.Add(new float3(x, -halfHeight, z));
                vertices.Add(new float3(x, halfHeight, z));
            }

            for (int i = 0; i < segments; i++)
            {
                var i0 = i * 2;
                var i1 = i * 2 + 1;
                var i2 = ((i + 1) % segments) * 2 + 1;
                var i3 = ((i + 1) % segments) * 2;

                faces.Add(new int[] { i0, i1, i2, i3 });
            }

            if (capped)
            {
                var bottomCenterIndex = vertices.Count;
                vertices.Add(new float3(0, -halfHeight, 0));

                var topCenterIndex = vertices.Count;
                vertices.Add(new float3(0, halfHeight, 0));

                for (int i = 0; i < segments; i++)
                {
                    var i0 = i * 2;
                    var i1 = ((i + 1) % segments) * 2;
                    faces.Add(new int[] { bottomCenterIndex, i0, i1 });

                    var i2 = i * 2 + 1;
                    var i3 = ((i + 1) % segments) * 2 + 1;
                    faces.Add(new int[] { topCenterIndex, i3, i2 });
                }
            }

            var meshData = new MeshData();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());

            return meshData;
        }
    }
}