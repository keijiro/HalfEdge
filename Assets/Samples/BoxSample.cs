using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class BoxSample : MonoBehaviour
{
    [SerializeField] float width = 2f;
    [SerializeField] float height = 2f;
    [SerializeField] float depth = 2f;
    [SerializeField] int subdivisions = 0;
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        var generator = new Box(width, height, depth, subdivisions);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}