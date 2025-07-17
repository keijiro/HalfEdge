using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public static class CreateLattice
    {
        public static Mesh Apply(Mesh mesh, float spacing)
        {
            // Create a lattice structure by connecting vertices with cylindrical struts
            // This is a simplified implementation that creates connections between nearby vertices
            var result = new Mesh();
            
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            // Get all original vertices
            var originalVertices = new List<float3>();
            foreach (var vertex in mesh.Vertices)
                originalVertices.Add(vertex.Position);
            
            // Find connections between vertices based on spacing (simplified)
            var connections = new List<(int v1, int v2)>();
            
            // Limit to first few vertices to avoid performance issues
            var maxVertices = math.min(originalVertices.Count, 8);
            
            for (int i = 0; i < maxVertices; i++)
            {
                for (int j = i + 1; j < maxVertices; j++)
                {
                    var distance = math.length(originalVertices[i] - originalVertices[j]);
                    if (distance <= spacing * 2.0f)
                    {
                        connections.Add((i, j));
                    }
                }
            }
            
            // Create cylindrical struts for each connection
            var thickness = spacing * 0.1f;
            var segments = 6;
            
            foreach (var (v1, v2) in connections)
            {
                var p1 = originalVertices[v1];
                var p2 = originalVertices[v2];
                var direction = math.normalize(p2 - p1);
                var length = math.length(p2 - p1);
                
                // Create a simple cylinder between the two points
                CreateCylinderBetweenPoints(p1, p2, thickness, segments, vertices, faces);
            }
            
            // Add spheres at limited vertex positions
            for (int i = 0; i < maxVertices; i++)
            {
                CreateSphereAtPoint(originalVertices[i], thickness * 1.5f, 4, vertices, faces);
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
        
        static void CreateCylinderBetweenPoints(float3 p1, float3 p2, float radius, int segments, 
                                              List<float3> vertices, List<int[]> faces)
        {
            var direction = math.normalize(p2 - p1);
            var length = math.length(p2 - p1);
            
            // Create perpendicular vectors
            var up = math.abs(direction.y) < 0.9f ? new float3(0, 1, 0) : new float3(1, 0, 0);
            var right = math.normalize(math.cross(direction, up));
            up = math.normalize(math.cross(right, direction));
            
            var startIndex = vertices.Count;
            
            // Create vertices for cylinder
            for (int i = 0; i < segments; i++)
            {
                var angle = i * 2.0f * math.PI / segments;
                var offset = math.cos(angle) * right + math.sin(angle) * up;
                offset *= radius;
                
                vertices.Add(p1 + offset);
                vertices.Add(p2 + offset);
            }
            
            // Create faces
            for (int i = 0; i < segments; i++)
            {
                var next = (i + 1) % segments;
                var i0 = startIndex + i * 2;
                var i1 = startIndex + i * 2 + 1;
                var i2 = startIndex + next * 2 + 1;
                var i3 = startIndex + next * 2;
                
                faces.Add(new int[] { i0, i1, i2, i3 });
            }
        }
        
        static void CreateSphereAtPoint(float3 center, float radius, int resolution, 
                                       List<float3> vertices, List<int[]> faces)
        {
            var startIndex = vertices.Count;
            
            // Create a simple icosphere-like structure
            for (int i = 0; i <= resolution; i++)
            {
                var lat = math.PI * (-0.5f + (float)i / resolution);
                var y = math.sin(lat);
                var r = math.cos(lat);
                
                for (int j = 0; j <= resolution; j++)
                {
                    var lon = 2.0f * math.PI * j / resolution;
                    var x = r * math.cos(lon);
                    var z = r * math.sin(lon);
                    
                    vertices.Add(center + new float3(x, y, z) * radius);
                }
            }
            
            // Create triangular faces
            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    var i0 = startIndex + i * (resolution + 1) + j;
                    var i1 = startIndex + (i + 1) * (resolution + 1) + j;
                    var i2 = startIndex + (i + 1) * (resolution + 1) + j + 1;
                    var i3 = startIndex + i * (resolution + 1) + j + 1;
                    
                    faces.Add(new int[] { i0, i1, i2 });
                    faces.Add(new int[] { i0, i2, i3 });
                }
            }
        }
    }
}