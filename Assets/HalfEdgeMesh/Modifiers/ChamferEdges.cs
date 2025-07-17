using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

namespace HalfEdgeMesh.Modifiers
{
    public static class ChamferEdges
    {
        public static Mesh Apply(Mesh mesh, float distance)
        {
            if (distance <= 0f) return mesh;

            var result = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            // Dictionary to map original face vertices to new shrunken vertices
            var faceVertexMap = new Dictionary<(Face, Vertex), int>();
            
            // First pass: Create shrunken faces
            foreach (var face in mesh.Faces)
            {
                var faceVertices = face.GetVertices();
                if (faceVertices.Count < 3) continue;
                
                // Calculate face center
                var center = float3.zero;
                foreach (var vertex in faceVertices)
                    center += vertex.Position;
                center /= faceVertices.Count;
                
                // Create new vertices moved toward center
                var newFaceIndices = new List<int>();
                
                foreach (var vertex in faceVertices)
                {
                    var direction = math.normalize(center - vertex.Position);
                    var newPos = vertex.Position + direction * distance;
                    
                    vertices.Add(newPos);
                    var newIndex = vertices.Count - 1;
                    newFaceIndices.Add(newIndex);
                    
                    // Store mapping for chamfer face creation
                    faceVertexMap[(face, vertex)] = newIndex;
                }
                
                // Add the shrunken face
                faces.Add(newFaceIndices.ToArray());
            }
            
            // Second pass: Create chamfer faces along edges
            var processedEdges = new HashSet<Edge>();
            
            foreach (var edge in mesh.Edges)
            {
                if (processedEdges.Contains(edge)) continue;
                processedEdges.Add(edge);
                
                var he1 = edge.HalfEdge;
                var he2 = he1.Twin;
                
                if (he1?.Face == null || he2?.Face == null) continue;
                
                var face1 = he1.Face;
                var face2 = he2.Face;
                var v1 = he1.Origin;
                var v2 = he1.Destination;
                
                // Get the shrunken vertices for both faces
                if (!faceVertexMap.ContainsKey((face1, v1)) || 
                    !faceVertexMap.ContainsKey((face1, v2)) ||
                    !faceVertexMap.ContainsKey((face2, v1)) || 
                    !faceVertexMap.ContainsKey((face2, v2))) continue;
                
                var face1_v1 = faceVertexMap[(face1, v1)];
                var face1_v2 = faceVertexMap[(face1, v2)];
                var face2_v1 = faceVertexMap[(face2, v1)];
                var face2_v2 = faceVertexMap[(face2, v2)];
                
                // Create quad chamfer face
                // Reverse winding order to fix normal direction
                faces.Add(new int[] { face1_v1, face2_v1, face2_v2, face1_v2 });
            }
            
            // Third pass: Create corner faces at vertices
            foreach (var vertex in mesh.Vertices)
            {
                var cornerVertices = new List<int>();
                
                // Get faces around this vertex in proper order using half-edge structure
                var halfEdge = vertex.HalfEdge;
                var current = halfEdge;
                
                do
                {
                    if (current.Face != null && faceVertexMap.ContainsKey((current.Face, vertex)))
                    {
                        cornerVertices.Add(faceVertexMap[(current.Face, vertex)]);
                    }
                    
                    current = current.Twin?.Next;
                    if (current == null) break;
                } while (current != halfEdge);
                
                // Create corner face if we have 3 or more vertices
                if (cornerVertices.Count >= 3)
                {
                    // Reverse for consistent outward-facing normals
                    cornerVertices.Reverse();
                    faces.Add(cornerVertices.ToArray());
                }
            }
            
            result.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return result;
        }
    }
}