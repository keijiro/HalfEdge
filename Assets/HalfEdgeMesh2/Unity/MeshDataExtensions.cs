using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshDataExtensions
    {
        public static Mesh ToUnityMesh(this ref MeshData meshData) =>
            MeshConversion.ToUnityMesh(ref meshData);

        public static void UpdateUnityMesh(this ref MeshData meshData, Mesh mesh) =>
            MeshConversion.UpdateUnityMesh(mesh, ref meshData);
    }
}