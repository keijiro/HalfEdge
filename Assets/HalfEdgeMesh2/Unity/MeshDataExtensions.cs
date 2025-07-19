using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshDataExtensions
    {
        public static Mesh ToUnityMesh(this ref MeshData meshData, NormalGenerationMode mode = NormalGenerationMode.Smooth) =>
            MeshConversion.ToUnityMesh(ref meshData, mode);

        public static void UpdateUnityMesh(this ref MeshData meshData, Mesh mesh, NormalGenerationMode mode = NormalGenerationMode.Smooth) =>
            MeshConversion.UpdateUnityMesh(mesh, ref meshData, mode);
    }
}