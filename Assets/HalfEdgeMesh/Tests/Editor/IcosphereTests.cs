using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class IcosphereTests
    {
        [Test]
        public void Generate_NoSubdivisions_Creates20FacesAnd12Vertices()
        {
            var generator = new Icosphere(1.0f, 0);
            var mesh = generator.Generate();

            Assert.AreEqual(12, mesh.Vertices.Count);
            Assert.AreEqual(20, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_OneSubdivision_CreatesCorrectTopology()
        {
            var generator = new Icosphere(1.0f, 1);
            var mesh = generator.Generate();

            // Each face subdivides into 4 faces: 20 * 4 = 80
            Assert.AreEqual(80, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_AllVertices_OnSphere()
        {
            var radius = 2.0f;
            var generator = new Icosphere(radius, 2);
            var mesh = generator.Generate();

            foreach (var vertex in mesh.Vertices)
            {
                var distance = math.length(vertex.Position);
                Assert.AreEqual(radius, distance, 0.001f);
            }
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Icosphere(1.0f, 1);
            var mesh = generator.Generate();

            // All edges should have twins (closed mesh)
            foreach (var halfEdge in mesh.HalfEdges)
            {
                Assert.IsNotNull(halfEdge.Twin);
                Assert.AreEqual(halfEdge, halfEdge.Twin.Twin);
            }

            // All vertices should have half-edges
            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }

            // Check face consistency
            foreach (var face in mesh.Faces)
            {
                var he = face.HalfEdge;
                var start = he;
                int count = 0;
                do
                {
                    Assert.AreEqual(face, he.Face);
                    he = he.Next;
                    count++;
                } while (he != start && count < 5);
                Assert.AreEqual(3, count);
            }
        }

        [Test]
        public void Generate_FaceNormals_PointOutward()
        {
            var generator = new Icosphere(1.0f, 1);
            var mesh = generator.Generate();

            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;

                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = math.normalize(math.cross(edge1, edge2));
                var center = (v0 + v1 + v2) / 3.0f;

                // Normal should point in same general direction as center
                var dotProduct = math.dot(normal, math.normalize(center));
                Assert.Greater(dotProduct, 0.0f, "Face normal should point outward");
            }
        }
    }
}