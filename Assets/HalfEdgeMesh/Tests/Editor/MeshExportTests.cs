using NUnit.Framework;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;
using UnityEngine;

public class MeshExportTests
{
    [Test]
    public void ToUnityMesh_GeneratesValidMesh()
    {
        var generator = new Box(1f, 1f, 1f, 0);
        var mesh = generator.Generate();

        var unityMesh = mesh.ToUnityMesh();

        Assert.IsNotNull(unityMesh);
        Assert.AreEqual(8, unityMesh.vertexCount);
        Assert.AreEqual(36, unityMesh.triangles.Length);
    }

    [Test]
    public void ToUnityMesh_PreservesVertexPositions()
    {
        var vertices = new float3[]
        {
            new float3(0, 0, 0),
            new float3(1, 0, 0),
            new float3(0.5f, 1, 0)
        };

        var faces = new int[][]
        {
            new int[] { 0, 1, 2 }
        };

        var builder = new IndexedMesh(vertices, faces);
        var mesh = builder.Build();
        var unityMesh = mesh.ToUnityMesh();

        for (int i = 0; i < vertices.Length; i++)
        {
            var expected = vertices[i];
            var actual = unityMesh.vertices[i];
            Assert.AreEqual(expected.x, actual.x, 0.001f);
            Assert.AreEqual(expected.y, actual.y, 0.001f);
            Assert.AreEqual(expected.z, actual.z, 0.001f);
        }
    }

    [Test]
    public void ToUnityMesh_HandlesPolygonalFaces()
    {
        var vertices = new float3[]
        {
            new float3(0, 0, 0),
            new float3(1, 0, 0),
            new float3(1, 1, 0),
            new float3(0, 1, 0)
        };

        var faces = new int[][]
        {
            new int[] { 0, 1, 2, 3 }
        };

        var builder = new IndexedMesh(vertices, faces);
        var mesh = builder.Build();
        var unityMesh = mesh.ToUnityMesh();

        Assert.AreEqual(4, unityMesh.vertexCount);
        Assert.AreEqual(6, unityMesh.triangles.Length);
    }

    [Test]
    public void HalfEdge_Topology_IsConsistent()
    {
        var generator = new Box(1f, 1f, 1f, 0);
        var mesh = generator.Generate();

        foreach (var halfEdge in mesh.HalfEdges)
        {
            Assert.IsNotNull(halfEdge.Next);
            Assert.IsNotNull(halfEdge.Origin);
            Assert.IsNotNull(halfEdge.Face);

            if (halfEdge.Twin != null)
            {
                Assert.AreEqual(halfEdge, halfEdge.Twin.Twin);
                Assert.AreEqual(halfEdge.Origin, halfEdge.Twin.Destination);
                Assert.AreEqual(halfEdge.Destination, halfEdge.Twin.Origin);
            }
        }

        foreach (var face in mesh.Faces)
        {
            var start = face.HalfEdge;
            var current = start;
            var count = 0;

            do
            {
                Assert.AreEqual(face, current.Face);
                current = current.Next;
                count++;
            } while (current != start && count < 100);

            Assert.AreEqual(start, current);
            Assert.Less(count, 100);
        }
    }
}