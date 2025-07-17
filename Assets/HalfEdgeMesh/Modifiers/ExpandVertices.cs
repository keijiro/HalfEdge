using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public static class ExpandVertices
    {
        public static Mesh Apply(Mesh mesh, float distance)
        {
            // Simple implementation: move vertices outward along their average normal
            var result = new Mesh();
            
            // Copy original data but modify vertex positions
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
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
                vertices.Add(vertex.Position + normal * distance);
            }
            
            // Copy faces
            foreach (var face in mesh.Faces)
            {
                var faceVertices = face.GetVertices();
                var indices = new int[faceVertices.Count];
                for (int i = 0; i < faceVertices.Count; i++)
                {
                    indices[i] = mesh.Vertices.IndexOf(faceVertices[i]);
                }
                faces.Add(indices);
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
    }
}