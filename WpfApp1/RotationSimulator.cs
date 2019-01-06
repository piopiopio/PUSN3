using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using System.Windows.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WpfApp1;
using Cursor = WpfApp1.Cursor;


public class RotationSimulator : ViewModelBase
{

    public RotationSimulator(GLControl _glControl, GLControl _glControl1)
    {

    }
    private Puma _puma = new Puma(new double[] { 1, 1, 1, 99, 99, 99 }, new double[] { 0, Math.PI/4, 0, Math.PI / 4, Math.PI / 4, Math.PI / 4 });

    public Puma Puma1
    {
        get { return _puma; }
        set
        {
            _puma = value;
        }
    }
    private bool _slerp = false;

    private bool _showCursor = false;
    public bool ShowCursor
    {
        get { return _showCursor; }
        set
        {
            _showCursor = value;
            OnPropertyChanged(nameof(ShowCursor));
            Refresh();
        }
    }


    private bool _showPuma = true;
    public bool ShowPuma
    {
        get
        {
            return _showPuma;
        }
        set
        {
            _showPuma = value;
            OnPropertyChanged(nameof(ShowPuma));
            Refresh();
        }
    }

    public bool SLERP
    {
        get { return _slerp; }
        set
        {
            _slerp = value;
            OnPropertyChanged(nameof(SLERP));
            Refresh();
        }
    }
    private Vector3d Offset = new Vector3d();

    private bool _showAllFrames = false;

    public bool ShowAllFrames
    {
        get { return _showAllFrames; }
        set
        {
            _showAllFrames = value;
            OnPropertyChanged(nameof(ShowAllFrames));
            if (_showAllFrames == true)
            {
                GenerateAnimationFrames(_simulationTime, _framesNumber);
            }
            Refresh();
        }
    }

    public RotationSimulator()
    {


    }

    private double _simulationTime = 2;

    public double SimulationTime
    {

        get { return _simulationTime; }
        set
        {
            _simulationTime = value;
            OnPropertyChanged(nameof(SimulationTime));
        }
    }


    private int _framesNumber = 100;

    public int FramesNumber
    {

        get { return _framesNumber; }
        set
        {
            _framesNumber = value;
            OnPropertyChanged(nameof(FramesNumber));
        }
    }


    private bool _startCordinateSystem = true;

    public bool StartCordinateSystem
    {
        get { return _startCordinateSystem; }
        set
        {
            _startCordinateSystem = true;
            OnPropertyChanged(nameof(StartCordinateSystem));
            OnPropertyChanged(nameof(FinishCordinateSystem));
            OnPropertyChanged(nameof(TempCursor));
        }
    }

    public bool FinishCordinateSystem
    {
        get { return !_startCordinateSystem; }
        set
        {
            _startCordinateSystem = false;
            OnPropertyChanged(nameof(StartCordinateSystem));
            OnPropertyChanged(nameof(FinishCordinateSystem));
            OnPropertyChanged(nameof(TempCursor));
        }
    }

    public Cursor TempCursor
    {
        get
        {
            if (_startCordinateSystem)
            {
                return Cursor0;
            }
            else
            {
                return Cursor1;
            }

        }
        set
        {
            if (_startCordinateSystem)
            {
                Cursor0 = value;
            }
            else
            {
                Cursor1 = value;
            }

        }
    }

    public Cursor Cursor0 { get; set; } = new Cursor(new Vector3d(0, -2, 0));
    public Cursor Cursor1 { get; set; } = new Cursor(new Vector3d(0, 2, 0));



    private int _animationSpeed = 20;

    public int AnimationSpeed
    {
        get { return _animationSpeed; }
        set { _animationSpeed = value; }
    }


    public void OnLoad()
    {

    }

    public void OnUpdateFrame()
    {
        //Material1.OnUpdateFrame();
        //Cutter1.OnUpdateFrame();

    }

    private double counter = 0;
    //private double scale = 0.005;
    private double scale = 1;

    public double Scale
    {
        get => scale;
        set => scale = Math.Max(value, 0.0001);
    }

    private Vector3? frozenCutterCeneterPointPosition;

