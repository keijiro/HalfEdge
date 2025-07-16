using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class SphereSample : MonoBehaviour
{
    [SerializeField] float radius = 1f;
    [SerializeField] int resolution = 0;
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    void Start()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) return;

        var generator = new Sphere(radius, resolution);
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        meshFilter.mesh = unityMesh;
    }
}
