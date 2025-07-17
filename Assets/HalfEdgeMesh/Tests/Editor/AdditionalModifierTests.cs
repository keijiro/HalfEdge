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
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            var chamfered = ChamferEdges.Apply(mesh, 0.1f);
            
            Assert.Greater(chamfered.Vertices.Count, 0);
            Assert.Greater(chamfered.Faces.Count, 0);
            
            // Verify all faces are valid
            foreach (var face in chamfered.Faces)
            {
                var vertices = face.GetVertices();
                Assert.GreaterOrEqual(vertices.Count, 3);
            }
        }

        [Test]
        public void CreateLattice_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            var lattice = CreateLattice.Apply(mesh, 3.0f); // Large spacing for fewer connections
            
            Assert.Greater(lattice.Vertices.Count, 0);
            Assert.Greater(lattice.Faces.Count, 0);
            
            // Basic validation only
            Assert.IsNotNull(lattice);
        }

        [Test]
        public void SplitFaces_ApplyToPlane_CreatesValidMesh()
        {
            var generator = new HalfEdgeMesh.Generators.Plane(2, 2, 2);
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
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            var skewed = SkewMesh.Apply(mesh, 15.0f, new float3(1, 0, 0));
            
            Assert.AreEqual(mesh.Vertices.Count, skewed.Vertices.Count);
            Assert.AreEqual(mesh.Faces.Count, skewed.Faces.Count);
            
            // Verify connectivity
            foreach (var vertex in skewed.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }
        }

        [Test]
        public void SplitFaces_WithNonIntersectingPlane_PreservesOriginalMesh()
        {
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            // Plane that doesn't intersect the box
            var planeNormal = new float3(0, 1, 0);
            var planePoint = new float3(0, 5, 0);
            
            var split = SplitFaces.Apply(mesh, planeNormal, planePoint);
            
            // Should have same number of faces since no splitting occurred
            Assert.AreEqual(mesh.Faces.Count, split.Faces.Count);
        }

        [Test]
        public void CreateLattice_WithDifferentSpacing_ProducesValidResults()
        {
            var generator = new Tetrahedron(1.0f);
            var mesh = generator.Generate();
            
            var lattice = CreateLattice.Apply(mesh, 2.0f);
            
            // Just verify it produces valid output
            Assert.IsNotNull(lattice);
            Assert.Greater(lattice.Vertices.Count, 0);
        }
    }
}