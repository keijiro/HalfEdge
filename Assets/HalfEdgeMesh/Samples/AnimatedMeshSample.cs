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
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    HalfEdgeMesh.Mesh baseMesh;

    void Start()
    {
        GenerateBaseMesh();
    }

    void GenerateBaseMesh()
    {
        var generator = new Cylinder(1f, 3f, new int2(segments, heightSegments), true);
        baseMesh = generator.Generate();
    }

    void Update()
    {
        if (baseMesh == null) return;

        var generator = new Cylinder(1f, 3f, new int2(segments, heightSegments), true);
        var meshData = generator.Generate();

        var twistAmount = Mathf.Sin(Time.time * animationSpeed) * maxTwistAngle * Mathf.Deg2Rad;
        var twistMesh = new TwistMesh(
            new float3(0, 1, 0),
            float3.zero,
            twistAmount,
            2f
        );
        twistMesh.Apply(meshData);

        var scaleAmount = 1f + Mathf.Sin(Time.time * animationSpeed * 2f) * 0.2f;
        var stretchMesh = new StretchMesh(scaleAmount);
        stretchMesh.Apply(meshData);

        var unityMesh = meshData.ToUnityMesh(shadingMode);
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}