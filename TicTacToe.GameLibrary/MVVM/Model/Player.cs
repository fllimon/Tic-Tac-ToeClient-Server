using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.GameLibrary.MVVM.ViewModel;

namespace TicTacToe.GameLibrary.MVVM.Model
{
    [Serializable]
    public class Player : ClientServerViewModel
    {
        private PlayerStatus _status;

        public string Name { get; set; }

        public PlayerData PlayerType { get; set; }

        public PlayerStatus PlayerStatus 
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;

                OnPropertyChanged(nameof(PlayerStatus));
            }
        }
    }
}
