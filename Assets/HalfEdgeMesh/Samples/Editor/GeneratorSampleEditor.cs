using UnityEngine;
using UnityEditor;
using Unity.Mathematics;
using HalfEdgeMesh.Generators;

[CustomEditor(typeof(GeneratorSample))]
public class GeneratorSampleEditor : Editor
{
    SerializedProperty _generatorType;

    // Generator properties
    SerializedProperty _subdivisions;
    SerializedProperty _boxSize;
    SerializedProperty _boxSegments;
    SerializedProperty _planeSegments;
    SerializedProperty _planeSize;
    SerializedProperty _sphereRadius;
    SerializedProperty _sphereSegments;
    SerializedProperty _icosphereRadius;
    SerializedProperty _cylinderRadius;
    SerializedProperty _cylinderHeight;
    SerializedProperty _cylinderSegments;
    SerializedProperty _cylinderCapped;
    SerializedProperty _coneRadius;
    SerializedProperty _coneHeight;
    SerializedProperty _coneSegments;
    SerializedProperty _torusMajorRadius;
    SerializedProperty _torusMinorRadius;
    SerializedProperty _torusSegments;
    SerializedProperty _polyhedronSize;

    // Modifier properties
    SerializedProperty _useChamferVertices;
    SerializedProperty _chamferVertexDistance;
    SerializedProperty _useChamferEdges;
    SerializedProperty _chamferEdgeDistance;
    SerializedProperty _useExtrude;
    SerializedProperty _extrudeDistance;
    SerializedProperty _useSplitFaces;
    SerializedProperty _splitPlaneNormal;
    SerializedProperty _splitPlanePoint;
    SerializedProperty _useSkew;
    SerializedProperty _skewAngle;
    SerializedProperty _skewDirection;
    SerializedProperty _useSmooth;
    SerializedProperty _smoothIterations;
    SerializedProperty _useStretch;
    SerializedProperty _stretchScale;
    SerializedProperty _useTwist;
    SerializedProperty _twistAngle;
    SerializedProperty _twistAxis;
    SerializedProperty _useExpand;
    SerializedProperty _expandDistance;

    // Rendering properties
    SerializedProperty _shadingMode;

