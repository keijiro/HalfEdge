using UnityEngine;
using UnityEditor;
using HalfEdgeMesh2.Samples;

namespace HalfEdgeMesh2.Editor
{
    [CustomEditor(typeof(GeneratorSample))]
    public class GeneratorSampleEditor : UnityEditor.Editor
    {
        SerializedProperty generatorType;
        SerializedProperty normalMode;
        SerializedProperty animateSize;
        SerializedProperty boxSize;
        SerializedProperty boxSegments;
        SerializedProperty sphereRadius;
        SerializedProperty sphereSegments;

        // Modifier properties
        SerializedProperty applySmooth;
        SerializedProperty smoothingFactor;
        SerializedProperty smoothingIterations;

        void OnEnable()
        {
            generatorType = serializedObject.FindProperty("generatorType");
            normalMode = serializedObject.FindProperty("normalMode");
            animateSize = serializedObject.FindProperty("animateSize");
            boxSize = serializedObject.FindProperty("boxSize");
            boxSegments = serializedObject.FindProperty("boxSegments");
            sphereRadius = serializedObject.FindProperty("sphereRadius");
            sphereSegments = serializedObject.FindProperty("sphereSegments");

            // Find modifier properties
            applySmooth = serializedObject.FindProperty("applySmooth");
            smoothingFactor = serializedObject.FindProperty("smoothingFactor");
            smoothingIterations = serializedObject.FindProperty("smoothingIterations");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Generator Settings
            EditorGUILayout.LabelField("Generator Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(generatorType);
            EditorGUILayout.PropertyField(normalMode);
            EditorGUILayout.Space();

            // Animation Settings
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(animateSize);
            EditorGUILayout.Space();

            // Generator-specific settings
            var selectedType = (GeneratorSample.GeneratorType)generatorType.enumValueIndex;

            switch (selectedType)
            {
                case GeneratorSample.GeneratorType.Box:
                    EditorGUILayout.LabelField("Box Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(boxSize);
                    EditorGUILayout.PropertyField(boxSegments);
                    break;

                case GeneratorSample.GeneratorType.Sphere:
                    EditorGUILayout.LabelField("Sphere Settings", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(sphereRadius);
                    EditorGUILayout.PropertyField(sphereSegments);
                    break;
            }

            EditorGUILayout.Space();

            // Modifier Settings
            EditorGUILayout.LabelField("Modifiers", EditorStyles.boldLabel);
            
            // Smooth settings
            EditorGUILayout.PropertyField(applySmooth);
            if (applySmooth.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(smoothingFactor);
                EditorGUILayout.PropertyField(smoothingIterations);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}