using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class BasicUsageExample
    {
        [Test]
        public void Example_CreateSimpleBox()
        {
            var builder = new MeshBuilder();
            
            // Define box vertices
            var size = new float3(1, 1, 1);
            var halfSize = size * 0.5f;
            
            // Add 8 vertices of a box
            builder.AddVertex(new float3(-halfSize.x, -halfSize.y, -halfSize.z)); // 0
            builder.AddVertex(new float3( halfSize.x, -halfSize.y, -halfSize.z)); // 1
            builder.AddVertex(new float3( halfSize.x,  halfSize.y, -halfSize.z)); // 2
            builder.AddVertex(new float3(-halfSize.x,  halfSize.y, -halfSize.z)); // 3
            builder.AddVertex(new float3(-halfSize.x, -halfSize.y,  halfSize.z)); // 4
            builder.AddVertex(new float3( halfSize.x, -halfSize.y,  halfSize.z)); // 5
            builder.AddVertex(new float3( halfSize.x,  halfSize.y,  halfSize.z)); // 6
            builder.AddVertex(new float3(-halfSize.x,  halfSize.y,  halfSize.z)); // 7
            
            // Add 6 faces (each face as 2 triangles)
            // Front face (Z-)
            builder.AddFace(0, 1, 2);
            builder.AddFace(0, 2, 3);
            
            // Back face (Z+)
            builder.AddFace(5, 4, 7);
            builder.AddFace(5, 7, 6);
            
            // Left face (X-)
            builder.AddFace(4, 0, 3);
            builder.AddFace(4, 3, 7);
            
            // Right face (X+)
            builder.AddFace(1, 5, 6);
            builder.AddFace(1, 6, 2);
            
            // Bottom face (Y-)
            builder.AddFace(4, 5, 1);
            builder.AddFace(4, 1, 0);
            
            // Top face (Y+)
            builder.AddFace(3, 2, 6);
            builder.AddFace(3, 6, 7);
            
            // Build the mesh
            var mesh = builder.Build(Allocator.Temp);
            try
            {
                Assert.AreEqual(8, mesh.vertexCount);
                Assert.AreEqual(36, mesh.halfEdgeCount); // 12 triangles * 3 half-edges each
                Assert.AreEqual(12, mesh.faceCount);
                
                // Validate mesh structure
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
                
                // Count edges
                var edgeCount = MeshOperations.CountEdges(ref mesh);
                Assert.AreEqual(18, edgeCount); // Box has 12 edges + 6 diagonal edges from triangulation
                
                // Compute bounds
                MeshOperations.ComputeBounds(ref mesh, out var boundsCenter, out var boundsSize);
                Assert.AreEqual(Vector3.zero, (Vector3)boundsCenter);
                Assert.AreEqual(Vector3.one, (Vector3)boundsSize);
                
                // Compute normals
                var faceNormals = new NativeArray<float3>(mesh.faceCount, Allocator.Temp);
                try
                {
                    MeshOperations.ComputeFaceNormals(ref mesh, ref faceNormals);
                    
                    // Check that we have valid normals
                    for (int i = 0; i < mesh.faceCount; i++)
                    {
                        var length = math.length(faceNormals[i]);
                        Assert.AreEqual(1f, length, 0.001f);
                    }
                }
                finally
                {
                    faceNormals.Dispose();
                }
                
                Debug.Log($"Successfully created a box mesh with {mesh.vertexCount} vertices, {edgeCount} edges, and {mesh.faceCount} faces");
            }
            finally
            {
                mesh.Dispose();
            }
        }
    }
}