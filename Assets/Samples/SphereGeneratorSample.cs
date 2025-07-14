using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class SphereGeneratorSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] int subdivisions = 2;
    [SerializeField] SphereGenerator.SphereType sphereType = SphereGenerator.SphereType.UV;
    [SerializeField] MeshData.ShadingMode shadingMode = MeshData.ShadingMode.Smooth;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        var generator = new SphereGenerator(radius, subdivisions, sphereType);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}