using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public class StretchMesh
    {
        float3 scale;
        float3 center;

        public StretchMesh(float uniformScale, float3 center = default)
        {
            this.scale = new float3(uniformScale, uniformScale, uniformScale);
            this.center = center;
        }

        public StretchMesh(float3 scale, float3 center = default)
        {
            this.scale = scale;
            this.center = center;
        }

        public void Apply(Mesh mesh)
        {
            foreach (var vertex in mesh.Vertices)
            {
                var relativePos = vertex.Position - center;
                vertex.Position = center + relativePos * scale;
            }
        }
    }
}