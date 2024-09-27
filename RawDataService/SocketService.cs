using SocketIOClient;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace RawDataService {
    internal class SocketService {

        private static SocketIOClient.SocketIO socket;

        public static async Task Start() {
            socket = new SocketIOClient.SocketIO(ConfigurationManager.AppSettings["SOCKET_HOST"]);
            socket.OnConnected += (sender, e) => {
                Console.WriteLine("connected");
            };

            Console.WriteLine("Trying to connect Socket Server");
            await socket.ConnectAsync(); 
        }
    }
}
