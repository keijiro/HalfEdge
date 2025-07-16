using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using System.Linq;

namespace HalfEdgeMesh.Tests
{
    public class BoxTests
    {
        [Test]
        public void Generate_NoSubdivisions_Creates6FacesAnd8Vertices()
        {
            var generator = new Box(1, 1, 1, 0);
            var mesh = generator.Generate();

            Assert.AreEqual(8, mesh.Vertices.Count);
            Assert.AreEqual(6, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
            {
                Assert.AreEqual(4, face.GetVertices().Count);
            }
        }

        [Test]
        public void Generate_OneSubdivision_CreatesCorrectTopology()
        {
            var generator = new Box(1, 1, 1, 1);
            var mesh = generator.Generate();

            // Each original face (6) should be subdivided into 4 faces
            Assert.AreEqual(24, mesh.Faces.Count, "Should have 24 faces after 1 subdivision");

            // All faces should be quads
            foreach (var face in mesh.Faces)
            {
                Assert.AreEqual(4, face.GetVertices().Count, "All faces should be quads");
            }

            // Check vertex count: 8 original + 12 edge midpoints + 6 face centers = 26
            Assert.AreEqual(26, mesh.Vertices.Count, "Should have 26 vertices after 1 subdivision");
        }

        [Test]
        public void Generate_TwoSubdivisions_CreatesCorrectTopology()
        {
            var generator = new Box(1, 1, 1, 2);
            var mesh = generator.Generate();

            // Each face after first subdivision (24) should be subdivided into 4 faces
            Assert.AreEqual(96, mesh.Faces.Count, "Should have 96 faces after 2 subdivisions");

            // All faces should be quads
            foreach (var face in mesh.Faces)
            {
                Assert.AreEqual(4, face.GetVertices().Count, "All faces should be quads");
            }
        }

        [Test]
        public void Generate_SubdividedFaces_HaveCorrectConnectivity()
        {
            var generator = new Box(1, 1, 1, 1);
            var mesh = generator.Generate();

            // Check that all edges have proper twin connections
            foreach (var edge in mesh.Edges)
            {
                Assert.IsNotNull(edge.HalfEdge.Twin, "Every half-edge should have a twin");
                Assert.AreEqual(edge.HalfEdge, edge.HalfEdge.Twin.Twin, "Twin relationship should be symmetric");
            }

            // Check that all vertices have outgoing half-edges
            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge, "Every vertex should have an outgoing half-edge");
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin, "Vertex should be the origin of its half-edge");
            }

            // Check face consistency
            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                Assert.AreEqual(4, vertices.Count, "All faces should be quads");

                // Check that face vertices form a closed loop
                var he = face.HalfEdge;
                var start = he;
                int count = 0;
                do
                {
                    Assert.AreEqual(face, he.Face, "Half-edge should belong to its face");
                    he = he.Next;
                    count++;
                } while (he != start && count < 10); // Safety limit

                Assert.AreEqual(4, count, "Face should have exactly 4 half-edges");
            }
        }

        [Test]
        public void Generate_SubdividedBox_PreservesBoxShape()
        {
            var generator = new Box(2, 2, 2, 1);
            var mesh = generator.Generate();

            // Check that all vertices are within the box bounds
            foreach (var vertex in mesh.Vertices)
            {
                Assert.LessOrEqual(math.abs(vertex.Position.x), 1.01f, "X coordinate should be within bounds");
                Assert.LessOrEqual(math.abs(vertex.Position.y), 1.01f, "Y coordinate should be within bounds");
                Assert.LessOrEqual(math.abs(vertex.Position.z), 1.01f, "Z coordinate should be within bounds");
            }

            // Check that vertices exist on all 6 faces of the box
            var tolerance = 0.01f;
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.x - 1) < tolerance), "Should have vertices on +X face");
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.x + 1) < tolerance), "Should have vertices on -X face");
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.y - 1) < tolerance), "Should have vertices on +Y face");
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.y + 1) < tolerance), "Should have vertices on -Y face");
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.z - 1) < tolerance), "Should have vertices on +Z face");
            Assert.IsTrue(mesh.Vertices.Any(v => math.abs(v.Position.z + 1) < tolerance), "Should have vertices on -Z face");
        }
    }
}