    void OnEnable()
    {
        _generatorType = serializedObject.FindProperty("_generatorType");

        // Generator properties
        _subdivisions = serializedObject.FindProperty("_subdivisions");
        _boxSize = serializedObject.FindProperty("_boxSize");
        _boxSegments = serializedObject.FindProperty("_boxSegments");
        _planeSegments = serializedObject.FindProperty("_planeSegments");
        _planeSize = serializedObject.FindProperty("_planeSize");
        _sphereRadius = serializedObject.FindProperty("_sphereRadius");
        _sphereSegments = serializedObject.FindProperty("_sphereSegments");
        _icosphereRadius = serializedObject.FindProperty("_icosphereRadius");
        _cylinderRadius = serializedObject.FindProperty("_cylinderRadius");
        _cylinderHeight = serializedObject.FindProperty("_cylinderHeight");
        _cylinderSegments = serializedObject.FindProperty("_cylinderSegments");
        _cylinderCapped = serializedObject.FindProperty("_cylinderCapped");
        _coneRadius = serializedObject.FindProperty("_coneRadius");
        _coneHeight = serializedObject.FindProperty("_coneHeight");
        _coneSegments = serializedObject.FindProperty("_coneSegments");
        _torusMajorRadius = serializedObject.FindProperty("_torusMajorRadius");
        _torusMinorRadius = serializedObject.FindProperty("_torusMinorRadius");
        _torusSegments = serializedObject.FindProperty("_torusSegments");
        _polyhedronSize = serializedObject.FindProperty("_polyhedronSize");

        // Modifier properties
        _useChamferVertices = serializedObject.FindProperty("_useChamferVertices");
        _chamferVertexDistance = serializedObject.FindProperty("_chamferVertexDistance");
        _useChamferEdges = serializedObject.FindProperty("_useChamferEdges");
        _chamferEdgeDistance = serializedObject.FindProperty("_chamferEdgeDistance");
        _useExtrude = serializedObject.FindProperty("_useExtrude");
        _extrudeDistance = serializedObject.FindProperty("_extrudeDistance");
        _useSplitFaces = serializedObject.FindProperty("_useSplitFaces");
        _splitPlaneNormal = serializedObject.FindProperty("_splitPlaneNormal");
        _splitPlanePoint = serializedObject.FindProperty("_splitPlanePoint");
        _useSkew = serializedObject.FindProperty("_useSkew");
        _skewAngle = serializedObject.FindProperty("_skewAngle");
        _skewDirection = serializedObject.FindProperty("_skewDirection");
        _useSmooth = serializedObject.FindProperty("_useSmooth");
        _smoothIterations = serializedObject.FindProperty("_smoothIterations");
        _useStretch = serializedObject.FindProperty("_useStretch");
        _stretchScale = serializedObject.FindProperty("_stretchScale");
        _useTwist = serializedObject.FindProperty("_useTwist");
        _twistAngle = serializedObject.FindProperty("_twistAngle");
        _twistAxis = serializedObject.FindProperty("_twistAxis");
        _useExpand = serializedObject.FindProperty("_useExpand");
        _expandDistance = serializedObject.FindProperty("_expandDistance");

        // Rendering properties
        _shadingMode = serializedObject.FindProperty("_shadingMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Base Mesh", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_generatorType);

        var type = (GeneratorSample.GeneratorType)_generatorType.enumValueIndex;

        switch (type)
        {
            case GeneratorSample.GeneratorType.Box: DrawBoxGUI(); break;
            case GeneratorSample.GeneratorType.Plane: DrawPlaneGUI(); break;
            case GeneratorSample.GeneratorType.Sphere: DrawSphereGUI(); break;
            case GeneratorSample.GeneratorType.Icosphere: DrawIcosphereGUI(); break;
            case GeneratorSample.GeneratorType.Cylinder: DrawCylinderGUI(); break;
            case GeneratorSample.GeneratorType.Cone: DrawConeGUI(); break;
            case GeneratorSample.GeneratorType.Torus: DrawTorusGUI(); break;
            case GeneratorSample.GeneratorType.Tetrahedron:
            case GeneratorSample.GeneratorType.Octahedron:
            case GeneratorSample.GeneratorType.Dodecahedron:
                DrawPolyhedronGUI();
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);

        DrawModifierGUI(_useChamferVertices, _chamferVertexDistance);
        DrawModifierGUI(_useChamferEdges, _chamferEdgeDistance);
        DrawModifierGUI(_useExtrude, _extrudeDistance);
        DrawModifierGUI(_useSplitFaces, _splitPlaneNormal, _splitPlanePoint);
        DrawModifierGUI(_useSkew, _skewAngle, _skewDirection);
        DrawModifierGUI(_useSmooth, _smoothIterations);
        DrawModifierGUI(_useStretch, _stretchScale);
        DrawModifierGUI(_useTwist, _twistAngle, _twistAxis);
        DrawModifierGUI(_useExpand, _expandDistance);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_shadingMode);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawBoxGUI()
    {
        EditorGUILayout.PropertyField(_boxSize, new GUIContent("Size"));
        EditorGUILayout.PropertyField(_boxSegments, new GUIContent("Segments"));
    }

    void DrawPlaneGUI()
    {
        EditorGUILayout.PropertyField(_planeSize, new GUIContent("Size"));
        EditorGUILayout.PropertyField(_planeSegments, new GUIContent("Segments"));
    }

    void DrawSphereGUI()
    {
        EditorGUILayout.PropertyField(_sphereRadius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(_sphereSegments, new GUIContent("Segments"));
    }

    void DrawIcosphereGUI()
    {
        EditorGUILayout.PropertyField(_icosphereRadius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(_subdivisions, new GUIContent("Subdivisions"));
    }

    void DrawCylinderGUI()
    {
        EditorGUILayout.PropertyField(_cylinderRadius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(_cylinderHeight, new GUIContent("Height"));
        EditorGUILayout.PropertyField(_cylinderSegments, new GUIContent("Segments"));
        EditorGUILayout.PropertyField(_cylinderCapped, new GUIContent("Capped"));
    }

    void DrawConeGUI()
    {
        EditorGUILayout.PropertyField(_coneRadius, new GUIContent("Radius"));
        EditorGUILayout.PropertyField(_coneHeight, new GUIContent("Height"));
        EditorGUILayout.PropertyField(_coneSegments, new GUIContent("Segments"));
    }

    void DrawTorusGUI()
    {
        EditorGUILayout.PropertyField(_torusMajorRadius, new GUIContent("Major Radius"));
        EditorGUILayout.PropertyField(_torusMinorRadius, new GUIContent("Minor Radius"));
        EditorGUILayout.PropertyField(_torusSegments, new GUIContent("Segments"));
    }

    void DrawPolyhedronGUI()
    {
        EditorGUILayout.PropertyField(_polyhedronSize, new GUIContent("Size"));
    }

    void DrawModifierGUI(SerializedProperty use, params SerializedProperty[] props)
    {
        EditorGUILayout.PropertyField(use);
        if (use.boolValue)
        {
            EditorGUI.indentLevel++;
            foreach (var p in props) EditorGUILayout.PropertyField(p);
            EditorGUI.indentLevel--;
        }
    }
}
