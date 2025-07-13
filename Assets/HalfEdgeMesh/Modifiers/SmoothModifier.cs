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
                    var neighbors = GetNeighborVertices(vertex, mesh);
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

        List<Vertex> GetNeighborVertices(Vertex vertex, MeshData mesh)
        {
            var neighbors = new HashSet<Vertex>();

            // Find all half-edges connected to this vertex
            foreach (var halfEdge in mesh.HalfEdges)
            {
                if (halfEdge.Origin == vertex && halfEdge.Destination != null)
                {
                    neighbors.Add(halfEdge.Destination);
                }
                else if (halfEdge.Destination == vertex && halfEdge.Origin != null)
                {
                    neighbors.Add(halfEdge.Origin);
                }
            }

            return new List<Vertex>(neighbors);
        }
    }
}