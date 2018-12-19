using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        #region Constructors and Destructors

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MainViewModel1;



            var timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += TimerOnTick;
            timer.Start();
            MainViewModel1.RotationSimulator1.OnLoad();
            //var Width = glControl.Width;
            //var Height = glControl.Height;
            

            //GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
           // var p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Width / (float)Height, 1.0f, 64.0f);
           // var p = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)Width / (float)Height, 1.0f, 64.0f);
            var p = Matrix4.CreateOrthographic(7,7, -100.0f, 664.0f);
            GL.LoadMatrix(ref p);

            GL.MatrixMode(MatrixMode.Modelview);
            
            //var mv = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            //GL.LoadMatrix(ref mv);            

            var CameraPosition=new Vector3(0,1,0);
            var TargetPosition=new Vector3(0,0,0);
            var UpVectorInWorldSpace=new Vector3(0,1,0);

            var mv = Matrix4.LookAt(CameraPosition, TargetPosition, UpVectorInWorldSpace);
            GL.LoadMatrix(ref mv);


            //GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Lighting);
            //GL.Enable(EnableCap.ColorMaterial);
            //float[] light_position = { 0, 80, 0 };
            //float[] light_diffuse = { 0.2f, 0.1f, 0.1f };
            //float[] light_ambient = { 0.2f, 0.1f, 0.1f };
            //float[] light_specular = { 0.2f, 0.1f, 0.1f };
            //GL.Light(LightName.Light0, LightParameter.Position, light_position);
            //GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            //GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            //GL.Light(LightName.Light0, LightParameter.Specular, light_specular);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.ColorMaterial);
            float[] light_position = { 100, 500, 100 };
            float[] light_diffuse = { 0.01f, 0.01f, 0.005f };
            float[] light_ambient = { 0.2f, 0.1f, 0.1f };
            float[] light_specular = { 0.0f, 0.0f, 0.0f };
            GL.Light(LightName.Light0, LightParameter.Position, light_position);
            GL.Light(LightName.Light0, LightParameter.Diffuse, light_diffuse);
            GL.Light(LightName.Light0, LightParameter.Ambient, light_ambient);
            GL.Light(LightName.Light0, LightParameter.Specular, light_specular);

            GL.Enable(EnableCap.Light0);


           
            MainViewModel1.RotationSimulator1.RefreshScene += Scene_RefreshScene;
            MainViewModel1.RotationSimulator1.Cursor0.RefreshScene += Scene_RefreshScene;
            MainViewModel1.RotationSimulator1.Cursor1.RefreshScene += Scene_RefreshScene;
        }

        #endregion

        #region Fields

        private MainViewModel MainViewModel1 = new MainViewModel();

        //public MainViewModel MainViewModel1
        //{
        //    get { return _mainViewModel; }
        //    set
        //    {
        //        _mainViewModel = value;

        //    }
        //}


        private int frames;

        private GLControl glControl;
        private GLControl glControl1;

        private DateTime lastMeasureTime;

        #endregion

        #region Methods

        private void GlControlOnPaint(object sender, PaintEventArgs e)
        {
            Refresh();
        }

        public void Refresh()
        {
            ////  glControl.MakeCurrent();
            // GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //  GL.LoadIdentity();
            //  GL.MatrixMode(MatrixMode.Projection);
            //    GL.LoadIdentity();
            // var halfWidth = glControl.Width / 2;
            //var halfHeight = (float)(glControl.Height / 2);
            //  GL.Ortho(-10, 10, 10, -10, 10, -10);
            GL.Viewport(0, 0, glControl.Size.Width, glControl.Size.Height);
            //double Scale = 5;
            //GL.Scale(Scale,Scale,Scale);
            //renderer.Render();



            //  GL.Viewport(0, 0, Width, Height);

            var CameraPosition = new Vector3(0, 0, 8);
            var TargetPosition = new Vector3(0, 0, 0);
            var UpVectorInWorldSpace = new Vector3(0, 1, 0);
            var mv = Matrix4.LookAt(CameraPosition, TargetPosition, UpVectorInWorldSpace);



            MainViewModel1.RotationSimulator1.OnUpdateFrame();
            MainViewModel1.RotationSimulator1.OnRenderFrame(_alphaX, _alphaY, _alphaZ, mv);





            glControl.SwapBuffers();
            //glControl1.SwapBuffers();

            frames++;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(lastMeasureTime) > TimeSpan.FromSeconds(1))
            {
                Title = "Rotation Comparator: " + frames + "fps";
                frames = 0;
                lastMeasureTime = DateTime.Now;
            }
            //Refresh();
            // glControl.Invalidate();
           
        }

        //void Scene_RefreshScene(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    Refresh();

        //    //glControl.Invalidate();
        //}
        void Scene_RefreshScene(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainViewModel1.RotationSimulator1.GenerateAnimationFrames(MainViewModel1.RotationSimulator1.SimulationTime,
                MainViewModel1.RotationSimulator1.FramesNumber);
            Refresh();

            //glControl.Invalidate();
        }

        private void _glControl_MouseWheel(object sender, MouseEventArgs e)
        {
            MainViewModel1.RotationSimulator1.Scale += e.Delta;
            Refresh();
        }

        private double eX;
        private double eY;

        private void _glControl_MouseDown(object sender, MouseEventArgs e)
        {
            eX = e.X;
            eY = e.Y;
        }

        private void _glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                MouseMoveRotate(e.X, e.Y);
                Refresh();
            }
        }

        private double _fi;
        private double _teta;
        private double _alphaX, _alphaY, _alphaZ;




        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var temp = e.GetPosition(this);
            eX = temp.X;
            eY = temp.Y;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var temp = e.GetPosition(this);
            if (e.RightButton == MouseButtonState.Pressed)
            {
                MouseMoveRotate((int)temp.X, (int)temp.Y);
                Refresh();
            }
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.OemMinus:
                    MainViewModel1.RotationSimulator1.Scale -= 100 / 1000.0;
                    Refresh();
                    break;
                case Key.OemPlus:
                    MainViewModel1.RotationSimulator1.Scale += 100 / 1000.0;
                    Refresh();
                    break;
            }
        }

        private void MouseMoveRotate(int fi, int teta)
        {
            _fi = fi - eX;
            _teta = teta - eY;
            eX = fi;
            eY = teta;

            //_alphaX += 16 * 4 * _teta / 750;
            //_alphaY += 16 * 4 * _fi / 1440;
            //_alphaZ += 0;


            _alphaX += 16 * 4 * _teta / 1440;
            _alphaY += 16 * 4 * _fi / 1440;
            _alphaZ += 0;
        }


        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MainViewModel1.RotationSimulator1.Scale += e.Delta / 1000.0;
            Refresh();
        }


        private void OpentkWindow_OnInitialized(object sender, EventArgs e)
        {
            glControl = new GLControl();
            //glControl.MakeCurrent();
            glControl.TopLevel = false;
            glControl.Paint += GlControlOnPaint;
            // glControl.MouseWheel += _glControl_MouseWheel;
            glControl.MouseDown += _glControl_MouseDown;
            glControl.MouseMove += _glControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
            (sender as WindowsFormsHost).Child = glControl;
        }

        //private void OpentkWindow_OnInitialized1(object sender, EventArgs e)
        //{
        //    glControl1 = new GLControl();
        //    //glControl.MakeCurrent();
        //    glControl1.TopLevel = false;
        //    //glControl1.Paint += GlControlOnPaint;
        //    // glControl.MouseWheel += _glControl_MouseWheel;
        //    glControl1.MouseDown += _glControl_MouseDown;
        //    glControl1.MouseMove += _glControl_MouseMove;
        //    glControl1.MouseWheel += GlControl_MouseWheel;
        //    (sender as WindowsFormsHost).Child = glControl1;
        //}
        void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            return;
        }

        
        private void StartSimulation_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel1.RotationSimulator1.StartSimulation();
        }

        private void DefaultView_OnClick(object sender, RoutedEventArgs e)
        {
            _alphaX = 0;
            _alphaY = 0;
            _alphaZ = 0;
            MainViewModel1.RotationSimulator1.Scale = 1;
            glControl.Invalidate();
        }

       



        private void StopSimulation_OnClick_imulation_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel1.RotationSimulator1.StopSimulation();
        }

        private void PauseSimulation_OnClickSimulation_OnClick_imulation_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel1.RotationSimulator1.PauseSimulation();
        }


    }

    #endregion
}