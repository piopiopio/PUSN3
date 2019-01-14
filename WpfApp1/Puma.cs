using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;

namespace WpfApp1
{
    public class Puma : ViewModelBase
    {
        private Vector4d StartQuaternion;
        private Vector4d EndQuaternion;

        public Vector3d _X5 = new Vector3d(1, 0, 0);
        public Vector3d _Z5 = new Vector3d(0, 0, 1);
        public Vector3d _P5_0 = new Vector3d(2, 0, 0);

        public Vector3d _P5_1 = new Vector3d(2, 0, 0);
        private const double jointIlustrationRadius = 0.15;
        private const double jointIlustrationHeight = 0.4;

        private const int DegreesOfFreedom = 6;
        private Vector3d[] PumaPointsList = new Vector3d[DegreesOfFreedom];
        private double[] PumaArmsLengths = new double[DegreesOfFreedom];
        private double[] PumaJointAngles = new double[DegreesOfFreedom - 1];


        private double[] PumaArmsLengthsLast = new double[DegreesOfFreedom];
        private double[] PumaJointAnglesLast = new double[DegreesOfFreedom - 1];

        private double[] PumaArmsLengthsFirst = new double[DegreesOfFreedom];
        private double[] PumaJointAnglesFirst = new double[DegreesOfFreedom - 1];

        private Vector4d[] DHdescription = new Vector4d[DegreesOfFreedom];
        private Matrix4d[] PumaConfigurationDescription = new Matrix4d[DegreesOfFreedom];
        private Vector3d Origin;
        private Vector3d Color;
        private Cursor Wrist = new Cursor();
        private List<double> Raport;

        public Puma(double[] _pumaArmsLengths, double[] _pumaJointAngles, Vector3d origin, Vector3d color)
        {
            PumaArmsLengths = _pumaArmsLengths;
            PumaJointAngles = _pumaJointAngles;



            CalculateInverseKinematics(_X5, _P5_0, _Z5);
            Origin = origin;
            Color = color;
            //  TestInverseKinematic();
            // TestPreliminaryInverseKinematic();
        }

        //public Puma(double[] _pumaArmsLengths, double[] _pumaJointAngles, Vector3d x5, Vector3d p5)
        //{
        //    PumaArmsLengths = _pumaArmsLengths;
        //    PumaJointAngles = _pumaJointAngles;

        //    CalculateInverseKinematics(x5, p5);
        //    SetUpEffector(p5, p5);
        //    //  TestInverseKinematic();
        //    // TestPreliminaryInverseKinematic();
        //}


        public void SetUpEffector(Vector4d startQuaternion, Vector3d start, Vector4d endQuaternion, Vector3d end)
        {
            p3Flag = null;
            _P5_1 = end;
            _P5_0 = start;
            StartQuaternion = startQuaternion;
            EndQuaternion = endQuaternion;
            var x = new Vector3d(1, 0, 0);
            var z = new Vector3d(0, 0, 1);

            var RotatedVector = QuaternionHelpers.HamiltonProduct(endQuaternion, QuaternionHelpers.AxisVectorToQuaternion(x));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(endQuaternion));
            _X5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            RotatedVector = QuaternionHelpers.HamiltonProduct(endQuaternion, QuaternionHelpers.AxisVectorToQuaternion(z));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(endQuaternion));
            _Z5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            CalculateInverseKinematics(_X5, end, _Z5);
            // CalculateKinematics();

            PumaArmsLengthsLast[1] = PumaArmsLengths[1];
            for (int i = 0; i < 5; i++)
            {
                PumaJointAnglesLast[i] = PumaJointAngles[i];
            }

            // x = new Vector3d(1, 0, 0);

