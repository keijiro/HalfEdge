using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Box
    {
        float width;
        float height;
        float depth;
        int3 segments;

        public Box(float width, float height, float depth, int3 segments)
        {
            this.width = width;
            this.height = height;
            this.depth = depth;
            this.segments = math.max(segments, 1);
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();
            var vertexMap = new Dictionary<float3, int>();

            var hw = width * 0.5f;
            var hh = height * 0.5f;
            var hd = depth * 0.5f;

            // Front face (-Z)
            CreateFace(vertices, faces, vertexMap,
                new float3(-hw, -hh, -hd), new float3(-hw, hh, -hd),
                new float3(hw, hh, -hd), new float3(hw, -hh, -hd),
                segments.x, segments.y);

            // Back face (+Z)
            CreateFace(vertices, faces, vertexMap,
                new float3(hw, -hh, hd), new float3(hw, hh, hd),
                new float3(-hw, hh, hd), new float3(-hw, -hh, hd),
                segments.x, segments.y);

            // Left face (-X)
            CreateFace(vertices, faces, vertexMap,
                new float3(-hw, -hh, hd), new float3(-hw, hh, hd),
                new float3(-hw, hh, -hd), new float3(-hw, -hh, -hd),
                segments.z, segments.y);

            // Right face (+X)
            CreateFace(vertices, faces, vertexMap,
                new float3(hw, -hh, -hd), new float3(hw, hh, -hd),
                new float3(hw, hh, hd), new float3(hw, -hh, hd),
                segments.z, segments.y);

            // Top face (+Y)
            CreateFace(vertices, faces, vertexMap,
                new float3(-hw, hh, -hd), new float3(-hw, hh, hd),
                new float3(hw, hh, hd), new float3(hw, hh, -hd),
                segments.x, segments.z);

            // Bottom face (-Y)
            CreateFace(vertices, faces, vertexMap,
                new float3(-hw, -hh, hd), new float3(-hw, -hh, -hd),
                new float3(hw, -hh, -hd), new float3(hw, -hh, hd),
                segments.x, segments.z);

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }

        void CreateFace(List<float3> vertices, List<int[]> faces, Dictionary<float3, int> vertexMap,
            float3 corner0, float3 corner1, float3 corner2, float3 corner3,
            int segmentsU, int segmentsV)
        {
            var faceVertices = new int[(segmentsU + 1) * (segmentsV + 1)];

            // Generate vertices
            for (int v = 0; v <= segmentsV; v++)
            {
                var t = v / (float)segmentsV;
                var p0 = math.lerp(corner0, corner3, t);
                var p1 = math.lerp(corner1, corner2, t);

                for (int u = 0; u <= segmentsU; u++)
                {
                    var s = u / (float)segmentsU;
                    var vertex = math.lerp(p0, p1, s);
                    
                    // Round to avoid floating point precision issues
                    vertex = math.round(vertex * 10000f) / 10000f;

                    int index;
                    if (!vertexMap.TryGetValue(vertex, out index))
                    {
                        index = vertices.Count;
                        vertices.Add(vertex);
                        vertexMap[vertex] = index;
                    }
                    
                    faceVertices[v * (segmentsU + 1) + u] = index;
                }
            }

            // Generate faces
            for (int v = 0; v < segmentsV; v++)
            {
                for (int u = 0; u < segmentsU; u++)
                {
                    var i0 = faceVertices[v * (segmentsU + 1) + u];
                    var i1 = faceVertices[v * (segmentsU + 1) + u + 1];
                    var i2 = faceVertices[(v + 1) * (segmentsU + 1) + u + 1];
                    var i3 = faceVertices[(v + 1) * (segmentsU + 1) + u];

                    faces.Add(new int[] { i0, i1, i2, i3 });
                }
            }
        }
    }
}