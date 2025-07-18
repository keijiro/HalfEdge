using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;

namespace HalfEdgeMesh.Tests
{
    public class AdditionalModifierTests
    {
        [Test]
        public void ChamferEdges_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(new float3(1, 1, 1), new int3(1, 1, 1));
            var mesh = generator.Generate();
            
            var chamfered = ChamferEdges.Apply(mesh, 0.1f);
            
            // Chamfering edges should increase vertex and face count
            Assert.Greater(chamfered.Vertices.Count, mesh.Vertices.Count);
            Assert.Greater(chamfered.Faces.Count, mesh.Faces.Count);
            
            // Verify all faces are valid
            foreach (var face in chamfered.Faces)
            {
                var vertices = face.GetVertices();
                Assert.GreaterOrEqual(vertices.Count, 3);
            }
        }


        [Test]
        public void SplitFaces_ApplyToPlane_CreatesValidMesh()
        {
            var generator = new HalfEdgeMesh.Generators.Plane(new float2(2, 2), new int2(2, 2));
            var mesh = generator.Generate();
            
            var planeNormal = new float3(1, 0, 0);
            var planePoint = new float3(0, 0, 0);
            
            var split = SplitFaces.Apply(mesh, planeNormal, planePoint);
            
            Assert.Greater(split.Vertices.Count, 0);
            Assert.Greater(split.Faces.Count, 0);
            
            // Verify all faces are valid
            foreach (var face in split.Faces)
            {
                var vertices = face.GetVertices();
                Assert.GreaterOrEqual(vertices.Count, 3);
            }
        }

        [Test]
        public void SkewMesh_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(new float3(1, 1, 1), new int3(1, 1, 1));
            var mesh = generator.Generate();
            var originalVertexCount = mesh.Vertices.Count;
            var originalFaceCount = mesh.Faces.Count;
            
            new SkewMesh(15.0f * math.PI / 180.0f, new float3(1, 0, 0)).Apply(mesh);
            
            Assert.AreEqual(originalVertexCount, mesh.Vertices.Count);
            Assert.AreEqual(originalFaceCount, mesh.Faces.Count);
            
            // Verify connectivity
            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }
        }

        [Test]
        public void SplitFaces_WithNonIntersectingPlane_PreservesOriginalMesh()
        {
            var generator = new Box(new float3(1, 1, 1), new int3(1, 1, 1));
            var mesh = generator.Generate();
            
            // Plane that doesn't intersect the box
            var planeNormal = new float3(0, 1, 0);
            var planePoint = new float3(0, 5, 0);
            
            var split = SplitFaces.Apply(mesh, planeNormal, planePoint);
            
            // Should have same number of faces since no splitting occurred
            Assert.AreEqual(mesh.Faces.Count, split.Faces.Count);
        }

    }
}