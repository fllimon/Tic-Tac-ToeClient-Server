using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TicTacToeClient.MVVM.ViewModel
{
    class ClientViewModel : INotifyPropertyChanged
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