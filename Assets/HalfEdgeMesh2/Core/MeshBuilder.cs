using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2
{
    public class MeshBuilder
    {
        List<Vertex> vertices = new List<Vertex>();
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        List<Face> faces = new List<Face>();
        Dictionary<(int, int), int> edgeMap = new Dictionary<(int, int), int>();

        public int AddVertex(float3 position)
        {
            var index = vertices.Count;
            vertices.Add(new Vertex(position));
            return index;
        }

        public int AddFace(int v0, int v1, int v2)
        {
            Span<int> vertices = stackalloc int[3] { v0, v1, v2 };
            return AddFace(vertices);
        }

        public int AddFace(int v0, int v1, int v2, int v3)
        {
            Span<int> vertices = stackalloc int[4] { v0, v1, v2, v3 };
            return AddFace(vertices);
        }

        public int AddFace(ReadOnlySpan<int> vertexIndices)
        {
            if (vertexIndices.Length < 3)
                throw new ArgumentException("Face must have at least 3 vertices");

            var faceIndex = faces.Count;
            var firstHalfEdge = halfEdges.Count;

            // Create half-edges for this face
            for (var i = 0; i < vertexIndices.Length; i++)
            {
                var v0 = vertexIndices[i];
                var v1 = vertexIndices[(i + 1) % vertexIndices.Length];
                var heIndex = halfEdges.Count;

                // Create half-edge
                var he = new HalfEdge
                {
                    vertex = v0,
                    face = faceIndex,
                    next = firstHalfEdge + ((i + 1) % vertexIndices.Length),
                    twin = -1 // Will be set later
                };

                halfEdges.Add(he);

                // Update vertex outgoing half-edge if not set
                if (vertices[v0].halfEdge == -1)
                {
                    var vertex = vertices[v0];
                    vertex.halfEdge = heIndex;
                    vertices[v0] = vertex;
                }

                // Store edge mapping for twin connection
                edgeMap[(v0, v1)] = heIndex;
            }

            // Create face
            faces.Add(new Face(firstHalfEdge));

            return faceIndex;
        }

        public MeshData Build(Allocator allocator)
        {
            // Connect twins
            ConnectTwins();

            // Create mesh data
            var meshData = new MeshData(vertices.Count, halfEdges.Count, faces.Count, allocator);

            // Copy data
            for (var i = 0; i < vertices.Count; i++)
                meshData.AddVertex(vertices[i]);

            for (var i = 0; i < halfEdges.Count; i++)
                meshData.AddHalfEdge(halfEdges[i]);

            for (var i = 0; i < faces.Count; i++)
                meshData.AddFace(faces[i]);

            return meshData;
        }

        public void Clear()
        {
            vertices.Clear();
            halfEdges.Clear();
            faces.Clear();
            edgeMap.Clear();
        }

        void ConnectTwins()
        {
            foreach (var kvp in edgeMap)
            {
                var (v0, v1) = kvp.Key;
                var heIndex = kvp.Value;

                // Look for opposite edge
                if (edgeMap.TryGetValue((v1, v0), out var twinIndex))
                {
                    var he = halfEdges[heIndex];
                    he.twin = twinIndex;
                    halfEdges[heIndex] = he;
                }
            }
        }
    }
}