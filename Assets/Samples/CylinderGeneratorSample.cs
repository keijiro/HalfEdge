using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class CylinderGeneratorSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] float height = 2f;
    [SerializeField] int segments = 16;
    [SerializeField] bool capped = true;
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
        var generator = new CylinderGenerator(radius, height, segments, capped);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            segments = Mathf.Max(3, segments);
            GenerateMesh();
        }
    }
}