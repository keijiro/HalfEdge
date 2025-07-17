using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

namespace HalfEdgeMesh.Modifiers
{
    public static class ChamferVertices
    {
        public static Mesh Apply(Mesh mesh, float distance)
        {
            if (distance <= 0f) return mesh;

            var result = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            // Dictionary to map original vertices to their chamfer vertices
            var vertexChamferMap = new Dictionary<Vertex, List<int>>();
            
            // Process each vertex
            foreach (var vertex in mesh.Vertices)
            {
                var chamferIndices = new List<int>();
                
                // Get all edges connected to this vertex
                var edges = new List<(Vertex neighbor, float3 direction)>();
                var halfEdge = vertex.HalfEdge;
                var current = halfEdge;
                
                do
                {
                    if (current.Destination != null)
                    {
                        var direction = math.normalize(current.Destination.Position - vertex.Position);
                        edges.Add((current.Destination, direction));
                    }
                    current = current.Twin?.Next;
                    if (current == null) break;
                } while (current != halfEdge);
                
                // Create new vertices along each edge
                foreach (var (neighbor, direction) in edges)
                {
                    var chamferPos = vertex.Position + direction * distance;
                    vertices.Add(chamferPos);
                    chamferIndices.Add(vertices.Count - 1);
                }
                
                vertexChamferMap[vertex] = chamferIndices;
            }
            
            // Process each face
            foreach (var face in mesh.Faces)
            {
                var faceVertices = face.GetVertices();
                
                // For each face, we need to create smaller faces using the chamfer vertices
                var newFaceIndices = new List<int>();
                
                for (int i = 0; i < faceVertices.Count; i++)
                {
                    var currentVertex = faceVertices[i];
                    var nextVertex = faceVertices[(i + 1) % faceVertices.Count];
                    var prevVertex = faceVertices[(i - 1 + faceVertices.Count) % faceVertices.Count];
                    
                    // Find the chamfer vertices on the edges to neighbors
                    var chamferVertices = vertexChamferMap[currentVertex];
                    
                    // Find which chamfer vertex corresponds to each neighbor
                    int toPrevIndex = -1;
                    int toNextIndex = -1;
                    
                    var edges = GetVertexEdges(currentVertex);
                    for (int j = 0; j < edges.Count; j++)
                    {
                        if (edges[j] == prevVertex) toPrevIndex = chamferVertices[j];
                        if (edges[j] == nextVertex) toNextIndex = chamferVertices[j];
                    }
                    
                    if (toPrevIndex >= 0 && toNextIndex >= 0)
                    {
                        // Add the two chamfer vertices in the correct order
                        newFaceIndices.Add(toPrevIndex);
                        newFaceIndices.Add(toNextIndex);
                    }
                }
                
                // Create the new face if we have enough vertices
                if (newFaceIndices.Count >= 3)
                {
                    faces.Add(newFaceIndices.ToArray());
                }
            }
            
            // Create chamfer faces at each original vertex
            foreach (var kvp in vertexChamferMap)
            {
                var vertex = kvp.Key;
                var chamferIndices = kvp.Value;
                if (chamferIndices.Count >= 3)
                {
                    // Reverse the order to get correct winding
                    chamferIndices.Reverse();
                    faces.Add(chamferIndices.ToArray());
                }
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
        
        static List<Vertex> GetVertexEdges(Vertex vertex)
        {
            var neighbors = new List<Vertex>();
            var halfEdge = vertex.HalfEdge;
            var current = halfEdge;
            
            do
            {
                if (current.Destination != null)
                {
                    neighbors.Add(current.Destination);
                }
                current = current.Twin?.Next;
                if (current == null) break;
            } while (current != halfEdge);
            
            return neighbors;
        }
    }
}