using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace TicTacToeServer.MVVM.ViewModel
{
    class Server : ServerViewModel
    {
        private const int SERVER_PORT = 24000;
        private bool _isPending = false;
        private string _acceptText = string.Empty;
        private TcpListener _serverSocket;

        public Server()
        {

            Press = new Command(o =>
            {
                IsPending = true;
                StartServer();
            });


            Exit = new Command(o =>
            {
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
       
        public async  void StartServer()
        {
            IPAddress ip = IPAddress.Any;
            _serverSocket = new TcpListener(ip, SERVER_PORT);
            _serverSocket.Start();

            try
            {

                TcpClient client = await _serverSocket.AcceptTcpClientAsync();
                AcceptText = $"Client Connected: {client.Client.RemoteEndPoint}";

                byte[] buff = new byte[1024];

                int numberReciveBytes = 0;

                while (true)
                {
                    if (client.Connected)
                    {
                        numberReciveBytes = client.Receive(buff);

                        if (numberReciveBytes > 0)
                        {
                            AcceptText = Encoding.UTF8.GetString(buff, 0, numberReciveBytes);
                        }

                        client.Send(buff);
                        Array.Clear(buff, 0, buff.Length);
                        numberReciveBytes = 0;
                    }
                    else
                    {
                        client.Close();
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                throw;
            }
        }

    }
}
