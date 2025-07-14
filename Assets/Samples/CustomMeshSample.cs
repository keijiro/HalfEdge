using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;

public class CustomMeshSample : MonoBehaviour
{
    [SerializeField] MeshData.ShadingMode shadingMode = MeshData.ShadingMode.Smooth;

    void Start()
    {
        GenerateCustomMesh();
    }

    void GenerateCustomMesh()
    {
        // Create a simple pyramid (square pyramid)
        // This demonstrates IndexedMeshBuilder with mixed quad and triangle faces
        var vertices = new float3[]
        {
            // Base vertices (square base at Y=0)
            new float3(-1, 0, -1), // 0: bottom-left-back
            new float3( 1, 0, -1), // 1: bottom-right-back
            new float3( 1, 0,  1), // 2: bottom-right-fron
            new float3(-1, 0,  1), // 3: bottom-left-fron

            // Apex vertex
            new float3( 0, 2,  0)  // 4: top center
        };

        var faces = new int[][]
        {
            // Base face (quad) - counter-clockwise when viewed from below (outward normal)
            new int[] { 0, 1, 2, 3 },

            // Side faces (triangles) - counter-clockwise when viewed from outside
            new int[] { 0, 4, 1 }, // back face
            new int[] { 1, 4, 2 }, // right face
            new int[] { 2, 4, 3 }, // front face
            new int[] { 3, 4, 0 }  // left face
        };

        var builder = new IndexedMeshBuilder(vertices, faces);
        var meshData = builder.Build();
        var unityMesh = meshData.ToUnityMesh(shadingMode);

        GetComponent<MeshFilter>().mesh = unityMesh;
    }
}