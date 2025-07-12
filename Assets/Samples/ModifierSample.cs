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
    
    void Start()
    {
        GenerateMesh();
    }
    
    void GenerateMesh()
    {
        var generator = new BoxGenerator(boxSize, boxSize, boxSize, subdivisions);
        var meshData = generator.Generate();
        
        if (useExtrude)
        {
            var extrudeModifier = new ExtrudeModifier(extrudeDistance, true);
            extrudeModifier.Apply(meshData);
        }
        
        if (useScale)
        {
            var scaleModifier = new ScaleModifier(new float3(scaleVector.x, scaleVector.y, scaleVector.z));
            scaleModifier.Apply(meshData);
        }
        
        if (useTwist)
        {
            var twistModifier = new TwistModifier(
                new float3(twistAxis.x, twistAxis.y, twistAxis.z),
                float3.zero,
                twistAngle * Mathf.Deg2Rad,
                boxSize * 2f
            );
            twistModifier.Apply(meshData);
        }
        
        if (useSmooth)
        {
            var smoothModifier = new SmoothModifier(smoothingFactor, smoothIterations);
            smoothModifier.Apply(meshData);
        }
        
        var unityMesh = meshData.ToUnityMesh();
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}