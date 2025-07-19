namespace HalfEdgeMesh2
{
    public struct Face
    {
        public int halfEdge;
        
        public Face(int halfEdge) => this.halfEdge = halfEdge;
    }
}