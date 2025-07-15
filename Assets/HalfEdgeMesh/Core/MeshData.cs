using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace HalfEdgeMesh
{
    public class MeshData
    {
        public List<Vertex> Vertices { get; private set; }
        public List<Face> Faces { get; private set; }
        public List<HalfEdge> HalfEdges { get; private set; }
        public List<Edge> Edges { get; private set; }

        public MeshData()
        {
            Vertices = new List<Vertex>();
            Faces = new List<Face>();
            HalfEdges = new List<HalfEdge>();
            Edges = new List<Edge>();
        }

        public void InitializeFromIndexedFaces(float3[] vertexPositions, int[][] faceIndices)
        {
            Clear();

            foreach (var pos in vertexPositions)
                Vertices.Add(new Vertex(pos));

            var halfEdgeMap = new Dictionary<(int, int), HalfEdge>();

            foreach (var indices in faceIndices)
            {
                var face = new Face();
                Faces.Add(face);

                var faceHalfEdges = new List<HalfEdge>();

                for (int i = 0; i < indices.Length; i++)
                {
                    var v0 = indices[i];
                    var v1 = indices[(i + 1) % indices.Length];

                    var halfEdge = new HalfEdge
                    {
                        Origin = Vertices[v0],
                        Face = face
                    };

                    HalfEdges.Add(halfEdge);
                    faceHalfEdges.Add(halfEdge);
                    halfEdgeMap[(v0, v1)] = halfEdge;

                    if (Vertices[v0].HalfEdge == null)
                        Vertices[v0].HalfEdge = halfEdge;
                }

                for (int i = 0; i < faceHalfEdges.Count; i++)
                {
                    faceHalfEdges[i].Next = faceHalfEdges[(i + 1) % faceHalfEdges.Count];
                }

                face.HalfEdge = faceHalfEdges[0];
            }

            foreach (var kvp in halfEdgeMap)
            {
                var (v0, v1) = kvp.Key;
                var halfEdge = kvp.Value;

                if (halfEdgeMap.TryGetValue((v1, v0), out var twin))
                {
                    halfEdge.Twin = twin;
                    twin.Twin = halfEdge;
                }
            }

            var processedPairs = new HashSet<(int, int)>();
            foreach (var kvp in halfEdgeMap)
            {
                var (v0, v1) = kvp.Key;
                if (v0 > v1) continue;

                if (!processedPairs.Contains((v0, v1)))
                {
                    Edges.Add(new Edge(kvp.Value));
                    processedPairs.Add((v0, v1));
                }
            }
        }

        public void Clear()
        {
            Vertices.Clear();
            Faces.Clear();
            HalfEdges.Clear();
            Edges.Clear();
        }

        public enum ShadingMode
        {
            Smooth,
            Flat
        }

        public Mesh ToUnityMesh() => ToUnityMesh(ShadingMode.Smooth);

        public Mesh ToUnityMesh(ShadingMode shadingMode)
        {
            var mesh = new Mesh();

            if (shadingMode == ShadingMode.Smooth)
            {
                var vertices = new List<Vector3>();
                var triangles = new List<int>();
                var vertexMap = new Dictionary<Vertex, int>();

                for (int i = 0; i < Vertices.Count; i++)
                {
                    vertices.Add(Vertices[i].Position);
                    vertexMap[Vertices[i]] = i;
                }

                foreach (var face in Faces)
                {
                    var faceVertices = face.GetVertices();
                    if (faceVertices.Count < 3) continue;

                    for (int i = 1; i < faceVertices.Count - 1; i++)
                    {
                        triangles.Add(vertexMap[faceVertices[0]]);
                        triangles.Add(vertexMap[faceVertices[i]]);
                        triangles.Add(vertexMap[faceVertices[i + 1]]);
                    }
                }

                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
            else // Flat shading
            {
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var triangles = new List<int>();
                var vertexIndex = 0;

                foreach (var face in Faces)
                {
                    var faceVertices = face.GetVertices();
                    if (faceVertices.Count < 3) continue;

                    // Calculate face normal
                    var v0 = faceVertices[0].Position;
                    var v1 = faceVertices[1].Position;
                    var v2 = faceVertices[2].Position;
                    var faceNormal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

                    // Triangulate the face with duplicated vertices
                    for (int i = 1; i < faceVertices.Count - 1; i++)
                    {
                        // Add three vertices for this triangle
                        vertices.Add(faceVertices[0].Position);
                        vertices.Add(faceVertices[i].Position);
                        vertices.Add(faceVertices[i + 1].Position);

                        // Add the same normal for all three vertices
                        normals.Add(faceNormal);
                        normals.Add(faceNormal);
                        normals.Add(faceNormal);

                        // Add triangle indices
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 2);

                        vertexIndex += 3;
                    }
                }

                mesh.vertices = vertices.ToArray();
                mesh.normals = normals.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.RecalculateBounds();
            }

            return mesh;
        }

        public HalfEdge SplitEdge(Edge edge, float t = 0.5f)
        {
            var he1 = edge.HalfEdge;
            var he2 = he1.Twin;

            var v0 = he1.Origin;
            var v1 = he1.Destination;

            var newVertex = new Vertex(math.lerp(v0.Position, v1.Position, t));
            Vertices.Add(newVertex);

            var newHe1 = new HalfEdge
            {
                Origin = newVertex,
                Face = he1.Face,
                Next = he1.Next
            };
            HalfEdges.Add(newHe1);

            he1.Next = newHe1;
            newVertex.HalfEdge = newHe1;

            if (he2 != null)
            {
                var newHe2 = new HalfEdge
                {
                    Origin = newVertex,
                    Face = he2.Face,
                    Next = he2.Next
                };
                HalfEdges.Add(newHe2);

                he2.Next = newHe2;

                he1.Twin = newHe2;
                newHe2.Twin = he1;

                he2.Twin = newHe1;
                newHe1.Twin = he2;
            }

            Edges.Add(new Edge(newHe1));

            return newHe1;
        }

        public Face ExtrudeFace(Face face, float3 direction, float distance)
        {
            var vertices = face.GetVertices();
            var halfEdges = face.GetHalfEdges();

            var newVertices = new List<Vertex>();
            var newTopFace = new Face();
            Faces.Add(newTopFace);

            foreach (var v in vertices)
            {
                var newVertex = new Vertex(v.Position + direction * distance);
                Vertices.Add(newVertex);
                newVertices.Add(newVertex);
            }

            var topHalfEdges = new List<HalfEdge>();
            for (int i = 0; i < newVertices.Count; i++)
            {
                var he = new HalfEdge
                {
                    Origin = newVertices[i],
                    Face = newTopFace
                };
                HalfEdges.Add(he);
                topHalfEdges.Add(he);
                newVertices[i].HalfEdge = he;
            }

            for (int i = 0; i < topHalfEdges.Count; i++)
            {
                topHalfEdges[i].Next = topHalfEdges[(i + 1) % topHalfEdges.Count];
            }

            newTopFace.HalfEdge = topHalfEdges[0];

            for (int i = 0; i < vertices.Count; i++)
            {
                var v0 = vertices[i];
                var v1 = vertices[(i + 1) % vertices.Count];
                var v2 = newVertices[(i + 1) % vertices.Count];
                var v3 = newVertices[i];

                var sideFace = new Face();
                Faces.Add(sideFace);

                var he0 = new HalfEdge { Origin = v0, Face = sideFace };
                var he1 = new HalfEdge { Origin = v1, Face = sideFace };
                var he2 = new HalfEdge { Origin = v2, Face = sideFace };
                var he3 = new HalfEdge { Origin = v3, Face = sideFace };

                HalfEdges.Add(he0);
                HalfEdges.Add(he1);
                HalfEdges.Add(he2);
                HalfEdges.Add(he3);

                he0.Next = he1;
                he1.Next = he2;
                he2.Next = he3;
                he3.Next = he0;

                sideFace.HalfEdge = he0;
            }

            return newTopFace;
        }

        public void TriangulateFace(Face face)
        {
            var vertices = face.GetVertices();
            if (vertices.Count <= 3) return;

            var halfEdges = face.GetHalfEdges();
            var v0 = vertices[0];

            for (int i = 2; i < vertices.Count - 1; i++)
            {
                var newFace = new Face();
                Faces.Add(newFace);

                var he0 = new HalfEdge { Origin = v0, Face = newFace };
                var he1 = new HalfEdge { Origin = vertices[i], Face = newFace };
                var he2 = new HalfEdge { Origin = vertices[i + 1], Face = newFace };

                HalfEdges.Add(he0);
                HalfEdges.Add(he1);
                HalfEdges.Add(he2);

                he0.Next = he1;
                he1.Next = he2;
                he2.Next = he0;

                newFace.HalfEdge = he0;
            }

            var firstTriFace = new Face();
            Faces.Add(firstTriFace);

            var the0 = new HalfEdge { Origin = v0, Face = firstTriFace };
            var the1 = new HalfEdge { Origin = vertices[1], Face = firstTriFace };
            var the2 = new HalfEdge { Origin = vertices[2], Face = firstTriFace };

            HalfEdges.Add(the0);
            HalfEdges.Add(the1);
            HalfEdges.Add(the2);

            the0.Next = the1;
            the1.Next = the2;
            the2.Next = the0;

            firstTriFace.HalfEdge = the0;

            Faces.Remove(face);
            foreach (var he in halfEdges)
            {
                HalfEdges.Remove(he);
            }
        }
    }
}