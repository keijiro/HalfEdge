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
                new int[] { 0, 1, 2, 3 },
                new int[] { 5, 4, 7, 6 },
                new int[] { 4, 0, 3, 7 },
                new int[] { 1, 5, 6, 2 },
                new int[] { 3, 2, 6, 7 },
                new int[] { 4, 5, 1, 0 }
            };
            
            meshData.InitializeFromIndexedFaces(vertices, faces);
            
            for (int i = 0; i < subdivisions; i++)
                Subdivide(meshData);
            
            return meshData;
        }
        
        void Subdivide(MeshData meshData)
        {
            var originalFaces = meshData.Faces.ToArray();
            var edgeMidpoints = new System.Collections.Generic.Dictionary<Edge, Vertex>();
            
            foreach (var edge in meshData.Edges.ToArray())
            {
                var midpoint = meshData.SplitEdge(edge);
                edgeMidpoints[edge] = midpoint.Origin;
            }
            
            foreach (var face in originalFaces)
            {
                var vertices = face.GetVertices();
                if (vertices.Count == 4)
                {
                    var center = new Vertex((vertices[0].Position + vertices[1].Position + 
                                           vertices[2].Position + vertices[3].Position) * 0.25f);
                    meshData.Vertices.Add(center);
                    
                    meshData.Faces.Remove(face);
                    
                    for (int i = 0; i < 4; i++)
                    {
                        var v0 = vertices[i];
                        var v1 = vertices[(i + 1) % 4];
                        
                        Vertex mid0 = null, mid1 = null;
                        foreach (var kvp in edgeMidpoints)
                        {
                            var edge = kvp.Key;
                            if ((edge.V0 == v0 && edge.V1 == v1) || (edge.V0 == v1 && edge.V1 == v0))
                                mid0 = kvp.Value;
                            
                            var v3 = vertices[(i + 3) % 4];
                            if ((edge.V0 == v3 && edge.V1 == v0) || (edge.V0 == v0 && edge.V1 == v3))
                                mid1 = kvp.Value;
                        }
                        
                        if (mid0 != null && mid1 != null)
                        {
                            var newVertices = new float3[] { v0.Position, mid0.Position, center.Position, mid1.Position };
                            var newFace = new int[][] { new int[] { 0, 1, 2, 3 } };
                            
                            var tempMesh = new MeshData();
                            tempMesh.InitializeFromIndexedFaces(newVertices, newFace);
                        }
                    }
                }
            }
        }
    }
}