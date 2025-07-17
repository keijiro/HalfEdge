using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Plane
    {
        int widthSegments;
        int heightSegments;
        float size;

        public Plane(int widthSegments, int heightSegments, float size)
        {
            this.widthSegments = widthSegments;
            this.heightSegments = heightSegments;
            this.size = size;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();

            var halfSize = size * 0.5f;
            var widthStep = size / widthSegments;
            var heightStep = size / heightSegments;

            var vertices = new float3[(widthSegments + 1) * (heightSegments + 1)];
            var index = 0;

            for (int y = 0; y <= heightSegments; y++)
            {
                for (int x = 0; x <= widthSegments; x++)
                {
                    vertices[index++] = new float3(
                        x * widthStep - halfSize,
                        0,
                        y * heightStep - halfSize
                    );
                }
            }

            var faces = new int[widthSegments * heightSegments][];
            index = 0;

            for (int y = 0; y < heightSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    var i0 = y * (widthSegments + 1) + x;
                    var i1 = i0 + 1;
                    var i2 = i0 + (widthSegments + 1) + 1;
                    var i3 = i0 + (widthSegments + 1);

                    faces[index++] = new int[] { i0, i3, i2, i1 };
                }
            }

            meshData.InitializeFromIndexedFaces(vertices, faces);
            return meshData;
        }
    }
}