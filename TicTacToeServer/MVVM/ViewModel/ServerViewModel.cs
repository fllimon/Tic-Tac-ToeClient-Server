using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TicTacToeServer.MVVM.ViewModel
{
    class ServerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}