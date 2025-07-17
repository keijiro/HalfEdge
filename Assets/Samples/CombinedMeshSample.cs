using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;
using System.Collections.Generic;

public class CombinedMeshSample : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] float spacing = 3f;
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;
    [SerializeField] bool showAllGenerators = true;
    [SerializeField] int generatorsPerRow = 5;

    void Start()
    {
        CreateMeshGrid();
    }

    void CreateMeshGrid()
    {
        var generators = new List<(string name, System.Func<HalfEdgeMesh.Mesh> generator)>();
        
        // Basic Primitives
        generators.Add(("Box", () => new Box(1f, 1f, 1f, 0).Generate()));
        generators.Add(("Cylinder", () => new Cylinder(0.8f, 1.5f, 12, true).Generate()));
        generators.Add(("Sphere", () => new Sphere(0.9f, 2).Generate()));
        
        // New Generators
        generators.Add(("Plane", CreatePlaneGenerator));
        generators.Add(("Icosphere", CreateIcosphereGenerator));
        generators.Add(("Cone", CreateConeGenerator));
        generators.Add(("Torus", CreateTorusGenerator));
        generators.Add(("Tetrahedron", CreateTetrahedronGenerator));
        generators.Add(("Octahedron", CreateOctahedronGenerator));
        generators.Add(("Icosahedron", CreateIcosahedronGenerator));
        generators.Add(("Dodecahedron", CreateDodecahedronGenerator));
        generators.Add(("Extrusion", CreateExtrusionSample));
        generators.Add(("Lathe", CreateLatheSample));

        if (!showAllGenerators)
        {
            // Show only the first 3 for compatibility
            generators = generators.GetRange(0, 3);
        }

        for (int i = 0; i < generators.Count; i++)
        {
            var (name, generator) = generators[i];
            var row = i / generatorsPerRow;
            var col = i % generatorsPerRow;
            var position = new Vector3(col * spacing, row * -spacing, 0);
            CreateMeshObject(name, generator(), position);
        }
    }

    HalfEdgeMesh.Mesh CreatePlaneGenerator() => new HalfEdgeMesh.Generators.Plane(3, 3, 2f).Generate();
    HalfEdgeMesh.Mesh CreateIcosphereGenerator() => new Icosphere(0.9f, 1).Generate();
    HalfEdgeMesh.Mesh CreateConeGenerator() => new Cone(0.8f, 1.5f, 12).Generate();
    HalfEdgeMesh.Mesh CreateTorusGenerator() => new Torus(0.6f, 0.3f, 16, 8).Generate();
    HalfEdgeMesh.Mesh CreateTetrahedronGenerator() => new Tetrahedron(1.5f).Generate();
    HalfEdgeMesh.Mesh CreateOctahedronGenerator() => new Octahedron(1.2f).Generate();
    HalfEdgeMesh.Mesh CreateIcosahedronGenerator() => new Icosahedron(1.0f).Generate();
    HalfEdgeMesh.Mesh CreateDodecahedronGenerator() => new Dodecahedron(1.2f).Generate();

    HalfEdgeMesh.Mesh CreateExtrusionSample()
    {
        var profile = new List<float3>
        {
            new float3(0.2f, 0, 0.2f),
            new float3(0.7f, 0, 0.2f),
            new float3(0.7f, 0, 0.7f),
            new float3(0.2f, 0, 0.7f)
        };
        return new Extrusion(profile, 1.0f).Generate();
    }

    HalfEdgeMesh.Mesh CreateLatheSample()
    {
        var profile = new List<float2>
        {
            new float2(0.3f, -0.8f),
            new float2(0.8f, -0.4f),
            new float2(0.9f, 0.0f),
            new float2(0.7f, 0.4f),
            new float2(0.4f, 0.8f)
        };
        return new Lathe(profile, 16).Generate();
    }

    void CreateMeshObject(string objectName, HalfEdgeMesh.Mesh meshData, Vector3 position)
    {
        var meshObject = new GameObject(objectName);
        meshObject.transform.parent = transform;
        meshObject.transform.position = position;

        var meshFilter = meshObject.AddComponent<MeshFilter>();
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();

        if (material != null)
            meshRenderer.material = material;
        else
            meshRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

        var unityMesh = meshData.ToUnityMesh(shadingMode);
        meshFilter.mesh = unityMesh;
    }
}