using NUnit.Framework;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;

public class MeshGenerationTests
{
    [Test]
    public void BoxGenerator_GeneratesExpectedVertexCount()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var mesh = generator.Generate();
        Assert.AreEqual(8, mesh.Vertices.Count);
    }

    [Test]
    public void BoxGenerator_GeneratesExpectedFaceCount()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var mesh = generator.Generate();
        Assert.AreEqual(6, mesh.Faces.Count);
    }

    [Test]
    public void CylinderGenerator_GeneratesCorrectVertexCount()
    {
        var segments = 8;
        var generator = new CylinderGenerator(1f, 2f, segments, true);
        var mesh = generator.Generate();
        Assert.AreEqual(segments * 2 + 2, mesh.Vertices.Count);
    }

    [Test]
    public void SphereGenerator_UVSphere_GeneratesValidMesh()
    {
        var generator = new SphereGenerator(1f, 0, SphereGenerator.SphereType.UV);
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
    public void SphereGenerator_Icosphere_GeneratesValidMesh()
    {
        var generator = new SphereGenerator(1f, 0, SphereGenerator.SphereType.Icosphere);
        var mesh = generator.Generate();

        Assert.AreEqual(12, mesh.Vertices.Count);
        Assert.AreEqual(20, mesh.Faces.Count);

        foreach (var vertex in mesh.Vertices)
        {
            var distance = math.length(vertex.Position);
            Assert.AreEqual(1f, distance, 0.001f);
        }
    }

    [Test]
    public void IndexedMeshBuilder_BuildsCorrectMesh()
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

        var builder = new IndexedMeshBuilder(vertices, faces);
        var mesh = builder.Build();

        Assert.AreEqual(4, mesh.Vertices.Count);
        Assert.AreEqual(1, mesh.Faces.Count);
    }

    [Test]
    public void ToUnityMesh_SmoothShading_SharesVertices()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(MeshData.ShadingMode.Smooth);

        // A box has 8 unique vertices when using smooth shading
        Assert.AreEqual(8, unityMesh.vertices.Length);
        // 6 faces * 2 triangles per face * 3 vertices per triangle = 36 indices
        Assert.AreEqual(36, unityMesh.triangles.Length);
    }

    [Test]
    public void ToUnityMesh_FlatShading_DuplicatesVertices()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(MeshData.ShadingMode.Flat);

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

        var builder = new IndexedMeshBuilder(vertices, faces);
        var meshData = builder.Build();
        var unityMesh = meshData.ToUnityMesh(MeshData.ShadingMode.Flat);

        // Check that all normals for the base face point downward
        var baseNormal = unityMesh.normals[0];
        Assert.AreEqual(-1f, baseNormal.y, 0.001f);
        Assert.AreEqual(baseNormal, unityMesh.normals[1]);
        Assert.AreEqual(baseNormal, unityMesh.normals[2]);
    }
}