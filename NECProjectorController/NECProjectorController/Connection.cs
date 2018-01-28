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

        // Check the connection and set the IsConnected
        public bool GetConnectionStatus() {

            // Check to see if the client is connected to the server
            try {
                if (client.Client.Poll(0, SelectMode.SelectRead)) {
                    byte[] checkConn = new byte[1];
                    if (client.Client.Receive(checkConn, SocketFlags.Peek) == 0) {
                        client.Client.Disconnect(true);
                        IsConnected = false;
                    } else {
                        IsConnected = true;
                    }
                }
            } catch (System.Net.Sockets.SocketException) {
                client.Client.Disconnect(true); // Disconnect the TCP Client (to allow reconnection)
                IsConnected = false;
            }

            return IsConnected;
        }

        // Send a message on to the projector
        public void SendMessage(byte[] command) {
            Console.Write("Sending: ");
            for (int i = 0; i < command.Length; i++)
                Console.Write(command[i].ToString("X2") + " ");
            Console.WriteLine();

            stream = client.GetStream();
            stream.Write(command, offset: 0, size: command.Length);
        }

        // Recieve a TCP Response from the server
        public byte[] RecieveMessage() {
            byte[] data = new byte[1024];

            stream = client.GetStream();
            stream.Read(data, 0, data.Length);

            // Get the ending index of the message
            int zeroCount = 0;
            int lastNonZero = 0;
            for (int i = 0; i < data.Length; i++) {
                if (data[i] == 0)
                    zeroCount++;
                else {
                    lastNonZero = i;
                    zeroCount = 0;
                }

                if (zeroCount > 5)
                    break;
            }

            // Put the data into a new byte array using the index
            byte[] message = new byte[lastNonZero + 1];
            for (int i = 0; i <= lastNonZero; i++) {
                message[i] = data[i];
            }

            Console.Write("Recieving: ");
            for (int i = 0; i < message.Length; i++)
                Console.Write(message[i].ToString("X2") + " ");
            Console.WriteLine();

            return message;
        }
    }
}
