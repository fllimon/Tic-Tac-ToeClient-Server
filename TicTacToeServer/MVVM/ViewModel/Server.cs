using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Windows.Input;
using TicTacToe.GameLibrary.Command;
using TicTacToe.GameLibrary.MVVM.Model;
using TicTacToe.GameLibrary.MVVM.ViewModel;
using System.Threading.Tasks;
using TicTacToe.GameLibrary.MVVM;
using System.Collections.ObjectModel;
using MahApps.Metro.Controls;
using System.Linq;
using System.Text;

namespace TicTacToeServer.MVVM.ViewModel
{
    class Server : ClientServerViewModel
    {
        private const string DEFAULT_FILE_LOG = "../../log.txt";
        private const int SERVER_PORT = 24000;

        private bool _isPending = false;
        private bool _isVisible = true;
        private string _acceptText = "Server started... Avaliable connections!";
        private TcpListener _serverSocket;
        private int _count = 0;
        private List<TcpClient> _clients = null;
        private Dictionary<TcpClient, Player> _players;
        private Player _first = null;
        private Player _second = null;
        private GameField _gameField = null;
        private byte[] _buff = null; 
        
        public Server()
        {
            _clients = new List<TcpClient>(2);
            _players = new Dictionary<TcpClient,Player>();
            
            Press = new Command(o =>
            {
                IsVisible = false;
                IsPending = true;

                StartServer();
            });


            Exit = new Command(o =>
            {
                _clients.Clear();
                _players.Clear();

                if (_buff != null)
                {
                    Array.Clear(_buff, 0, _buff.Length);    
                }

                if (_serverSocket != null)
                {
                    _serverSocket.Stop();
                }

                Environment.Exit(0);
            });
        }

        #region ============ PROPERTIES ============================

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;

                OnPropertyChanged();
            }
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

        public int PlayerCount
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;

                OnPropertyChanged();
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

                OnPropertyChanged();
            }
        }

        public ICommand Press { get; set; }

        public ICommand Exit { get; set; }

        #endregion

        public void StartServer()
        {
            IPAddress ip = IPAddress.Any;
            _serverSocket = new TcpListener(ip, SERVER_PORT);

            try
            {
                _serverSocket.Start();
                _serverSocket.BeginAcceptTcpClient(OnAcceptClient, _serverSocket);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);

                _clients.Clear();
                _players.Clear();
                PlayerCount = 0;
                _serverSocket.BeginAcceptTcpClient(OnAcceptClient, _serverSocket);
            }
           
        }

        private void OnAcceptClient(IAsyncResult ar)
        {
            TcpListener listener = ar.AsyncState as TcpListener;

            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                AcceptText = "Client connected...";

                listener.BeginAcceptTcpClient(OnAcceptClient, listener);

                lock (_clients)
                {
                    _clients.Add(client);
                    PlayerCount = _clients.Count;
                }

                _buff = new byte[1024];
                client.GetStream().BeginRead(_buff, 0, _buff.Length, OnCompleteReadData, client);

            }
            catch (Exception ex)
            {
                _serverSocket.Stop();
            }
        }

        private void OnCompleteReadData(IAsyncResult ar)
        {
            TcpClient client = null;
            int byteCount = 0;

            try
            {
                lock (_clients)
                {
                    client = ar.AsyncState as TcpClient;

                    byteCount = client.GetStream().EndRead(ar);

                    if (byteCount == 0)
                    {
                        AcceptText = $"Client disconnected: {client.Client.RemoteEndPoint}";
                        _clients.Remove(client);
                        _players.Remove(client);
                        PlayerCount = _clients.Count;
                        
                        return;
                    }

                    string data = Encoding.UTF8.GetString(_buff, 0, byteCount);
                    Player player = JsonSerializer.Deserialize<Player>(data);

                    AcceptText = $"{player.Name} - {player.PlayerType}";

                    _players.Add(client, player);

                    _buff = new byte[1024];
                    client.GetStream().BeginRead(_buff, 0, _buff.Length, OnCompleteReadData, client);

                    if (_clients.Count >= 2)
                    {
                        InitializePlayers();

                        if (_first != null && _second != null)
                        {
                            InitGameField(_first, _second);
                            SendGameFieldToAllPlayers();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_clients)
                {
                    AcceptText = $"Client disconnected: {client.Client.RemoteEndPoint}";
                    _clients.Remove(client);
                    _players.Remove(client);
                    PlayerCount = _clients.Count;
                }
            }
        }

        private void InitializePlayers()
        {
            if (_players.Count == 2)
            {
                _first = InitializePlayer(PlayerData.X);
                _second = InitializePlayer(PlayerData.O);
            }
        }

        private Player InitializePlayer(PlayerData type)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[_clients[i]].PlayerType == type)
                {
                    Player player = _players[_clients[i]];

                    return player;
                }
            }

            return new Player();
        }

        private void SendGameFieldToAllPlayers()
        {
            if(_gameField == null)
            {
                return;
            }

            foreach (var client in _clients)
            {
                if (client.Client.Connected)
                {
                    SendGameFieldToServer(client, _gameField.Markers);
                }
            }
        }

        private void SendGameFieldToServer(TcpClient client, ObservableCollection<Marker> data)
        {
            if (data == null)
            {
                return;
            }

            lock (_clients)
            {
                string serializeData = JsonSerializer.Serialize(data);

                _buff = new byte[1024];
                _buff = Encoding.UTF8.GetBytes(serializeData);

                client.GetStream().BeginWrite(_buff, 0, _buff.Length, OnCompleteSendData, client);
            }
        }

        private void OnCompleteSendData(IAsyncResult ar)
        {
            try
            {
                TcpClient client = ar.AsyncState as TcpClient;
                client.GetStream().EndWrite(ar);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void InitGameField(Player first, Player second)
        {
            if (_clients.Count == 2)
            {
                if (_gameField == null)
                {
                    _gameField = new GameField(first, second);
                }
            }
        }

        public void WriteLog(string data)
        {
            using (StreamWriter wr = new StreamWriter(DEFAULT_FILE_LOG, true))
            {
                wr.WriteLine($"[LOG][{DateTime.Now}] - {data}");
            }
        }
    }
}
