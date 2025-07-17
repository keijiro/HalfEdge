using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public class ExpandVertices
    {
        float distance;

        public ExpandVertices(float distance)
        {
            this.distance = distance;
        }

        public void Apply(Mesh mesh)
        {
            if (distance == 0f) return;

            var newPositions = new Dictionary<Vertex, float3>();
            
            foreach (var vertex in mesh.Vertices)
            {
                // Calculate average normal from adjacent faces
                var normal = float3.zero;
                var count = 0;
                
                var he = vertex.HalfEdge;
                var start = he;
                do
                {
                    if (he.Face != null)
                    {
                        var faceVertices = he.Face.GetVertices();
                        if (faceVertices.Count >= 3)
                        {
                            var v0 = faceVertices[0].Position;
                            var v1 = faceVertices[1].Position;
                            var v2 = faceVertices[2].Position;
                            var faceNormal = math.normalize(math.cross(v1 - v0, v2 - v0));
                            normal += faceNormal;
                            count++;
                        }
                    }
                    he = he.Twin?.Next;
                    if (he == null) break;
                } while (he != null && he != start && count < 10);
                
                if (count > 0)
                    normal = math.normalize(normal / count);
                
                // Move vertex outward
                newPositions[vertex] = vertex.Position + normal * distance;
            }
            
            // Apply new positions
            foreach (var kvp in newPositions)
                kvp.Key.Position = kvp.Value;
        }
    }
}