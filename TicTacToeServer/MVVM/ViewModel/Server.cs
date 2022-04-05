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
using TicTacToeServer.MVVM.Model;

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
        private int _updateStep = 0;
        private List<ClientModel> _clients = null;
        private Dictionary<TcpClient, Player> _players;
        private Player _first = null;
        private Player _second = null;
        private GameField _gameField = null;
        private byte[] _bufferSend = null;
        private byte[] _bufferRecive = null;
        
        public Server()
        {
            _clients = new List<ClientModel>(2);
            _players = new Dictionary<TcpClient,Player>();
            _bufferRecive = new byte[1024];
            _bufferSend = new byte[1024];
            
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

                if (_bufferSend != null)
                {
                    Array.Clear(_bufferSend, 0, _bufferSend.Length);    
                }

                if (_bufferRecive != null)
                {
                    Array.Clear(_bufferRecive, 0, _bufferRecive.Length);
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
            ClientModel model = null;

            try
            {
                TcpClient client = listener.EndAcceptTcpClient(ar);
                AcceptText = "Client connected...";

                listener.BeginAcceptTcpClient(OnAcceptClient, listener);

                lock (_clients)
                {
                    _clients.Add(model = new ClientModel { Client = client, ClientId = client.Client.RemoteEndPoint.ToString(), Recive = new byte[1024], Send = new byte[1024]});
                    PlayerCount = _clients.Count;
                }

                client.GetStream().BeginRead(model.Recive, 0, model.Recive.Length, OnCompleteReadData, client);
            }
            catch (Exception ex)
            {
                _serverSocket.Stop();
            }
        }

        private void OnCompleteReadData(IAsyncResult ar)
        {
            TcpClient client = null;
            ClientModel model = null;
            int byteCount = 0;
            
            try
            {
                lock (_clients)
                {
                    client = ar.AsyncState as TcpClient;
                    model = _clients.Find(x => x.ClientId == client.Client.RemoteEndPoint.ToString());

                    byteCount = client.GetStream().EndRead(ar);

                    if (byteCount == 0)
                    {
                        WriteLog($"Player - {_players.GetValueOrDefault(model.Client).PlayerType} has disconnected! {model.ClientId}");
                        _clients.Remove(model);
                        AcceptText = $"Player - {_players.GetValueOrDefault(model.Client).PlayerType} has disconnected!";
                        _players.Remove(model.Client);
                        PlayerCount = _clients.Count;

                        return;
                    }

                    string data = Encoding.UTF8.GetString(model.Recive, 0, byteCount);
                    DeserializePlayerData(model, data);

                    if (_updateStep > 0)
                    {
                        SendDataToClient(model, _gameField.Markers);

                        _players[model.Client] = _players[model.Client].PlayerType == PlayerData.X ? _gameField.PlayerOne : _gameField.PlayerTwo;
                        SendDataToClient(model, _players[model.Client]);


                        SendPlayerDataToOther(_players[model.Client]);
                        SendGameFieldToOther(model);
                    }

                    model.Recive = new byte[1024];
                    model.Client.GetStream().BeginRead(model.Recive, 0, model.Recive.Length, OnCompleteReadData, model.Client);
                }

                if (_gameField == null)
                {
                    if (_clients.Count >= 2)
                    {
                        InitializePlayers();

                        if (_first != null && _second != null)
                        {
                            InitGameField(_first, _second);
                            SendGameFieldToAll();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lock (_clients)
                {
                    AcceptText = $"Client disconnected: {model.Client.Client.RemoteEndPoint}";
                    _clients.Remove(model);
                    _players.Remove(model.Client);
                    PlayerCount = _clients.Count;
                }
            }
        }

        private void DeserializePlayerData(ClientModel model, string data)
        {
            try
            {
                if (!string.IsNullOrEmpty(data))
                {
                    if (data.Contains("PlayerMarker"))
                    {
                        UpdateMarkerInGameField(data);
                    }
                    else
                    {
                        Player player = JsonSerializer.Deserialize<Player>(data);

                        if (player != null)
                        {
                            AcceptText = $"{player.Name} - {player.PlayerType}";
                            _players.Add(model.Client, player);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }

        private void UpdateMarkerInGameField(string data)
        {
            Marker marker = JsonSerializer.Deserialize<Marker>(data);
            
            if (marker != null)
            {
                _gameField.PlayerChoise(marker);
                _updateStep++;
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
                if (_players[_clients[i].Client].PlayerType == type)
                {
                    Player player = _players[_clients[i].Client];

                    return player;
                }
            }

            return new Player();
        }

        private void SendGameFieldToAll()
        {
            if(_gameField == null)
            {
                return;
            }

            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].Client.Connected)
                {
                    SendDataToClient(_clients[i], _gameField.Markers);
                }
            }
        }

        private void SendPlayerDataToOther(Player player)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_players[_clients[i].Client].PlayerType != player.PlayerType)
                {
                    SendDataToClient(_clients[i], _players[_clients[i].Client]);
                }
            }
        }

        private void SendGameFieldToOther(ClientModel model)
        {
            for (int i = 0; i < _clients.Count; i++)
            {
                if (_clients[i].ClientId != model.ClientId)
                {
                    SendDataToClient(_clients[i], _gameField.Markers);
                }
            }
        }

        private void SendDataToClient(ClientModel model, object data)
        {
            if (data == null)
            {
                return;
            }

            string serializeData = JsonSerializer.Serialize(data);

            model.Send = new byte[1024];
            model.Send = Encoding.UTF8.GetBytes(serializeData);
            model.Client.GetStream().BeginWrite(model.Send, 0, model.Send.Length, OnCompleteSendData, model.Client);
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
