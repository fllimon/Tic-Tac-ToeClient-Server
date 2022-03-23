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
        private int _x;
        private int _y;
        private PlayerData _playerMarker;

        public Marker(int x, int y, PlayerData data)
        {
            _x = x;
            _y = y;
            _playerMarker = data;
        }

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

        public int X
        {
            get
            {
                return _x * 170;
            }
            private set
            {
                _x = value;
            }
        }

        public int Y
        {
            get
            {
                return _y * 170;
            }
            private set
            {
                _y = value;
            }
        }
    }
}
