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
        public string ClientId { get; set; }

        public TcpClient Client { get; set; }

        public byte[] Recive { get; set; }

        public byte[] Send { get; set; }
    }
}
