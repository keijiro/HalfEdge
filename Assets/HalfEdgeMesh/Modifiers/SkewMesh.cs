using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Modifiers
{
    public class SkewMesh
    {
        float angle;
        float3 direction;

        public SkewMesh(float angle, float3 direction)
        {
            this.angle = angle;
            this.direction = math.normalize(direction);
        }

        public void Apply(Mesh mesh)
        {
            if (angle == 0f) return;

            var newPositions = new Dictionary<Vertex, float3>();
            var skewDirection = math.normalize(direction);
            var angleRad = angle; // Already in radians from GeneratorSample
            
            // Calculate mesh bounds to determine skew axis
            var bounds = CalculateBounds(mesh);
            var center = (bounds.min + bounds.max) * 0.5f;
            var size = bounds.max - bounds.min;
            
            // Determine the primary axis for skewing (largest dimension)
            var primaryAxis = 0; // X = 0, Y = 1, Z = 2
            if (size.y > size.x && size.y > size.z) primaryAxis = 1;
            else if (size.z > size.x && size.z > size.y) primaryAxis = 2;
            
            // Transform each vertex
            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                var relativePos = pos - center;
                
                // Apply skew based on position along primary axis
                float skewFactor = 0;
                switch (primaryAxis)
                {
                    case 0: // X-axis
                        skewFactor = relativePos.x / (size.x * 0.5f);
                        break;
                    case 1: // Y-axis
                        skewFactor = relativePos.y / (size.y * 0.5f);
                        break;
                    case 2: // Z-axis
                        skewFactor = relativePos.z / (size.z * 0.5f);
                        break;
                }
                
                // Apply skew transformation
                var skewOffset = skewDirection * (skewFactor * math.tan(angleRad) * math.length(size) * 0.1f);
                var newPos = pos + skewOffset;
                
                newPositions[vertex] = newPos;
            }
            
            // Apply new positions
            foreach (var kvp in newPositions)
                kvp.Key.Position = kvp.Value;
        }
        
        static (float3 min, float3 max) CalculateBounds(Mesh mesh)
        {
            if (mesh.Vertices.Count == 0)
                return (float3.zero, float3.zero);
            
            var min = mesh.Vertices[0].Position;
            var max = mesh.Vertices[0].Position;
            
            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                min = math.min(min, pos);
                max = math.max(max, pos);
            }
            
            return (min, max);
        }
    }
}