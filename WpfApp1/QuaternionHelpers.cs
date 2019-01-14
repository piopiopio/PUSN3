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

            //(q1.W * wk.Z0 + q1.X0 * wk.Y0 −q1.Y0* wk.X0 + q1.Z0 * wk.W),
            //        (q1.W* wk.W − q1.X0* wk.X0 − q1.Y0* wk.Y0 −q1.Z0* wk.Z0)
            //            //);

            return temp;
        }

       public static Vector4d ConjugatedQuaternion(Vector4d q1)
       {
           return new Vector4d(-q1.X, -q1.Y, -q1.Z, q1.W);
       }
    }
}
