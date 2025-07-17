using System.Collections.Generic;
using Unity.Mathematics;

namespace HalfEdgeMesh.Generators
{
    public class Extrusion
    {
        List<float3> profile;
        float height;

        public Extrusion(List<float3> profile, float height)
        {
            this.profile = profile;
            this.height = height;
        }

        public Mesh Generate()
        {
            var meshData = new Mesh();
            var vertices = new List<float3>();
            var faces = new List<int[]>();

            var profileCount = profile.Count;

            // Generate vertices for bottom and top
            for (int i = 0; i < profileCount; i++)
            {
                var point = profile[i];
                vertices.Add(new float3(point.x, -height * 0.5f, point.z)); // Bottom
                vertices.Add(new float3(point.x, height * 0.5f, point.z));  // Top
            }

            // Generate side faces
            for (int i = 0; i < profileCount; i++)
            {
                var next = (i + 1) % profileCount;
                
                var bottomCurrent = i * 2;
                var topCurrent = i * 2 + 1;
                var bottomNext = next * 2;
                var topNext = next * 2 + 1;

                // Create quad face for each side
                faces.Add(new int[] { bottomCurrent, topCurrent, topNext, bottomNext });
            }

            // Generate bottom face (if profile forms a closed shape)
            if (profileCount > 2)
            {
                var bottomFace = new int[profileCount];
                for (int i = 0; i < profileCount; i++)
                    bottomFace[i] = i * 2; // Bottom vertices
                faces.Add(bottomFace);

                // Generate top face
                var topFace = new int[profileCount];
                for (int i = 0; i < profileCount; i++)
                    topFace[profileCount - 1 - i] = i * 2 + 1; // Top vertices (reversed)
                faces.Add(topFace);
            }

            meshData.InitializeFromIndexedFaces(vertices.ToArray(), faces.ToArray());
            return meshData;
        }
    }
}