            RotatedVector = QuaternionHelpers.HamiltonProduct(startQuaternion, QuaternionHelpers.AxisVectorToQuaternion(x));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(startQuaternion));
            _X5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            RotatedVector = QuaternionHelpers.HamiltonProduct(startQuaternion, QuaternionHelpers.AxisVectorToQuaternion(z));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(startQuaternion));
            _Z5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            CalculateInverseKinematics(_X5, start, _Z5);
            PumaArmsLengthsFirst[1] = PumaArmsLengths[1];
            for (int i = 0; i < 5; i++)
            {

                PumaJointAnglesFirst[i] = PumaJointAngles[i];
            }

            //CalculateKinematics();

        }


        // public List<double> animationRaport = new List<double>();

        public void PumaNextFrameMoveInternalInterpolation(double t)
        {
            PumaArmsLengths[1] = (1 - t) * PumaArmsLengthsFirst[1] + t * PumaArmsLengthsLast[1];

            for (int i = 0; i < 5; i++)
            {
                PumaJointAngles[i] = (1 - t) * PumaJointAnglesFirst[i] + t * PumaJointAnglesLast[i];

            }
            // CalculateKinematics();
            // animationRaport.Add(PumaArmsLengths[1]);
        }
        public List<Vector3d> animationRaport = new List<Vector3d>();
        //public double[] backup=new double[6];
        //public double[] delta=new double[6];
        public void PumaNextFrameMoveExternalInterpolation(double t)
        {
            Vector4d tempQuaternion;
            var tempP5 = (1 - t) * _P5_0 + t * _P5_1;


            var alpha = Math.Acos(StartQuaternion.X * EndQuaternion.X +
                                  StartQuaternion.Y * EndQuaternion.Y +
                                  StartQuaternion.Z * EndQuaternion.Z +
                                  StartQuaternion.W * EndQuaternion.W);

            if (Math.Abs(alpha) < 0.001)
            {
                tempQuaternion = StartQuaternion;
            }
            else
            {
                tempQuaternion = StartQuaternion * Math.Sin((1 - t) * alpha) / Math.Sin(alpha) + EndQuaternion * Math.Sin(t * alpha) / Math.Sin(alpha);
            }

            var x = new Vector3d(1, 0, 0);
            var z = new Vector3d(0, 0, 1);

            var RotatedVector = QuaternionHelpers.HamiltonProduct(tempQuaternion, QuaternionHelpers.AxisVectorToQuaternion(x));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(tempQuaternion));
            var temp_X5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            RotatedVector = QuaternionHelpers.HamiltonProduct(tempQuaternion, QuaternionHelpers.AxisVectorToQuaternion(z));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(tempQuaternion));
            var temp_Z5 = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);


            CalculateInverseKinematics(temp_X5, tempP5, temp_Z5);

            CalculateKinematics();
            if ((PumaPointsList[4] - tempP5).Length > 0.1)
            {
                //for (int i = 0; i < 5; i++)
                //{
                //    if (backup[0] != null)
                //    {
                //        backup[i] += delta[i];
                //        PumaJointAngles[i] = backup[i];
                //    }
                //    PumaArmsLengths[1] = backup[5];
                //    //backup[5] += delta[5];
                //}
            }
            else
            {
                //for (int i = 0; i < 5; i++)
                //{
                //    delta[i] = PumaJointAngles[i] - backup[i];
                //    backup[i] = PumaJointAngles[i];
                //}
                ////delta[5] = PumaJointAngles[5] - backup[5];
                //backup[5] = PumaArmsLengths[1];

            }
        }

        public void TestPreliminaryInverseKinematic()
        {



            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(1, 0, 0), new Vector3d(1, 0, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(0, 1, 0), new Vector3d(0, 1, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(0, -1, 0), new Vector3d(0, -1, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(-1, 0, 0), new Vector3d(2, 2, 2), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0, 0), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(1, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(0, 1, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(0, -1, 0), new Vector3d(0, -1, 0), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0, 1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(1, 0, 0), new Vector3d(1, 0, 1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(0, 1, 0), new Vector3d(0, 1, 1), new Vector3d(0, 0, 1));

            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0.1, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0, 1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(1, 0, 0), new Vector3d(1, 0.1, -1), new Vector3d(0, 0, 1));
            unitTest(new Vector3d(-1, 0, 0), new Vector3d(-1, 0, 1.1), new Vector3d(0, 0, 1));



        }

        void unitTest(Vector3d test_X5, Vector3d test_P5, Vector3d test_Z5)
        {
            double epsilon = 0.01;
            CalculateInverseKinematics(test_X5, test_P5, test_Z5);
            CalculateKinematics();
            var temp = (PumaPointsList[4] - test_P5).Length;
            if (temp > epsilon) MessageBox.Show("pos x: " + test_P5.X + " pos y: " + test_P5.Y + " pos z: " + test_P5.Z + " distance " + temp +
                                                "\n ori x" + test_X5.X + " ori y" + test_X5.Y + " ori z" + test_X5.Z);
        }

        public void TestInverseKinematic()
        {
            double epsilon = 0.01;
            Raport = new List<double>();
            Vector3d test_P5 = new Vector3d();
            Vector3d test_X5 = new Vector3d(-1, 0, 0);
            Vector3d test_Z5 = new Vector3d(-1, 0, 0);

            Vector3d startPoint = new Vector3d();
            double testRange = 5;
            int testSteps = 50;

            //TODO: test w obie strony
            for (int i = -testSteps; i <= testSteps; i++)
            {
                //TODO: test w obie strony
                for (int j = -testSteps; j <= testSteps; j++)
                {
                    //TODO: test w obie strony
                    for (int k = -testSteps; k <= testSteps; k++)
                    {
                        test_P5.X = startPoint.X + i * testRange / testSteps;
                        test_P5.Y = startPoint.Y + j * testRange / testSteps;
                        test_P5.Z = startPoint.Z + k * testRange / testSteps;
                        CalculateInverseKinematics(test_X5, test_P5, test_Z5);
                        CalculateKinematics();
                        var temp = (PumaPointsList[4] - test_P5).Length;
                        Raport.Add(temp);

                        if (double.IsNaN((PumaPointsList[4] - test_P5).Length) ||
                            double.IsInfinity((PumaPointsList[4] - test_P5).Length) || temp > epsilon)
                        {
                            MessageBox.Show("x: " + test_P5.X + " y: " + test_P5.Y + " z: " + test_P5.Z + " distance " +
                                            temp);
                            //return;
                        }
                    }
                }
            }
        }

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


        public double PrysmaticQ
        {
            get { return PumaArmsLengths[1]; }
            set
            {
                PumaArmsLengths[1] = value;
                OnPropertyChanged(nameof(PrysmaticQ));
                CalculateKinematics();
                Refresh();

            }
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

        Vector3d GetPositionFromConfiguration(Matrix4d temp)
        {
            return new Vector3d(temp[0, 3], temp[1, 3], temp[2, 3]);
        }

        void CalculateKinematics()
        {
            Matrix4d[] Transform = new Matrix4d[DegreesOfFreedom];
            Transform[0] = RotateZMatrix(PumaJointAngles[0]) * TranslateZ(PumaArmsLengths[0]);
            Transform[1] = Transform[0] * RotateYMatrix(PumaJointAngles[1]) * TranslateX(PumaArmsLengths[1]);
            Transform[2] = Transform[1] * RotateYMatrix(PumaJointAngles[2]) * TranslateZ(-PumaArmsLengths[2]);
            Transform[3] = Transform[2] * RotateZMatrix(PumaJointAngles[3]) * TranslateX(PumaArmsLengths[3]);
            Transform[4] = Transform[3] * RotateXMatrix(PumaJointAngles[4]);
            Transform[5] = Transform[4] * TranslateZ(0.5);


            for (int i = 0; i < Transform.Length; i++)
            {
                PumaPointsList[i] = GetPositionFromConfiguration(Transform[i]);
            }



        }

        private bool? p3Flag;
        public void CalculateInverseKinematics(Vector3d X5, Vector3d p5, Vector3d Z5)
        {
            int errorCode = 0;
            #region MyRegion
            //_X5 = X5;
            // _P5_0 = p5;
            // var p0=new Vector3d(0,0,0);
            // var p2=new Vector3d(0,0,PumaArmsLengths[0]);
            // var p4 = p5 - PumaArmsLengths[3] * X5;
            // var n024 = Vector3d.Cross((p4 - p2), (p2 - p0)).Normalized();
            // var z4 = Vector3d.Cross(n024, X5);
            // //TODO: In both cases the linear set of equations with additional length condition (18) or normalization
            //// (19) needs to be solved. If the vector n024 is parallel to the vector x5(both planes are parallel)
            // //there exists indefinite number of solutions and co-ordinates of p3 may be identified by the
            // //    minimal value of parameter q2(the length of the rod p2p3). 
            // Vector3d p3;
            // if (((p4 + PumaArmsLengths[2] * z4) - p2).Length < ((p4 - PumaArmsLengths[2] * z4) - p2).Length)
            // {
            //     p3 = ((p4 + PumaArmsLengths[2] * z4) - p2);
            // }
            // else
            // {
            //     p3 = ((p4 - PumaArmsLengths[2] * z4) - p2);
            // }
            // PumaJointAngles[0] = Math.Atan2(p4.Y0, p4.X0);
            // PumaJointAngles[1] = Angle(p2-p0, p3-p2)-Math.PI;
            // PumaArmsLengths[1] = (p3 - p2).Length;
            // PumaJointAngles[2] = Angle(p3-p2, p4-p3)-Math.PI/2;
            // PumaJointAngles[3] = Angle(n024, X5) +Math.PI/2;
            // //PumaJointAngles[4] = Angle(p3-p4,z5); 
            #endregion

            var p0 = new Vector3d(0, 0, 0);


            //_X5 = X5;
            //_P5_0 = p5;
            //_Z5 = Z5;


            // var p3 = p5;
            var p2 = new Vector3d(0, 0, PumaArmsLengths[0]);
            var p4 = p5 - PumaArmsLengths[3] * X5;
            var n024 = Vector3d.Cross((p4 - p0), (p2 - p0)).Normalized();

            if (double.IsNaN(n024.X) && double.IsNaN(n024.Y) && double.IsNaN(n024.Z))
            {
                n024 = Vector3d.Cross((p5 - p4), (p2 - p0)).Normalized();
                errorCode = 1;
            }

            Vector3d z4;
            //  if (n024.Normalized() != X5.Normalized())
            //  {
            z4 = Vector3d.Cross(n024, X5).Normalized();

            if (double.IsNaN(z4.X) && double.IsNaN(z4.Y) && double.IsNaN(z4.Z))
            {
                p4 = p5 - PumaArmsLengths[3] * X5 + new Vector3d(0.00012, 0.00023, 0.00031);
                n024 = Vector3d.Cross((p4 - p0), (p2 - p0)).Normalized();
                z4 = Vector3d.Cross(n024, X5).Normalized();
            }
            //}
            //else
            //{
            //    z4 = -(p2 - p0).Normalized();
            //}



            Vector3d p3;
            if (p3Flag == true)
            {
                p3 = (p4 + PumaArmsLengths[2] * z4);
            }
            else if (p3Flag == false)
            {
                p3 = (p4 - PumaArmsLengths[2] * z4);
            }
            else 
            {
                if (((p4 + PumaArmsLengths[2] * z4) - p2).Length < ((p4 - PumaArmsLengths[2] * z4) - p2).Length)
                {
                    p3 = (p4 + PumaArmsLengths[2] * z4);
                    p3Flag = true;
                }
                else
                {
                    p3 = (p4 - PumaArmsLengths[2] * z4);
                    p3Flag = true;
                }
            }


            PumaJointAngles[0] = Math.Atan2(p3.Y, p3.X);
            PumaJointAngles[1] = Angle(p2 - p0, p3 - p2) - Math.PI / 2;


            PumaArmsLengths[1] = (p3 - p2).Length;
            //PumaJointAngles[2] = Angle(p3 - p2, p4 - p3) - Math.PI / 2;
            PumaJointAngles[2] = Angle(p3 - p2, p4 - p3, -n024) - Math.PI / 2;

            if (n024.Normalized() != X5.Normalized() && n024.Normalized() != -X5.Normalized())
            {

                PumaJointAngles[3] = Angle(n024, X5, (p3 - p4).Normalized()) - Math.PI / 2;
            }
            else if (n024.Normalized() == X5.Normalized())
            {
                PumaJointAngles[3] = -Math.PI / 2;
            }
            else if (n024.Normalized() == -X5.Normalized())
            {
                PumaJointAngles[3] = Math.PI / 2;
            }
            else
            {
                MessageBox.Show("Error");
            }



            //PumaJointAngles[3] = PumaJointAngles[3] - PumaJointAngles[0];
            //PumaJointAngles[3] = -Angle(n024, X5, (p3-p4).Normalized());
            //  PumaJointAngles[4] = Angle(p3 - p4, z5);

            if (errorCode == 1)
            {
                var temp = p5 - p4;

                PumaJointAngles[3] = Math.Atan2(temp.Y, temp.X);

                //  if ((p2-p0).Normalized() == p3.Normalized() )
                //  {
                //PumaJointAngles[2] = 0;
                if (p4.Normalized() == p2.Normalized())
                {
                    //PumaJointAngles[3] = Math.Atan2(temp.Z0, temp.Y0);
                    //PumaJointAngles[3] = Angle(p5 - p4, new Vector3d(-1, 0, 0), -z4);
                    PumaJointAngles[3] = Math.Atan2(temp.Y, -temp.X);
                }

                // }

                if (p2 == p3)
                {
                    PumaJointAngles[1] = Angle(p2 - p0, p5 - p4);
                }
            }


            PumaJointAngles[4] = Angle(p3 - p4, Z5, X5);


        }


        //void CalculateKinematicsDH()
        //{
        //    DHdescription[0] = new Vector4d(PumaJointAngles[0], PumaArmsLengths[0], 0, -Math.PI / 2);
        //    DHdescription[1] = new Vector4d(PumaJointAngles[1] - Math.PI / 2, 0, PumaArmsLengths[1], 0);
        //    DHdescription[2] = new Vector4d(PumaJointAngles[2], 0, PumaArmsLengths[2], 0);
        //    DHdescription[3] = new Vector4d(Math.PI / 2, 0, 0, Math.PI / 2);
        //    DHdescription[4] = new Vector4d(PumaJointAngles[3], 0, PumaArmsLengths[3], +Math.PI / 2);
        //    DHdescription[5] = new Vector4d(Math.PI / 2, 0, 0, Math.PI / 2);
        //    DHdescription[6] = new Vector4d(PumaJointAngles[4], 0, 0, 0);
        //    //  DHdescription[5] = new Vector4d(PumaJointAngles[5] - Math.PI / 2, 0, 0, 0);
        //    Matrix4d Tn = Matrix4d.Identity;
        //    for (int i = 0; i < DegreesOfFreedom; i++)
        //    {
        //        Tn = Tn * Hi(DHdescription[i]);
        //        PumaConfigurationDescription[i] = Tn;
        //    }

        //}

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

            // GL.Color3(0,254,0);
            GL.PointSize(20);
            GL.Begin(BeginMode.Points);
            GL.Color3(System.Drawing.Color.Coral);
            GL.Vertex3(_P5_0 + Origin);
            GL.Color3(System.Drawing.Color.AliceBlue);
            GL.Vertex3(_P5_1 + Origin);
            GL.End();

            GL.PointSize(20);
            GL.Begin(BeginMode.Points);
            GL.Color3(System.Drawing.Color.DarkOrchid);
            foreach (var item in animationRaport)
            {
                GL.Vertex3(item + Origin);
            }
            GL.End();

            Cylinder.DrawCylinder(Origin, Origin + PumaPointsList[0], Color, 0.1);

            for (int i = 0; i < DegreesOfFreedom - 1; i++)
            {
                Cylinder.DrawCylinder(Origin + PumaPointsList[i], Origin + PumaPointsList[i + 1], Color, 0.1);
            }



            Cylinder.DrawCylinder(Origin + PumaPointsList[3] + jointIlustrationHeight * (PumaPointsList[3] - PumaPointsList[2]).Normalized() / 2,
                                    Origin + PumaPointsList[3] - jointIlustrationHeight * (PumaPointsList[3] - PumaPointsList[2]).Normalized() / 2,
                new Vector3d(0, 1, 1), jointIlustrationRadius);


            for (int i = 0; i < 2; i++)
            {
                Cylinder.DrawCylinder(Origin + PumaPointsList[i] + Vector3d.Cross(PumaPointsList[i], PumaPointsList[i + 1]).Normalized() * jointIlustrationHeight / 2,
                    Origin + PumaPointsList[i] - Vector3d.Cross(PumaPointsList[i], PumaPointsList[i + 1]).Normalized() * jointIlustrationHeight / 2,
                    new Vector3d(0, 1, 1),
                    jointIlustrationRadius);
            }

            Cylinder.DrawCylinder(Origin + PumaPointsList[2] + jointIlustrationHeight * (PumaPointsList[2] - PumaPointsList[1]).Normalized() / 2,
                Origin + PumaPointsList[2] - jointIlustrationHeight * (PumaPointsList[2] - PumaPointsList[1]).Normalized() / 2,
                 new Vector3d(0, 1, 1), jointIlustrationRadius);


            Cylinder.DrawCylinder(Origin + PumaPointsList[3] + jointIlustrationHeight * (PumaPointsList[3] - PumaPointsList[2]).Normalized() / 2,
                Origin + PumaPointsList[3] - jointIlustrationHeight * (PumaPointsList[3] - PumaPointsList[2]).Normalized() / 2,
                new Vector3d(0, 1, 1), jointIlustrationRadius);

            // Wrist.Draw(Matrix3d.Identity);
        }


        public static Matrix4d RotateXMatrix(double alphaX)
        {
            Matrix4d result = new Matrix4d(1, 0, 0, 0,
                0, Math.Cos(alphaX), Math.Sin(-alphaX), 0,
                0, Math.Sin(alphaX), Math.Cos(alphaX), 0,
                0, 0, 0, 1);
            //result.Transpose();
            return result;
        }

        public static Matrix4d RotateYMatrix(double alphaY)
        {
            Matrix4d result = new Matrix4d(Math.Cos(alphaY), 0, Math.Sin(alphaY), 0,
                0, 1, 0, 0,
                -Math.Sin(alphaY), 0, Math.Cos(alphaY), 0,
                0, 0, 0, 1);
            //result.Transpose();
            return result;

        }

        public static Matrix4d RotateZMatrix(double alphaZ)
        {
            Matrix4d result = new Matrix4d(
                Math.Cos(alphaZ), -Math.Sin(alphaZ), 0, 0,
                Math.Sin(alphaZ), Math.Cos(alphaZ), 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
            // result.Transpose();
            return result;
        }

        public static Matrix4d TranslateX(double x)
        {
            Matrix4d result = new Matrix4d(
                1, 0, 0, x,
                0, 1, 0, 0,
                0, 0, 1, 0,
                    0, 0, 0, 1);
            // result.Transpose();
            return result;
        }

        public static Matrix4d TranslateY(double y)
        {
            Matrix4d result = new Matrix4d(
                1, 0, 0, 0,
                0, 1, 0, y,
                0, 0, 1, 0,
                0, 0, 0, 1);
            // result.Transpose();
            return result;
        }

        public static Matrix4d TranslateZ(double z)
        {
            Matrix4d result = new Matrix4d(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, z,
                0, 0, 0, 1);
            // result.Transpose();
            return result;
        }

        public static double Angle(Vector3d v, Vector3d w)
        {
            if (v.Length == 0 || w.Length == 0)
            {
                return 0;
                MessageBox.Show("v.length: " + v.Length + "w.length: " + w.Length);
            }

            double cos = Vector3d.Dot(v, w) / (v.Length * w.Length);
            double sin = Vector3d.Cross(v, w).Length / (v.Length * w.Length);
            var temp = Math.Atan2(sin, cos);
            return temp;
        }


        public static double Angle(Vector3d v, Vector3d w, Vector3d z)
        {
            if (v.Length == 0 || w.Length == 0)
            {
                return 0;
                //MessageBox.Show("v.length: " + v.Length + "w.length: " + w.Length);
            }



            var n = Vector3d.Cross(v, w);
            //double cos = Vector3d.Dot(v, w) / (v.Length * w.Length);
            //double sin = Vector3d.Cross(v, w).Length / (v.Length * w.Length); 
            //var temp= Math.Atan2(sin, cos);
            var temp = Math.Acos(Vector3d.Dot(v, w) / (v.Length * w.Length));

            var QuaternionZ = new Vector4d(Math.Sin(temp / 2) * z.X, Math.Sin(temp / 2) * z.Y, Math.Sin(temp / 2) * z.Z, Math.Cos(temp / 2)).Normalized();
            var QuaternionV = QuaternionHelpers.AxisVectorToQuaternion(v).Normalized();

            var RotatedVector = QuaternionHelpers.HamiltonProduct(QuaternionZ, QuaternionHelpers.AxisVectorToQuaternion(v));
            RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector, QuaternionHelpers.ConjugatedQuaternion(QuaternionZ));
            Vector3d v_rotated = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

            //if(!(double.IsInfinity(v_rotated.X0) || double.IsNaN(v_rotated.X0) || double.IsInfinity(v_rotated.Y0) || double.IsNaN(v_rotated.Y0) || double.IsInfinity(v_rotated.Z0) || double.IsNaN(v_rotated.Z0)))
            //{
            if ((w.Normalized() - v_rotated.Normalized()).Length < 0.0001)
            {
                //    MessageBox.Show("tempif: " + temp + "distance: " + (w - v_rotated).Length);
                return temp;
            }
            else
            {
                QuaternionZ = new Vector4d(Math.Sin(-temp / 2) * z.X, Math.Sin(-temp / 2) * z.Y,
                    Math.Sin(-temp / 2) * z.Z, Math.Cos(-temp / 2)).Normalized();
                QuaternionV = QuaternionHelpers.AxisVectorToQuaternion(v).Normalized();

                RotatedVector =
                    QuaternionHelpers.HamiltonProduct(QuaternionZ, QuaternionHelpers.AxisVectorToQuaternion(v));
                RotatedVector = QuaternionHelpers.HamiltonProduct(RotatedVector,
                    QuaternionHelpers.ConjugatedQuaternion(QuaternionZ));
                v_rotated = QuaternionHelpers.QuaternionToAxisVector(RotatedVector);

                //   MessageBox.Show("tempelse: " + -temp + "distance: " +
                //                    (w.Normalized() - v_rotated.Normalized()).Length);
                return -temp;
            }
            //}
            //else
            //{
            //    return temp;
            //}
        }
    }
}