    public void OnRenderFrame(double alphaX, double alphaY, double alphaZ, Matrix4 LookAtMatrix)
    {

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.PushAttrib(AttribMask.LightingBit);
        GL.PushAttrib(AttribMask.LightingBit);
        GL.Disable(EnableCap.Lighting);
        GL.Color3(Color.White);
        GL.PopAttrib();
        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadIdentity();
        //   var modelview = Matrix4.LookAt(0.0f, 3.5f, 3.5f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f);
        GL.LoadMatrix(ref LookAtMatrix);

        //counter += e.Time;
        // counter += 0.1;
        //  GL.Rotate(counter * 10, 0, 1, 0);
        GL.Rotate(alphaX, 1, 0, 0);
        GL.Rotate(alphaY, 0, 1, 0);
        GL.Rotate(alphaZ, 0, 0, 1);
        GL.Scale(Scale, Scale, Scale);
        //GL.Rotate(0, 0, 1, 0);

        if (ShowCursor)
        {
            Cursor0.Draw(true);
            Cursor1.Draw(true);

            Cursor0.Draw(false);
            Cursor1.Draw(false);


            if (CurrentCursors.Item1 != null)
            {
                CurrentCursors.Item1.Draw(false);
            }

            if (CurrentCursors.Item2 != null)
            {
                CurrentCursors.Item2.Draw(true);
            }

            if (_showAllFrames)
            {
                foreach (var item in AnimationFramesList)
                {
                    item.Item1.Draw(false);
                    item.Item2.Draw(true);
                }
            }

        }

        if (ShowPuma)
        {
            _puma.Draw();
        }
    }





    private DispatcherTimer timer;
    int _stepNumber = 0;

    public int StepNumber
    {
        get
        {
            return _stepNumber;
        }
        set
        {
            _stepNumber = value;
            OnPropertyChanged(nameof(StepNumber));
        }
    }

    private bool _simulationResultButtonIsEnabled = true;

    public bool SimulationResultButtonIsEnabled
    {
        get { return _simulationResultButtonIsEnabled; }
        set
        {
            _simulationResultButtonIsEnabled = value;
            OnPropertyChanged(nameof(SimulationResultButtonIsEnabled));
        }
    }

    private bool _simulationStartButtonIsEnabled = true;

    public bool SimulationStartButtonIsEnabled
    {
        get { return _simulationStartButtonIsEnabled; }
        set
        {
            _simulationStartButtonIsEnabled = value;
            OnPropertyChanged(nameof(SimulationStartButtonIsEnabled));
        }
    }

    private bool _loadPathButtonIsEnabled = true;

    public bool LoadPathButtonIsEnabled
    {
        get { return _loadPathButtonIsEnabled; }
        set
        {
            _loadPathButtonIsEnabled = value;
            OnPropertyChanged(nameof(LoadPathButtonIsEnabled));
        }
    }
    public void StartSimulation()
    {
        SimulationStartButtonIsEnabled = false;

        if (pauseSimulationFlag)
        {
            pauseSimulationFlag = false;
            lastMeasureTime = DateTime.Now - BackupTime;
        }
        else
        {
            Cursor1.ConditionEndQuaternionToNearer(Cursor0);
            //var debug= Vector4d.Dot(Cursor0._quaternion, Cursor1._quaternion);

            SimulationResultButtonIsEnabled = false;
            SimulationStartButtonIsEnabled = false;
            LoadPathButtonIsEnabled = false;
            StepNumber = 0;

            timer = new DispatcherTimer(DispatcherPriority.Render);
            //timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Interval = TimeSpan.FromSeconds(_simulationTime / (_framesNumber - 1));
            timer.Tick += TimerOnTick;
            lastMeasureTime = DateTime.Now;
        }

        timer.Start();

    }


    private Tuple<Cursor, Cursor> CurrentCursors = new Tuple<Cursor, Cursor>(null, null);
    private List<Tuple<Cursor, Cursor>> AnimationFramesList = new List<Tuple<Cursor, Cursor>>();
    private DateTime lastMeasureTime = new DateTime();


