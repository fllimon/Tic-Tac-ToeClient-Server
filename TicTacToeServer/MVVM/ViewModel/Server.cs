using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using TicTacToe.GameLibrary.Command;
using TicTacToe.GameLibrary.MVVM.Model;
using TicTacToe.GameLibrary.MVVM.ViewModel;
using TicTacToe.GameLibrary.Command;
using System.Threading.Tasks;
using TicTacToe.GameLibrary.MVVM;

namespace TicTacToeServer.MVVM.ViewModel
{
    class Server : ClientServerViewModel
    {
        private bool _isPending = false;
        private string _acceptText = "Server started... Avaliable connections!";
        private const int SERVER_PORT = 24000;
        private TcpListener _serverSocket;
        private static List<TcpClient> _tcpClients = null;
        private Player _onePlayer = null;
        private Player _twoPlayer = null;
        private GameField _gameField = null;

        public Server()
        {
            _tcpClients = new List<TcpClient>();
            
            Press = new Command(o =>
            {
                IsPending = true;

                StartServer();
            });


            Exit = new Command(o =>
            {
                _serverSocket.Stop();

                _tcpClients.RemoveRange(0, _tcpClients.Count);

                Environment.Exit(0);
            });
        }

        public bool IsPending 
        {
            get
            {
                return _isPending;
            }
            set
            {
                _isPending = value;

                OnPropertyChanged(nameof(IsPending));
            }
        }

        public string AcceptText
        {
            get
            {
                return _acceptText;
            }
            set
            {
                _acceptText = value;

                OnPropertyChanged(nameof(AcceptText));
            }
        }

        public ICommand Press { get; set; }

        public ICommand Exit { get; set; }

        public async Task StartServer()
        {
            IPAddress ip = IPAddress.Any;
            _serverSocket = new TcpListener(ip, SERVER_PORT);

            try
            {
                _serverSocket.Start();
                TcpClient returnedClient = null;

                while(true)
                {
                    returnedClient = await _serverSocket.AcceptTcpClientAsync();

                    if (returnedClient != null)
                    {
                        AcceptText = string.Format($"Client connected! {returnedClient.Client.RemoteEndPoint}");
                        _tcpClients.Add(returnedClient);
                    }

                    InitGameField();

                    ReadDataFromClient(returnedClient);

                    if (_gameField != null)
                    {
                        AcceptText = string.Format($"Init: {_gameField.Markers.Count}");

                        SendToClient(_gameField.Markers, returnedClient);
                    }
                } 
            }
            catch (SocketException ex)
            {
                throw;
            }
        }

        private async Task ReadDataFromClient(TcpClient client)
        {
            AcceptText = await JsonSerializer.DeserializeAsync<string>(client.GetStream());
        }

        private async Task SendToClient(object data, TcpClient client)
        {
            if (_tcpClients.Count == 0)
            {
                return;
            }

            if (data == null)
            {
                return;
            }

            await JsonSerializer.SerializeAsync(client.GetStream(), data);
        }

        private void RemoveClient(TcpClient client)
        {
            if (_tcpClients.Contains(client))
            {
                _tcpClients.Remove(client);
            }
        }

        private void InitGameField()
        {
            if (_tcpClients.Count > 0)
            {
                _onePlayer = new Player();
                _twoPlayer = new Player();

                if (_gameField == null)
                {
                    _gameField = new GameField(_onePlayer, _twoPlayer);
                }
            }
        }

    }
}
