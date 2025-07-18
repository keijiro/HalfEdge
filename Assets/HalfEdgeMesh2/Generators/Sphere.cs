using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2.Generators
{
    public static class Sphere
    {
        public static MeshData Generate(float radius, int2 segments, Allocator allocator)
        {
            var clampedSegments = math.max(segments, 3);
            using var builder = new MeshBuilder(Allocator.Temp);

            var longitudeSegments = clampedSegments.x;
            var latitudeSegments = clampedSegments.y;

            // Generate vertices
            for (var lat = 0; lat <= latitudeSegments; lat++)
            {
                var theta = lat * math.PI / latitudeSegments;
                var sinTheta = math.sin(theta);
                var cosTheta = math.cos(theta);

                for (var lon = 0; lon <= longitudeSegments; lon++)
                {
                    var phi = lon * 2 * math.PI / longitudeSegments;
                    var sinPhi = math.sin(phi);
                    var cosPhi = math.cos(phi);

                    var x = cosPhi * sinTheta;
                    var y = cosTheta;
                    var z = sinPhi * sinTheta;

                    builder.AddVertex(new float3(x, y, z) * radius);
                }
            }

            // Generate faces with correct winding order for outward normals
            for (var lat = 0; lat < latitudeSegments; lat++)
            {
                for (var lon = 0; lon < longitudeSegments; lon++)
                {
                    var i0 = lat * (longitudeSegments + 1) + lon;
                    var i1 = i0 + longitudeSegments + 1;
                    var i2 = i1 + 1;
                    var i3 = i0 + 1;

                    if (lat == 0)
                    {
                        // Top cap - triangles
                        builder.AddFace(i0, i2, i1);
                    }
                    else if (lat == latitudeSegments - 1)
                    {
                        // Bottom cap - triangles (reversed for correct outward normal)
                        builder.AddFace(i0, i3, i1);
                    }
                    else
                    {
                        // Middle section - quads (reversed order for correct winding)
                        builder.AddFace(i0, i3, i2, i1);
                    }
                }
            }

            return builder.Build(allocator);
        }
    }
}