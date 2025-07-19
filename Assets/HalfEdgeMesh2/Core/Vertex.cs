using Unity.Mathematics;

namespace HalfEdgeMesh2
{
    public struct Vertex
    {
        public float3 position;
        public int halfEdge;
        
        public Vertex(float3 position) => (this.position, halfEdge) = (position, -1);
        
        public Vertex(float3 position, int halfEdge) => (this.position, this.halfEdge) = (position, halfEdge);
    }
}