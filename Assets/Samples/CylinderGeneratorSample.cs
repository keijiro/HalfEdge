using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class CylinderGeneratorSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] float height = 2f;
    [SerializeField] int segments = 16;
    [SerializeField] int heightSegments = 1;
    [SerializeField] bool capped = true;
    [SerializeField] MeshData.ShadingMode shadingMode = MeshData.ShadingMode.Smooth;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        var generator = new CylinderGenerator(radius, height, segments, heightSegments, capped);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}