using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh
{
    public class Face
    {
        public HalfEdge HalfEdge { get; set; }
        
        public float3 Normal
        {
            get
            {
                var vertices = GetVertices();
                if (vertices.Count < 3) return float3.zero;
                
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;
                
                return math.normalize(math.cross(v1 - v0, v2 - v0));
            }
        }
        
        public List<Vertex> GetVertices()
        {
            var vertices = new List<Vertex>();
            if (HalfEdge == null) return vertices;
            
            var start = HalfEdge;
            var current = start;
            do
            {
                vertices.Add(current.Origin);
                current = current.Next;
            } while (current != start && current != null);
            
            return vertices;
        }
        
        public List<HalfEdge> GetHalfEdges()
        {
            var halfEdges = new List<HalfEdge>();
            if (HalfEdge == null) return halfEdges;
            
            var start = HalfEdge;
            var current = start;
            do
            {
                halfEdges.Add(current);
                current = current.Next;
            } while (current != start && current != null);
            
            return halfEdges;
        }
    }
}