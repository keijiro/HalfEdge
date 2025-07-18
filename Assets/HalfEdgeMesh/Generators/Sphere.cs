using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class Sphere
    {
        float radius;
        int2 segments;

        public Sphere(float radius, int2 segments)
        {
            this.radius = radius;
            this.segments = math.max(segments, 3);
        }

        public Mesh Generate()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var longitudeSegments = segments.x;
            var latitudeSegments = segments.y;

            // Generate vertices
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                var theta = lat * math.PI / latitudeSegments;
                var sinTheta = math.sin(theta);
                var cosTheta = math.cos(theta);

                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    var phi = lon * 2 * math.PI / longitudeSegments;
                    var sinPhi = math.sin(phi);
                    var cosPhi = math.cos(phi);

                    var x = cosPhi * sinTheta;
                    var y = cosTheta;
                    var z = sinPhi * sinTheta;

                    vertices.Add(new float3(x, y, z) * radius);
                }
            }

            // Generate faces with correct winding order for outward normals
            for (int lat = 0; lat < latitudeSegments; lat++)
            {
                for (int lon = 0; lon < longitudeSegments; lon++)
                {
                    var i0 = lat * (longitudeSegments + 1) + lon;
                    var i1 = i0 + longitudeSegments + 1;
                    var i2 = i1 + 1;
                    var i3 = i0 + 1;

                    if (lat == 0)
                    {
                        // Top cap - triangles 
                        faces.Add(new int[] { i0, i2, i1 });
                    }
                    else if (lat == latitudeSegments - 1)
                    {
                        // Bottom cap - triangles (reversed for correct outward normal)
                        faces.Add(new int[] { i0, i3, i1 });
                    }
                    else
                    {
                        // Middle section - quads (reversed order for correct winding)
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