using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2
{
    [BurstCompile]
    public static class MeshOperations
    {
        [BurstCompile]
        public static void ComputeFaceNormals(ref MeshData mesh, ref NativeArray<float3> normals)
        {
            for (var faceIndex = 0; faceIndex < mesh.faceCount; faceIndex++)
            {
                var face = mesh.faces[faceIndex];
                var he0 = mesh.halfEdges[face.halfEdge];
                var he1 = mesh.halfEdges[he0.next];
                var he2 = mesh.halfEdges[he1.next];

                var v0 = mesh.vertices[he0.vertex].position;
                var v1 = mesh.vertices[he1.vertex].position;
                var v2 = mesh.vertices[he2.vertex].position;

                var normal = math.normalize(math.cross(v1 - v0, v2 - v0));
                normals[faceIndex] = normal;
            }
        }

        [BurstCompile]
        public static void ComputeVertexNormals(ref MeshData mesh, ref NativeArray<float3> normals)
        {
            var faceNormals = new NativeArray<float3>(mesh.faceCount, Allocator.Temp);
            ComputeFaceNormals(ref mesh, ref faceNormals);

            for (var i = 0; i < mesh.vertexCount; i++)
                normals[i] = float3.zero;

            for (var faceIndex = 0; faceIndex < mesh.faceCount; faceIndex++)
            {
                var face = mesh.faces[faceIndex];
                var faceNormal = faceNormals[faceIndex];

                var he = mesh.halfEdges[face.halfEdge];
                var startHe = he;

                do
                {
                    normals[he.vertex] += faceNormal;
                    he = mesh.halfEdges[he.next];
                } while (he.next != startHe.next);
            }

            for (var i = 0; i < mesh.vertexCount; i++)
                normals[i] = math.normalize(normals[i]);

            faceNormals.Dispose();
        }

        [BurstCompile]
        public static void ComputeBounds(ref MeshData mesh, out float3 boundsCenter, out float3 boundsSize)
        {
            if (mesh.vertexCount == 0)
            {
                boundsCenter = float3.zero;
                boundsSize = float3.zero;
                return;
            }

            float3 min = mesh.vertices[0].position;
            float3 max = mesh.vertices[0].position;

            for (var i = 1; i < mesh.vertexCount; i++)
            {
                var pos = mesh.vertices[i].position;
                min = math.min(min, pos);
                max = math.max(max, pos);
            }

            boundsCenter = (min + max) * 0.5f;
            boundsSize = max - min;
        }

        [BurstCompile]
        public static bool ValidateMesh(ref MeshData mesh)
        {
            for (var i = 0; i < mesh.vertexCount; i++)
            {
                var vertex = mesh.vertices[i];
                if (vertex.halfEdge >= 0 && vertex.halfEdge >= mesh.halfEdgeCount)
                    return false;
            }

            for (var i = 0; i < mesh.halfEdgeCount; i++)
            {
                var he = mesh.halfEdges[i];

                if (he.next < 0 || he.next >= mesh.halfEdgeCount)
                    return false;

                if (he.vertex < 0 || he.vertex >= mesh.vertexCount)
                    return false;

                if (he.face >= 0 && he.face >= mesh.faceCount)
                    return false;

                if (he.twin >= 0)
                {
                    if (he.twin >= mesh.halfEdgeCount)
                        return false;

                    if (mesh.halfEdges[he.twin].twin != i)
                        return false;
                }
            }

            for (var i = 0; i < mesh.faceCount; i++)
            {
                var face = mesh.faces[i];
                if (face.halfEdge < 0 || face.halfEdge >= mesh.halfEdgeCount)
                    return false;

                if (mesh.halfEdges[face.halfEdge].face != i)
                    return false;
            }

            return true;
        }

        [BurstCompile]
        public static int CountEdges(ref MeshData mesh)
        {
            var edgeCount = 0;

            for (var i = 0; i < mesh.halfEdgeCount; i++)
            {
                var he = mesh.halfEdges[i];

                if (he.twin < 0 || i < he.twin)
                    edgeCount++;
            }

            return edgeCount;
        }

        [BurstCompile]
        public static int GetVertexValence(ref MeshData mesh, int vertexIndex)
        {
            var vertex = mesh.vertices[vertexIndex];
            if (vertex.halfEdge < 0)
                return 0;

            var valence = 0;
            var startHeIndex = vertex.halfEdge;
            var currentHeIndex = startHeIndex;

            do
            {
                valence++;

                var he = mesh.halfEdges[currentHeIndex];

                if (he.twin >= 0)
                    currentHeIndex = mesh.halfEdges[he.twin].next;
                else
                    break;
            } while (currentHeIndex != startHeIndex && valence < 1000);

            return valence;
        }
    }
}