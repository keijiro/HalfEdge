using UnityEngine;
using Unity.Mathematics;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using HalfEdgeMesh.Modifiers;
using System.Collections.Generic;

[ExecuteInEditMode]
public class GeneratorSample : MonoBehaviour
{
    #region Base Mesh Settings

    public enum GeneratorType
    {
        Box, Plane, Sphere, Icosphere, Cylinder, Cone, Torus,
        Tetrahedron, Octahedron, Dodecahedron
    }

    [SerializeField] GeneratorType _generatorType = GeneratorType.Box;

    // Common parameters
    [SerializeField] int _subdivisions = 4;

    // Box
    [SerializeField] float3 _boxSize = new float3(2f, 1f, 1.5f);
    [SerializeField] int3 _boxSegments = new int3(1, 1, 1);

    // Plane
    [SerializeField] float2 _planeSize = new float2(2f, 2f);
    [SerializeField] int2 _planeSegments = new int2(8, 8);

    // Sphere
    [SerializeField] float _sphereRadius = 1f;
    [SerializeField] int2 _sphereSegments = new int2(16, 12);

    // Icosphere
    [SerializeField] float _icosphereRadius = 1f;

    // Cylinder
    [SerializeField] float _cylinderRadius = 0.8f;
    [SerializeField] float _cylinderHeight = 2f;
    [SerializeField] int2 _cylinderSegments = new int2(16, 1);
    [SerializeField] bool _cylinderCapped = true;

    // Cone
    [SerializeField] float _coneRadius = 1f;
    [SerializeField] float _coneHeight = 1.5f;
    [SerializeField] int _coneSegments = 16;

    // Torus
    [SerializeField] float _torusMajorRadius = 1f;
    [SerializeField] float _torusMinorRadius = 0.3f;
    [SerializeField] int2 _torusSegments = new int2(16, 12);

    // Polyhedron
    [SerializeField] float _polyhedronSize = 1f;


    #endregion

    #region Modifier Settings

    [SerializeField] bool _useChamferVertices = false;
    [SerializeField] float _chamferVertexDistance = 0.1f;

    [SerializeField] bool _useChamferEdges = false;
    [SerializeField] float _chamferEdgeDistance = 0.1f;

    [SerializeField] bool _useExtrude = false;
    [SerializeField] float _extrudeDistance = 0.3f;


    [SerializeField] bool _useSplitFaces = false;
    [SerializeField] float3 _splitPlaneNormal = new float3(0, 1, 0);
    [SerializeField] float3 _splitPlanePoint = float3.zero;

    [SerializeField] bool _useSkew = false;
    [SerializeField] float _skewAngle = 15f;
    [SerializeField] float3 _skewDirection = new float3(1, 0, 0);

    [SerializeField] bool _useSmooth = false;
    [SerializeField] int _smoothIterations = 1;

    [SerializeField] bool _useStretch = false;
    [SerializeField] float3 _stretchScale = new float3(1.5f, 1, 1);

    [SerializeField] bool _useTwist = false;
    [SerializeField] float _twistAngle = 45f;
    [SerializeField] float3 _twistAxis = new float3(0, 1, 0);

    [SerializeField] bool _useExpand = false;
    [SerializeField] float _expandDistance = 0.2f;

    #endregion

    #region Rendering Settings

    [SerializeField] HalfEdgeMesh.Mesh.NormalGenerationMode _shadingMode = HalfEdgeMesh.Mesh.NormalGenerationMode.Smooth;

    #endregion

    #region Private members

    UnityEngine.Mesh _mesh;
    bool _dirty = true;

    #endregion

    #region MonoBehaviour implementation

