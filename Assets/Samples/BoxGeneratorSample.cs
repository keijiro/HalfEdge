using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class BoxGeneratorSample : MonoBehaviour
{
    [SerializeField] float width = 2f;
    [SerializeField] float height = 2f;
    [SerializeField] float depth = 2f;
    [SerializeField] int subdivisions = 0;
    
    void Start()
    {
        GenerateMesh();
    }
    
    void GenerateMesh()
    {
        var generator = new BoxGenerator(width, height, depth, subdivisions);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}