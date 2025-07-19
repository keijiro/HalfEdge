using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshConversion
    {
        public static Mesh ToUnityMesh(ref MeshData meshData, bool calculateNormals = true)
        {
            var mesh = new Mesh();
            
            var vertices = ExtractVertices(ref meshData);
            var triangles = ExtractTriangles(ref meshData);
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            
            if (calculateNormals)
            {
                if (HasValidNormals(ref meshData))
                    mesh.normals = CalculateNormals(ref meshData);
                else
                    mesh.RecalculateNormals();
            }
            
            mesh.RecalculateBounds();
            return mesh;
        }
        
        static Vector3[] ExtractVertices(ref MeshData meshData)
        {
            var vertices = new Vector3[meshData.vertexCount];
            for (var i = 0; i < meshData.vertexCount; i++)
                vertices[i] = meshData.vertices[i].position;
            return vertices;
        }
        
        static int[] ExtractTriangles(ref MeshData meshData)
        {
            var triangles = new List<int>();
            
            for (var faceIndex = 0; faceIndex < meshData.faceCount; faceIndex++)
            {
                var face = meshData.faces[faceIndex];
                var faceVertices = GetFaceVertices(ref meshData, face.halfEdge);
                
                for (var i = 1; i < faceVertices.Count - 1; i++)
                {
                    triangles.Add(faceVertices[0]);
                    triangles.Add(faceVertices[i]);
                    triangles.Add(faceVertices[i + 1]);
                }
            }
            
            return triangles.ToArray();
        }
        
        static List<int> GetFaceVertices(ref MeshData meshData, int startHalfEdge)
        {
            var vertices = new List<int>();
            var currentHe = startHalfEdge;
            
            do
            {
                var he = meshData.halfEdges[currentHe];
                vertices.Add(he.vertex);
                currentHe = he.next;
            } while (currentHe != startHalfEdge);
            
            return vertices;
        }
        
        static bool HasValidNormals(ref MeshData meshData)
        {
            for (var i = 0; i < meshData.faceCount; i++)
            {
                var face = meshData.faces[i];
                var vertices = GetFaceVertices(ref meshData, face.halfEdge);
                if (vertices.Count < 3) 
                    return false;
            }
            return meshData.faceCount > 0;
        }
        
        static Vector3[] CalculateNormals(ref MeshData meshData)
        {
            var normals = new NativeArray<float3>(meshData.vertexCount, Allocator.Temp);
            MeshOperations.ComputeVertexNormals(ref meshData, ref normals);
            
            var result = new Vector3[meshData.vertexCount];
            for (var i = 0; i < meshData.vertexCount; i++)
                result[i] = normals[i];
                
            normals.Dispose();
            return result;
        }
    }
}