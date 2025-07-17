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
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            var chamfered = ChamferVertices.Apply(mesh, 0.1f);
            
            Assert.AreEqual(mesh.Vertices.Count, chamfered.Vertices.Count);
            Assert.AreEqual(mesh.Faces.Count, chamfered.Faces.Count);
        }

        [Test]
        public void ExpandVertices_ApplyToBox_CreatesValidMesh()
        {
            var generator = new Box(1, 1, 1);
            var mesh = generator.Generate();
            
            var expanded = ExpandVertices.Apply(mesh, 0.1f);
            
            Assert.AreEqual(mesh.Vertices.Count, expanded.Vertices.Count);
            Assert.AreEqual(mesh.Faces.Count, expanded.Faces.Count);
        }
    }
}