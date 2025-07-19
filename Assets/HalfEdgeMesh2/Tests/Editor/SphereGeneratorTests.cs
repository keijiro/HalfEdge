using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using HalfEdgeMesh2.Generators;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class SphereGeneratorTests
    {
        [Test]
        public void Generate_ValidParameters_CreatesValidMesh()
        {
            var radius = 1.0f;
            var segments = new int2(8, 6);

            var mesh = Sphere.Generate(radius, segments, Allocator.Temp);
            try
            {
                Assert.Greater(mesh.vertexCount, 0, "Should have vertices");
                Assert.Greater(mesh.faceCount, 0, "Should have faces");
                Assert.Greater(mesh.halfEdgeCount, 0, "Should have half-edges");

                // Validate mesh structure
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");

                // Check vertex positions are on sphere surface
                for (var i = 0; i < mesh.vertexCount; i++)
                {
                    var vertex = mesh.vertices[i];
                    var distance = math.length(vertex.position);
                    Assert.AreEqual(radius, distance, 0.001f, $"Vertex {i} should be on sphere surface");
                }
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_MinimumSegments_CreatesValidMesh()
        {
            var radius = 0.5f;
            var segments = new int2(3, 3);

            var mesh = Sphere.Generate(radius, segments, Allocator.Temp);
            try
            {
                Assert.Greater(mesh.vertexCount, 0);
                Assert.Greater(mesh.faceCount, 0);
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_LowSegments_ClampsToMinimum()
        {
            var radius = 1.0f;
            var segments = new int2(1, 2); // Below minimum

            var mesh = Sphere.Generate(radius, segments, Allocator.Temp);
            try
            {
                // Should still create valid mesh with clamped segments
                Assert.Greater(mesh.vertexCount, 0);
                Assert.Greater(mesh.faceCount, 0);
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_DifferentRadii_ScalesCorrectly()
        {
            var segments = new int2(6, 4);
            var radius1 = 1.0f;
            var radius2 = 2.0f;

            var mesh1 = Sphere.Generate(radius1, segments, Allocator.Temp);
            var mesh2 = Sphere.Generate(radius2, segments, Allocator.Temp);

            try
            {
                Assert.AreEqual(mesh1.vertexCount, mesh2.vertexCount, "Same segments should produce same vertex count");

                // Check scaling
                for (var i = 0; i < mesh1.vertexCount; i++)
                {
                    var pos1 = mesh1.vertices[i].position;
                    var pos2 = mesh2.vertices[i].position;
                    var expectedPos2 = pos1 * (radius2 / radius1);

                    Assert.AreEqual(expectedPos2.x, pos2.x, 0.001f, $"X coordinate scaling for vertex {i}");
                    Assert.AreEqual(expectedPos2.y, pos2.y, 0.001f, $"Y coordinate scaling for vertex {i}");
                    Assert.AreEqual(expectedPos2.z, pos2.z, 0.001f, $"Z coordinate scaling for vertex {i}");
                }
            }
            finally
            {
                mesh1.Dispose();
                mesh2.Dispose();
            }
        }

        [Test]
        public void Generate_HighSegments_CreatesDetailedMesh()
        {
            var radius = 1.0f;
            var segments = new int2(16, 12);

            var mesh = Sphere.Generate(radius, segments, Allocator.Temp);
            try
            {
                // High segment count should produce many vertices and faces
                Assert.Greater(mesh.vertexCount, 100, "High segments should produce many vertices");
                Assert.Greater(mesh.faceCount, 150, "High segments should produce many faces");
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }
    }
}