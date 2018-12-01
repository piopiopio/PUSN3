using System;
using System.Linq;
using ModelowanieGeometryczne.ViewModel;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WpfApp1
{
    public class Cursor : ViewModelBase
    {


        public Vector3d Origin = new Vector3d(0, 0, 0);
        Vector3d OriginOffset;
        private Vector3d[] Axis = new Vector3d[3] {new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)};

        private int axisLength = 100;
        private Vector3d[] AxisColors = new Vector3d[3] {Vector3d.UnitX, Vector3d.UnitY, Vector3d.UnitZ};
        //Euler ZYX
        public Vector3d EulerAngles = new Vector3d();

        public double X
        {
            get => Origin.X;
            set
            {
                Origin.X = value;
                OnPropertyChanged(nameof(X));
            }
        }

        public double Y
        {
            get => Origin.Y;
            set
            {
                Origin.Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        public double Z
        {
            get => Origin.Z;
            set
            {
                Origin.Z = value;
                OnPropertyChanged(nameof(Z));
            }
        }


        public double EulerFi
        {
            get => EulerAngles.X * 180 / Math.PI;
            set
            {
                EulerAngles.X = (Math.PI / 180) * value;
                OnPropertyChanged(nameof(EulerFi));
                _quaternion = ConvertEulerToQuaternion(EulerAngles);
                RefreshQuaternion();
            }
        }

        public double EulerTeta
        {
            get => EulerAngles.Y * 180 / Math.PI;
            set
            {
                EulerAngles.Y = (Math.PI / 180) * value;
                OnPropertyChanged(nameof(EulerTeta));
                _quaternion = ConvertEulerToQuaternion(EulerAngles);
                RefreshQuaternion();
            }
        }


        public double EulerPsi
        {
            get => EulerAngles.Z * 180 / Math.PI;
            set
            {
                EulerAngles.Z = (Math.PI / 180) * value;
                OnPropertyChanged(nameof(EulerPsi));
                _quaternion = ConvertEulerToQuaternion(EulerAngles);
                RefreshQuaternion();
            }
        }

        private Vector4d _quaternion = new Vector4d();

        public double QuaternionX
        {
            get => _quaternion.X;
            set
            {
                _quaternion.X = value;
                OnPropertyChanged(nameof(QuaternionX));
                ConvertQuaternionToEulerAngles(_quaternion);
                RefreshEulerAngles();
            }
        }

        public double QuaternionY
        {
            get => _quaternion.Y;
            set
            {
                _quaternion.Y = value;
                OnPropertyChanged(nameof(QuaternionY));
               EulerAngles= ConvertQuaternionToEulerAngles(_quaternion);
                RefreshEulerAngles();
            }
        }

        public double QuaternionZ
        {
            get => _quaternion.Z;
            set
            {
                _quaternion.Z = value;
                OnPropertyChanged(nameof(QuaternionZ));
                EulerAngles=ConvertQuaternionToEulerAngles(_quaternion);
                RefreshEulerAngles();
            }
        }

        public double QuaternionW
        {
            get => _quaternion.W;
            set
            {
                _quaternion.W = value;
                OnPropertyChanged(nameof(QuaternionW));
                EulerAngles= ConvertQuaternionToEulerAngles(_quaternion);
                RefreshEulerAngles();
            }
        }

        public void RefreshEulerAngles()
        {
            OnPropertyChanged(nameof(EulerFi));
            OnPropertyChanged(nameof(EulerTeta));
            OnPropertyChanged(nameof(EulerPsi));
        }

    public void RefreshQuaternion()
        {
            OnPropertyChanged(nameof(QuaternionX));
            OnPropertyChanged(nameof(QuaternionY));
            OnPropertyChanged(nameof(QuaternionZ));
            OnPropertyChanged(nameof(QuaternionW));
        }
        public Cursor(Vector3d originOffset=new Vector3d())
        {
            OriginOffset = originOffset;
        }


        public void Draw()
        {
            GL.LineWidth(10);
            GL.Begin(BeginMode.Lines);

            for (int i = 0; i < Axis.Count(); i++)
            {
                GL.Color3(AxisColors[i].X, AxisColors[i].Y, AxisColors[i].Z);
                GL.Vertex3(Origin + OriginOffset);
                GL.Vertex3(Origin + OriginOffset + RotateXMatrix(EulerAngles.X).Multiply(Axis[i])* axisLength);
            }
            GL.End();
            GL.Flush();
        }

        public Vector3d ConvertQuaternionToEulerAngles(Vector4d q)
        { Vector3d temp=new Vector3d();
            double sinr_cosp = +2.0 * (q.W* q.X + q.Y * q.Z);
            double cosr_cosp = +1.0 - 2.0 * (q.X * q.X + q.Y * q.Y);
            temp.X = Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch (y-axis rotation)
            double sinp = +2.0 * (q.W * q.W - q.W * q.X);
            if (Math.Abs(sinp) >= 1)
                temp.Y = Math.PI / 2*Math.Sign(sinp); // use 90 degrees if out of range
            else
                temp.Y = Math.Asin(sinp);

            // yaw (z-axis rotation)
            double siny_cosp = +2.0 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = +1.0 - 2.0 * (q.Y * q.Y + q.Z * q.Z);
            temp.Z = Math.Atan2(siny_cosp, cosy_cosp);
            return temp;
        }
        public Vector4d ConvertEulerToQuaternion(Vector3d eulerAngles)
        {
          return  ConvertEulerToQuaternion(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        } 
        
        public Vector4d ConvertEulerToQuaternion(double fi, double teta, double psi)
        {
            double cy = Math.Cos(fi * 0.5);
            double sy = Math.Sin(fi * 0.5);
            double cp = Math.Cos(teta * 0.5);
            double sp = Math.Sin(teta * 0.5);
            double cr = Math.Cos(psi * 0.5);
            double sr = Math.Sin(psi * 0.5);

            Vector3d temp=new Vector3d();
           
            temp.X = cy * cp * sr - sy * sp * cr;
            temp.Y = sy * cp * sr + cy * sp * cr;
            temp.Z = sy * cp * cr - cy * sp * sr;
           // temp = temp.Normalized();
            Vector4d q = new Vector4d();
            q.X = temp.X;
            q.Y = temp.Y;
            q.Z = temp.Z;
            q.W = cy * cp * cr + sy * sp * sr;

            return q;
        }

        public static Matrix3d RotateXMatrix(double alphaX)
        {
            Matrix3d result = new Matrix3d(1, 0, 0, 
                0, Math.Cos(alphaX), Math.Sin(-alphaX),
                0, Math.Sin(alphaX), Math.Cos(alphaX));
            //result.Transpose();
            return result;
        }

        public static Matrix3d RotateYMatrix(double alphaY)
        {
            Matrix3d result = new Matrix3d(Math.Cos(alphaY), 0, Math.Sin(alphaY),
                0, 1, 0,
                -Math.Sin(alphaY), 0, Math.Cos(alphaY));
            //result.Transpose();
            return result;

        }

        public static Matrix3d RotateZMatrix(double alphaZ)
        {
            Matrix3d result = new Matrix3d(Math.Cos(alphaZ), -Math.Sin(alphaZ), 0,
                Math.Sin(alphaZ), Math.Cos(alphaZ), 0,
                0, 0, 1);
            // result.Transpose();
            return result;
        }
    }
}

