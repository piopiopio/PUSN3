using OpenTK.Graphics.OpenGL;

namespace WpfApp1
{
    public class MainViewModel : ViewModelBase
    {

        private RotationSimulator _rotationSimulator = new RotationSimulator();

        public RotationSimulator RotationSimulator1
        {
            get
            {
                return _rotationSimulator;
            }
            set
            {
                _rotationSimulator = value;
                OnPropertyChanged(nameof(RotationSimulator1));
                
            }
        }
    }
}

