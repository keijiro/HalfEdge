using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class BoxGeneratorSample : MonoBehaviour
{
    [SerializeField] float width = 2f;
    [SerializeField] float height = 2f;
    [SerializeField] float depth = 2f;
    [SerializeField] int subdivisions = 0;
    [SerializeField] Material material;
    
    void Start()
    {
        SetupMeshComponents();
        GenerateMesh();
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
    
    void GenerateMesh()
    {
        var generator = new BoxGenerator(width, height, depth, subdivisions);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
            GenerateMesh();
    }
}