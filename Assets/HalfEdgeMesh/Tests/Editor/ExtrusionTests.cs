using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using System.Collections.Generic;

namespace HalfEdgeMesh.Tests
{
    public class ExtrusionTests
    {
        [Test]
        public void Generate_SimpleProfile_CreatesValidMesh()
        {
            var profile = new List<float3>
            {
                new float3(0, 0, 0),
                new float3(1, 0, 0),
                new float3(1, 0, 1),
                new float3(0, 0, 1)
            };

            var generator = new Extrusion(profile, 2.0f);
            var mesh = generator.Generate();

            Assert.AreEqual(8, mesh.Vertices.Count); // 4 profile points * 2 (top/bottom)
            Assert.AreEqual(6, mesh.Faces.Count); // 4 sides + 2 caps

            foreach (var vertex in mesh.Vertices)
            {
                Assert.LessOrEqual(math.abs(vertex.Position.y), 1.01f);
            }
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var profile = new List<float3>
            {
                new float3(0, 0, 0),
                new float3(1, 0, 0),
                new float3(0, 0, 1)
            };

            var generator = new Extrusion(profile, 1.0f);
            var mesh = generator.Generate();

            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }
        }
    }
}