using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;

public class CustomMeshSample : MonoBehaviour
{
    [SerializeField] Material material;
    
    void Start()
    {
        SetupMeshComponents();
        GenerateCustomMesh();
    }
    
    void SetupMeshComponents()
    {
        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        
        if (GetComponent<MeshRenderer>() == null)
        {
            var renderer = gameObject.AddComponent<MeshRenderer>();
            if (material != null)
                renderer.material = material;
            else
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
    }
    
    void GenerateCustomMesh()
    {
        var vertices = new float3[]
        {
            new float3(0, 0, 0),
            new float3(2, 0, 0),
            new float3(2, 2, 0),
            new float3(0, 2, 0),
            new float3(1, 1, 2),
            new float3(0, 0, 1),
            new float3(2, 0, 1),
            new float3(2, 2, 1),
            new float3(0, 2, 1)
        };
        
        var faces = new int[][]
        {
            new int[] { 0, 1, 2, 3 },
            new int[] { 4, 7, 6, 5 },
            new int[] { 0, 5, 6, 1 },
            new int[] { 1, 6, 7, 2 },
            new int[] { 2, 7, 8, 3 },
            new int[] { 3, 8, 5, 0 },
            new int[] { 0, 3, 4, 5 },
            new int[] { 1, 4, 7, 2 },
            new int[] { 5, 8, 4 },
            new int[] { 6, 4, 8 },
            new int[] { 7, 8, 4 },
            new int[] { 6, 8, 7 }
        };
        
        var builder = new IndexedMeshBuilder(vertices, faces);
        var meshData = builder.Build();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}