namespace HalfEdgeMesh
{
    public class HalfEdge
    {
        public HalfEdge Next { get; set; }
        public HalfEdge Twin { get; set; }
        public Vertex Origin { get; set; }
        public Face Face { get; set; }

        public HalfEdge Previous
        {
            get
            {
                var current = this;
                while (current.Next != this)
                    current = current.Next;
                return current;
            }
        }

        public Vertex Destination => Next?.Origin;
    }
}