    public void GenerateAnimationFrames(double simulationTime, int framesNumber)
    {
        AnimationFramesList.Clear();
        for (int i = 0; i <= framesNumber; i++)
        {
            var temp = new Cursor();
            double a = ((double)(framesNumber - i) / framesNumber);
            //double b = ((double)i / framesNumber);
            //temp.EulerAngles = a * Cursor0.EulerAngles + b * Cursor1.EulerAngles;
            //temp.Origin = a * Cursor0.Origin + b * Cursor1.Origin;

            //AnimationFramesList.Add(temp);
            AnimationFramesList.Add(new Tuple<Cursor, Cursor>(CursorAngleByEuler(Cursor0, Cursor1, a), CursorAngleByQuaternion(Cursor0, Cursor1, a, _slerp)));
        }
    }

    public TimeSpan BackupTime;

    private void TimerOnTick(object sender, EventArgs e)
    {
        //if (StepNumber > (AnimationFramesList.Count - 1))
        //{
        //    timer.Stop();
        //}
        //else
        //{
        BackupTime = DateTime.Now.Subtract(lastMeasureTime);
        var a = BackupTime.TotalMilliseconds / (_simulationTime * 1000);

        if (a > 1)
        {
            a = 1;
            timer.Stop();
            CurrentCursors = new Tuple<Cursor, Cursor>(null, null);
            SimulationStartButtonIsEnabled = true;
        }
        else
        {

            CurrentCursors = new Tuple<Cursor, Cursor>(CursorAngleByEuler(Cursor0, Cursor1, a), CursorAngleByQuaternion(Cursor0, Cursor1, a, _slerp));
            // CurrentCursor = AnimationFramesList[StepNumber];

        }
        Refresh();
        // }

        // StepNumber++;

    }


    private Cursor CursorAngleByEuler(Cursor StartCursor, Cursor EndCursor, double AnimationProgress)
    {
        //Animation progress range 0 do 1
        var temp = new Cursor();

        temp.EulerAngles = (1 - AnimationProgress) * Cursor0.EulerAngles + AnimationProgress * Cursor1.EulerAngles;
        temp.Origin = (1 - AnimationProgress) * Cursor0.Origin + AnimationProgress * Cursor1.Origin;
        return temp;
    }

    private Cursor CursorAngleByQuaternion(Cursor StartCursor, Cursor EndCursor, double AnimationProgress, bool slerp)
    {
        //Animation progress range 0 do 1
        var temp = new Cursor();
        if (slerp)
        {
            var alpha = Math.Acos(StartCursor._quaternion.X * EndCursor._quaternion.X +
                                  StartCursor._quaternion.Y * EndCursor._quaternion.Y +
                                  StartCursor._quaternion.Z * EndCursor._quaternion.Z +
                                  StartCursor._quaternion.W * EndCursor._quaternion.W);

            if (Math.Abs(alpha) < 0.001)
            {
                temp._quaternion = StartCursor._quaternion;
            }
            else
            {
                temp._quaternion =
                    StartCursor._quaternion * Math.Sin((1 - AnimationProgress) * alpha) / Math.Sin(alpha) +
                    EndCursor._quaternion * Math.Sin(AnimationProgress * alpha) / Math.Sin(alpha);
            }
        }
        else
        {

            temp._quaternion = (AnimationProgress * EndCursor._quaternion + (1 - AnimationProgress) * StartCursor._quaternion).Normalized();

        }

        temp.Origin = (1 - AnimationProgress) * StartCursor.Origin + AnimationProgress * EndCursor.Origin;
        return temp;
    }

    private bool _showPath = false;
    public bool ShowPath
    {
        get { return _showPath; }
        set
        {
            _showPath = value;
            OnPropertyChanged(nameof(ShowPath));
            Refresh();
        }
    }

    private bool pauseSimulationFlag = false;
    public void PauseSimulation()
    {

        pauseSimulationFlag = true;
        if (timer != null)
        {
            timer.Stop();
        }
        SimulationStartButtonIsEnabled = true;
    }

    public void StopSimulation()
    {

        if (timer != null)
        {
            timer.Stop();
        }
        pauseSimulationFlag = false;
        CurrentCursors = new Tuple<Cursor, Cursor>(null, null);
        Refresh();
        SimulationStartButtonIsEnabled = true;
    }



}