using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Unity;
using HalfEdgeMesh2.Generators;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class NormalGenerationTests
    {
        [Test]
        public void ToUnityMesh_SmoothMode_SharesVertices()
        {
            // Create a simple box
            using var builder = new MeshBuilder(Allocator.Temp);
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

            // Add 6 faces as quads
            builder.AddFace(0, 1, 2, 3);  // Front
            builder.AddFace(5, 4, 7, 6);  // Back
            builder.AddFace(4, 0, 3, 7);  // Left
            builder.AddFace(1, 5, 6, 2);  // Right
            builder.AddFace(4, 5, 1, 0);  // Bottom
            builder.AddFace(3, 2, 6, 7);  // Top

            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh(NormalGenerationMode.Smooth);

                // Smooth shading should share vertices (8 unique vertices)
                Assert.AreEqual(8, unityMesh.vertexCount, "Smooth mode should share vertices");

                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        [Test]
        public void ToUnityMesh_FlatMode_DuplicatesVertices()
        {
            // Create a simple box
            using var builder = new MeshBuilder(Allocator.Temp);
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

            // Add 6 faces as quads
            builder.AddFace(0, 1, 2, 3);  // Front
            builder.AddFace(5, 4, 7, 6);  // Back
            builder.AddFace(4, 0, 3, 7);  // Left
            builder.AddFace(1, 5, 6, 2);  // Right
            builder.AddFace(4, 5, 1, 0);  // Bottom
            builder.AddFace(3, 2, 6, 7);  // Top

            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var unityMesh = meshData.ToUnityMesh(NormalGenerationMode.Flat);

                // Flat shading should duplicate vertices (6 faces * 2 triangles * 3 vertices = 36)
                Assert.AreEqual(36, unityMesh.vertexCount, "Flat mode should duplicate vertices for each triangle");

                Object.DestroyImmediate(unityMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        [Test]
        public void ToUnityMesh_DefaultMode_UsesSmoothShading()
        {
            using var builder = new MeshBuilder(Allocator.Temp);
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0, 1, 0));
            builder.AddFace(0, 1, 2);

            var meshData = builder.Build(Allocator.Temp);
            try
            {
                var defaultMesh = meshData.ToUnityMesh();
                var smoothMesh = meshData.ToUnityMesh(NormalGenerationMode.Smooth);

                // Default should be same as smooth
                Assert.AreEqual(smoothMesh.vertexCount, defaultMesh.vertexCount);
                Assert.AreEqual(smoothMesh.triangles.Length, defaultMesh.triangles.Length);

                Object.DestroyImmediate(defaultMesh);
                Object.DestroyImmediate(smoothMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        [Test]
        public void UpdateUnityMesh_WithModes_UpdatesCorrectly()
        {
            var meshData = Sphere.Generate(1.0f, new int2(8, 6), Allocator.Temp);
            var mesh = new Mesh();

            try
            {
                // Test smooth mode
                meshData.UpdateUnityMesh(mesh, NormalGenerationMode.Smooth);
                var smoothVertexCount = mesh.vertexCount;

                // Test flat mode
                meshData.UpdateUnityMesh(mesh, NormalGenerationMode.Flat);
                var flatVertexCount = mesh.vertexCount;

                // Flat should have more vertices than smooth
                Assert.Greater(flatVertexCount, smoothVertexCount, "Flat mode should have more vertices");

                Object.DestroyImmediate(mesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        [Test]
        public void ExtensionMethods_WorkCorrectly()
        {
            // Use a box instead of single triangle to see the difference between modes
            using var builder = new MeshBuilder(Allocator.Temp);
            var halfSize = 0.5f;

            // Add 8 vertices of a box
            builder.AddVertex(new float3(-halfSize, -halfSize, -halfSize)); // 0
            builder.AddVertex(new float3( halfSize, -halfSize, -halfSize)); // 1
            builder.AddVertex(new float3( halfSize,  halfSize, -halfSize)); // 2
            builder.AddVertex(new float3(-halfSize,  halfSize, -halfSize)); // 3

            // Add one quad face
            builder.AddFace(0, 1, 2, 3);

            var meshData = builder.Build(Allocator.Temp);
            try
            {
                // Test extension methods
                var smoothMesh = meshData.ToUnityMesh(NormalGenerationMode.Smooth);
                var flatMesh = meshData.ToUnityMesh(NormalGenerationMode.Flat);

                Assert.AreEqual(4, smoothMesh.vertexCount, "Smooth mode should share vertices");
                Assert.AreEqual(6, flatMesh.vertexCount, "Flat mode should duplicate vertices (2 triangles * 3 vertices)");

                var testMesh = new Mesh();
                meshData.UpdateUnityMesh(testMesh, NormalGenerationMode.Flat);
                Assert.AreEqual(6, testMesh.vertexCount);

                Object.DestroyImmediate(smoothMesh);
                Object.DestroyImmediate(flatMesh);
                Object.DestroyImmediate(testMesh);
            }
            finally
            {
                meshData.Dispose();
            }
        }
    }
}