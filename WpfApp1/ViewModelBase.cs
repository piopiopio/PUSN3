using System.ComponentModel;

namespace WpfApp1
{
    public class ViewModelBase: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler RefreshScene;

        protected void Refresh()
        {
            //BusyEllipseLed = 1;
            if (RefreshScene != null)
                RefreshScene(this, new PropertyChangedEventArgs("RefreshScene"));
            //BusyEllipseLed = 0;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
         
            if (PropertyChanged!=null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}

