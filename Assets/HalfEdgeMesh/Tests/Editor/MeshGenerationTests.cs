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
}