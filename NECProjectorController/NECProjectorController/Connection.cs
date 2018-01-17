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
        private byte[] data;
        private NetworkStream stream;
        TcpClient client;
        IPAddress ipAddress;
        IPEndPoint endpoint;

        // Singleton constructor
        private Connection() {
            ipAddress = IPAddress.Parse("127.0.0.1");
            endpoint = new IPEndPoint(ipAddress, PORT);

            client = new TcpClient();

            // Need a new thread to look for connections
            System.Threading.Thread connThread;
            connThread = new System.Threading.Thread(new System.Threading.ThreadStart(StartConnection));
            connThread.Start();
        }

        // Singleton get instance
        public static Connection GetInstance() {
            if (conn == null) {
                conn = new Connection();
            }
            return conn;
        }

        // Called through the new thread, runs until a projector is connected
        private void StartConnection() {
            isConnected = false;
            while (!isConnected) {
                try {
                    client.Connect(endpoint);
                    Console.WriteLine("Connected to projector...");
                    IsConnected = true;
                } catch (System.Net.Sockets.SocketException) {
                    Console.WriteLine("Searching for connection...");
                }
            }
        }

        // Send a message on to the projector
        public void SendMessage(byte[] command) {
            stream = client.GetStream();
            stream.Write(command, 0, command.Length);
        }
    }
}
