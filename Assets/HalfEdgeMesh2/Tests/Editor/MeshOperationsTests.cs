using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace HalfEdgeMesh2.Tests
{
    [TestFixture]
    public class MeshOperationsTests
    {
        private MeshData CreateTriangleMesh()
        {
            var builder = new MeshBuilder();
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0, 0, 1));
            builder.AddFace(0, 2, 1);  // CCW order for upward normal
            return builder.Build(Allocator.Temp);
        }

        private MeshData CreateTetrahedronMesh()
        {
            var builder = new MeshBuilder();

            // Add vertices
            builder.AddVertex(new float3(0, 0, 0));
            builder.AddVertex(new float3(1, 0, 0));
            builder.AddVertex(new float3(0.5f, 0, math.sqrt(0.75f)));
            builder.AddVertex(new float3(0.5f, math.sqrt(2f/3f), math.sqrt(0.75f) / 3f));

            // Add faces
            builder.AddFace(0, 1, 2);  // Bottom
            builder.AddFace(0, 3, 1);  // Front
            builder.AddFace(1, 3, 2);  // Right
            builder.AddFace(2, 3, 0);  // Left

            return builder.Build(Allocator.Temp);
        }

        [Test]
        public void ComputeFaceNormals_Triangle_ReturnsCorrectNormal()
        {
            var mesh = CreateTriangleMesh();
            var normals = new NativeArray<float3>(mesh.faceCount, Allocator.Temp);
            try
            {
                MeshOperations.ComputeFaceNormals(ref mesh, ref normals);

                // Normal should point in Y direction
                var expectedNormal = new float3(0, 1, 0);
                var actualNormal = normals[0];

                Assert.AreEqual(expectedNormal.x, actualNormal.x, 0.001f);
                Assert.AreEqual(expectedNormal.y, actualNormal.y, 0.001f);
                Assert.AreEqual(expectedNormal.z, actualNormal.z, 0.001f);
            }
            finally
            {
                mesh.Dispose();
                normals.Dispose();
            }
        }

        [Test]
        public void ComputeVertexNormals_Tetrahedron_ReturnsNormalizedNormals()
        {
            var mesh = CreateTetrahedronMesh();
            var normals = new NativeArray<float3>(mesh.vertexCount, Allocator.Temp);
            try
            {
                MeshOperations.ComputeVertexNormals(ref mesh, ref normals);

                // All normals should be normalized
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    var length = math.length(normals[i]);
                    Assert.AreEqual(1f, length, 0.001f);
                }
            }
            finally
            {
                mesh.Dispose();
                normals.Dispose();
            }
        }

        [Test]
        public void ComputeBounds_Triangle_ReturnsCorrectBounds()
        {
            var mesh = CreateTriangleMesh();
            try
            {
                MeshOperations.ComputeBounds(ref mesh, out var boundsCenter, out var boundsSize);

                Assert.AreEqual(new Vector3(0.5f, 0, 0.5f), (Vector3)boundsCenter);
                Assert.AreEqual(new Vector3(1, 0, 1), (Vector3)boundsSize);
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void CountEdges_Tetrahedron_Returns6Edges()
        {
            var mesh = CreateTetrahedronMesh();
            try
            {
                var edgeCount = MeshOperations.CountEdges(ref mesh);
                Assert.AreEqual(6, edgeCount); // Tetrahedron has 6 edges
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void GetVertexValence_Tetrahedron_Returns3ForEachVertex()
        {
            var mesh = CreateTetrahedronMesh();
            try
            {
                // In a tetrahedron, each vertex is connected to 3 others
                for (int i = 0; i < mesh.vertexCount; i++)
                {
                    var valence = MeshOperations.GetVertexValence(ref mesh, i);
                    Assert.AreEqual(3, valence);
                }
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void ValidateMesh_ValidMesh_ReturnsTrue()
        {
            var mesh = CreateTetrahedronMesh();
            try
            {
                Assert.IsTrue(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }

        [Test]
        public void ValidateMesh_InvalidReferences_ReturnsFalse()
        {
            var mesh = CreateTriangleMesh();
            try
            {
                // Corrupt a half-edge reference
                var he = mesh.halfEdges[0];
                he.next = 999; // Invalid index
                mesh.halfEdges[0] = he;

                Assert.IsFalse(MeshOperations.ValidateMesh(ref mesh));
            }
            finally
            {
                mesh.Dispose();
            }
        }
    }
}