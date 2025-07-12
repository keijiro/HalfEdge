using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public class TwistModifier
    {
        float3 axis;
        float3 center;
        float angle;
        float falloffDistance;
        
        public TwistModifier(float3 axis, float3 center, float angle, float falloffDistance = 0f)
        {
            this.axis = math.normalize(axis);
            this.center = center;
            this.angle = angle;
            this.falloffDistance = falloffDistance;
        }
        
        public void Apply(MeshData mesh)
        {
            foreach (var vertex in mesh.Vertices)
            {
                var relativePos = vertex.Position - center;
                var axisProjection = math.dot(relativePos, axis) * axis;
                var perpendicular = relativePos - axisProjection;
                
                var distance = math.length(axisProjection);
                var twistAmount = angle;
                
                if (falloffDistance > 0f)
                {
                    var falloff = math.saturate(distance / falloffDistance);
                    twistAmount *= falloff;
                }
                
                var rotation = quaternion.AxisAngle(axis, twistAmount);
                var rotatedPerpendicular = math.rotate(rotation, perpendicular);
                
                vertex.Position = center + axisProjection + rotatedPerpendicular;
            }
        }
    }
}