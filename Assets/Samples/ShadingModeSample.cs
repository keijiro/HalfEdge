using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;

public class ShadingModeSample : MonoBehaviour
{
    [SerializeField] MeshData.ShadingMode shadingMode = MeshData.ShadingMode.Smooth;
    [SerializeField] bool generateBox = true;

    void Start()
    {
        UpdateMesh();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
            UpdateMesh();
    }

    void UpdateMesh()
    {
        MeshData meshData;

        if (generateBox)
        {
            var boxGen = new BoxGenerator(2, 2, 2);
            meshData = boxGen.Generate();
        }
        else
        {
            // Create a simple pyramid for testing
            var vertices = new float3[]
            {
                new float3(-1, 0, -1),
                new float3( 1, 0, -1),
                new float3( 1, 0,  1),
                new float3(-1, 0,  1),
                new float3( 0, 2,  0)
            };

            var faces = new int[][]
            {
                new int[] { 0, 1, 2, 3 },
                new int[] { 0, 4, 1 },
                new int[] { 1, 4, 2 },
                new int[] { 2, 4, 3 },
                new int[] { 3, 4, 0 }
            };

            var builder = new IndexedMeshBuilder(vertices, faces);
            meshData = builder.Build();
        }

        var unityMesh = meshData.ToUnityMesh(shadingMode);
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}