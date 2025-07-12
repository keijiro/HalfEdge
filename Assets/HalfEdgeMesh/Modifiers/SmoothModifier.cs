using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Modifiers
{
    public class SmoothModifier
    {
        float smoothingFactor;
        int iterations;
        
        public SmoothModifier(float smoothingFactor = 0.5f, int iterations = 1)
        {
            this.smoothingFactor = math.clamp(smoothingFactor, 0f, 1f);
            this.iterations = math.max(1, iterations);
        }
        
        public void Apply(MeshData mesh)
        {
            for (int iter = 0; iter < iterations; iter++)
            {
                var newPositions = new Dictionary<Vertex, float3>();
                
                foreach (var vertex in mesh.Vertices)
                {
                    var neighbors = GetNeighborVertices(vertex);
                    if (neighbors.Count == 0)
                    {
                        newPositions[vertex] = vertex.Position;
                        continue;
                    }
                    
                    var avgPosition = float3.zero;
                    foreach (var neighbor in neighbors)
                        avgPosition += neighbor.Position;
                    
                    avgPosition /= neighbors.Count;
                    
                    newPositions[vertex] = math.lerp(vertex.Position, avgPosition, smoothingFactor);
                }
                
                foreach (var kvp in newPositions)
                    kvp.Key.Position = kvp.Value;
            }
        }
        
        List<Vertex> GetNeighborVertices(Vertex vertex)
        {
            var neighbors = new HashSet<Vertex>();
            
            if (vertex.HalfEdge == null) return new List<Vertex>();
            
            var start = vertex.HalfEdge;
            var current = start;
            
            do
            {
                if (current.Destination != null)
                    neighbors.Add(current.Destination);
                
                if (current.Twin != null && current.Twin.Next != null)
                    current = current.Twin.Next;
                else
                    break;
                    
            } while (current != start && current != null);
            
            return new List<Vertex>(neighbors);
        }
    }
}