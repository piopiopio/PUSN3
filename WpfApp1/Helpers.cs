using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WpfApp1
{
   public static class Helpers
    {
        public static Vector4d Multiply(this Matrix4d m, Vector4d v)
        {
            return new Vector4d(v.X * m.M11 + v.Y * m.M12 + v.Z * m.M13 + v.W * m.M14,
                v.X * m.M21 + v.Y * m.M22 + v.Z * m.M23 + v.W * m.M24,
                v.X * m.M31 + v.Y * m.M32 + v.Z * m.M33 + v.W * m.M34,
                v.X * m.M41 + v.Y * m.M42 + v.Z * m.M43 + v.W * m.M44);
        }

        public static Vector3d Multiply(this Matrix3d m, Vector3d v)
        {
            return new Vector3d(v.X * m.M11 + v.Y * m.M12 + v.Z * m.M13, 
                v.X * m.M21 + v.Y * m.M22 + v.Z * m.M23,
                v.X * m.M31 + v.Y * m.M32 + v.Z * m.M33);
        }
    }
}
