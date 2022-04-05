using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TicTacToe.GameLibrary.MVVM.Model;
using TicTacToe.GameLibrary.MVVM.ViewModel;
using TicTacToe.GameLibrary;

namespace TicTacToe.GameLibrary.MVVM
{
    public class GameField : ClientServerViewModel
    {
        private const int DEFAULT_WIDTH = 3;
        private const int DEFAULT_HEIGHT = 3;
        private ObservableCollection<Marker> _markers;
        private Player _playerOne = null;
        private Player _playerTwo = null;
        private int _steps;
        private bool _isWin = false;

        public GameField(Player playerOne, Player playerTwo)
        {
            _playerOne = playerOne;
            _playerTwo = playerTwo;
            _steps = 0;
            _markers = new ObservableCollection<Marker>();

            InitializeGameField();
        }

        public Player PlayerOne
        {
            get { return _playerOne; }
        }

        public Player PlayerTwo
        { 
            get 
            { 
                return _playerTwo; 
            } 
        }

        public ObservableCollection<Marker> Markers
        {
            get
            {
                return _markers;
            }
            private set
            {
                _markers = value;

                OnPropertyChanged();
            }
        }

        public void InitializeGameField()
        {
            InitGameField();
        }

        private void InitGameField()
        {
            _markers.Clear();

            for (int i = 0; i < DEFAULT_WIDTH; i++)
            {
                for (int j = 0; j < DEFAULT_HEIGHT; j++)
                {
                    _markers.Add(new Marker { X = i * 170, Y = j * 170, PlayerMarker = PlayerData.Empty });
                }
            }

            OnPropertyChanged(nameof(Markers));
        }

        public void PlayerChoise(Marker marker)
        {
            if (marker.PlayerMarker == PlayerData.Empty)
            {
                PlayerChoise(marker, _playerOne, _playerTwo);
            }
        }

        private void PlayerChoise(Marker marker, Player first, Player second)
        {
            if (IsFirstChoise(marker))
            {
                if (first.PlayerType == PlayerData.X)
                {
                    ChangeMarker(marker, first);
                    first.PlayerStatus = PlayerStatus.Await;
                    second.PlayerStatus = PlayerStatus.Move;
                }
                else
                {
                    ChangeMarker(marker, second);
                    second.PlayerStatus = PlayerStatus.Await;
                    second.PlayerStatus = PlayerStatus.Move;
                }
            }
            else
            {
                if (first.PlayerStatus == PlayerStatus.Move)
                {
                    ChangeMarker(marker, first);
                    first.PlayerStatus = PlayerStatus.Await;
                    second.PlayerStatus = PlayerStatus.Move;
                }
                else
                {
                    ChangeMarker(marker, second);
                    second.PlayerStatus = PlayerStatus.Await;
                    first.PlayerStatus = PlayerStatus.Move;
                }
            }
        }

        private bool IsFirstChoise(Marker marker)
        {
            bool isFirstChoise = false;

            if (_steps == 0)
            {
                isFirstChoise = true;
                _steps++;
            }

            return isFirstChoise;
        }

        private void ChangeMarker(Marker marker, Player player)
        {
            for (int i = 0; i < Markers.Count; i++)
            {
                if ((Markers[i].X == marker.X) &&
                    (Markers[i].Y == marker.Y) &&
                    (Markers[i].PlayerMarker == PlayerData.Empty))
                {
                    Markers[i].PlayerMarker = player.PlayerType;

                    return;
                }
            }
            //foreach (Marker item in Markers)
            //{
            //    if ((item.X == marker.X) && 
            //        (item.Y == marker.Y) &&
            //        (item.PlayerMarker == PlayerData.Empty))
            //    {
            //        item.PlayerMarker = player.PlayerType;

            //        return;
            //    }
            //}
        }
    }
}
