using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Generators;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Samples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SphereSample : MonoBehaviour
    {
        [SerializeField] float radius = 1.0f;
        [SerializeField] int2 segments = new int2(16, 12);
        [SerializeField] bool animateSegments = true;

        MeshFilter meshFilter;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        void Update() => GenerateMesh();

        void GenerateMesh()
        {
            var currentSegments = segments;

            if (animateSegments)
            {
                var t = Time.time;
                var lonVariation = (int)(4 * math.sin(t * 0.5f));
                var latVariation = (int)(3 * math.cos(t * 0.7f));
                currentSegments += new int2(lonVariation, latVariation);
                currentSegments = math.max(currentSegments, 3);
            }

            var meshData = Sphere.Generate(radius, currentSegments, Allocator.Persistent);
            try
            {
                if (meshFilter.mesh == null)
                    meshFilter.mesh = new Mesh();

                meshData.UpdateUnityMesh(meshFilter.mesh);
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