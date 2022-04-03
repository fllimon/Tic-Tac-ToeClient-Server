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

            Press = new Command.Command(o =>
            {
                Marker current = (Marker)o;

                PlayerChoise(current);
            });

            Exit = new Command.Command(o =>
            {
                Environment.Exit(0);
            });
        }

        public ICommand Press { get; set; }

        public ICommand Exit { get; set; }

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
                    _markers.Add(new Marker(i * 170, j * 170, PlayerData.Empty));
                }
            }

            OnPropertyChanged(nameof(Markers));
        }

        private void PlayerChoise(Marker marker)
        {
            Player playerX = GetPlayerXOrO(_playerOne, _playerTwo, PlayerData.X);
            Player player0 = GetPlayerXOrO(_playerOne, _playerTwo, PlayerData.O);

            if (marker.PlayerMarker == PlayerData.Empty)
            {
                PlayerChoise(marker, playerX, player0);
            }
        }

        private void PlayerChoise(Marker marker, Player playerX, Player player0)
        {
            if (IsFirstChoise(marker))
            {
                ChangeMarker(marker, playerX);
                playerX.PlayerStatus = PlayerStatus.Await;
                player0.PlayerStatus = PlayerStatus.Move;
            }
            else
            {
                if (playerX.PlayerStatus == PlayerStatus.Move)
                {
                    ChangeMarker(marker, playerX);
                    playerX.PlayerStatus = PlayerStatus.Await;
                    player0.PlayerStatus = PlayerStatus.Move;
                }
                else
                {
                    ChangeMarker(marker, player0);
                    player0.PlayerStatus = PlayerStatus.Await;
                    playerX.PlayerStatus = PlayerStatus.Move;
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

        private Player GetPlayerXOrO(Player playerOne, Player playerTwo, PlayerData type)
        {
            if (playerOne.PlayerType == type)
            {
                return playerOne;
            }
            else
            {
                return playerTwo;
            }
        }

        private void ChangeMarker(Marker marker, Player player)
        {
            foreach (Marker item in Markers)
            {
                if (item.Equals(marker))
                {
                    item.PlayerMarker = player.PlayerType;
                }
            }
        }
    }
}
