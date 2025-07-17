using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class OctahedronTests
    {
        [Test]
        public void Generate_Creates8FacesAnd6Vertices()
        {
            var generator = new Octahedron(1.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(6, mesh.Vertices.Count);
            Assert.AreEqual(8, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_VerticesFormRegularOctahedron()
        {
            var size = 2.0f;
            var generator = new Octahedron(size);
            var mesh = generator.Generate();

            // All vertices should be at distance size/2 from center
            var expectedDistance = size * 0.5f;
            foreach (var vertex in mesh.Vertices)
            {
                var distance = math.length(vertex.Position);
                Assert.AreEqual(expectedDistance, distance, 0.001f);
            }

            // Check that vertices are along coordinate axes
            var tolerance = 0.001f;
            var found = new bool[6];
            
            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                
                if (math.abs(pos.x - expectedDistance) < tolerance && math.abs(pos.y) < tolerance && math.abs(pos.z) < tolerance)
                    found[0] = true; // +X
                else if (math.abs(pos.x + expectedDistance) < tolerance && math.abs(pos.y) < tolerance && math.abs(pos.z) < tolerance)
                    found[1] = true; // -X
                else if (math.abs(pos.x) < tolerance && math.abs(pos.y - expectedDistance) < tolerance && math.abs(pos.z) < tolerance)
                    found[2] = true; // +Y
                else if (math.abs(pos.x) < tolerance && math.abs(pos.y + expectedDistance) < tolerance && math.abs(pos.z) < tolerance)
                    found[3] = true; // -Y
                else if (math.abs(pos.x) < tolerance && math.abs(pos.y) < tolerance && math.abs(pos.z - expectedDistance) < tolerance)
                    found[4] = true; // +Z
                else if (math.abs(pos.x) < tolerance && math.abs(pos.y) < tolerance && math.abs(pos.z + expectedDistance) < tolerance)
                    found[5] = true; // -Z
            }

            for (int i = 0; i < 6; i++)
                Assert.IsTrue(found[i], $"Vertex {i} not found at expected position");
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Octahedron(1.0f);
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
            var generator = new Octahedron(1.0f);
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
                var faceCenter = (v0 + v1 + v2) / 3.0f;

                // Normal should point in same general direction as face center
                var dotProduct = math.dot(normal, math.normalize(faceCenter));
                Assert.Greater(dotProduct, 0.5f, "Face normal should point outward");
            }
        }

        [Test]
        public void Generate_AllFacesAreTriangles()
        {
            var generator = new Octahedron(2.0f);
            var mesh = generator.Generate();

            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                Assert.AreEqual(3, vertices.Count, "All octahedron faces should be triangles");
                
                // Check that vertices form a valid triangle (not collinear)
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;
                
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var crossProduct = math.cross(edge1, edge2);
                var area = math.length(crossProduct) * 0.5f;
                
                Assert.Greater(area, 0.001f, "Face should have non-zero area");
            }
        }
    }
}