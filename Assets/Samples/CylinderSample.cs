using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class CylinderSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] float height = 2f;
    [SerializeField] int segments = 16;
    [SerializeField] int heightSegments = 1;
    [SerializeField] bool capped = true;
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        var generator = new Cylinder(radius, height, segments, heightSegments, capped);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}