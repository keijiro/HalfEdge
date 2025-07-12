using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Generators
{
    public class SphereGenerator
    {
        public enum SphereType
        {
            UV,
            Icosphere
        }
        
        float radius;
        int subdivisions;
        SphereType type;
        
        public SphereGenerator(float radius, int subdivisions, SphereType type = SphereType.UV)
        {
            this.radius = radius;
            this.subdivisions = subdivisions;
            this.type = type;
        }
        
        public MeshData Generate()
        {
            if (type == SphereType.Icosphere)
                return GenerateIcosphere();
            else
                return GenerateUVSphere();
        }
        
        MeshData GenerateUVSphere()
        {
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            
            var segments = 16 * (int)math.pow(2, subdivisions);
            var rings = segments / 2;
            
            for (int lat = 0; lat <= rings; lat++)
            {
                var theta = lat * math.PI / rings;
                var sinTheta = math.sin(theta);
                var cosTheta = math.cos(theta);
                
                for (int lon = 0; lon <= segments; lon++)
                {
                    var phi = lon * 2 * math.PI / segments;
                    var sinPhi = math.sin(phi);
                    var cosPhi = math.cos(phi);
                    
                    var x = cosPhi * sinTheta;
                    var y = cosTheta;
                    var z = sinPhi * sinTheta;
                    
                    vertices.Add(new float3(x, y, z) * radius);
                }
            }
            
            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    var i0 = lat * (segments + 1) + lon;
                    var i1 = i0 + segments + 1;
                    var i2 = i1 + 1;
                    var i3 = i0 + 1;
                    
                    if (lat == 0)
                    {
                        faces.Add(new int[] { i0, i1, i2 });
                    }
                    else if (lat == rings - 1)
                    {
                        faces.Add(new int[] { i0, i1, i3 });
                    }
                    else
                    {
                        faces.Add(new int[] { i0, i1, i2, i3 });
                    }
                }
            }
            
            var meshData = new MeshData();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            
            return meshData;
        }
        
        MeshData GenerateIcosphere()
        {
            var t = (1f + math.sqrt(5f)) / 2f;
            
            var vertices = new List<float3>
            {
                math.normalize(new float3(-1,  t,  0)) * radius,
                math.normalize(new float3( 1,  t,  0)) * radius,
                math.normalize(new float3(-1, -t,  0)) * radius,
                math.normalize(new float3( 1, -t,  0)) * radius,
                
                math.normalize(new float3( 0, -1,  t)) * radius,
                math.normalize(new float3( 0,  1,  t)) * radius,
                math.normalize(new float3( 0, -1, -t)) * radius,
                math.normalize(new float3( 0,  1, -t)) * radius,
                
                math.normalize(new float3( t,  0, -1)) * radius,
                math.normalize(new float3( t,  0,  1)) * radius,
                math.normalize(new float3(-t,  0, -1)) * radius,
                math.normalize(new float3(-t,  0,  1)) * radius
            };
            
            var faces = new List<int[]>
            {
                new int[] { 0, 11, 5 },
                new int[] { 0, 5, 1 },
                new int[] { 0, 1, 7 },
                new int[] { 0, 7, 10 },
                new int[] { 0, 10, 11 },
                
                new int[] { 1, 5, 9 },
                new int[] { 5, 11, 4 },
                new int[] { 11, 10, 2 },
                new int[] { 10, 7, 6 },
                new int[] { 7, 1, 8 },
                
                new int[] { 3, 9, 4 },
                new int[] { 3, 4, 2 },
                new int[] { 3, 2, 6 },
                new int[] { 3, 6, 8 },
                new int[] { 3, 8, 9 },
                
                new int[] { 4, 9, 5 },
                new int[] { 2, 4, 11 },
                new int[] { 6, 2, 10 },
                new int[] { 8, 6, 7 },
                new int[] { 9, 8, 1 }
            };
            
            var meshData = new MeshData();
            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            
            for (int i = 0; i < subdivisions; i++)
                SubdivideIcosphere(meshData);
            
            return meshData;
        }
        
        void SubdivideIcosphere(MeshData meshData)
        {
            var originalFaces = meshData.Faces.ToArray();
            var edgeMidpoints = new Dictionary<Edge, Vertex>();
            
            foreach (var edge in meshData.Edges.ToArray())
            {
                var midPos = math.normalize((edge.V0.Position + edge.V1.Position) * 0.5f) * radius;
                var midpoint = meshData.SplitEdge(edge);
                midpoint.Origin.Position = midPos;
                edgeMidpoints[edge] = midpoint.Origin;
            }
            
            foreach (var face in originalFaces)
            {
                meshData.TriangulateFace(face);
            }
        }
    }
}