using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class BoxGenerator
    {
        float width;
        float height;
        float depth;
        int subdivisions;

        public BoxGenerator(float width, float height, float depth, int subdivisions = 0)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.subdivisions = subdivisions;
        }

        public MeshData Generate()
        {
            var meshData = new MeshData();

            var hw = width * 0.5f;
            var hh = height * 0.5f;
            var hd = depth * 0.5f;

            var vertices = new float3[]
            {
                new float3(-hw, -hh, -hd),
                new float3( hw, -hh, -hd),
                new float3( hw,  hh, -hd),
                new float3(-hw,  hh, -hd),
                new float3(-hw, -hh,  hd),
                new float3( hw, -hh,  hd),
                new float3( hw,  hh,  hd),
                new float3(-hw,  hh,  hd)
            };

            var faces = new int[][]
            {
                new int[] { 0, 3, 2, 1 }, // front face (-Z)
                new int[] { 5, 6, 7, 4 }, // back face (+Z)
                new int[] { 4, 7, 3, 0 }, // left face (-X)
                new int[] { 1, 2, 6, 5 }, // right face (+X)
                new int[] { 3, 7, 6, 2 }, // top face (+Y)
                new int[] { 4, 0, 1, 5 }  // bottom face (-Y)
            };

            meshData.InitializeFromIndexedFaces(vertices, faces);

            for (int i = 0; i < subdivisions; i++)
                Subdivide(meshData);

            return meshData;
        }

        void Subdivide(MeshData meshData)
        {
            // For simplicity, rebuild the entire mesh with more subdivisions
            // Extract current face information
            var faceData = new System.Collections.Generic.List<float3[]>();

            foreach (var face in meshData.Faces)
            {
                var vertices = face.GetVertices();
                if (vertices.Count == 4)
                {
                    var v0 = vertices[0].Position;
                    var v1 = vertices[1].Position;
                    var v2 = vertices[2].Position;
                    var v3 = vertices[3].Position;

                    // Calculate subdivision points
                    var mid01 = (v0 + v1) * 0.5f;
                    var mid12 = (v1 + v2) * 0.5f;
                    var mid23 = (v2 + v3) * 0.5f;
                    var mid30 = (v3 + v0) * 0.5f;
                    var center = (v0 + v1 + v2 + v3) * 0.25f;

                    // Create 4 subdivided quads
                    faceData.Add(new float3[] { v0, mid01, center, mid30 });
                    faceData.Add(new float3[] { mid01, v1, mid12, center });
                    faceData.Add(new float3[] { center, mid12, v2, mid23 });
                    faceData.Add(new float3[] { mid30, center, mid23, v3 });
                }
            }

            // Clear and rebuild the mesh
            meshData.Clear();

            // Collect all unique vertices
            var allVertices = new List<float3>();
            var allFaces = new List<int[]>();
            var vertexToIndex = new Dictionary<float3, int>();

            foreach (var faceVerts in faceData)
            {
                var faceIndices = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    var vertex = faceVerts[i];
                    if (!vertexToIndex.ContainsKey(vertex))
                    {
                        vertexToIndex[vertex] = allVertices.Count;
                        allVertices.Add(vertex);
                    }
                    faceIndices[i] = vertexToIndex[vertex];
                }
                allFaces.Add(faceIndices);
            }

            // Rebuild the mesh using the indexed face representation
            meshData.InitializeFromIndexedFaces(allVertices.ToArray(), allFaces.ToArray());
        }
    }
}