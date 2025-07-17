using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public static class SplitFaces
    {
        public static Mesh Apply(Mesh mesh, float3 planeNormal, float3 planePoint)
        {
            // Split faces that intersect with the given plane
            var result = new Mesh();
            
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            var normal = math.normalize(planeNormal);
            
            foreach (var face in mesh.Faces)
            {
                var faceVertices = face.GetVertices();
                var positions = new List<float3>();
                var distances = new List<float>();
                
                // Get vertex positions and distances to plane
                foreach (var vertex in faceVertices)
                {
                    positions.Add(vertex.Position);
                    var toPoint = vertex.Position - planePoint;
                    distances.Add(math.dot(toPoint, normal));
                }
                
                // Check if face crosses the plane
                bool hasPositive = false;
                bool hasNegative = false;
                
                foreach (var dist in distances)
                {
                    if (dist > 0.001f) hasPositive = true;
                    if (dist < -0.001f) hasNegative = true;
                }
                
                if (!hasPositive || !hasNegative)
                {
                    // Face doesn't cross plane, add as-is
                    var indices = new int[positions.Count];
                    for (int i = 0; i < positions.Count; i++)
                    {
                        vertices.Add(positions[i]);
                        indices[i] = vertices.Count - 1;
                    }
                    faces.Add(indices);
                }
                else
                {
                    // Face crosses plane, split it
                    SplitPolygon(positions, distances, normal, planePoint, vertices, faces);
                }
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
        
        static void SplitPolygon(List<float3> positions, List<float> distances, float3 normal, float3 planePoint,
                                List<float3> vertices, List<int[]> faces)
        {
            var positivePoly = new List<float3>();
            var negativePoly = new List<float3>();
            
            for (int i = 0; i < positions.Count; i++)
            {
                var curr = positions[i];
                var currDist = distances[i];
                var next = positions[(i + 1) % positions.Count];
                var nextDist = distances[(i + 1) % positions.Count];
                
                // Add current vertex to appropriate side
                if (currDist >= 0)
                    positivePoly.Add(curr);
                else
                    negativePoly.Add(curr);
                
                // Check if edge crosses plane
                if ((currDist > 0 && nextDist < 0) || (currDist < 0 && nextDist > 0))
                {
                    // Calculate intersection point
                    var t = currDist / (currDist - nextDist);
                    var intersection = math.lerp(curr, next, t);
                    
                    positivePoly.Add(intersection);
                    negativePoly.Add(intersection);
                }
            }
            
            // Add both polygons if they have enough vertices
            if (positivePoly.Count >= 3)
            {
                var indices = new int[positivePoly.Count];
                for (int i = 0; i < positivePoly.Count; i++)
                {
                    vertices.Add(positivePoly[i]);
                    indices[i] = vertices.Count - 1;
                }
                faces.Add(indices);
            }
            
            if (negativePoly.Count >= 3)
            {
                var indices = new int[negativePoly.Count];
                for (int i = 0; i < negativePoly.Count; i++)
                {
                    vertices.Add(negativePoly[i]);
                    indices[i] = vertices.Count - 1;
                }
                faces.Add(indices);
            }
        }
    }
}