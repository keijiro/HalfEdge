using NUnit.Framework;
using HalfEdgeMesh;
using HalfEdgeMesh.Generators;
using Unity.Mathematics;
using UnityEngine;

public class WindingOrderTests
{
    [Test]
    public void Box_HasCorrectWindingOrder()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(1, 1, 1));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    [Test]
    public void Box_WithSubdivision_HasCorrectWindingOrder()
    {
        var generator = new Box(new float3(1f, 1f, 1f), new int3(2, 2, 2));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    [Test]
    public void Sphere_UV_HasCorrectWindingOrder()
    {
        var generator = new Sphere(1f, new int2(2, 2));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    

    [Test]
    public void Cylinder_HasCorrectWindingOrder()
    {
        var generator = new Cylinder(1f, 2f, new int2(8, 1));
        var meshData = generator.Generate();
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    [Test]
    public void SimpleFace_HasCorrectWindingOrder()
    {
        // Create a simple quad facing forward (towards +Z)
        var vertices = new float3[]
        {
            new float3(-1, -1, 0), // bottom-lef
            new float3( 1, -1, 0), // bottom-righ
            new float3( 1,  1, 0), // top-righ
            new float3(-1,  1, 0)  // top-lef
        };

        var faces = new int[][]
        {
            new int[] { 0, 1, 2, 3 } // CCW when viewed from +Z
        };

        var meshData = new HalfEdgeMesh.Mesh();
        meshData.InitializeFromIndexedFaces(vertices, faces);
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    [Test]
    public void CustomPyramid_HasCorrectWindingOrder()
    {
        // Test the improved CustomMeshSample pyramid
        var vertices = new float3[]
        {
            new float3(-1, 0, -1), // 0: bottom-left-back
            new float3( 1, 0, -1), // 1: bottom-right-back
            new float3( 1, 0,  1), // 2: bottom-right-fron
            new float3(-1, 0,  1), // 3: bottom-left-fron
            new float3( 0, 2,  0)  // 4: top center
        };

        var faces = new int[][]
        {
            new int[] { 0, 1, 2, 3 }, // base face
            new int[] { 0, 4, 1 },    // back face
            new int[] { 1, 4, 2 },    // right face
            new int[] { 2, 4, 3 },    // front face
            new int[] { 3, 4, 0 }     // left face
        };

        var meshData = new HalfEdgeMesh.Mesh();
        meshData.InitializeFromIndexedFaces(vertices, faces);
        var unityMesh = meshData.ToUnityMesh();

        AssertMeshHasCorrectWindingOrder(unityMesh);
    }

    void AssertMeshHasCorrectWindingOrder(UnityEngine.Mesh mesh)
    {
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        // Check each triangle has correct winding order (CCW when viewed from outside)
        for (int i = 0; i < triangles.Length; i += 3)
        {
            var v0 = vertices[triangles[i]];
            var v1 = vertices[triangles[i + 1]];
            var v2 = vertices[triangles[i + 2]];

            // Calculate triangle normal using cross produc
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var normal = Vector3.Cross(edge1, edge2);

            // For a box, we expect normals to point outward
            // Calculate triangle center
            var center = (v0 + v1 + v2) / 3f;

            // For a unit box, the normal should generally point away from origin
            // (except for faces very close to origin)
            var dotProduct = Vector3.Dot(normal.normalized, center.normalized);

            // If the triangle is facing inward (dot product negative), winding is wrong
            Assert.Greater(dotProduct, -0.1f,
                $"Triangle {i/3} appears to have incorrect winding order. " +
                $"Normal: {normal}, Center: {center}, Dot: {dotProduct}");
        }

        // Additional check: ensure we have a reasonable number of triangles
        Assert.Greater(triangles.Length, 0, "Mesh should have triangles");
        Assert.AreEqual(0, triangles.Length % 3, "Triangle count should be multiple of 3");
    }
}