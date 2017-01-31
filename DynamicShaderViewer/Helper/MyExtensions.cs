using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using DynamicShaderViewer.ShaderParser;
using SharpDX;

namespace DynamicShaderViewer.Helper
{
    public static class MyExtensions
    {
        public static float[] ToArray(this Vector3D v)
        {
            return new[] {v.X, v.Y, v.Z};
        }

        public static float[] ToArray(this Vector2D v)
        {
            return new[] {v.X, v.Y};
        }

        public static float[] ToArray(this Color4D c)
        {
            return new[] {c.R, c.G, c.B, c.A};
        }

        public static Vector3 ToVector3(this Float3 f)
        {
            return new Vector3(f.X, f.Y, f.Z);
        }

        public static Vector4 ToVector4(this Float3 f)
        {
            return new Vector4(f.X, f.Y, f.Z, 1f);
            //overwrites next variable???
        }
    }
}
