using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using Unity.Mathematics;

public class ModifierSample : MonoBehaviour
{
    [Header("Base Mesh")]
    [SerializeField] float boxSize = 2f;
    [SerializeField] int subdivisions = 1;

    [Header("Extrude Modifier")]
    [SerializeField] bool useExtrude = true;
    [SerializeField] float extrudeDistance = 0.3f;

    [Header("Scale Modifier")]
    [SerializeField] bool useScale = false;
    [SerializeField] Vector3 scaleVector = Vector3.one;

    [Header("Twist Modifier")]
    [SerializeField] bool useTwist = false;
    [SerializeField] float twistAngle = 45f;
    [SerializeField] Vector3 twistAxis = Vector3.up;

    [Header("Smooth Modifier")]
    [SerializeField] bool useSmooth = false;
    [SerializeField] float smoothingFactor = 0.5f;
    [SerializeField] int smoothIterations = 1;

    [Header("Rendering")]
    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    void Start()
    {
        GenerateMesh();
    }

    void GenerateMesh()
    {
        var generator = new Box(boxSize, boxSize, boxSize, subdivisions);
        var meshData = generator.Generate();

        if (useExtrude)
        {
            var extrudeFaces = new ExtrudeFaces(extrudeDistance, true);
            extrudeFaces.Apply(meshData);
        }

        if (useScale)
        {
            var stretchMesh = new StretchMesh(new float3(scaleVector.x, scaleVector.y, scaleVector.z));
            stretchMesh.Apply(meshData);
        }

        if (useTwist)
        {
            var twistMesh = new TwistMesh(
                new float3(twistAxis.x, twistAxis.y, twistAxis.z),
                float3.zero,
                twistAngle * Mathf.Deg2Rad,
                boxSize * 2f
            );
            twistMesh.Apply(meshData);
        }

        if (useSmooth)
        {
            var smoothVertices = new SmoothVertices(smoothingFactor, smoothIterations);
            smoothVertices.Apply(meshData);
        }

        var unityMesh = meshData.ToUnityMesh(shadingMode);
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}