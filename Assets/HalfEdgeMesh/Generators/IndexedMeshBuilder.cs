using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class IndexedMeshBuilder
    {
        float3[] vertices;
        int[][] faces;

        public IndexedMeshBuilder(float3[] vertices, int[][] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        public MeshData Build()
        {
            var meshData = new MeshData();
            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}