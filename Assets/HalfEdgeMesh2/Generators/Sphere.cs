using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2.Generators
{
    public static class Sphere
    {
        public static MeshData Generate(float radius, int2 segments, Allocator allocator)
        {
            var clampedSegments = math.max(segments, 3);
            var longitudeSegments = clampedSegments.x;
            var latitudeSegments = clampedSegments.y;

            // Estimate unique edges for initial capacity
            var horizontalEdges = latitudeSegments * (longitudeSegments + 1);
            var verticalEdges = (latitudeSegments + 1) * longitudeSegments;
            var uniqueEdges = horizontalEdges + verticalEdges;

            var builder = new MeshBuilder(Allocator.TempJob, uniqueEdges);

            GenerateVertices(ref builder, radius, longitudeSegments, latitudeSegments);
            GenerateFaces(ref builder, longitudeSegments, latitudeSegments);

            var result = builder.Build(allocator);
            builder.Dispose();
            return result;
        }

        static void GenerateVertices(ref MeshBuilder builder, float radius, int longitudeSegments, int latitudeSegments)
        {
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

                    var position = new float3(
                        cosPhi * sinTheta,
                        cosTheta,
                        sinPhi * sinTheta
                    ) * radius;

                    builder.AddVertex(position);
                }
            }
        }

        static void GenerateFaces(ref MeshBuilder builder, int longitudeSegments, int latitudeSegments)
        {
            for (var lat = 0; lat < latitudeSegments; lat++)
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
                    // Bottom cap - triangles
                    builder.AddFace(i0, i3, i1);
                }
                else
                {
                    // Middle section - quads
                    builder.AddFace(i0, i3, i2, i1);
                }
            }
        }
    }
}