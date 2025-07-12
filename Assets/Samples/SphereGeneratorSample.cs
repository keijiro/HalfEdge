using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class SphereGeneratorSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] int subdivisions = 2;
    [SerializeField] SphereGenerator.SphereType sphereType = SphereGenerator.SphereType.UV;
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
        var generator = new SphereGenerator(radius, subdivisions, sphereType);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            subdivisions = Mathf.Max(0, subdivisions);
            GenerateMesh();
        }
    }
}