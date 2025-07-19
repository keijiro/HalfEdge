using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Generators;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Samples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class BoxSample : MonoBehaviour
    {
        [SerializeField] float3 size = new float3(2, 2, 2);
        [SerializeField] int3 segments = new int3(2, 2, 2);
        [SerializeField] NormalGenerationMode normalMode = NormalGenerationMode.Smooth;
        [SerializeField] bool animateSegments = true;
        [SerializeField] bool animateSize = false;

        MeshFilter meshFilter;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        void Update() => GenerateMesh();

        void GenerateMesh()
        {
            var currentSize = size;
            var currentSegments = segments;

            if (animateSize)
            {
                var t = Time.time;
                var sizeVariation = new float3(
                    0.5f * math.sin(t * 0.7f),
                    0.3f * math.cos(t * 0.9f),
                    0.4f * math.sin(t * 1.1f)
                );
                currentSize += sizeVariation;
                currentSize = math.max(currentSize, 0.1f);
            }

            if (animateSegments)
            {
                var t = Time.time;
                var segmentVariation = new int3(
                    (int)(2 * math.sin(t * 0.5f)),
                    (int)(2 * math.cos(t * 0.6f)),
                    (int)(2 * math.sin(t * 0.8f))
                );
                currentSegments += segmentVariation;
                currentSegments = math.max(currentSegments, 1);
            }

            var meshData = Box.Generate(currentSize, currentSegments, Allocator.Persistent);
            try
            {
                if (meshFilter.mesh == null)
                    meshFilter.mesh = new Mesh();

                meshData.UpdateUnityMesh(meshFilter.mesh, normalMode);
            }
            finally
            {
                meshData.Dispose();
            }
        }

        void OnDestroy()
        {
            if (meshFilter?.mesh != null)
                DestroyImmediate(meshFilter.mesh);
        }
    }
}