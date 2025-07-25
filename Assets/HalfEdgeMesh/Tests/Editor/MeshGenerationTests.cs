using NUnit.Framework;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;

public class MeshGenerationTests
{
    [Test]
    public void Box_GeneratesExpectedVertexCount()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(1, 1, 1));
        var mesh = generator.Generate();
        Assert.AreEqual(8, mesh.Vertices.Count);
    }

    [Test]
    public void Box_GeneratesExpectedFaceCount()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(1, 1, 1));
        var mesh = generator.Generate();
        Assert.AreEqual(6, mesh.Faces.Count);
    }

    [Test]
    public void Cylinder_GeneratesCorrectVertexCount()
    {
        var segments = 8;
        var generator = new Cylinder(1f, 2f, new int2(segments, 1), true);
        var mesh = generator.Generate();
        Assert.AreEqual(segments * 2 + 2, mesh.Vertices.Count);
    }

    [Test]
    public void Sphere_UVSphere_GeneratesValidMesh()
    {
        var generator = new Sphere(1f, new int2(8, 6));
        var mesh = generator.Generate();

        Assert.Greater(mesh.Vertices.Count, 0);
        Assert.Greater(mesh.Faces.Count, 0);

        foreach (var vertex in mesh.Vertices)
        {
            var distance = math.length(vertex.Position);
            Assert.AreEqual(1f, distance, 0.001f);
        }
    }

    

    [Test]
    public void IndexedMesh_BuildsCorrectMesh()
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

        Assert.AreEqual(4, mesh.Vertices.Count);
        Assert.AreEqual(1, mesh.Faces.Count);
    }

    [Test]
    public void ToUnityMesh_SmoothShading_SharesVertices()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(1, 1, 1));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(Mesh.NormalGenerationMode.Smooth);

        // A box has 8 unique vertices when using smooth shading
        Assert.AreEqual(8, unityMesh.vertices.Length);
        // 6 faces * 2 triangles per face * 3 vertices per triangle = 36 indices
        Assert.AreEqual(36, unityMesh.triangles.Length);
    }

    [Test]
    public void ToUnityMesh_FlatShading_DuplicatesVertices()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(1, 1, 1));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(Mesh.NormalGenerationMode.Flat);

        // With flat shading, each triangle has its own vertices
        // 6 faces * 2 triangles per face * 3 vertices per triangle = 36 vertices
        Assert.AreEqual(36, unityMesh.vertices.Length);
        Assert.AreEqual(36, unityMesh.triangles.Length);

        // Check that normals are set
        Assert.AreEqual(36, unityMesh.normals.Length);
    }

    [Test]
    public void ToUnityMesh_FlatShading_GeneratesCorrectNormals()
    {
        // Create a simple pyramid to test face normals
        var vertices = new float3[]
        {
            new float3(-1, 0, -1),
            new float3( 1, 0, -1),
            new float3( 1, 0,  1),
            new float3(-1, 0,  1),
            new float3( 0, 2,  0)
        };

        var faces = new int[][]
        {
            new int[] { 0, 1, 2, 3 }, // base (should point down)
            new int[] { 0, 4, 1 },    // back face
            new int[] { 1, 4, 2 },    // right face
            new int[] { 2, 4, 3 },    // front face
            new int[] { 3, 4, 0 }     // left face
        };

        var builder = new IndexedMesh(vertices, faces);
        var meshData = builder.Build();
        var unityMesh = meshData.ToUnityMesh(Mesh.NormalGenerationMode.Flat);

        // Check that all normals for the base face point downward
        var baseNormal = unityMesh.normals[0];
        Assert.AreEqual(-1f, baseNormal.y, 0.001f);
        Assert.AreEqual(baseNormal, unityMesh.normals[1]);
        Assert.AreEqual(baseNormal, unityMesh.normals[2]);
    }
}