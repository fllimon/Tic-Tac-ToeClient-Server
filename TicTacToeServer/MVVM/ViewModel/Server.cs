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
using System.Threading.Tasks;
using TicTacToe.GameLibrary.MVVM;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace TicTacToeServer.MVVM.ViewModel
{
    class Server : ClientServerViewModel
    {
        private const string DEFAULT_FILE_LOG = "../../log.txt";

        private bool _isPending = false;
        private string _acceptText = "Server started... Avaliable connections!";
        private const int SERVER_PORT = 24000;
        private TcpListener _serverSocket;
        private int _count = 0;
        private List<TcpClient> _tcpClients = null;
        private Player _firstPlayer = null;
        private Player _secondPlayer = null;
        private GameField _gameField = null;
        private char[] _buffer = null;

        public Server()
        {
            _tcpClients = new List<TcpClient>();
            _firstPlayer = new Player();
            _secondPlayer = new Player();
            _buffer = new char[1024];

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
                while (true)
                {
                    var returnedClient = await _serverSocket.AcceptTcpClientAsync();
                    _tcpClients.Add(returnedClient);
                    PlayerCount = _tcpClients.Count;



                    if (_tcpClients.Count == 1)
                    {
                        ReadPlayerData(returnedClient, _firstPlayer);
                    }

                    InitGameField();

                    if (_gameField != null)
                    {
                        SendGameFieldToServer(returnedClient, _gameField.Markers);
                    }
                }
            }
            catch (SocketException ex)
            {
                throw;
            }
        }

        public async Task WriteData(TcpClient client, object data)
        {
            if (data == null)
            {
                return;
            }

            await JsonSerializer.SerializeAsync(client.GetStream(), data);
        }

        private async Task SendGameFieldToServer(TcpClient client, ObservableCollection<Marker> data)
        {
            if (data == null)
            {
                return;
            }

            await JsonSerializer.SerializeAsync(client.GetStream(), data);
        }

        public async Task ReadPlayerData(TcpClient client, Player player)
        {
            StreamReader reader = new StreamReader(client.GetStream());
           
            while (true)
            {
                int count = await reader.ReadAsync(_buffer, 0, _buffer.Length);

                if (count == 0)
                {
                    client.Close();

                    break;
                }

                string data = new string(_buffer,0, count);

                player = JsonSerializer.Deserialize<Player>(data);

                WriteLog(string.Format($"{player.Name} - {player.PlayerType}"));

                AcceptText = player.ToString();
                Array.Clear(_buffer, 0, count);
            }
        }

        private void InitGameField()
        {
            if (_tcpClients.Count > 0)
            {
                _firstPlayer = new Player();
                _secondPlayer = new Player();
                _secondPlayer.PlayerType = PlayerData.X;
                _secondPlayer.PlayerStatus = PlayerStatus.Await;

                if (_gameField == null)
                {
                    _gameField = new GameField(_firstPlayer, _secondPlayer);
                }
            }
        }

        public void WriteLog(string data)
        {
            FileStream fs = new FileStream(DEFAULT_FILE_LOG, FileMode.OpenOrCreate);

            using (StreamWriter wr = new StreamWriter(fs))
            {
                wr.WriteLine(data);
            }
        }
    }
}
