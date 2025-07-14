using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class CylinderGenerator
    {
        float radius;
        float height;
        int segments;
        int heightSegments;
        bool capped;

        public CylinderGenerator(float radius, float height, int segments, bool capped = true)
            : this(radius, height, segments, 1, capped)
        {
        }

        public CylinderGenerator(float radius, float height, int segments, int heightSegments, bool capped = true)
        {
            this.radius = radius;
            this.height = height;
            this.segments = math.max(3, segments);
            this.heightSegments = math.max(1, heightSegments);
            this.capped = capped;
        }

        public MeshData Generate()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var halfHeight = height * 0.5f;
            var angleStep = math.PI * 2f / segments;
            var heightStep = height / heightSegments;

            // Generate vertices in rings
            for (int h = 0; h <= heightSegments; h++)
            {
                var y = -halfHeight + h * heightStep;
                for (int i = 0; i < segments; i++)
                {
                    var angle = i * angleStep;
                    var x = math.cos(angle) * radius;
                    var z = math.sin(angle) * radius;

                    vertices.Add(new float3(x, y, z));
                }
            }

            // Generate side faces
            for (int h = 0; h < heightSegments; h++)
            {
                for (int i = 0; i < segments; i++)
                {
                    var i0 = h * segments + i;
                    var i1 = (h + 1) * segments + i;
                    var i2 = (h + 1) * segments + ((i + 1) % segments);
                    var i3 = h * segments + ((i + 1) % segments);

                    faces.Add(new int[] { i0, i1, i2, i3 });
                }
            }

            if (capped)
            {
                var bottomCenterIndex = vertices.Count;
                vertices.Add(new float3(0, -halfHeight, 0));

                var topCenterIndex = vertices.Count;
                vertices.Add(new float3(0, halfHeight, 0));

                // Bottom cap
                for (int i = 0; i < segments; i++)
                {
                    var i0 = i;
                    var i1 = (i + 1) % segments;
                    faces.Add(new int[] { bottomCenterIndex, i0, i1 });
                }

                // Top cap
                for (int i = 0; i < segments; i++)
                {
                    var i0 = heightSegments * segments + i;
                    var i1 = heightSegments * segments + ((i + 1) % segments);
                    faces.Add(new int[] { topCenterIndex, i1, i0 });
                }
            }

            var meshData = new MeshData();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());

            return meshData;
        }
    }
}