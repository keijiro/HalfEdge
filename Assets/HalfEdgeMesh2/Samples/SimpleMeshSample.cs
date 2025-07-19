using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Samples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SimpleMeshSample : MonoBehaviour
    {
        MeshFilter meshFilter;
        MeshBuilder builder;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            builder = new MeshBuilder();
        }

        void Update() => GenerateMesh();

        void GenerateMesh()
        {
            var meshData = CreatePyramid();
            try
            {
                if (meshFilter.mesh == null)
                    meshFilter.mesh = new Mesh();
                    
                meshData.UpdateUnityMesh(meshFilter.mesh, true);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        MeshData CreatePyramid()
        {
            builder.Clear();

            var animatedHeight = 1f + math.sin(Time.time * 2f) * 0.5f;

            builder.AddVertex(new float3(-0.5f, 0, -0.5f));
            builder.AddVertex(new float3( 0.5f, 0, -0.5f));
            builder.AddVertex(new float3( 0.5f, 0,  0.5f));
            builder.AddVertex(new float3(-0.5f, 0,  0.5f));
            builder.AddVertex(new float3(0, animatedHeight, 0));

            builder.AddFace(0, 1, 2, 3);
            builder.AddFace(0, 4, 1);
            builder.AddFace(1, 4, 2);
            builder.AddFace(2, 4, 3);
            builder.AddFace(3, 4, 0);

            return builder.Build(Allocator.Persistent);
        }

        void OnDestroy()
        {
            if (meshFilter?.mesh != null)
                DestroyImmediate(meshFilter.mesh);
        }
    }
}