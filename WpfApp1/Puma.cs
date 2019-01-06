using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WpfApp1
{
    public class Puma : Cursor
    {
        private const int DegreesOfFreedom = 6;
        // private Vector3d[] PumaPointsList = new Vector3d[DegreesOfFreedom + 1];
        private double[] PumaArmsLengths = new double[DegreesOfFreedom];
        private double[] PumaJointAngles = new double[DegreesOfFreedom];

        private Vector4d[] DHdescription = new Vector4d[DegreesOfFreedom];
        private Matrix4d[] PumaConfigurationDescription = new Matrix4d[DegreesOfFreedom];
        private Vector3d Origin = new Vector3d(0, 0, 0);
        private Cursor Wrist = new Cursor();

        public double Theta0
        {
            get { return PumaJointAngles[0]; }
            set
            {
                PumaJointAngles[0] = value;
                OnPropertyChanged(nameof(Theta0));
                CalculateKinematics();
                
                Refresh();

            }
        }

        public double Theta1
        {
            get { return PumaJointAngles[1]; }
            set
            {
                PumaJointAngles[1] = value;
                OnPropertyChanged(nameof(Theta1));
                CalculateKinematics();
                Refresh();
            }
        }

        public double Theta2
        {
            get { return PumaJointAngles[2]; }
            set
            {
                PumaJointAngles[2] = value;
                OnPropertyChanged(nameof(Theta2));
                CalculateKinematics();
                Refresh();
            }
        }
        public double Theta3
        {
            get { return PumaJointAngles[3]; }
            set
            {
                PumaJointAngles[3] = value;
                OnPropertyChanged(nameof(Theta3));
                CalculateKinematics();
                Refresh();
            }
        }

        public double Theta4
        {
            get { return PumaJointAngles[4]; }
            set
            {
                PumaJointAngles[4] = value;
                OnPropertyChanged(nameof(Theta4));
                CalculateKinematics();
                Refresh();
            }
        }


        public double Theta5
        {
            get { return PumaJointAngles[5]; }
            set
            {
                PumaJointAngles[5] = value;
                OnPropertyChanged(nameof(Theta5));
                CalculateKinematics();
                Refresh();
            }
        }

        public Puma(double[] _pumaArmsLengths, double[] _pumaJointAngles)
        {
            PumaArmsLengths = _pumaArmsLengths;
            PumaJointAngles = _pumaJointAngles;

            //PumaPointsList[0] = new Vector3d(0, 0, 0);
            //PumaPointsList[1] = new Vector3d(0, 1, 0);
            //PumaPointsList[2] = new Vector3d(1, 1, 0);
            //PumaPointsList[3] = new Vector3d(1, 2, 0);
            //PumaPointsList[4] = new Vector3d(2, 2, 1);
            //PumaPointsList[5] = new Vector3d(2, 2.1, 0);


        }

        Matrix4d Hi(Vector4d DH)
        {
            Matrix4d RotZTeta = new Matrix4d(
                Math.Cos(DH[0]), -Math.Sin(DH[0]), 0, 0,
                Math.Sin(DH[0]), Math.Cos(DH[0]), 0, 0,
                    0, 0, 1, 0,
                0, 0, 0, 1);

            Matrix4d TransZD = new Matrix4d(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, DH[1],
                0, 0, 0, 1
                );

            Matrix4d TransXA = new Matrix4d(
                1, 0, 0, DH[2],
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            Matrix4d RotXAlpa = new Matrix4d(
                1, 0, 0, 0,
                0, Math.Cos(DH[3]), -Math.Sin(DH[3]), 0,
                0, Math.Sin(DH[3]), Math.Cos(DH[3]), 0,
                0, 0, 0, 1);

            Matrix4d A = RotZTeta * TransZD * TransXA * RotXAlpa;
            return A;
        }


        void CalculateKinematics()
        {
            DHdescription[0] = new Vector4d(PumaJointAngles[0], PumaArmsLengths[0], 0, Math.PI / 2);
            DHdescription[1] = new Vector4d(PumaJointAngles[1] + Math.PI / 2, 0, PumaArmsLengths[1], 0);
            DHdescription[2] = new Vector4d(PumaJointAngles[2], 0, PumaArmsLengths[2], 0);
            DHdescription[3] = new Vector4d(PumaJointAngles[3], 0, 0, -Math.PI / 2);
            DHdescription[4] = new Vector4d(PumaJointAngles[4] + Math.PI / 2, 0, 0, Math.PI / 2);
            DHdescription[5] = new Vector4d(PumaJointAngles[5] - Math.PI / 2, 0, 0, 0);
            Matrix4d Tn = Matrix4d.Identity;
            for (int i = 0; i < DegreesOfFreedom; i++)
            {
                Tn = Tn * Hi(DHdescription[i]);
                PumaConfigurationDescription[i] = Tn;
            }

        }

        Vector3d GetPositionFromPumaConfigurationDescription(Matrix4d Tn)
        {
            return new Vector3d(Tn[0, 3], Tn[1, 3], Tn[2, 3]);
        }

        Matrix3d GetRotationMatrixFromPumaConfigurationSpace(Matrix4d Tn)
        {
            Matrix3d temp = new Matrix3d();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    temp[i, j] = Tn[i, j];
                }
            }

            return temp;
        }
        public void Draw()
        {
            CalculateKinematics();
            Cylinder.DrawCylinder(Origin, Origin + GetPositionFromPumaConfigurationDescription(PumaConfigurationDescription[0]), new Vector3d(0.0, 0.8, 0), 0.1);
            for (int i = 0; i < DegreesOfFreedom - 1; i++)
            {
                Cylinder.DrawCylinder(Origin + GetPositionFromPumaConfigurationDescription(PumaConfigurationDescription[i]), Origin + GetPositionFromPumaConfigurationDescription(PumaConfigurationDescription[i + 1]), new Vector3d(0.8, 0.8, 0.8), 0.1);
            }


            Wrist.Origin =
                Origin + GetPositionFromPumaConfigurationDescription(PumaConfigurationDescription[DegreesOfFreedom - 1]);

            Wrist.Draw(GetRotationMatrixFromPumaConfigurationSpace(PumaConfigurationDescription[PumaConfigurationDescription.Length- 1]));
            //Wrist.Draw(Matrix3d.Identity);
        }
    }
}
