using UnityEngine;

namespace HalfEdgeMesh2.Unity
{
    public static class MeshDataExtensions
    {
        public static Mesh ToUnityMesh(this ref MeshData meshData, bool calculateNormals = true) =>
            MeshConversion.ToUnityMesh(ref meshData, calculateNormals);
    }
}