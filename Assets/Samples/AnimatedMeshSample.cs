using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using Unity.Mathematics;

public class AnimatedMeshSample : MonoBehaviour
{
    [SerializeField] float animationSpeed = 1f;
    [SerializeField] float maxTwistAngle = 90f;
    [SerializeField] int segments = 12;
    [SerializeField] int heightSegments = 1;
    [SerializeField] MeshData.ShadingMode shadingMode = MeshData.ShadingMode.Smooth;

    MeshData baseMeshData;

    void Start()
    {
        GenerateBaseMesh();
    }

    void GenerateBaseMesh()
    {
        var generator = new CylinderGenerator(1f, 3f, segments, heightSegments, true);
        baseMeshData = generator.Generate();
    }

    void Update()
    {
        if (baseMeshData == null) return;

        var generator = new CylinderGenerator(1f, 3f, segments, heightSegments, true);
        var meshData = generator.Generate();

        var twistAmount = Mathf.Sin(Time.time * animationSpeed) * maxTwistAngle * Mathf.Deg2Rad;
        var twistModifier = new TwistModifier(
            new float3(0, 1, 0),
            float3.zero,
            twistAmount,
            2f
        );
        twistModifier.Apply(meshData);

        var scaleAmount = 1f + Mathf.Sin(Time.time * animationSpeed * 2f) * 0.2f;
        var scaleModifier = new ScaleModifier(scaleAmount);
        scaleModifier.Apply(meshData);

        var unityMesh = meshData.ToUnityMesh(shadingMode);
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}