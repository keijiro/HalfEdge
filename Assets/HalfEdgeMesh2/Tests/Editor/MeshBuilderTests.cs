using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class MeshBuilderTests
    {
        [Test]
        public void CreateTriangle_CreatesValidMesh()
        {
            using var builder = new MeshBuilder(Allocator.Temp);

            // Add vertices
            var v0 = builder.AddVertex(new float3(0, 0, 0));
            var v1 = builder.AddVertex(new float3(1, 0, 0));
            var v2 = builder.AddVertex(new float3(0, 1, 0));

            // Add face
            builder.AddFace(v0, v1, v2);

            // Build mesh
            var mesh = builder.Build(Allocator.Temp);
            try
            {
                Assert.AreEqual(3, mesh.vertexCount);
                Assert.AreEqual(3, mesh.halfEdgeCount);
                Assert.AreEqual(1, mesh.faceCount);

                // Validate mesh
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void CreateQuad_CreatesValidMeshWithTwins()
        {
            using var builder = new MeshBuilder(Allocator.Temp);

            // Add vertices
            var v0 = builder.AddVertex(new float3(0, 0, 0));
            var v1 = builder.AddVertex(new float3(1, 0, 0));
            var v2 = builder.AddVertex(new float3(1, 1, 0));
            var v3 = builder.AddVertex(new float3(0, 1, 0));

            // Add faces (two triangles)
            builder.AddFace(v0, v1, v2);
            builder.AddFace(v0, v2, v3);

            // Build mesh
            var mesh = builder.Build(Allocator.Temp);
            try
            {
                Assert.AreEqual(4, mesh.vertexCount);
                Assert.AreEqual(6, mesh.halfEdgeCount);
                Assert.AreEqual(2, mesh.faceCount);

                // Check that twins are connected
                var heWithTwin = -1;
                for (int i = 0; i < mesh.halfEdgeCount; i++)
                {
                    if (mesh.halfEdges[i].twin >= 0)
                    {
                        heWithTwin = i;
                        break;
                    }
                }

                Assert.GreaterOrEqual(heWithTwin, 0, "Should have at least one twin connection");

                // Validate mesh
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void EmptyMesh_IsValid()
        {
            using var builder = new MeshBuilder(Allocator.Temp);

            var mesh = builder.Build(Allocator.Temp);
            try
            {
                Assert.AreEqual(0, mesh.vertexCount);
                Assert.AreEqual(0, mesh.halfEdgeCount);
                Assert.AreEqual(0, mesh.faceCount);

                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void AddFace_WithReadOnlySpan_CreatesValidFace()
        {
            using var builder = new MeshBuilder(Allocator.Temp);

            var v0 = builder.AddVertex(new float3(0, 0, 0));
            var v1 = builder.AddVertex(new float3(1, 0, 0));
            var v2 = builder.AddVertex(new float3(0, 1, 0));
            var v3 = builder.AddVertex(new float3(1, 1, 0));
            var v4 = builder.AddVertex(new float3(0.5f, 0.5f, 1));

            // Test pentagon using ReadOnlySpan
            Span<int> pentagonVertices = stackalloc int[5] { v0, v1, v2, v3, v4 };
            builder.AddFace(pentagonVertices);

            var mesh = builder.Build(Allocator.Temp);
            try
            {
                Assert.AreEqual(5, mesh.vertexCount);
                Assert.AreEqual(1, mesh.faceCount);
                Assert.AreEqual(5, mesh.halfEdgeCount); // Pentagon has 5 half-edges

                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }
    }
}