using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Torus
    {
        float majorRadius;
        float minorRadius;
        int majorSegments;
        int minorSegments;

        public Torus(float majorRadius, float minorRadius, int majorSegments, int minorSegments)
        {
            this.majorRadius = majorRadius;
            this.minorRadius = minorRadius;
            this.majorSegments = majorSegments;
            this.minorSegments = minorSegments;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            // Generate vertices
            for (int i = 0; i < majorSegments; i++)
            {
                var majorAngle = i * math.PI * 2.0f / majorSegments;
                var majorCos = math.cos(majorAngle);
                var majorSin = math.sin(majorAngle);

                for (int j = 0; j < minorSegments; j++)
                {
                    var minorAngle = j * math.PI * 2.0f / minorSegments;
                    var minorCos = math.cos(minorAngle);
                    var minorSin = math.sin(minorAngle);

                    var x = (majorRadius + minorRadius * minorCos) * majorCos;
                    var y = minorRadius * minorSin;
                    var z = (majorRadius + minorRadius * minorCos) * majorSin;

                    vertices.Add(new float3(x, y, z));
                }
            }

            // Generate faces
            for (int i = 0; i < majorSegments; i++)
            {
                var nextI = (i + 1) % majorSegments;

                for (int j = 0; j < minorSegments; j++)
                {
                    var nextJ = (j + 1) % minorSegments;

                    var i0 = i * minorSegments + j;
                    var i1 = nextI * minorSegments + j;
                    var i2 = nextI * minorSegments + nextJ;
                    var i3 = i * minorSegments + nextJ;

                    faces.Add(new int[] { i0, i3, i2, i1 });
                }
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }
    }
}