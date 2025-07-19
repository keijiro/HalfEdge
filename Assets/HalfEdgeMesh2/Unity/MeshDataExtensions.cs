using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshDataExtensions
    {
        public static Mesh ToUnityMesh(this ref MeshData meshData, bool calculateNormals = true) =>
            MeshConversion.ToUnityMesh(ref meshData, calculateNormals);
            
        public static void UpdateUnityMesh(this ref MeshData meshData, Mesh mesh, bool calculateNormals = true) =>
            MeshConversion.UpdateUnityMesh(mesh, ref meshData, calculateNormals);
    }
}