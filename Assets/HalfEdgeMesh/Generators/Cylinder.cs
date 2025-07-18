using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class Cylinder
    {
        float radius;
        float height;
        int2 segments;
        bool capped;

        public Cylinder(float radius, float height, int2 segments, bool capped = true)
        {
            this.radius = radius;
            this.height = height;
            this.segments = math.max(segments, new int2(3, 1));
            this.capped = capped;
        }

        public Mesh Generate()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var radialSegments = segments.x;
            var heightSegments = segments.y;

            var halfHeight = height * 0.5f;
            var angleStep = math.PI * 2f / radialSegments;
            var heightStep = height / heightSegments;

            // Generate vertices in rings (Z-axis as vertical)
            for (int h = 0; h <= heightSegments; h++)
            {
                var z = -halfHeight + h * heightStep;
                for (int i = 0; i < radialSegments; i++)
                {
                    var angle = i * angleStep;
                    var x = math.cos(angle) * radius;
                    var y = math.sin(angle) * radius;

                    vertices.Add(new float3(x, y, z));
                }
            }

            // Generate side faces
            for (int h = 0; h < heightSegments; h++)
            {
                for (int i = 0; i < radialSegments; i++)
                {
                    var i0 = h * radialSegments + i;
                    var i1 = (h + 1) * radialSegments + i;
                    var i2 = (h + 1) * radialSegments + ((i + 1) % radialSegments);
                    var i3 = h * radialSegments + ((i + 1) % radialSegments);

                    faces.Add(new int[] { i0, i3, i2, i1 });
                }
            }

            if (capped)
            {
                var bottomCenterIndex = vertices.Count;
                vertices.Add(new float3(0, 0, -halfHeight));

                var topCenterIndex = vertices.Count;
                vertices.Add(new float3(0, 0, halfHeight));

                // Bottom cap
                for (int i = 0; i < radialSegments; i++)
                {
                    var i0 = i;
                    var i1 = (i + 1) % radialSegments;
                    faces.Add(new int[] { bottomCenterIndex, i1, i0 });
                }

                // Top cap
                for (int i = 0; i < radialSegments; i++)
                {
                    var i0 = heightSegments * radialSegments + i;
                    var i1 = heightSegments * radialSegments + ((i + 1) % radialSegments);
                    faces.Add(new int[] { topCenterIndex, i0, i1 });
                }
            }

            var meshData = new Mesh();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());

            return meshData;
        }
    }
}