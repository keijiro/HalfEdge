using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Plane
    {
        float2 size;
        int2 segments;

        public Plane(float2 size, int2 segments)
        {
            this.size = size;
            this.segments = math.max(segments, 1);
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            var halfSize = size * 0.5f;
            var widthStep = size.x / segments.x;
            var heightStep = size.y / segments.y;

            var vertices = new float3[(segments.x + 1) * (segments.y + 1)];
            var index = 0;

            for (int y = 0; y <= segments.y; y++)
            {
                for (int x = 0; x <= segments.x; x++)
                {
                    vertices[index++] = new float3(
                        x * widthStep - halfSize.x,
                        y * heightStep - halfSize.y,
                        0
                    );
                }
            }

            var faces = new int[segments.x * segments.y][];
            index = 0;

            for (int y = 0; y < segments.y; y++)
            {
                for (int x = 0; x < segments.x; x++)
                {
                    var i0 = y * (segments.x + 1) + x;
                    var i1 = i0 + 1;
                    var i2 = i0 + (segments.x + 1) + 1;
                    var i3 = i0 + (segments.x + 1);

                    faces[index++] = new int[] { i3, i0, i1, i2 };
                }
            }

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}