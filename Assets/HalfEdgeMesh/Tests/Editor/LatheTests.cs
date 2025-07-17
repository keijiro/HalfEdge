using NUnit.Framework;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;
using System.Collections.Generic;

namespace HalfEdgeMesh.Tests
{
    public class LatheTests
    {
        [Test]
        public void Generate_SimpleProfile_CreatesValidMesh()
        {
            var profile = new List<float2>
            {
                new float2(0.5f, -1.0f),
                new float2(1.0f, 0.0f),
                new float2(0.5f, 1.0f)
            };

            var segments = 8;
            var generator = new Lathe(profile, segments);
            var mesh = generator.Generate();

            Assert.AreEqual(segments * profile.Count, mesh.Vertices.Count);
            Assert.AreEqual(segments * (profile.Count - 1), mesh.Faces.Count);
        }

        [Test]
        public void Generate_ConnectivityValid()
        {
            var profile = new List<float2>
            {
                new float2(1.0f, -0.5f),
                new float2(1.0f, 0.5f)
            };

            var generator = new Lathe(profile, 6);
            var mesh = generator.Generate();

            foreach (var vertex in mesh.Vertices)
            {
                Assert.IsNotNull(vertex.HalfEdge);
                Assert.AreEqual(vertex, vertex.HalfEdge.Origin);
            }
        }
    }
}