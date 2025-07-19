using System;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2
{
    public struct MeshBuilder : System.IDisposable
    {
        NativeList<Vertex> vertices;
        NativeList<HalfEdge> halfEdges;
        NativeList<Face> faces;
        NativeHashMap<long, int> edgeMap;

        public MeshBuilder(Allocator allocator)
        {
            vertices = new NativeList<Vertex>(allocator);
            halfEdges = new NativeList<HalfEdge>(allocator);
            faces = new NativeList<Face>(allocator);
            edgeMap = new NativeHashMap<long, int>(16, allocator);
        }

        public void Dispose()
        {
            if (vertices.IsCreated) vertices.Dispose();
            if (halfEdges.IsCreated) halfEdges.Dispose();
            if (faces.IsCreated) faces.Dispose();
            if (edgeMap.IsCreated) edgeMap.Dispose();
        }

        public int AddVertex(float3 position)
        {
            var index = vertices.Length;
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

            var faceIndex = faces.Length;
            var firstHalfEdge = halfEdges.Length;

            // Create half-edges for this face
            for (var i = 0; i < vertexIndices.Length; i++)
            {
                var v0 = vertexIndices[i];
                var v1 = vertexIndices[(i + 1) % vertexIndices.Length];
                var heIndex = halfEdges.Length;

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
                var edgeKey = PackEdge(v0, v1);
                edgeMap[edgeKey] = heIndex;
            }

            // Create face
            faces.Add(new Face(firstHalfEdge));

            return faceIndex;
        }

        static long PackEdge(int v0, int v1) => ((long)v0 << 32) | (uint)v1;

        public MeshData Build(Allocator allocator)
        {
            // Connect twins
            ConnectTwins();

            // Create mesh data
            var meshData = new MeshData(vertices.Length, halfEdges.Length, faces.Length, allocator);

            // Copy data
            for (var i = 0; i < vertices.Length; i++)
                meshData.AddVertex(vertices[i]);

            for (var i = 0; i < halfEdges.Length; i++)
                meshData.AddHalfEdge(halfEdges[i]);

            for (var i = 0; i < faces.Length; i++)
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
            var keys = edgeMap.GetKeyArray(Allocator.Temp);
            try
            {
                for (var i = 0; i < keys.Length; i++)
                {
                    var edgeKey = keys[i];
                    var heIndex = edgeMap[edgeKey];

                    // Unpack edge
                    var v0 = (int)(edgeKey >> 32);
                    var v1 = (int)(edgeKey & 0xFFFFFFFF);

                    // Look for opposite edge
                    var oppositeKey = PackEdge(v1, v0);
                    if (edgeMap.TryGetValue(oppositeKey, out var twinIndex))
                    {
                        var he = halfEdges[heIndex];
                        he.twin = twinIndex;
                        halfEdges[heIndex] = he;
                    }
                }
            }
            finally
            {
                keys.Dispose();
            }
        }
    }
}