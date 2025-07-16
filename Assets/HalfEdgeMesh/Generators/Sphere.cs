using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class Sphere
    {
        float radius;
        int resolution;

        public Sphere(float radius, int resolution)
        {
            this.radius = radius;
            this.resolution = resolution;
        }

        public Mesh Generate()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var segments = 16 * (int)math.pow(2, resolution);
            var rings = segments / 2;

            for (int lat = 0; lat <= rings; lat++)
            {
                var theta = lat * math.PI / rings;
                var sinTheta = math.sin(theta);
                var cosTheta = math.cos(theta);

                for (int lon = 0; lon <= segments; lon++)
                {
                    var phi = lon * 2 * math.PI / segments;
                    var sinPhi = math.sin(phi);
                    var cosPhi = math.cos(phi);

                    var x = cosPhi * sinTheta;
                    var y = cosTheta;
                    var z = sinPhi * sinTheta;

                    vertices.Add(new float3(x, y, z) * radius);
                }
            }

            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    var i0 = lat * (segments + 1) + lon;
                    var i1 = i0 + segments + 1;
                    var i2 = i1 + 1;
                    var i3 = i0 + 1;

                    if (lat == 0)
                    {
                        faces.Add(new int[] { i0, i2, i1 });
                    }
                    else if (lat == rings - 1)
                    {
                        faces.Add(new int[] { i0, i3, i1 });
                    }
                    else
                    {
                        faces.Add(new int[] { i0, i3, i2, i1 });
                    }
                }
            }

            var meshData = new Mesh();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());

            return meshData;
        }
    }
}
