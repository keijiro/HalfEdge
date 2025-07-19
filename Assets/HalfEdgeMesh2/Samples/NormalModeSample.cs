using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using HalfEdgeMesh2.Generators;
using HalfEdgeMesh2.Unity;

namespace HalfEdgeMesh2.Samples
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class NormalModeSample : MonoBehaviour
    {
        [SerializeField] float radius = 1.0f;
        [SerializeField] int2 segments = new int2(8, 6);
        [SerializeField] NormalGenerationMode normalMode = NormalGenerationMode.Smooth;
        [SerializeField] bool animateMode = true;
        [SerializeField] float modeAnimationSpeed = 1.0f;

        MeshFilter meshFilter;
        float lastModeChangeTime;

        void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        void Update() => GenerateMesh();

        void GenerateMesh()
        {
            var currentMode = normalMode;

            if (animateMode)
            {
                // Switch between modes every few seconds
                var cycleTime = 4.0f / modeAnimationSpeed;
                if (Time.time - lastModeChangeTime > cycleTime)
                {
                    currentMode = currentMode == NormalGenerationMode.Smooth
                        ? NormalGenerationMode.Flat
                        : NormalGenerationMode.Smooth;
                    lastModeChangeTime = Time.time;
                }
            }

            var meshData = Sphere.Generate(radius, segments, Allocator.Persistent);
            try
            {
                if (meshFilter.mesh == null)
                    meshFilter.mesh = new Mesh();

                meshData.UpdateUnityMesh(meshFilter.mesh, currentMode);

                // Update object name to show current mode
                gameObject.name = $"Sphere ({currentMode} Normals)";
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