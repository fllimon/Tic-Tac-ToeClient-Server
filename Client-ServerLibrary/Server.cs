using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Client_ServerLibrary
{
    public class Server
    {
        private const int SERVER_PORT = 24000;
        private TcpListener _serverSocket;
        private List<TcpClient> _tcpClients;


        public Server()
        {
            _tcpClients = new List<TcpClient>();
        }

        public string ReadData { get; private set; }

        public async void StartServer()
        {
            IPAddress ip = IPAddress.Any;
            _serverSocket = new TcpListener(ip, SERVER_PORT);

            try
            {
                _serverSocket.Start();

                var returnedClient = await _serverSocket.AcceptTcpClientAsync();
                _tcpClients.Add(returnedClient);
                
                ReadClientDataAsync(returnedClient);

            }
            catch (SocketException ex)
            {
                throw;
            }
        }

        private async void ReadClientDataAsync(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;

            try
            {
                stream = client.GetStream();
                reader = new StreamReader(stream);

                char[] buff = new char[1024];

                while (true)
                {
                    int count = await reader.ReadAsync(buff, 0, buff.Length);

                    if (count == 0)
                    {
                        RemoveClient(client);

                        break;
                    }

                    ReadData = new string(buff);
                    Array.Clear(buff, 0, buff.Length);
                }

            }
            catch (EndOfStreamException ex)
            {
                throw;
            }
        }
        private void RemoveClient(TcpClient client)
        {
            if (_tcpClients.Contains(client))
            {
                _tcpClients.Remove(client);
            }
        }
    }
}
