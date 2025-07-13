namespace HalfEdgeMesh
{
    public class Edge
    {
        public HalfEdge HalfEdge { get; set; }

        public Edge(HalfEdge halfEdge)
        {
            HalfEdge = halfEdge;
        }

        public Vertex V0 => HalfEdge.Origin;
        public Vertex V1 => HalfEdge.Twin?.Origin;
    }
}