using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using Unity.Mathematics;

public class AnimatedMeshSample : MonoBehaviour
{
    [SerializeField] float animationSpeed = 1f;
    [SerializeField] float maxTwistAngle = 90f;
    [SerializeField] Material material;
    
    MeshData baseMeshData;
    
    void Start()
    {
        SetupMeshComponents();
        GenerateBaseMesh();
    }
    
    void SetupMeshComponents()
    {
        if (GetComponent<MeshFilter>() == null)
            gameObject.AddComponent<MeshFilter>();
        
        if (GetComponent<MeshRenderer>() == null)
        {
            var renderer = gameObject.AddComponent<MeshRenderer>();
            if (material != null)
                renderer.material = material;
            else
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }
    }
    
    void GenerateBaseMesh()
    {
        var generator = new CylinderGenerator(1f, 3f, 12, true);
        baseMeshData = generator.Generate();
    }
    
    void Update()
    {
        if (baseMeshData == null) return;
        
        var generator = new CylinderGenerator(1f, 3f, 12, true);
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
        
        var unityMesh = meshData.ToUnityMesh();
        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}