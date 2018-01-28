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
                StartConnection();
            } else {
                IsConnected = true;
                while (true) {
                    if(!GetConnectionStatus()) {
                        StartConnection();
                        break;
                    }
                }
            }
        }

        // Send a message on to the projector
        public void SendMessage(byte[] command) {
            stream = client.GetStream();
            stream.Write(command, offset: 0, size: command.Length);
        }

        // Check the connection and set the IsConnected
        public bool GetConnectionStatus() {

            // Check to see if the client is connected to the server
            try {
                if (client.Client.Poll(0, SelectMode.SelectRead)) {
                    byte[] checkConn = new byte[1];
                    if (client.Client.Receive(checkConn, SocketFlags.Peek) == 0) {
                        IsConnected = false;
                    } else {
                        IsConnected = true;
                    }
                }
            }
            catch(System.Net.Sockets.SocketException) {
                client.Client.Disconnect(true); // Disconnect the TCP Client (to allow reconnection)
                IsConnected = false;
            }

            return IsConnected;
        }

        // Recieve a TCP Response
        public void RecieveMessage() {
            
        }
    }
}
