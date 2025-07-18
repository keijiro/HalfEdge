using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class PlaneTests
    {
        [Test]
        public void Generate_SingleSegment_CreatesOneFaceAndFourVertices()
        {
            var generator = new Plane(new float2(1, 1), new int2(1, 1));
            var mesh = generator.Generate();

            Assert.AreEqual(4, mesh.Vertices.Count);
            Assert.AreEqual(1, mesh.Faces.Count);
            Assert.AreEqual(4, mesh.Faces[0].GetVertices().Count);
        }

        [Test]
        public void Generate_MultipleSegments_CreatesCorrectTopology()
        {
            var generator = new Plane(new float2(2, 2), new int2(3, 2));
            var mesh = generator.Generate();

            // (widthSegments + 1) * (heightSegments + 1) vertices
            Assert.AreEqual(4 * 3, mesh.Vertices.Count);
            // widthSegments * heightSegments faces
            Assert.AreEqual(3 * 2, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(4, face.GetVertices().Count);
        }

        [Test]
        public void Generate_VerticesPositioned_CorrectlyOnPlane()
        {
            var generator = new Plane(new float2(2, 2), new int2(2, 2));
            var mesh = generator.Generate();

            foreach (var vertex in mesh.Vertices)
            {
                // All vertices should have Z = 0 (on XY plane)
                Assert.AreEqual(0, vertex.Position.z, 0.001f);
                // All vertices should be within bounds for a 2x2 plane (range should be [-1, 1])
                Assert.LessOrEqual(math.abs(vertex.Position.x), 1.0f);
                Assert.LessOrEqual(math.abs(vertex.Position.y), 1.0f);
            }
        }

        [Test]
        public void Generate_CorrectVertexOrder_ForWindingConsistency()
        {
            var generator = new Plane(new float2(2, 2), new int2(2, 2));
            var mesh = generator.Generate();

            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;

                // Calculate face normal
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = math.normalize(math.cross(edge1, edge2));

                // For a plane in XY, normal should point up (positive Z)
                Assert.Greater(normal.z, 0.9f, "Face normal should point upward");
            }
        }

        [Test]
        public void Generate_ConnectivityValid_InternalEdgesHaveTwins()
        {
            var generator = new Plane(new float2(3, 3), new int2(3, 3));
            var mesh = generator.Generate();

            // Check internal edges have twins
            foreach (var halfEdge in mesh.HalfEdges)
            {
                if (halfEdge.Twin != null)
                {
                    Assert.AreEqual(halfEdge, halfEdge.Twin.Twin);
                }
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
                } while (he != start && count < 10);
                Assert.AreEqual(4, count);
            }
        }
    }
}