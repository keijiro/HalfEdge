using NUnit.Framework;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using Unity.Mathematics;

public class ModifierTests
{
    [Test]
    public void ExtrudeModifier_ExtrudesAllFaces()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var mesh = generator.Generate();
        
        var originalFaceCount = mesh.Faces.Count;
        var originalVertexCount = mesh.Vertices.Count;
        
        var modifier = new ExtrudeModifier(0.5f);
        modifier.Apply(mesh);
        
        Assert.Greater(mesh.Faces.Count, originalFaceCount);
        Assert.Greater(mesh.Vertices.Count, originalVertexCount);
    }
    
    [Test]
    public void ScaleModifier_ScalesUniformly()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var mesh = generator.Generate();
        
        var scaleFactor = 2f;
        var modifier = new ScaleModifier(scaleFactor);
        modifier.Apply(mesh);
        
        foreach (var vertex in mesh.Vertices)
        {
            var distance = math.length(vertex.Position);
            Assert.AreEqual(scaleFactor * math.sqrt(3f) / 2f, distance, 0.001f);
        }
    }
    
    [Test]
    public void TwistModifier_AppliesTwist()
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
        
        var originalY = mesh.Vertices[2].Position.y;
        
        var modifier = new TwistModifier(new float3(0, 1, 0), float3.zero, math.PI / 4f, 2f);
        modifier.Apply(mesh);
        
        Assert.AreEqual(originalY, mesh.Vertices[2].Position.y, 0.001f);
        Assert.AreNotEqual(vertices[2].x, mesh.Vertices[2].Position.x);
    }
    
    [Test]
    public void SmoothModifier_SmoothsVertices()
    {
        var generator = new BoxGenerator(1f, 1f, 1f, 1);
        var mesh = generator.Generate();
        
        var originalPositions = new float3[mesh.Vertices.Count];
        for (int i = 0; i < mesh.Vertices.Count; i++)
            originalPositions[i] = mesh.Vertices[i].Position;
        
        var modifier = new SmoothModifier(0.5f, 1);
        modifier.Apply(mesh);
        
        var hasChanged = false;
        for (int i = 0; i < mesh.Vertices.Count; i++)
        {
            if (!math.all(originalPositions[i] == mesh.Vertices[i].Position))
            {
                hasChanged = true;
                break;
            }
        }
        
        Assert.IsTrue(hasChanged);
    }
}