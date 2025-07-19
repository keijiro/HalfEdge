using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using HalfEdgeMesh2.Generators;
using System.Linq;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class BoxGeneratorTests
    {
        [Test]
        public void Generate_SimpleCube_CreatesValidMesh()
        {
            var size = new float3(1, 1, 1);
            var segments = new int3(1, 1, 1);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                Assert.AreEqual(8, mesh.vertexCount, "Simple cube should have 8 vertices");
                Assert.AreEqual(6, mesh.faceCount, "Simple cube should have 6 faces");
                Assert.Greater(mesh.halfEdgeCount, 0, "Should have half-edges");

                // Validate mesh structure
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_SubdividedCube_CreatesCorrectTopology()
        {
            var size = new float3(2, 2, 2);
            var segments = new int3(2, 2, 2);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                // Each face has 2x2 = 4 quads, total 6 faces * 4 = 24 quads
                Assert.AreEqual(24, mesh.faceCount, "Should have 24 faces with 2x2x2 segments");
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_DifferentSegments_CreatesCorrectFaceCount()
        {
            var size = new float3(1, 1, 1);
            var segments = new int3(3, 2, 4);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                // X faces: 2*4 = 8, Y faces: 3*4 = 12, Z faces: 3*2 = 6
                // Total: 8 + 12 + 6 = 26 quads
                var expectedFaces = (segments.y * segments.z) * 2 + // X faces  
                                  (segments.x * segments.z) * 2 + // Y faces
                                  (segments.x * segments.y) * 2;  // Z faces
                Assert.AreEqual(expectedFaces, mesh.faceCount, "Face count should match segment configuration");
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_MinimumSegments_ClampsToOne()
        {
            var size = new float3(1, 1, 1);
            var segments = new int3(0, -1, 0); // Invalid segments

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                // Should clamp to minimum of 1 segment per axis
                Assert.AreEqual(6, mesh.faceCount, "Should create simple cube with minimum segments");
                Assert.AreEqual(8, mesh.vertexCount, "Should have 8 vertices with minimum segments");
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_DifferentSizes_ScalesCorrectly()
        {
            var size1 = new float3(1, 1, 1);
            var size2 = new float3(2, 3, 4);
            var segments = new int3(1, 1, 1);

            var mesh1 = Box.Generate(size1, segments, Allocator.Temp);
            var mesh2 = Box.Generate(size2, segments, Allocator.Temp);

            try
            {
                Assert.AreEqual(mesh1.vertexCount, mesh2.vertexCount, "Same segments should produce same vertex count");
                Assert.AreEqual(mesh1.faceCount, mesh2.faceCount, "Same segments should produce same face count");

                // Check bounds
                MeshOperations.ComputeBounds(ref mesh1, out var bounds1Center, out var bounds1Size);
                MeshOperations.ComputeBounds(ref mesh2, out var bounds2Center, out var bounds2Size);

                Assert.AreEqual(0f, math.length(bounds1Center), 0.001f, "Cube should be centered at origin");
                Assert.AreEqual(0f, math.length(bounds2Center), 0.001f, "Cube should be centered at origin");
                
                Assert.AreEqual(0f, math.length(size1 - bounds1Size), 0.001f, "Bounds should match specified size");
                Assert.AreEqual(0f, math.length(size2 - bounds2Size), 0.001f, "Bounds should match specified size");
            }
            finally
            {
                mesh1.Dispose();
                mesh2.Dispose();
            }
        }

        [Test]
        public void Generate_HighSubdivisions_CreatesDetailedMesh()
        {
            var size = new float3(1, 1, 1);
            var segments = new int3(4, 4, 4);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                // Each face has 4x4 = 16 quads, total 6 faces * 16 = 96 quads
                Assert.AreEqual(96, mesh.faceCount, "Should have 96 faces with 4x4x4 segments");
                Assert.Greater(mesh.vertexCount, 50, "High subdivision should produce many vertices");
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh), "Generated mesh should be valid");
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_BoxVertices_AreWithinBounds()
        {
            var size = new float3(2, 3, 4);
            var segments = new int3(2, 2, 2);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                var halfSize = size * 0.5f;
                var tolerance = 0.001f;

                // Check that all vertices are within the box bounds
                for (var i = 0; i < mesh.vertexCount; i++)
                {
                    var vertex = mesh.vertices[i].position;
                    Assert.LessOrEqual(math.abs(vertex.x), halfSize.x + tolerance, "X coordinate should be within bounds");
                    Assert.LessOrEqual(math.abs(vertex.y), halfSize.y + tolerance, "Y coordinate should be within bounds");
                    Assert.LessOrEqual(math.abs(vertex.z), halfSize.z + tolerance, "Z coordinate should be within bounds");
                }
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void Generate_BoxFaces_HaveCorrectVertexCount()
        {
            var size = new float3(1, 1, 1);
            var segments = new int3(1, 1, 1);

            var mesh = Box.Generate(size, segments, Allocator.Temp);
            try
            {
                // All faces in a box should be quads (4 vertices)
                for (var faceIndex = 0; faceIndex < mesh.faceCount; faceIndex++)
                {
                    var face = mesh.faces[faceIndex];
                    var vertexCount = 0;
                    var currentHe = face.halfEdge;
                    var startHe = currentHe;

                    do
                    {
                        vertexCount++;
                        var he = mesh.halfEdges[currentHe];
                        currentHe = he.next;
                    } while (currentHe != startHe);

                    Assert.AreEqual(4, vertexCount, $"Face {faceIndex} should be a quad");
                }
            }
            finally
            {
                mesh.Dispose();
            }
        }

    }
}