using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;
using System.Collections.Generic;

public class CombinedMeshSample : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] float spacing = 3f;
    
    void Start()
    {
        CreateMeshGrid();
    }
    
    void CreateMeshGrid()
    {
        var generators = new List<(string name, System.Func<MeshData> generator)>
        {
            ("Box", () => new BoxGenerator(1f, 1f, 1f, 0).Generate()),
            ("Cylinder", () => new CylinderGenerator(0.8f, 1.5f, 12, true).Generate()),
            ("UV Sphere", () => new SphereGenerator(0.9f, 1, SphereGenerator.SphereType.UV).Generate()),
            ("Icosphere", () => new SphereGenerator(0.9f, 1, SphereGenerator.SphereType.Icosphere).Generate())
        };
        
        for (int i = 0; i < generators.Count; i++)
        {
            var (name, generator) = generators[i];
            CreateMeshObject(name, generator(), new Vector3(i * spacing, 0, 0));
        }
    }
    
    void CreateMeshObject(string objectName, MeshData meshData, Vector3 position)
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
        
        var unityMesh = meshData.ToUnityMesh();
        meshFilter.mesh = unityMesh;
    }
}