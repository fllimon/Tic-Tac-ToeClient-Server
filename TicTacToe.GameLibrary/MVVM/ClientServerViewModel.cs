using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TicTacToe.GameLibrary.MVVM.ViewModel
{
    public class ClientServerViewModel : INotifyPropertyChanged
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