using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public class ScaleModifier
    {
        float3 scale;
        float3 center;

        public ScaleModifier(float uniformScale, float3 center = default)
        {
            this.scale = new float3(uniformScale, uniformScale, uniformScale);
            this.center = center;
        }

        public ScaleModifier(float3 scale, float3 center = default)
        {
            this.scale = scale;
            this.center = center;
        }

        public void Apply(MeshData mesh)
        {
            foreach (var vertex in mesh.Vertices)
            {
                var relativePos = vertex.Position - center;
                vertex.Position = center + relativePos * scale;
            }
        }
    }
}