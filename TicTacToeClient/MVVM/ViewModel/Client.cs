using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using TicTacToe.GameLibrary.MVVM.Model;

namespace TicTacToeClient.MVVM.ViewModel
{
    class Client : ClientViewModel
    {
        private TcpClient _client = null;
        private string _port = "24000";
        private string _ip = "localhost";
        private string _acceptText = "Pending...";
        private bool _isPending = false;
        private ObservableCollection<Marker> _gameField = null;

        public Client()
        {
            _client = new TcpClient();
            GameField = new ObservableCollection<Marker>();

            Press = new Command(o =>
            {
                IsPending = true;

                ConnectToServer();
            });

            Exit = new Command(o => 
            {
                _client.Close();

                Environment.Exit(0);
            });
        }

        public ObservableCollection<Marker> GameField
        {
            get 
            {
                return _gameField; 
            }
            set
            {
                _gameField = value;

                OnPropertyChanged();
            }
        }

        public ICommand Press { get; set; }

        public ICommand Exit { get; set; }

        public string ClientIp 
        {
            get
            {
                return _ip;
            }
            set
            {
                _ip = value;

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

        public string Port
        {
            get 
            {
                return _port; 
            }
            set 
            { 
                _port = value; 
                
                OnPropertyChanged(); 
            }
        }

        private async Task ConnectToServer()
        {
            AcceptText = "Try connect to server...";
            int port = int.Parse(_port);

            try
            {
                await _client.ConnectAsync(ClientIp, port);
                
                while (true)
                {
                    ReadDataAsync(_client);
                }
            }
            catch (SocketException ex)
            {

                throw;
            }
        }

        private async Task ReadDataAsync(TcpClient client)
        {
            try
            {
                AcceptText = "Connected! Try Read Data";

                GameField = await JsonSerializer.DeserializeAsync<ObservableCollection<Marker>>(client.GetStream());

                AcceptText = string.Format($"Accept data from server - GameField: {GameField.Count}");

                if (GameField != null)
                {
                    string msg = "Success accep data";

                    await JsonSerializer.SerializeAsync(client.GetStream(), msg);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
