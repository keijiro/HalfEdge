using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Torus
    {
        float majorRadius;
        float minorRadius;
        int2 segments;

        public Torus(float majorRadius, float minorRadius, int2 segments)
        {
            this.majorRadius = majorRadius;
            this.minorRadius = minorRadius;
            this.segments = math.max(segments, 3);
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            // Generate vertices (Z-axis as vertical)
            for (int i = 0; i < segments.x; i++)
            {
                var majorAngle = i * math.PI * 2.0f / segments.x;
                var majorCos = math.cos(majorAngle);
                var majorSin = math.sin(majorAngle);

                for (int j = 0; j < segments.y; j++)
                {
                    var minorAngle = j * math.PI * 2.0f / segments.y;
                    var minorCos = math.cos(minorAngle);
                    var minorSin = math.sin(minorAngle);

                    var x = (majorRadius + minorRadius * minorCos) * majorCos;
                    var y = (majorRadius + minorRadius * minorCos) * majorSin;
                    var z = minorRadius * minorSin;

                    vertices.Add(new float3(x, y, z));
                }
            }

            // Generate faces
            for (int i = 0; i < segments.x; i++)
            {
                var nextI = (i + 1) % segments.x;

                for (int j = 0; j < segments.y; j++)
                {
                    var nextJ = (j + 1) % segments.y;

                    var i0 = i * segments.y + j;
                    var i1 = nextI * segments.y + j;
                    var i2 = nextI * segments.y + nextJ;
                    var i3 = i * segments.y + nextJ;

                    faces.Add(new int[] { i3, i0, i1, i2 });
                }
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }
    }
}