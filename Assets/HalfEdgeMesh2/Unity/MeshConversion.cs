using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshConversion
    {
        // Static buffers for memory reuse
        static NativeArray<Vector3> s_vertexBuffer;
        static NativeArray<int> s_triangleBuffer;
        static NativeArray<Vector3> s_normalBuffer;
        static bool s_buffersInitialized;
        public static Mesh ToUnityMesh(ref MeshData meshData, bool calculateNormals = true)
        {
            var mesh = new Mesh();
            UpdateMeshDataOptimized(mesh, ref meshData, calculateNormals);
            return mesh;
        }

        public static void UpdateUnityMesh(Mesh mesh, ref MeshData meshData, bool calculateNormals = true)
        {
            mesh.Clear();
            UpdateMeshDataOptimized(mesh, ref meshData, calculateNormals);
        }

        static void UpdateMeshDataOptimized(Mesh mesh, ref MeshData meshData, bool calculateNormals)
        {
            EnsureBufferCapacity(meshData.vertexCount, GetTriangleCount(ref meshData));

            // Extract vertices into static buffer
            ExtractVerticesOptimized(ref meshData, ref s_vertexBuffer);
            ExtractTrianglesOptimized(ref meshData, ref s_triangleBuffer);

            // Set mesh data using buffer slices
            var vertexSlice = s_vertexBuffer.GetSubArray(0, meshData.vertexCount);
            var triangleCount = GetTriangleCount(ref meshData);
            var triangleSlice = s_triangleBuffer.GetSubArray(0, triangleCount);

            mesh.SetVertices(vertexSlice);
            mesh.SetIndices(triangleSlice, MeshTopology.Triangles, 0);

            if (calculateNormals)
            {
                if (HasValidNormals(ref meshData))
                {
                    CalculateNormalsOptimized(ref meshData, ref s_normalBuffer);
                    var normalSlice = s_normalBuffer.GetSubArray(0, meshData.vertexCount);
                    mesh.SetNormals(normalSlice);
                }
                else
                    mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
        }

        static void EnsureBufferCapacity(int vertexCount, int triangleCount)
        {
            if (!s_buffersInitialized || s_vertexBuffer.Length < vertexCount)
            {
                if (s_buffersInitialized) s_vertexBuffer.Dispose();
                s_vertexBuffer = new NativeArray<Vector3>(math.max(vertexCount, 64), Allocator.Persistent);
            }

            if (!s_buffersInitialized || s_triangleBuffer.Length < triangleCount)
            {
                if (s_buffersInitialized) s_triangleBuffer.Dispose();
                s_triangleBuffer = new NativeArray<int>(math.max(triangleCount, 192), Allocator.Persistent);
            }

            if (!s_buffersInitialized || s_normalBuffer.Length < vertexCount)
            {
                if (s_buffersInitialized) s_normalBuffer.Dispose();
                s_normalBuffer = new NativeArray<Vector3>(math.max(vertexCount, 64), Allocator.Persistent);
            }

            s_buffersInitialized = true;
        }

        [BurstCompile]
        static int GetTriangleCount(ref MeshData meshData)
        {
            var triangleCount = 0;
            for (var faceIndex = 0; faceIndex < meshData.faceCount; faceIndex++)
            {
                var face = meshData.faces[faceIndex];
                var vertexCount = CountFaceVertices(ref meshData, face.halfEdge);
                triangleCount += (vertexCount - 2) * 3;
            }
            return triangleCount;
        }

        [BurstCompile]
        static void ExtractVerticesOptimized(ref MeshData meshData, ref NativeArray<Vector3> buffer)
        {
            for (var i = 0; i < meshData.vertexCount; i++)
                buffer[i] = meshData.vertices[i].position;
        }

        [BurstCompile]
        static void ExtractTrianglesOptimized(ref MeshData meshData, ref NativeArray<int> buffer)
        {
            var triangleIndex = 0;
            for (var faceIndex = 0; faceIndex < meshData.faceCount; faceIndex++)
            {
                var face = meshData.faces[faceIndex];
                ExtractFaceTrianglesDirectOptimized(ref meshData, face.halfEdge, ref buffer, ref triangleIndex);
            }
        }

        [BurstCompile]
        static void ExtractFaceTrianglesDirectOptimized(ref MeshData meshData, int startHalfEdge, ref NativeArray<int> triangles, ref int triangleIndex)
        {
            // Directly triangulate without intermediate storage for better performance
            var firstVertex = -1;
            var prevVertex = -1;
            var vertexIndex = 0;
            var currentHe = startHalfEdge;

            do
            {
                var he = meshData.halfEdges[currentHe];
                var vertex = he.vertex;

                if (vertexIndex == 0)
                    firstVertex = vertex;
                else if (vertexIndex > 1)
                {
                    // Create triangle: first, prev, current
                    triangles[triangleIndex++] = firstVertex;
                    triangles[triangleIndex++] = prevVertex;
                    triangles[triangleIndex++] = vertex;
                }

                prevVertex = vertex;
                vertexIndex++;
                currentHe = he.next;
            } while (currentHe != startHalfEdge);
        }

        static void CalculateNormalsOptimized(ref MeshData meshData, ref NativeArray<Vector3> buffer)
        {
            var normals = new NativeArray<float3>(meshData.vertexCount, Allocator.Temp);
            MeshOperations.ComputeVertexNormals(ref meshData, ref normals);

            for (var i = 0; i < meshData.vertexCount; i++)
                buffer[i] = normals[i];

            normals.Dispose();
        }

        static bool HasValidNormals(ref MeshData meshData)
        {
            // Use optimized version that doesn't create intermediate lists
            for (var i = 0; i < meshData.faceCount; i++)
            {
                var face = meshData.faces[i];
                var vertexCount = CountFaceVertices(ref meshData, face.halfEdge);
                if (vertexCount < 3)
                    return false;
            }
            return meshData.faceCount > 0;
        }

        [BurstCompile]
        static int CountFaceVertices(ref MeshData meshData, int startHalfEdge)
        {
            var count = 0;
            var currentHe = startHalfEdge;

            do
            {
                count++;
                var he = meshData.halfEdges[currentHe];
                currentHe = he.next;
            } while (currentHe != startHalfEdge);

            return count;
        }
    }
}