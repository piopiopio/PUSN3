using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WpfApp1
{
    class Cylinder
    {

        public static void DrawCylinder(Vector3d P0, Vector3d P1, Vector3d Color, double R, int divisions = 10)
        {
            Vector3d direction = (P1 - P0).Normalized();
            Vector3d perpendicular = new Vector3d(1, 1, 0);
            List<Vector3d> DrawPoints = new List<Vector3d>();
            List<Vector3d> Normals = new List<Vector3d>();
            //int divisions = 3;

            perpendicular.Z = -(perpendicular.X * direction.X + perpendicular.Y * direction.Y) / direction.Z;
            perpendicular = perpendicular.Normalized();
            if (double.IsNaN(perpendicular.Z) || double.IsInfinity(perpendicular.Z))
            {
                perpendicular = new Vector3d(1, 0, 1);
                perpendicular.Y = -(perpendicular.X * direction.X + perpendicular.Z * direction.Z) / direction.Y;
                perpendicular = perpendicular.Normalized();

                if (double.IsNaN(perpendicular.Y) || double.IsInfinity(perpendicular.Y))
                {
                    perpendicular = new Vector3d(0, 1, 1);
                    perpendicular.X = -(perpendicular.Y * direction.Y + perpendicular.Z * direction.Z) / direction.X;
                    perpendicular = perpendicular.Normalized();
                }

            }

            double alpha = 2 * Math.PI / (divisions);
            //double alpha =  Math.PI/2;
            Vector4d rotateQuaternion = new Vector4d(Math.Sin(alpha / 2) * direction.X, Math.Sin(alpha / 2) * direction.Y, Math.Sin(alpha / 2) * direction.Z, Math.Cos(alpha / 2)).Normalized();
             

            DrawPoints.Add(P0 + perpendicular * R);
            Normals.Add(DrawPoints.Last() - P0);
            DrawPoints.Add(P1 + perpendicular * R);
            Normals.Add(DrawPoints.Last() - P0);

            for (int i = 0; i < divisions; i++)
            {
                //var RotatedVector = quaternionMultiply(rotateQuaternion, AxisVectorToQuaternion(perpendicular));
                //RotatedVector = quaternionMultiply(RotatedVector, ConjugatedQuaternion(rotateQuaternion));
                //perpendicular = QuaternionToAxisVector(RotatedVector).Normalized();
                var RotatedVector = QuaternionHelpers.HamiltonProduct(rotateQuaternion, QuaternionHelpers.AxisVectorToQuaternion(perpendicular));
                RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(rotateQuaternion));
                perpendicular = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);
                DrawPoints.Add(P0 + perpendicular * R);
                Normals.Add(DrawPoints.Last() - P0);

                DrawPoints.Add(P1 + perpendicular * R);
                Normals.Add(DrawPoints.Last() - P0);
            }


            GL.Color3(1f, 0, 0);
            GL.Color3(Color.X, Color.Y, Color.Z);
            GL.Begin(BeginMode.TriangleStrip);

            foreach (var item in DrawPoints)
            {
                GL.Normal3(new Vector3d(item.X, item.Y, 0).Normalized());
                GL.Vertex3(item);
            }
            GL.End();
            GL.Flush();


            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(-direction.Normalized());
            GL.Vertex3(P0);
            
            for (int i = 0; i < DrawPoints.Count; i+=2)
            {
                GL.Normal3(-direction.Normalized());
                GL.Vertex3(DrawPoints[i]);
             
            }
            GL.End();
            GL.Flush();

            GL.Begin(BeginMode.TriangleFan);
            GL.Normal3(direction.Normalized());
            GL.Vertex3(P1);
         
            for (int i = 1; i < DrawPoints.Count; i += 2)
            {
                GL.Normal3(direction.Normalized());
                GL.Vertex3(DrawPoints[i]);
               
            }
            GL.End();
            GL.Flush();
        }
    }
}
