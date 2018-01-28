using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NECProjectorController {
    // The class responsible for handling the TCP Socket/Information passing

    class Connection {
        // Create the singleton object
        private static Connection conn;
        private int PORT = 7142;

        // isConnected Properties notify the MainWindow once
        // the connection status updates
        private bool isConnected;
        public bool IsConnected {
            get { return isConnected; }
            set {
                isConnected = value;
            }
        }

        // TCP Services
        private NetworkStream stream;
        TcpClient client;
        IPAddress ipAddress;
        IPEndPoint endpoint;

        // Singleton constructor
        private Connection() {
            ipAddress = IPAddress.Parse("127.0.0.1");
            endpoint = new IPEndPoint(ipAddress, PORT);

            client = new TcpClient();

            IsConnected = false;
            StartConnection();
        }

        // Singleton get instance
        public static Connection GetInstance() {
            if (conn == null) {
                conn = new Connection();
            }
            return conn;
        }

        // Try and start a new connection
        private void StartConnection() {
            client.BeginConnect(ipAddress, PORT, new AsyncCallback(TCP_Connect), client);
        }

        // Async connect to the server
        private void TCP_Connect(IAsyncResult ar) {
            if (!client.Connected) {
                client.BeginConnect(ipAddress, PORT, new AsyncCallback(TCP_Connect), client);
            } else
                IsConnected = true;
        }

        // Send a message on to the projector
        public void SendMessage(byte[] command) {
            stream = client.GetStream();
            stream.Write(command, offset: 0, size: command.Length);
        }

        // Recieve a TCP Response
        public void RecieveMessage() {

        }
    }
}
