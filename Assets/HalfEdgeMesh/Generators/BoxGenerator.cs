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
            var originalEdges = meshData.Edges.ToArray();
            var edgeMidpoints = new System.Collections.Generic.Dictionary<System.Tuple<Vertex, Vertex>, Vertex>();
            
            // Split all edges and store midpoints
            foreach (var edge in originalEdges)
            {
                var v0 = edge.HalfEdge.Origin;
                var v1 = edge.HalfEdge.Destination;
                var midpoint = meshData.SplitEdge(edge);
                
                // Store both direction mappings for easier lookup
                var key1 = System.Tuple.Create(v0, v1);
                var key2 = System.Tuple.Create(v1, v0);
                edgeMidpoints[key1] = midpoint.Origin;
                edgeMidpoints[key2] = midpoint.Origin;
            }
            
            // Clear all existing data to rebuild from scratch
            meshData.HalfEdges.Clear();
            meshData.Edges.Clear();
            meshData.Faces.Clear();
            
            // Reset vertex half-edge references
            foreach (var vertex in meshData.Vertices)
                vertex.HalfEdge = null;
            
            // Create new faces for each original face
            foreach (var face in originalFaces)
            {
                var vertices = face.GetVertices();
                if (vertices.Count == 4)
                {
                    // Calculate face center
                    var center = new Vertex((vertices[0].Position + vertices[1].Position + 
                                           vertices[2].Position + vertices[3].Position) * 0.25f);
                    meshData.Vertices.Add(center);
                    
                    // Create 4 new quad faces
                    for (int i = 0; i < 4; i++)
                    {
                        var v0 = vertices[i];
                        var v1 = vertices[(i + 1) % 4];
                        var v3 = vertices[(i + 3) % 4];
                        
                        // Find edge midpoints
                        var mid01 = edgeMidpoints[System.Tuple.Create(v0, v1)];
                        var mid30 = edgeMidpoints[System.Tuple.Create(v3, v0)];
                        
                        // Create new face with vertices in correct order
                        var newFaceVertices = new float3[] { v0.Position, mid01.Position, center.Position, mid30.Position };
                        var newFaceIndices = new int[][] { new int[] { 0, 1, 2, 3 } };
                        
                        // We need to manually create the face to use existing vertices
                        var vertexList = new Vertex[] { v0, mid01, center, mid30 };
                        CreateFaceFromVertices(meshData, vertexList);
                    }
                }
            }
            
            // Rebuild edge connectivity
            RebuildEdgeConnectivity(meshData);
        }
        
        void CreateFaceFromVertices(MeshData meshData, Vertex[] vertices)
        {
            var face = new Face();
            meshData.Faces.Add(face);
            
            var halfEdges = new HalfEdge[vertices.Length];
            
            // Create half-edges for this face
            for (int i = 0; i < vertices.Length; i++)
            {
                var halfEdge = new HalfEdge
                {
                    Origin = vertices[i],
                    Face = face
                };
                
                meshData.HalfEdges.Add(halfEdge);
                halfEdges[i] = halfEdge;
                
                if (vertices[i].HalfEdge == null)
                    vertices[i].HalfEdge = halfEdge;
            }
            
            // Set next pointers
            for (int i = 0; i < halfEdges.Length; i++)
            {
                halfEdges[i].Next = halfEdges[(i + 1) % halfEdges.Length];
            }
            
            face.HalfEdge = halfEdges[0];
        }
        
        void RebuildEdgeConnectivity(MeshData meshData)
        {
            var halfEdgeMap = new System.Collections.Generic.Dictionary<System.Tuple<Vertex, Vertex>, HalfEdge>();
            
            // First pass: collect all half-edges
            foreach (var he in meshData.HalfEdges)
            {
                var key = System.Tuple.Create(he.Origin, he.Destination);
                halfEdgeMap[key] = he;
            }
            
            // Second pass: set twin relationships and create edges
            var processedPairs = new System.Collections.Generic.HashSet<System.Tuple<Vertex, Vertex>>();
            
            foreach (var kvp in halfEdgeMap)
            {
                var key = kvp.Key;
                var he = kvp.Value;
                var reverseKey = System.Tuple.Create(key.Item2, key.Item1);
                
                if (processedPairs.Contains(key) || processedPairs.Contains(reverseKey))
                    continue;
                
                if (halfEdgeMap.TryGetValue(reverseKey, out var twin))
                {
                    he.Twin = twin;
                    twin.Twin = he;
                    
                    var edge = new Edge(he);
                    meshData.Edges.Add(edge);
                    
                    processedPairs.Add(key);
                    processedPairs.Add(reverseKey);
                }
            }
        }
    }
}