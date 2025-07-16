using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class IndexedMesh
    {
        float3[] vertices;
        int[][] faces;

        public IndexedMesh(float3[] vertices, int[][] faces)
        {
            this.vertices = vertices;
            this.faces = faces;
        }

        public Mesh Build()
        {
            var meshData = new Mesh();
            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}