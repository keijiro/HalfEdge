using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using Unity.Mathematics;
using System.Collections.Generic;

public class AllGeneratorsSample : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] Material material;
    [SerializeField] float spacing = 3f;
    [SerializeField] int generatorsPerRow = 5;
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;
    
    [Header("Generator Categories")]
    [SerializeField] bool showBasicPrimitives = true;
    [SerializeField] bool showPlatonicSolids = true;
    [SerializeField] bool showComplexShapes = true;
    [SerializeField] bool showProceduralShapes = true;

    [Header("Modifier Settings")]
    [SerializeField] bool applyModifiers = false;
    [SerializeField] float chamferAmount = 0.05f;
    [SerializeField] float expandAmount = 0.1f;

    void Start()
    {
        CreateAllGenerators();
    }

    void CreateAllGenerators()
    {
        var allGenerators = new List<(string name, string category, System.Func<HalfEdgeMesh.Mesh> generator)>();

        // Basic Primitives
        if (showBasicPrimitives)
        {
            allGenerators.Add(("Box", "Basic", () => new Box(1f, 1f, 1f, 0).Generate()));
            allGenerators.Add(("Plane", "Basic", () => new HalfEdgeMesh.Generators.Plane(3, 3, 2f).Generate()));
            allGenerators.Add(("Cylinder", "Basic", () => new Cylinder(0.8f, 1.5f, 12, true).Generate()));
            allGenerators.Add(("Cone", "Basic", () => new Cone(0.8f, 1.5f, 12).Generate()));
            allGenerators.Add(("Sphere", "Basic", () => new Sphere(0.9f, 2).Generate()));
            allGenerators.Add(("Icosphere", "Basic", () => new Icosphere(0.9f, 1).Generate()));
        }

        // Platonic Solids
        if (showPlatonicSolids)
        {
            allGenerators.Add(("Tetrahedron", "Platonic", () => new Tetrahedron(1.2f).Generate()));
            allGenerators.Add(("Octahedron", "Platonic", () => new Octahedron(1.0f).Generate()));
            allGenerators.Add(("Icosahedron", "Platonic", () => new Icosahedron(0.9f).Generate()));
            allGenerators.Add(("Dodecahedron", "Platonic", () => new Dodecahedron(1.0f).Generate()));
        }

        // Complex Shapes
        if (showComplexShapes)
        {
            allGenerators.Add(("Torus", "Complex", () => new Torus(0.6f, 0.3f, 16, 8).Generate()));
        }

        // Procedural Shapes
        if (showProceduralShapes)
        {
            allGenerators.Add(("Extrusion", "Procedural", () => CreateExtrusionSample()));
            allGenerators.Add(("Lathe", "Procedural", () => CreateLatheSample()));
        }

        // Create mesh objects
        for (int i = 0; i < allGenerators.Count; i++)
        {
            var (name, category, generator) = allGenerators[i];
            var row = i / generatorsPerRow;
            var col = i % generatorsPerRow;
            var position = new Vector3(col * spacing, row * -spacing, 0);
            
            var meshData = generator();
            
            // Apply modifiers if enabled
            if (applyModifiers)
            {
                if (chamferAmount > 0)
                    meshData = ChamferVertices.Apply(meshData, chamferAmount);
                    
                if (expandAmount > 0)
                    meshData = ExpandVertices.Apply(meshData, expandAmount);
            }
            
            CreateMeshObject($"{name} ({category})", meshData, position);
        }
    }

    HalfEdgeMesh.Mesh CreateExtrusionSample()
    {
        // Create a star-shaped profile
        var profile = new List<float3>();
        var points = 5;
        var outerRadius = 0.8f;
        var innerRadius = 0.4f;
        
        for (int i = 0; i < points * 2; i++)
        {
            var angle = i * math.PI / points;
            var radius = (i % 2 == 0) ? outerRadius : innerRadius;
            var x = math.cos(angle) * radius;
            var z = math.sin(angle) * radius;
            profile.Add(new float3(x, 0, z));
        }
        
        return new Extrusion(profile, 1.0f).Generate();
    }

    HalfEdgeMesh.Mesh CreateLatheSample()
    {
        // Create a vase-like profile
        var profile = new List<float2>
        {
            new float2(0.2f, -1.0f),
            new float2(0.6f, -0.8f),
            new float2(0.8f, -0.4f),
            new float2(0.7f, 0.0f),
            new float2(0.9f, 0.4f),
            new float2(0.8f, 0.8f),
            new float2(0.5f, 1.0f)
        };
        
        return new Lathe(profile, 20).Generate();
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

        // Add a label
        var labelObject = new GameObject("Label");
        labelObject.transform.parent = meshObject.transform;
        labelObject.transform.localPosition = Vector3.down * 1.5f;
        labelObject.transform.LookAt(Camera.main?.transform ?? transform);
        labelObject.transform.Rotate(0, 180, 0);

        var textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = objectName;
        textMesh.fontSize = 20;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
    }
}