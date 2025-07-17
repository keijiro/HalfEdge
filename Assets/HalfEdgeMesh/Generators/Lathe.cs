using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Lathe
    {
        List<float2> profile;
        int segments;

        public Lathe(List<float2> profile, int segments)
        {
            this.profile = profile;
            this.segments = segments;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var profileCount = profile.Count;
            var angleStep = math.PI * 2.0f / segments;

            // Generate vertices by rotating profile around Y axis
            for (int i = 0; i < segments; i++)
            {
                var angle = i * angleStep;
                var cos = math.cos(angle);
                var sin = math.sin(angle);

                for (int j = 0; j < profileCount; j++)
                {
                    var point = profile[j];
                    var x = point.x * cos;
                    var y = point.y;
                    var z = point.x * sin;
                    vertices.Add(new float3(x, y, z));
                }
            }

            // Generate faces
            for (int i = 0; i < segments; i++)
            {
                var nextI = (i + 1) % segments;

                for (int j = 0; j < profileCount - 1; j++)
                {
                    var i0 = i * profileCount + j;
                    var i1 = nextI * profileCount + j;
                    var i2 = nextI * profileCount + j + 1;
                    var i3 = i * profileCount + j + 1;

                    faces.Add(new int[] { i0, i1, i2, i3 });
                }
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }
    }
}