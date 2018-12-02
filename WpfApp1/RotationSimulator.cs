using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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

    private bool _slerp = false;

    public bool SLERP
    {
        get { return _slerp; }
        set
        {
            _slerp = value;
            OnPropertyChanged(nameof(SLERP));
        }
    }
    private Vector3d Offset=new Vector3d();

    private bool _showAllFrames = true;

    public bool ShowAllFrames
    {
        get { return _showAllFrames; }
        set
        {
            _showAllFrames = value;
            OnPropertyChanged(nameof(ShowAllFrames));
        }
    }

    public RotationSimulator()
    {


    }

    private double _simulationTime = 10;

    public double SimulationTime
    {

        get { return _simulationTime; }
        set
        {
            _simulationTime = value;
            OnPropertyChanged(nameof(SimulationTime));
        }
    }


    private int _framesNumber = 10;

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

    public Cursor Cursor0 { get; set; } = new Cursor(new Vector3d(0,200,0));
    public Cursor Cursor1 { get; set; } = new Cursor(new Vector3d(0, -200, 0));



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
    private double scale = 0.005;

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
        GL.Rotate(alphaX, 0, 0, 1);
        GL.Scale(Scale, Scale, Scale);
        GL.Rotate(50, 0, 1, 0);

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
            }
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

        SimulationResultButtonIsEnabled = false;
        SimulationStartButtonIsEnabled = false;
        LoadPathButtonIsEnabled = false;
        StepNumber = 0;
        GenerateAnimationFrames(_simulationTime, _framesNumber);
        timer = new DispatcherTimer();
        //timer.Interval = TimeSpan.FromMilliseconds(30);
        timer.Interval = TimeSpan.FromSeconds(_simulationTime / (_framesNumber - 1));
        timer.Tick += TimerOnTick;
        lastMeasureTime=DateTime.Now;
        timer.Start();

    }

    private Tuple<Cursor,Cursor> CurrentCursors = new Tuple<Cursor, Cursor>(null,null);
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
            AnimationFramesList.Add(new Tuple<Cursor, Cursor>(CursorAngleByEuler(Cursor0, Cursor1,a),null));
        }
    }


    private void TimerOnTick(object sender, EventArgs e)
    {
        //if (StepNumber > (AnimationFramesList.Count - 1))
        //{
        //    timer.Stop();
        //}
        //else
        //{
            var a=DateTime.Now.Subtract(lastMeasureTime).TotalMilliseconds/(_simulationTime*1000);
            if (a > 1)
            {
                a = 1;
                timer.Stop();
            }
            

            CurrentCursors = new Tuple<Cursor, Cursor>(CursorAngleByEuler(Cursor0, Cursor1,a), CursorAngleByQuaternion(Cursor0, Cursor1, a, false));
           // CurrentCursor = AnimationFramesList[StepNumber];
            Refresh();
       // }

       // StepNumber++;

    }


    private Cursor CursorAngleByEuler(Cursor StartCursor, Cursor EndCursor, double AnimationProgress)
    {
        //Animation progress range 0 do 1
        var temp = new Cursor();
        var b = 1 - AnimationProgress;
        temp.EulerAngles = AnimationProgress * Cursor0.EulerAngles + b * Cursor1.EulerAngles;
        temp.Origin = AnimationProgress * Cursor0.Origin + b * Cursor1.Origin;
        return temp;
    }

    private Cursor CursorAngleByQuaternion(Cursor StartCursor, Cursor EndCursor, double AnimationProgress, bool slerp)
    {
        //Animation progress range 0 do 1
        var temp = new Cursor();
        if (slerp)
        {

        }
        else
        {
            var b = 1 - AnimationProgress;
            temp._quaternion = AnimationProgress * Cursor0._quaternion + b * Cursor1._quaternion;
            temp.Origin = AnimationProgress * Cursor0.Origin + b * Cursor1.Origin;
        }


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

    private bool stopSimulationFlag = false;
    public void StopSimulation()
    {
        stopSimulationFlag = true;

    }



}