using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class MeshConversionTests
    {
        [Test]
        public void ToUnityMesh_Triangle_CreatesValidUnityMesh()
        {
            var builder = new MeshBuilder();
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0, 0, 1));
            builder.AddFace(0, 2, 1);
            
            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh();
                
                Assert.AreEqual(3, unityMesh.vertexCount);
                Assert.AreEqual(3, unityMesh.triangles.Length);
                
                var vertices = unityMesh.vertices;
                Assert.AreEqual(new Vector3(0, 0, 0), vertices[0]);
                Assert.AreEqual(new Vector3(1, 0, 0), vertices[1]);
                Assert.AreEqual(new Vector3(0, 0, 1), vertices[2]);
                
                var triangles = unityMesh.triangles;
                Assert.AreEqual(0, triangles[0]);
                Assert.AreEqual(2, triangles[1]);
                Assert.AreEqual(1, triangles[2]);
                
                Assert.IsNotNull(unityMesh.normals);
                Assert.AreEqual(3, unityMesh.normals.Length);
                
                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
        
        [Test]
        public void ToUnityMesh_Quad_CreatesCorrectTriangulation()
        {
            var builder = new MeshBuilder();
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(1, 1, 0));
            builder.AddVertex(new float3(0, 1, 0));
            builder.AddFace(0, 1, 2, 3);
            
            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh();
                
                Assert.AreEqual(4, unityMesh.vertexCount);
                Assert.AreEqual(6, unityMesh.triangles.Length); // 2 triangles * 3 vertices each
                
                var triangles = unityMesh.triangles;
                Assert.AreEqual(0, triangles[0]);
                Assert.AreEqual(1, triangles[1]);
                Assert.AreEqual(2, triangles[2]);
                Assert.AreEqual(0, triangles[3]);
                Assert.AreEqual(2, triangles[4]);
                Assert.AreEqual(3, triangles[5]);
                
                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
        
        [Test]
        public void ToUnityMesh_WithoutNormals_SkipsNormalCalculation()
        {
            var builder = new MeshBuilder();
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0, 0, 1));
            builder.AddFace(0, 2, 1);
            
            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh(calculateNormals: false);
                
                Assert.AreEqual(3, unityMesh.vertexCount);
                Assert.That(unityMesh.normals == null || unityMesh.normals.Length == 0, "Normals should be null or empty when not calculated");
                
                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
        
        [Test]
        public void ToUnityMesh_Box_CreatesCorrectMesh()
        {
            var builder = new MeshBuilder();
            
            var size = new float3(1, 1, 1);
            var halfSize = size * 0.5f;
            
            builder.AddVertex(new float3(-halfSize.x, -halfSize.y, -halfSize.z)); // 0
            builder.AddVertex(new float3( halfSize.x, -halfSize.y, -halfSize.z)); // 1
            builder.AddVertex(new float3( halfSize.x,  halfSize.y, -halfSize.z)); // 2
            builder.AddVertex(new float3(-halfSize.x,  halfSize.y, -halfSize.z)); // 3
            builder.AddVertex(new float3(-halfSize.x, -halfSize.y,  halfSize.z)); // 4
            builder.AddVertex(new float3( halfSize.x, -halfSize.y,  halfSize.z)); // 5
            builder.AddVertex(new float3( halfSize.x,  halfSize.y,  halfSize.z)); // 6
            builder.AddVertex(new float3(-halfSize.x,  halfSize.y,  halfSize.z)); // 7
            
            builder.AddFace(0, 1, 2, 3);  // Front
            builder.AddFace(5, 4, 7, 6);  // Back
            builder.AddFace(4, 0, 3, 7);  // Left
            builder.AddFace(1, 5, 6, 2);  // Right
            builder.AddFace(4, 5, 1, 0);  // Bottom
            builder.AddFace(3, 2, 6, 7);  // Top
            
            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh();
                
                Assert.AreEqual(8, unityMesh.vertexCount);
                Assert.AreEqual(36, unityMesh.triangles.Length); // 6 faces * 2 triangles * 3 vertices
                
                var bounds = unityMesh.bounds;
                Assert.AreEqual(Vector3.zero, bounds.center, "Bounds center should be at origin");
                Assert.AreEqual(Vector3.one, bounds.size, "Bounds size should be unit cube");
                
                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
        
        [Test]
        public void UpdateUnityMesh_Triangle_UpdatesExistingMesh()
        {
            var builder = new MeshBuilder();
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0, 0, 1));
            builder.AddFace(0, 2, 1);
            
            var meshData = builder.Build(Allocator.Temp);
            var unityMesh = new Mesh();
            
            try
            {
                meshData.UpdateUnityMesh(unityMesh);
                
                Assert.AreEqual(3, unityMesh.vertexCount);
                Assert.AreEqual(3, unityMesh.triangles.Length);
                
                var vertices = unityMesh.vertices;
                Assert.AreEqual(new Vector3(0, 0, 0), vertices[0]);
                Assert.AreEqual(new Vector3(1, 0, 0), vertices[1]);
                Assert.AreEqual(new Vector3(0, 0, 1), vertices[2]);
                
                var triangles = unityMesh.triangles;
                Assert.AreEqual(0, triangles[0]);
                Assert.AreEqual(2, triangles[1]);
                Assert.AreEqual(1, triangles[2]);
                
                Assert.IsNotNull(unityMesh.normals);
                Assert.AreEqual(3, unityMesh.normals.Length);
                
                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
        
        [Test]
        public void UpdateUnityMesh_ReuseMesh_NoDestroyRequired()
        {
            var unityMesh = new Mesh();
            
            for (var i = 0; i < 3; i++)
            {
                var builder = new MeshBuilder();
                builder.AddVertex(new float3(0, 0, 0));
                builder.AddVertex(new float3(1, 0, 0));
                builder.AddVertex(new float3(0, 0, 1 + i)); // Different Z position each iteration
                builder.AddFace(0, 2, 1);
                
                var meshData = builder.Build(Allocator.Temp);
                try
                {
                    meshData.UpdateUnityMesh(unityMesh);
                    
                    Assert.AreEqual(3, unityMesh.vertexCount);
                    Assert.AreEqual(3, unityMesh.triangles.Length);
                    
                    var vertices = unityMesh.vertices;
                    Assert.AreEqual(new Vector3(0, 0, 1 + i), vertices[2], $"Z position should be updated on iteration {i}");
                }
                finally
                {
                    meshData.Dispose();
                }
            }
            
            Object.DestroyImmediate(unityMesh);
        }
    }
}