using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;

namespace HalfEdgeMesh2.Generators
{
    public static class Sphere
    {
        static readonly ProfilerMarker s_GenerateVerticesMarker = new ProfilerMarker("Sphere.GenerateVertices");
        static readonly ProfilerMarker s_GenerateFacesMarker = new ProfilerMarker("Sphere.GenerateFaces");
        static readonly ProfilerMarker s_BuildMeshMarker = new ProfilerMarker("Sphere.BuildMesh");
        public static MeshData Generate(float radius, int2 segments, Allocator allocator)
        {
            var clampedSegments = math.max(segments, 3);
            
            var longitudeSegments = clampedSegments.x;
            var latitudeSegments = clampedSegments.y;
            
            // Calculate estimated vertex count for initial capacity
            // Sphere has (lat+1) * (lon+1) vertices
            var totalVertices = (latitudeSegments + 1) * (longitudeSegments + 1);
            using var builder = new MeshBuilder(Allocator.Temp, totalVertices);

            // Generate vertices
            using (s_GenerateVerticesMarker.Auto())
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

                        var x = cosPhi * sinTheta;
                        var y = cosTheta;
                        var z = sinPhi * sinTheta;

                        builder.AddVertex(new float3(x, y, z) * radius);
                    }
                }
            }

            // Generate faces with correct winding order for outward normals
            using (s_GenerateFacesMarker.Auto())
            {
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
            }

            using (s_BuildMeshMarker.Auto())
            {
                return builder.Build(allocator);
            }
        }
    }
}