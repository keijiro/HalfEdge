namespace HalfEdgeMesh2
{
    public struct HalfEdge
    {
        public int next;
        public int twin;
        public int vertex;
        public int face;

        public HalfEdge(int next, int twin, int vertex, int face) =>
            (this.next, this.twin, this.vertex, this.face) = (next, twin, vertex, face);

        public static HalfEdge Boundary(int next, int twin, int vertex) =>
            new HalfEdge(next, twin, vertex, -1);
    }
}