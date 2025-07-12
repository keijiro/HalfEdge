using Unity.Mathematics;

namespace HalfEdgeMesh
{
    public class Vertex
    {
        public float3 Position { get; set; }
        public HalfEdge HalfEdge { get; set; }
        
        public Vertex(float3 position)
        {
            Position = position;
        }
    }
}