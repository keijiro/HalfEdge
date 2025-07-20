using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using HalfEdgeMesh2.Generators;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Samples
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GeneratorSample : MonoBehaviour
    {
        public enum GeneratorType
        {
            Box,
            Sphere
        }

        [SerializeField] GeneratorType generatorType = GeneratorType.Box;
        [SerializeField] NormalGenerationMode normalMode = NormalGenerationMode.Smooth;
        [SerializeField] bool animateSize = false;
        [SerializeField] float3 boxSize = new float3(2, 2, 2);
        [SerializeField] int3 boxSegments = new int3(2, 2, 2);
        [SerializeField] float sphereRadius = 1.0f;
        [SerializeField] int2 sphereSegments = new int2(16, 12);

        MeshFilter meshFilter;

        // Profiler markers
        static readonly ProfilerMarker s_GenerateMeshMarker = new ProfilerMarker("GeneratorSample.GenerateMesh");
        static readonly ProfilerMarker s_UpdateUnityMeshMarker = new ProfilerMarker("GeneratorSample.UpdateUnityMesh");
        Mesh generatedMesh;
        bool needsMeshInitialization;
        GeneratorType lastGeneratorType;
        NormalGenerationMode lastNormalMode;

        // Box state
        float3 lastBoxSize;
        int3 lastBoxSegments;

        // Sphere state
        float lastSphereRadius;
        int2 lastSphereSegments;

        // Animation state
        bool lastAnimateSize;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();

            // Create managed mesh with DontSave flag
            if (generatedMesh == null)
            {
                generatedMesh = new Mesh();
                generatedMesh.name = "Generated Mesh";
                generatedMesh.hideFlags = HideFlags.DontSave;
            }

            meshFilter.sharedMesh = generatedMesh;
            UpdateState();
            GenerateMesh();
        }

        void Update()
        {
            // Handle deferred mesh initialization
            if (needsMeshInitialization)
            {
                InitializeMeshIfNeeded();
                needsMeshInitialization = false;
            }

            bool needsUpdate = HasChanges();

            // Only animate in play mode
            var shouldAnimate = animateSize && Application.isPlaying;

            if (shouldAnimate || needsUpdate)
            {
                GenerateMesh();
                UpdateState();
            }
        }

        bool HasChanges()
        {
            return generatorType != lastGeneratorType ||
                   normalMode != lastNormalMode ||
                   !boxSize.Equals(lastBoxSize) ||
                   !boxSegments.Equals(lastBoxSegments) ||
                   sphereRadius != lastSphereRadius ||
                   !sphereSegments.Equals(lastSphereSegments) ||
                   animateSize != lastAnimateSize;
        }

        void UpdateState()
        {
            lastGeneratorType = generatorType;
            lastNormalMode = normalMode;
            lastBoxSize = boxSize;
            lastBoxSegments = boxSegments;
            lastSphereRadius = sphereRadius;
            lastSphereSegments = sphereSegments;
            lastAnimateSize = animateSize;
        }

        void GenerateMesh()
        {
            using (s_GenerateMeshMarker.Auto())
            {
                MeshData meshData;

                switch (generatorType)
                {
                    case GeneratorType.Box:
                        meshData = GenerateBox();
                        break;
                    case GeneratorType.Sphere:
                        meshData = GenerateSphere();
                        break;
                    default:
                        return;
                }

                try
                {
                    // Ensure we have a managed mesh
                    if (generatedMesh == null)
                    {
                        generatedMesh = new Mesh();
                        generatedMesh.name = "Generated Mesh";
                        generatedMesh.hideFlags = HideFlags.DontSave;
                        meshFilter.sharedMesh = generatedMesh;
                    }

                    using (s_UpdateUnityMeshMarker.Auto())
                    {
                        meshData.UpdateUnityMesh(generatedMesh, normalMode);
                    }
                }
                finally
                {
                    meshData.Dispose();
                }
            }
        }

        MeshData GenerateBox()
        {
            var currentSize = boxSize;
            var currentSegments = boxSegments;

            if (animateSize && Application.isPlaying)
            {
                var t = Time.time;
                var scale = math.lerp(0.9f, 1.0f, (math.sin(t) + 1.0f) * 0.5f);
                currentSize *= scale;
            }

            return Box.Generate(currentSize, currentSegments, Allocator.Persistent);
        }

        MeshData GenerateSphere()
        {
            var currentRadius = sphereRadius;
            var currentSegments = sphereSegments;

            if (animateSize && Application.isPlaying)
            {
                var t = Time.time;
                var scale = math.lerp(0.9f, 1.0f, (math.sin(t) + 1.0f) * 0.5f);
                currentRadius *= scale;
            }

            return Sphere.Generate(currentRadius, currentSegments, Allocator.Persistent);
        }

        void OnDestroy()
        {
            if (generatedMesh != null)
            {
                DestroyImmediate(generatedMesh);
                generatedMesh = null;
            }
        }

        void OnValidate()
        {
            // Clamp values to valid ranges
            boxSegments = math.max(boxSegments, 1);
            sphereSegments = math.max(sphereSegments, 3);
            sphereRadius = math.max(sphereRadius, 0.01f);
            boxSize = math.max(boxSize, 0.01f);

            // Schedule mesh initialization for next Update
            needsMeshInitialization = true;
        }

        void InitializeMeshIfNeeded()
        {
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();

            if (generatedMesh == null && meshFilter != null)
            {
                generatedMesh = new Mesh();
                generatedMesh.name = "Generated Mesh";
                generatedMesh.hideFlags = HideFlags.DontSave;
                meshFilter.sharedMesh = generatedMesh;
            }
        }
    }
}