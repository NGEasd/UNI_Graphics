using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaSzeminarium
{
    internal class DezsaModelDescriptor
    {
        public const float fenceWidth = 0.4f;
        public GL gl;
        public List<ModelObjectDescriptor> Fences;
        public List<Matrix4X4<float>> Transformations;

        public DezsaModelDescriptor(GL gl)
        {
            this.gl = gl;
            Fences = new List<ModelObjectDescriptor>();
            Transformations = new List<Matrix4X4<float>>();

            float rotationAngle = (float)Math.PI / 9;
            float distanceR = fenceWidth / (2 * (float)Math.Tan(Math.PI/18));

            for (int i = 0; i < 18; i++)
            {
                var translation = Matrix4X4.CreateTranslation(0, 0, distanceR);
                var rotation = Matrix4X4.CreateRotationY(rotationAngle * i);
                var transformation = translation * rotation;
                Transformations.Add(transformation);

                var fence = ModelObjectDescriptor.CreateCube(gl);
                Fences.Add(fence);
            }
        }

        public void Dispose()
        {
            foreach (var fence in Fences)
            {
                fence.Dispose();
            }
            Fences.Clear();
        }

    }

}