    void OnValidate()
    {
        // パラメーター検証
        _cylinderSegments.x = Mathf.Max(3, _cylinderSegments.x);
        _cylinderSegments.y = Mathf.Max(1, _cylinderSegments.y);
        _coneSegments = Mathf.Max(3, _coneSegments);
        _torusSegments.x = Mathf.Max(3, _torusSegments.x);
        _torusSegments.y = Mathf.Max(3, _torusSegments.y);
        _planeSegments.x = Mathf.Max(1, _planeSegments.x);
        _planeSegments.y = Mathf.Max(1, _planeSegments.y);
        _sphereSegments.x = Mathf.Max(3, _sphereSegments.x);
        _sphereSegments.y = Mathf.Max(3, _sphereSegments.y);
        _subdivisions = Mathf.Max(0, _subdivisions);
        _boxSegments.x = Mathf.Max(1, _boxSegments.x);
        _boxSegments.y = Mathf.Max(1, _boxSegments.y);
        _boxSegments.z = Mathf.Max(1, _boxSegments.z);

        _dirty = true;
    }

    void OnDestroy()
    {
        if (_mesh != null) DestroyImmediate(_mesh);
    }

    void Update()
    {
        if (_dirty)
        {
            GenerateMesh();
            _dirty = false;
        }
    }

    #endregion

    #region Mesh generation

    void GenerateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new UnityEngine.Mesh();
            _mesh.hideFlags = HideFlags.DontSave;
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        var heMesh = CreateBaseMesh();
        heMesh = ApplyModifiers(heMesh);

        if (_mesh != null) DestroyImmediate(_mesh);
        _mesh = heMesh.ToUnityMesh(_shadingMode);
        _mesh.hideFlags = HideFlags.DontSave;
        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    HalfEdgeMesh.Mesh CreateBaseMesh()
    {
        switch (_generatorType)
        {
            case GeneratorType.Box: return new Box(_boxSize, _boxSegments).Generate();
            case GeneratorType.Plane: return new HalfEdgeMesh.Generators.Plane(_planeSize, _planeSegments).Generate();
            case GeneratorType.Sphere: return new Sphere(_sphereRadius, _sphereSegments).Generate();
            case GeneratorType.Icosphere: return new Icosphere(_icosphereRadius, _subdivisions).Generate();
            case GeneratorType.Cylinder: return new Cylinder(_cylinderRadius, _cylinderHeight, _cylinderSegments, _cylinderCapped).Generate();
            case GeneratorType.Cone: return new Cone(_coneRadius, _coneHeight, _coneSegments).Generate();
            case GeneratorType.Torus: return new Torus(_torusMajorRadius, _torusMinorRadius, _torusSegments).Generate();
            case GeneratorType.Tetrahedron: return new Tetrahedron(_polyhedronSize).Generate();
            case GeneratorType.Octahedron: return new Octahedron(_polyhedronSize).Generate();
            case GeneratorType.Dodecahedron: return new Dodecahedron(_polyhedronSize).Generate();
        }
        return null;
    }

    HalfEdgeMesh.Mesh ApplyModifiers(HalfEdgeMesh.Mesh mesh)
    {
        if (_useChamferVertices) mesh = ChamferVertices.Apply(mesh, _chamferVertexDistance);
        if (_useChamferEdges) mesh = ChamferEdges.Apply(mesh, _chamferEdgeDistance);
        if (_useExtrude) new ExtrudeFaces(_extrudeDistance, true).Apply(mesh);
        if (_useSplitFaces) mesh = SplitFaces.Apply(mesh, _splitPlaneNormal, _splitPlanePoint);
        if (_useSkew) new SkewMesh(_skewAngle * Mathf.Deg2Rad, _skewDirection).Apply(mesh);
        if (_useSmooth) new SmoothVertices(0.5f, _smoothIterations).Apply(mesh);
        if (_useStretch) new StretchMesh(_stretchScale).Apply(mesh);
        if (_useTwist) new TwistMesh(_twistAxis, float3.zero, _twistAngle * Mathf.Deg2Rad).Apply(mesh);
        if (_useExpand) new ExpandVertices(_expandDistance).Apply(mesh);
        return mesh;
    }

    #endregion
}
