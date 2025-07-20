using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace HalfEdgeMesh2
{
    [BurstCompile]
    public struct MeshBuilder : System.IDisposable
    {
        // Edge entry for deferred edge map population
        struct EdgeEntry
        {
            public long key;
            public int halfEdgeIndex;
        }

        NativeList<Vertex> vertices;
        NativeList<HalfEdge> halfEdges;
        NativeList<Face> faces;
        EdgeHashMap edgeMap;
        NativeList<EdgeEntry> edgeBuffer;

        public MeshBuilder(Allocator allocator, int initialCapacity = 16)
        {
            vertices = new NativeList<Vertex>(initialCapacity, allocator);
            halfEdges = new NativeList<HalfEdge>(initialCapacity * 4, allocator);
            faces = new NativeList<Face>(initialCapacity, allocator);

            var edgeCapacity = initialCapacity * 8; // Estimated edge count for quad-dominant meshes
            edgeMap = new EdgeHashMap(edgeCapacity, allocator);
            edgeBuffer = new NativeList<EdgeEntry>(edgeCapacity, allocator);
        }

        public void Dispose()
        {
            if (vertices.IsCreated) vertices.Dispose();
            if (halfEdges.IsCreated) halfEdges.Dispose();
            if (faces.IsCreated) faces.Dispose();
            if (edgeMap.IsCreated) edgeMap.Dispose();
            if (edgeBuffer.IsCreated) edgeBuffer.Dispose();
        }

        public int AddVertex(float3 position)
        {
            var index = vertices.Length;
            vertices.Add(new Vertex(position));
            return index;
        }

        public int AddFace(int v0, int v1, int v2)
        {
            var faceIndex = faces.Length;
            var firstHalfEdge = halfEdges.Length;

            AddFaceInternal(stackalloc int[] { v0, v1, v2 }, faceIndex, firstHalfEdge);
            faces.Add(new Face(firstHalfEdge));
            return faceIndex;
        }

        public int AddFace(int v0, int v1, int v2, int v3)
        {
            var faceIndex = faces.Length;
            var firstHalfEdge = halfEdges.Length;

            AddFaceInternal(stackalloc int[] { v0, v1, v2, v3 }, faceIndex, firstHalfEdge);
            faces.Add(new Face(firstHalfEdge));
            return faceIndex;
        }

        public int AddFace(ReadOnlySpan<int> vertexIndices)
        {
            if (vertexIndices.Length < 3)
                throw new ArgumentException("Face must have at least 3 vertices");

            var faceIndex = faces.Length;
            var firstHalfEdge = halfEdges.Length;

            AddFaceInternal(vertexIndices, faceIndex, firstHalfEdge);
            faces.Add(new Face(firstHalfEdge));
            return faceIndex;
        }

        public MeshData Build(Allocator allocator)
        {
            PopulateEdgeMap();
            ConnectTwins();

            var meshData = new MeshData(vertices.Length, halfEdges.Length, faces.Length, allocator);

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
            edgeBuffer.Clear();
        }

        // Core face creation logic shared by all AddFace variants
        void AddFaceInternal(ReadOnlySpan<int> vertexIndices, int faceIndex, int firstHalfEdge)
        {
            for (var i = 0; i < vertexIndices.Length; i++)
            {
                var v0 = vertexIndices[i];
                var v1 = vertexIndices[(i + 1) % vertexIndices.Length];
                var heIndex = halfEdges.Length;

                var he = new HalfEdge
                {
                    vertex = v0,
                    face = faceIndex,
                    next = firstHalfEdge + ((i + 1) % vertexIndices.Length),
                    twin = -1
                };

                halfEdges.Add(he);

                // Update vertex outgoing half-edge if not set
                if (vertices[v0].halfEdge == -1)
                {
                    var vertex = vertices[v0];
                    vertex.halfEdge = heIndex;
                    vertices[v0] = vertex;
                }

                // Store edge mapping in buffer for parallel processing
                var edgeKey = PackEdge(v0, v1);
                edgeBuffer.Add(new EdgeEntry { key = edgeKey, halfEdgeIndex = heIndex });
            }
        }

        void PopulateEdgeMap()
        {
            if (edgeBuffer.Length == 0)
                return;

            var job = new PopulateEdgeMapJob
            {
                edgeBuffer = edgeBuffer.AsArray(),
                edgeMap = edgeMap
            };

            var batchSize = math.max(32, edgeBuffer.Length / (JobsUtility.JobWorkerCount * 8));
            var handle = job.Schedule(edgeBuffer.Length, batchSize);
            handle.Complete();
        }

        void ConnectTwins()
        {
            var keys = edgeMap.GetKeyArray(Allocator.TempJob);

            var job = new ConnectTwinsJob
            {
                keys = keys,
                edgeMap = edgeMap,
                halfEdges = halfEdges
            };

            var batchSize = math.max(32, keys.Length / (JobsUtility.JobWorkerCount * 8));
            var handle = job.Schedule(keys.Length, batchSize);
            handle.Complete();

            keys.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static long PackEdge(int v0, int v1) => ((long)v0 << 32) | (uint)v1;

        [BurstCompile]
        struct PopulateEdgeMapJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<EdgeEntry> edgeBuffer;
            [NativeDisableParallelForRestriction] public EdgeHashMap edgeMap;

            public void Execute(int index)
            {
                var entry = edgeBuffer[index];
                edgeMap.TryAdd(entry.key, entry.halfEdgeIndex);
            }
        }

        [BurstCompile]
        struct ConnectTwinsJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<long> keys;
            [ReadOnly] public EdgeHashMap edgeMap;
            [NativeDisableParallelForRestriction] public NativeList<HalfEdge> halfEdges;

            public void Execute(int index)
            {
                var edgeKey = keys[index];
                if (!edgeMap.TryGetValue(edgeKey, out var heIndex))
                    return;

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
    }
}