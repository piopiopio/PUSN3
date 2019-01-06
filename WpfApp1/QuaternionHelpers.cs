using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace WpfApp1
{
    public static class QuaternionHelpers
    {
        public static Vector4d HamiltonProduct(Vector4d q1, Vector4d q2)
        {
            double w = q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z;
            double x = q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y;
            double y = q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X;
            double z = q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W;
            Vector4d temp = new Vector4d(x, y, z, w);
            // temp = temp.Normalized();
            return temp;
        }

        public static Vector4d AxisVectorToQuaternion(Vector3d v)
        {
            return new Vector4d(v.X, v.Y, v.Z, 0);
        }

        public static Vector3d QuaternionToAxisVector(Vector4d q)
        {
            return new Vector3d(q.X, q.Y, q.Z);
        }


       public static Vector4d quaternionMultiply(Vector4d q1, Vector4d q2)
        {
            Vector4d temp = new Vector4d(
                q1.W * q2.X + q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y,

                q1.W * q2.Y - q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X,

                (q1.W * q2.Z + q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W),
                (q1.W * q2.W - q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z)
            );

            //(q1.W * wk.Z + q1.X * wk.Y −q1.Y* wk.X + q1.Z * wk.W),
            //        (q1.W* wk.W − q1.X* wk.X − q1.Y* wk.Y −q1.Z* wk.Z)
            //            //);

            return temp;
        }

       public static Vector4d ConjugatedQuaternion(Vector4d q1)
       {
           return new Vector4d(-q1.X, -q1.Y, -q1.Z, q1.W);
       }
    }
}
