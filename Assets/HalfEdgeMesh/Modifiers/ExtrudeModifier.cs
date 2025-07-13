using Unity.Mathematics;
using System.Collections.Generic;

namespace HalfEdgeMesh.Modifiers
{
    public class ExtrudeModifier
    {
        float distance;
        List<Face> facesToExtrude;
        bool useNormalDirection;
        float3 customDirection;

        public ExtrudeModifier(float distance, bool useNormalDirection = true)
        {
            this.distance = distance;
            this.useNormalDirection = useNormalDirection;
            this.customDirection = new float3(0, 1, 0);
            this.facesToExtrude = new List<Face>();
        }

        public ExtrudeModifier(float distance, float3 direction)
        {
            this.distance = distance;
            this.useNormalDirection = false;
            this.customDirection = math.normalize(direction);
            this.facesToExtrude = new List<Face>();
        }

        public void AddFace(Face face)
        {
            if (!facesToExtrude.Contains(face))
                facesToExtrude.Add(face);
        }

        public void AddFaces(IEnumerable<Face> faces)
        {
            foreach (var face in faces)
                AddFace(face);
        }

        public void Apply(MeshData mesh)
        {
            if (facesToExtrude.Count == 0)
                facesToExtrude.AddRange(mesh.Faces);

            foreach (var face in facesToExtrude.ToArray())
            {
                var direction = useNormalDirection ? face.Normal : customDirection;
                mesh.ExtrudeFace(face, direction, distance);
            }

            facesToExtrude.Clear();
        }
    }
}