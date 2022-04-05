using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.GameLibrary.MVVM.ViewModel;

namespace TicTacToe.GameLibrary.MVVM.Model
{
    public class Marker : ClientServerViewModel
    {
        private PlayerData _playerMarker;

        public int X { get; set; }

        public int Y { get; set; }

        public PlayerData PlayerMarker
        {
            get
            {
                return _playerMarker;
            }
            set
            {
                _playerMarker = value;

                OnPropertyChanged(nameof(PlayerMarker));
            }
        }
    }
}
