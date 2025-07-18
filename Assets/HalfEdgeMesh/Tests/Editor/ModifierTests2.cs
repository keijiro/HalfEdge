using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;

namespace HalfEdgeMesh.Tests
{
    public class ModifierTests2
    {
        [Test]
        public void ChamferVertices_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(new float3(1, 1, 1), new int3(1, 1, 1));
            var mesh = generator.Generate();
            
            var chamfered = ChamferVertices.Apply(mesh, 0.1f);
            
            // Chamfering vertices should increase vertex and face count
            Assert.Greater(chamfered.Vertices.Count, mesh.Vertices.Count);
            Assert.Greater(chamfered.Faces.Count, mesh.Faces.Count);
        }

        [Test]
        public void ExpandVertices_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(new float3(1, 1, 1), new int3(1, 1, 1));
            var mesh = generator.Generate();
            var originalVertexCount = mesh.Vertices.Count;
            var originalFaceCount = mesh.Faces.Count;
            
            new ExpandVertices(0.1f).Apply(mesh);
            
            Assert.AreEqual(originalVertexCount, mesh.Vertices.Count);
            Assert.AreEqual(originalFaceCount, mesh.Faces.Count);
        }
    }
}