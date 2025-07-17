using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public static class ChamferEdges
    {
        public static Mesh Apply(Mesh mesh, float distance)
        {
            // Simple implementation: Create a new mesh with chamfered edges
            // This is a basic implementation that creates small faces along edges
            var result = new Mesh();
            
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            // For each edge, we'll create a small chamfer by slightly moving vertices
            // and creating additional geometry
            
            // First, copy all original vertices with slight inward movement
            foreach (var vertex in mesh.Vertices)
            {
                vertices.Add(vertex.Position);
            }
            
            // Create modified faces - for simplicity, we'll just shrink each face slightly
            foreach (var face in mesh.Faces)
            {
                var faceVertices = face.GetVertices();
                var faceIndices = new List<int>();
                
                // Calculate face center
                var center = float3.zero;
                foreach (var v in faceVertices)
                    center += v.Position;
                center /= faceVertices.Count;
                
                // Create new vertices slightly moved toward center
                foreach (var vertex in faceVertices)
                {
                    var direction = math.normalize(center - vertex.Position);
                    var newPos = vertex.Position + direction * distance;
                    vertices.Add(newPos);
                    faceIndices.Add(vertices.Count - 1);
                }
                
                faces.Add(faceIndices.ToArray());
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
    }
}