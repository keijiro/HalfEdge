using UnityEngine;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;

public class QuickShadingTest : MonoBehaviour
{
    void Start()
    {
        // Test both shading modes
        TestShadingModes();
    }

    void TestShadingModes()
    {
        Debug.Log("Testing shading modes...");

        var generator = new BoxGenerator(1f, 1f, 1f, 0);
        var meshData = generator.Generate();

        // Test smooth shading
        var smoothMesh = meshData.ToUnityMesh(MeshData.ShadingMode.Smooth);
        Debug.Log($"Smooth shading - Vertices: {smoothMesh.vertices.Length}, Triangles: {smoothMesh.triangles.Length}");

        // Test flat shading
        var flatMesh = meshData.ToUnityMesh(MeshData.ShadingMode.Flat);
        Debug.Log($"Flat shading - Vertices: {flatMesh.vertices.Length}, Triangles: {flatMesh.triangles.Length}, Normals: {flatMesh.normals.Length}");

        // Verify flat shading has more vertices due to duplication
        if (flatMesh.vertices.Length > smoothMesh.vertices.Length)
            Debug.Log("✓ Flat shading correctly duplicates vertices");
        else
            Debug.LogError("✗ Flat shading vertex count issue");

        // Verify normals are set for flat shading
        if (flatMesh.normals.Length == flatMesh.vertices.Length)
            Debug.Log("✓ Flat shading has correct normal count");
        else
            Debug.LogError("✗ Flat shading normal count issue");

        Debug.Log("Shading mode test completed!");
    }
}