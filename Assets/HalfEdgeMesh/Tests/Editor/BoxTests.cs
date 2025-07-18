using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh.Generators;
using System.Linq;

namespace HalfEdgeMesh.Tests
{
    public class BoxTests
    {
        [Test]
        public void Generate_NoSubdivisions_Creates6FacesAnd8Vertices()
        {
            var generator = new Box(1, 1, 1, new int3(1, 1, 1));
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
            var generator = new Box(1, 1, 1, new int3(2, 2, 2));
            var mesh = generator.Generate();

            // Each face has 2x2 = 4 quads, total 6 faces * 4 = 24 quads
            Assert.AreEqual(24, mesh.Faces.Count, "Should have 24 faces with 2x2x2 segments");

            // All faces should be quads
            foreach (var face in mesh.Faces)
            {
                Assert.AreEqual(4, face.GetVertices().Count, "All faces should be quads");
            }

            // Check vertex count for 2x2x2 segments
            Assert.AreEqual(26, mesh.Vertices.Count, "Should have 26 vertices with 2x2x2 segments");
        }

        [Test]
        public void Generate_TwoSubdivisions_CreatesCorrectTopology()
        {
            var generator = new Box(1, 1, 1, new int3(4, 4, 4));
            var mesh = generator.Generate();

            // Each face has 4x4 = 16 quads, total 6 faces * 16 = 96 quads
            Assert.AreEqual(96, mesh.Faces.Count, "Should have 96 faces with 4x4x4 segments");

            // All faces should be quads
            foreach (var face in mesh.Faces)
            {
                Assert.AreEqual(4, face.GetVertices().Count, "All faces should be quads");
            }
        }

        [Test]
        public void Generate_SubdividedFaces_HaveCorrectConnectivity()
        {
            var generator = new Box(1, 1, 1, new int3(2, 2, 2));
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
            var generator = new Box(2, 2, 2, new int3(2, 2, 2));
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

        [Test]
        public void Generate_SimpleCube_HasCorrectFaceNormals()
        {
            var generator = new Box(2, 2, 2, new int3(1, 1, 1));
            var mesh = generator.Generate();
            var unityMesh = mesh.ToUnityMesh();

            // Check that face normals point outward
            var vertices = unityMesh.vertices;
            var normals = unityMesh.normals;
            var triangles = unityMesh.triangles;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var v0 = vertices[triangles[i]];
                var v1 = vertices[triangles[i + 1]];
                var v2 = vertices[triangles[i + 2]];

                // Calculate face normal using cross product
                var edge1 = v1 - v0;
                var edge2 = v2 - v0;
                var faceNormal = Vector3.Cross(edge1, edge2).normalized;

                // Calculate face center
                var faceCenter = (v0 + v1 + v2) / 3f;

                // For a cube, the outward normal should point away from the center
                // The dot product of face center and face normal should be positive
                var dotProduct = Vector3.Dot(faceCenter.normalized, faceNormal);
                
                Assert.Greater(dotProduct, 0f, 
                    $"Face normal should point outward from cube center. Face at {faceCenter}, normal {faceNormal}, dot product {dotProduct}");
            }
        }

        [Test]
        public void Generate_SimpleCube_HasCorrectWindingOrder()
        {
            var generator = new Box(2, 2, 2, new int3(1, 1, 1));
            var mesh = generator.Generate();

            // Check winding order for each face
            foreach (var face in mesh.Faces)
            {
                var vertices = face.GetVertices();
                Assert.AreEqual(4, vertices.Count, "Face should be a quad");

                // Calculate face normal using first three vertices
                var v0 = vertices[0].Position;
                var v1 = vertices[1].Position;
                var v2 = vertices[2].Position;

                var edge1 = v1 - v0;
                var edge2 = v2 - v1;
                var faceNormal = math.normalize(math.cross(edge1, edge2));

                // Calculate face center
                var faceCenter = (v0 + v1 + v2 + vertices[3].Position) / 4f;

                // For a cube, the outward normal should point away from the center
                var dotProduct = math.dot(faceCenter, faceNormal);
                
                Assert.Greater(dotProduct, 0f, 
                    $"Face normal should point outward from cube center. Face at {faceCenter}, normal {faceNormal}, dot product {dotProduct}");
            }
        }
    }
}