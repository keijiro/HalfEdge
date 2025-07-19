using System;
using Unity.Collections;

namespace HalfEdgeMesh2
{
    public struct MeshData : IDisposable
    {
        public NativeArray<Vertex> vertices;
        public NativeArray<HalfEdge> halfEdges;
        public NativeArray<Face> faces;
        
        public int vertexCount;
        public int halfEdgeCount;
        public int faceCount;
        
        public bool IsCreated => vertices.IsCreated;
        
        public MeshData(int maxVertices, int maxHalfEdges, int maxFaces, Allocator allocator)
        {
            vertices = new NativeArray<Vertex>(maxVertices, allocator);
            halfEdges = new NativeArray<HalfEdge>(maxHalfEdges, allocator);
            faces = new NativeArray<Face>(maxFaces, allocator);
            
            vertexCount = 0;
            halfEdgeCount = 0;
            faceCount = 0;
        }
        
        public void Dispose()
        {
            if (vertices.IsCreated)
                vertices.Dispose();
            if (halfEdges.IsCreated)
                halfEdges.Dispose();
            if (faces.IsCreated)
                faces.Dispose();
        }
        
        public int AddVertex(Vertex vertex)
        {
            if (vertexCount >= vertices.Length)
                throw new InvalidOperationException("Vertex array is full");
                
            var index = vertexCount;
            vertices[index] = vertex;
            vertexCount++;
            return index;
        }
        
        public int AddHalfEdge(HalfEdge halfEdge)
        {
            if (halfEdgeCount >= halfEdges.Length)
                throw new InvalidOperationException("HalfEdge array is full");
                
            var index = halfEdgeCount;
            halfEdges[index] = halfEdge;
            halfEdgeCount++;
            return index;
        }
        
        public int AddFace(Face face)
        {
            if (faceCount >= faces.Length)
                throw new InvalidOperationException("Face array is full");
                
            var index = faceCount;
            faces[index] = face;
            faceCount++;
            return index;
        }
        
        public MeshData Compact(Allocator allocator)
        {
            var result = new MeshData
            {
                vertices = new NativeArray<Vertex>(vertexCount, allocator),
                halfEdges = new NativeArray<HalfEdge>(halfEdgeCount, allocator),
                faces = new NativeArray<Face>(faceCount, allocator),
                vertexCount = vertexCount,
                halfEdgeCount = halfEdgeCount,
                faceCount = faceCount
            };
            
            NativeArray<Vertex>.Copy(vertices, result.vertices, vertexCount);
            NativeArray<HalfEdge>.Copy(halfEdges, result.halfEdges, halfEdgeCount);
            NativeArray<Face>.Copy(faces, result.faces, faceCount);
            
            return result;
        }
    }
}