using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class CylinderGeneratorSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] float height = 2f;
    [SerializeField] int segments = 16;
    [SerializeField] bool capped = true;
    
    void Start()
    {
        GenerateMesh();
    }
    
    void GenerateMesh()
    {
        var generator = new CylinderGenerator(radius, height, segments, capped);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();
        
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}