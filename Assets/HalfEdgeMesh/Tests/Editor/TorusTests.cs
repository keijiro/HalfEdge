using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

namespace HalfEdgeMesh.Tests
{
    public class TorusTests
    {
        [Test]
        public void Generate_BasicTorus_CreatesCorrectTopology()
        {
            var majorSegments = 8;
            var minorSegments = 6;
            var generator = new Torus(1.0f, 0.3f, new int2(majorSegments, minorSegments));
            var mesh = generator.Generate();

            // Vertices: majorSegments * minorSegments
            Assert.AreEqual(majorSegments * minorSegments, mesh.Vertices.Count);
            
            // Faces: majorSegments * minorSegments (all quads)
            Assert.AreEqual(majorSegments * minorSegments, mesh.Faces.Count);

            // All faces should be quads
            foreach (var face in mesh.Faces)
                Assert.AreEqual(4, face.GetVertices().Count);
        }

        [Test]
        public void Generate_VerticesOnTorusSurface()
        {
            var majorRadius = 2.0f;
            var minorRadius = 0.5f;
            var generator = new Torus(majorRadius, minorRadius, new int2(12, 8));
            var mesh = generator.Generate();

            foreach (var vertex in mesh.Vertices)
            {
                var pos = vertex.Position;
                
                // Distance from Z axis (new vertical axis)
                var distFromZAxis = math.sqrt(pos.x * pos.x + pos.y * pos.y);
                
                // Distance from major circle
                var distFromMajorCircle = math.abs(distFromZAxis - majorRadius);
                
                // Distance from torus center on minor circle
                var distFromMinorCenter = math.sqrt(distFromMajorCircle * distFromMajorCircle + pos.z * pos.z);
                
                Assert.AreEqual(minorRadius, distFromMinorCenter, 0.001f);
            }
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var generator = new Torus(1.0f, 0.3f, new int2(10, 8));
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
                } while (he != start && count < 6);
                Assert.AreEqual(4, count);
            }
        }

        [Test]
        public void Generate_FaceNormals_PointOutward()
        {
            var generator = new Torus(1.0f, 0.3f, new int2(8, 6));
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
                var center = (v0 + v1 + v2 + vertices[3].Position) / 4.0f;

                // Project center to major circle (now in XY plane, Z=0)
                var majorCirclePoint = new float3(center.x, center.y, 0);
                majorCirclePoint = math.normalize(majorCirclePoint) * 1.0f; // majorRadius

                // Vector from major circle to face center
                var torusNormal = math.normalize(center - majorCirclePoint);

                var dotProduct = math.dot(normal, torusNormal);
                Assert.Greater(dotProduct, 0.5f, "Face normal should point outward from torus");
            }
        }

        [Test]
        public void Generate_DifferentParameters_ProducesValidMesh()
        {
            var parameters = new[]
            {
                (major: 3, minor: 4),
                (major: 4, minor: 6),
                (major: 6, minor: 8),
                (major: 16, minor: 12)
            };

            foreach (var (major, minor) in parameters)
            {
                var generator = new Torus(2.0f, 0.5f, new int2(major, minor));
                var mesh = generator.Generate();

                Assert.AreEqual(major * minor, mesh.Vertices.Count);
                Assert.AreEqual(major * minor, mesh.Faces.Count);

                foreach (var face in mesh.Faces)
                    Assert.AreEqual(4, face.GetVertices().Count);
            }
        }
    }
}