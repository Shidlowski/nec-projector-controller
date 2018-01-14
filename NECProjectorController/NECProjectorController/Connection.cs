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

        // TCP Services
        byte[] data;
        NetworkStream stream;
        string message;
        TcpClient client;
        IPAddress ipAddress;
        IPEndPoint endpoint;

        // Singleton constructor
        private Connection() {
            data = new byte[1024];
            
            ipAddress = IPAddress.Parse("127.0.0.1");
            endpoint = new IPEndPoint(ipAddress, PORT);

            client = new TcpClient();

            try { 
                client.Connect(endpoint);
                Console.WriteLine("Connected to projector...");

                message = "Hello";
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            catch(System.Net.Sockets.SocketException) {
                Console.WriteLine("Unable to make a connection, the emulator may not be running...");
            }
        }

        // Singleton get instance
        public static Connection GetInstance() {
            if(conn == null) {
                conn = new Connection();
            }
            return conn;
        }
    }
}
