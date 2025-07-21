using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2.Modifiers
{
    public static class SmoothVertices
    {
        public static void Apply(MeshData meshData, float smoothingFactor, int iterations)
        {
            smoothingFactor = math.clamp(smoothingFactor, 0f, 1f);
            iterations = math.max(1, iterations);

            for (var iter = 0; iter < iterations; iter++)
            {
                var newPositions = new float3[meshData.vertexCount];

                for (var vertexIndex = 0; vertexIndex < meshData.vertexCount; vertexIndex++)
                {
                    var neighbors = FindNeighborVertices(meshData, vertexIndex);
                    if (neighbors.Count == 0)
                    {
                        newPositions[vertexIndex] = meshData.vertices[vertexIndex].position;
                        continue;
                    }

                    // Calculate average position of neighbors
                    var averagePosition = float3.zero;
                    foreach (var neighborIndex in neighbors)
                        averagePosition += meshData.vertices[neighborIndex].position;
                    averagePosition /= neighbors.Count;

                    // Interpolate between current position and average
                    var currentPosition = meshData.vertices[vertexIndex].position;
                    newPositions[vertexIndex] = math.lerp(currentPosition, averagePosition, smoothingFactor);
                }

                // Apply all position changes
                for (var i = 0; i < meshData.vertexCount; i++)
                {
                    var vertex = meshData.vertices[i];
                    vertex.position = newPositions[i];
                    meshData.vertices[i] = vertex;
                }
            }
        }

        static HashSet<int> FindNeighborVertices(MeshData meshData, int vertexIndex)
        {
            var neighbors = new HashSet<int>();

            // Find all half-edges that start from this vertex
            for (var heIndex = 0; heIndex < meshData.halfEdgeCount; heIndex++)
            {
                var halfEdge = meshData.halfEdges[heIndex];
                
                // If this half-edge starts from our vertex, the next vertex is a neighbor
                if (halfEdge.vertex == vertexIndex)
                {
                    var nextHalfEdge = meshData.halfEdges[halfEdge.next];
                    neighbors.Add(nextHalfEdge.vertex);
                }
                
                // If this half-edge ends at our vertex, the start vertex is a neighbor
                var nextHalfEdge2 = meshData.halfEdges[halfEdge.next];
                if (nextHalfEdge2.vertex == vertexIndex)
                {
                    neighbors.Add(halfEdge.vertex);
                }
            }

            return neighbors;
        }
    }
}