using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class DodecahedronTests
    {
        [Test]
        public void Generate_Creates12FacesAnd20Vertices()
        {
            var generator = new Dodecahedron(1.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(20, mesh.Vertices.Count);
            Assert.AreEqual(12, mesh.Faces.Count);

            foreach (var face in mesh.Faces)
                Assert.AreEqual(5, face.GetVertices().Count);
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Dodecahedron(1.0f);
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

        [Test]
        public void Generate_AllFacesPointOutward()
        {
            var generator = new Dodecahedron(1.0f);
            var mesh = generator.Generate();

            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                Assert.AreEqual(5, vertices.Count, "Each face should have 5 vertices");

                // Calculate face normal using cross product
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;
                
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var normal = math.normalize(math.cross(edge1, edge2));

                // Calculate face center
                var center = float3.zero;
                foreach (var vertex in vertices)
                    center += vertex.Position;
                center /= vertices.Count;

                // For convex polyhedra, the normal should point away from the origin
                // Check if the normal points outward from the centroid
                var dotProduct = math.dot(normal, center);
                Assert.Greater(dotProduct, 0, 
                    $"Face normal should point outward. Face center: {center}, Normal: {normal}, Dot product: {dotProduct}");
            }
        }

        [Test]
        public void Generate_VerticesHaveCorrectGoldenRatioProperties()
        {
            var generator = new Dodecahedron(2.0f); // size = 2 for easier testing
            var mesh = generator.Generate();

            var phi = (1.0f + math.sqrt(5.0f)) / 2.0f; // Golden ratio
            var invPhi = 1.0f / phi;
            var tolerance = 0.001f;

            // Check that vertices include the expected golden ratio coordinates
            var vertices = mesh.Vertices;
            Assert.AreEqual(20, vertices.Count);

            // Verify some vertices have golden ratio coordinates
            bool foundGoldenRatioVertex = false;
            foreach (var vertex in vertices)
            {
                var pos = vertex.Position;
                if (math.abs(pos.y - phi) < tolerance || math.abs(pos.y + phi) < tolerance)
                {
                    foundGoldenRatioVertex = true;
                    break;
                }
            }
            Assert.IsTrue(foundGoldenRatioVertex, "Should find vertices with golden ratio coordinates");
        }

        [Test]
        public void Generate_AllFacesAreValid()
        {
            var generator = new Dodecahedron(1.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(12, mesh.Faces.Count, "Should have exactly 12 faces");
            
            // Check that all faces have valid vertices
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                var face = mesh.Faces[i];
                var vertices = face.GetVertices();
                
                Assert.AreEqual(5, vertices.Count, $"Face {i} should have 5 vertices, but has {vertices.Count}");
                
                // Check for duplicate vertices in the same face
                var vertexIndices = new System.Collections.Generic.List<int>();
                foreach (var vertex in vertices)
                {
                    var vertexIndex = mesh.Vertices.IndexOf(vertex);
                    vertexIndices.Add(vertexIndex);
                }
                var uniqueVertices = new System.Collections.Generic.HashSet<int>(vertexIndices);
                Assert.AreEqual(5, uniqueVertices.Count, $"Face {i} has duplicate vertices: [{string.Join(", ", vertexIndices)}]");
            }
        }
    }
}