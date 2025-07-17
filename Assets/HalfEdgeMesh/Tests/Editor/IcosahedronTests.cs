using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class IcosahedronTests
    {
        [Test]
        public void Generate_Creates20FacesAnd12Vertices()
        {
            var generator = new Icosahedron(1.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(12, mesh.Vertices.Count);
            Assert.AreEqual(20, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Icosahedron(1.0f);
            var mesh = generator.Generate();

            foreach (var halfEdge in mesh.HalfEdges)
            {
                Assert.IsNotNull(halfEdge.Twin);
                Assert.AreEqual(halfEdge, halfEdge.Twin.Twin);
            }

            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }
        }
    }
}