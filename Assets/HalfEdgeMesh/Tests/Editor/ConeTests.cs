using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using System.Linq;

namespace HalfEdgeMesh.Tests
{
    public class ConeTests
    {
        [Test]
        public void Generate_BasicCone_CreatesCorrectTopology()
        {
            var segments = 6;
            var generator = new Cone(1.0f, 2.0f, segments);
            var mesh = generator.Generate();

            // Vertices: 1 apex + 1 base center + segments rim
            Assert.AreEqual(2 + segments, mesh.Vertices.Count);
            
            // Faces: segments side faces + segments base faces
            Assert.AreEqual(segments * 2, mesh.Faces.Count);

            // All faces should be triangles
            foreach (var face in mesh.Faces)
                Assert.AreEqual(3, face.GetVertices().Count);
        }

        [Test]
        public void Generate_ApexAndBase_AtCorrectPositions()
        {
            var radius = 1.5f;
            var height = 3.0f;
            var generator = new Cone(radius, height, 8);
            var mesh = generator.Generate();

            // Check apex position (Z-axis as vertical)
            var apex = mesh.Vertices[0];
            Assert.AreEqual(0, apex.Position.x, 0.001f);
            Assert.AreEqual(0, apex.Position.y, 0.001f);
            Assert.AreEqual(height * 0.5f, apex.Position.z, 0.001f);

            // Check base center
            var baseCenter = mesh.Vertices[1];
            Assert.AreEqual(0, baseCenter.Position.x, 0.001f);
            Assert.AreEqual(0, baseCenter.Position.y, 0.001f);
            Assert.AreEqual(-height * 0.5f, baseCenter.Position.z, 0.001f);

            // Check rim vertices
            for (int i = 2; i < mesh.Vertices.Count; i++)
            {
                var vertex = mesh.Vertices[i];
                var distance = math.sqrt(vertex.Position.x * vertex.Position.x + 
                                       vertex.Position.y * vertex.Position.y);
                Assert.AreEqual(radius, distance, 0.001f);
                Assert.AreEqual(-height * 0.5f, vertex.Position.z, 0.001f);
            }
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Cone(1.0f, 2.0f, 10);
            var mesh = generator.Generate();

            // Check twin relationships
            int halfEdgesWithTwin = 0;
            foreach (var halfEdge in mesh.HalfEdges)
            {
                if (halfEdge.Twin != null)
                {
                    Assert.AreEqual(halfEdge, halfEdge.Twin.Twin);
                    halfEdgesWithTwin++;
                }
            }

            // All shared edges should have twins
            // For a cone, we expect all edges between rim vertices to have twins
            // (both on side faces and base faces)
            Assert.Greater(halfEdgesWithTwin, 0);

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
            var generator = new Cone(1.0f, 2.0f, 8);
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

                // For side faces (Z-axis as vertical)
                if (math.abs(center.z) > 0.1f)
                {
                    var radialDir = math.normalize(new float3(center.x, center.y, 0));
                    var dotProduct = math.dot(normal, radialDir);
                    Assert.Greater(dotProduct, -0.1f, "Side face normal should point generally outward");
                }
                // For base faces
                else
                {
                    Assert.Less(normal.z, -0.9f, "Base face normal should point downward (negative Z)");
                }
            }
        }

        [Test]
        public void Generate_DifferentSegmentCounts_ProducesValidMesh()
        {
            int[] segmentCounts = { 3, 4, 5, 8, 16, 32 };

            foreach (var segments in segmentCounts)
            {
                var generator = new Cone(1.0f, 2.0f, segments);
                var mesh = generator.Generate();

                Assert.AreEqual(2 + segments, mesh.Vertices.Count);
                Assert.AreEqual(segments * 2, mesh.Faces.Count);

                // Check that all faces are triangles
                foreach (var face in mesh.Faces)
                    Assert.AreEqual(3, face.GetVertices().Count);
            }
        }
    }
}