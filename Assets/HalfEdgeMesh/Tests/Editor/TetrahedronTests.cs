using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class TetrahedronTests
    {
        [Test]
        public void Generate_Creates4FacesAnd4Vertices()
        {
            var generator = new Tetrahedron(1.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(4, mesh.Vertices.Count);
            Assert.AreEqual(4, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_VerticesFormRegularTetrahedron()
        {
            var size = 2.0f;
            var generator = new Tetrahedron(size);
            var mesh = generator.Generate();

            // All vertices should be equidistant from center
            var center = float3.zero;
            foreach (var vertex in mesh.Vertices)
                center += vertex.Position;
            center /= 4.0f;

            var distances = new float[4];
            for (int i = 0; i < 4; i++)
                distances[i] = math.length(mesh.Vertices[i].Position - center);

            // All distances should be equal
            for (int i = 1; i < 4; i++)
                Assert.AreEqual(distances[0], distances[i], 0.001f);

            // Check edge lengths are equal
            var edgeLength = math.length(mesh.Vertices[0].Position - mesh.Vertices[1].Position);
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    var length = math.length(mesh.Vertices[i].Position - mesh.Vertices[j].Position);
                    Assert.AreEqual(edgeLength, length, 0.001f);
                }
            }
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Tetrahedron(1.0f);
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
        }

        [Test]
        public void Generate_FaceNormals_PointOutward()
        {
            var generator = new Tetrahedron(1.0f);
            var mesh = generator.Generate();

            // Calculate center
            var center = float3.zero;
            foreach (var vertex in mesh.Vertices)
                center += vertex.Position;
            center /= 4.0f;

            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;

                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = math.normalize(math.cross(edge1, edge2));
                var faceCenter = (v0 + v1 + v2) / 3.0f;

                var toCenter = faceCenter - center;
                var dotProduct = math.dot(normal, toCenter);
                Assert.Greater(dotProduct, 0.0f, "Face normal should point outward");
            }
        }
    }
}