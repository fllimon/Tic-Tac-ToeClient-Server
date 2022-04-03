using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToeServer.MVVM.Model
{
    class ClientModel
    {
        public TcpClient Client { get; set; }

        public byte[] ReciveData { get; set; }

        public byte[] TransferData { get; set; }

        public string ClientId { get; set; }

        public ClientModel(TcpClient client, string id, byte[] recive, byte[] transfer)
        {
            Client = client;
            ReciveData = recive;
            TransferData = transfer;
            ClientId = id;
        }
    }
}
