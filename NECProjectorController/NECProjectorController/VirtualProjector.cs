using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NECProjectorController {
    // This virtual projector object that will be altered by the controller

    class VirtualProjector {

        // Instance Variables
        private bool powerStatus = false;
        private int[] input = new int[] { 0, 1, 2, 3, 4, 5, 6 }; // VGA1, VGA2, Video, Component, HDMI1, HDMI2, LAN/Network
        private int activeInput = 0;
        private bool isMuted = false;
        private int volume = 20;
        private bool projectorConnected;

        // Create the connection to the TCP Server
        private Connection conn;

        // List of input data, used in the input command
        public enum InputList {
            VGA1 = 0x01,
            VGA2 = 0x02,
            Video1 = 0x06,
            YPbPr = 0x10,
            HDMI1 = 0x1A,
            HDMI2 = 0x1B,
            HDBaseT = 0x20
        };

        // List of byte arrays for the commands
        private List<byte[]> commands = new List<byte[]>() {
            new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x02 },   // Power On 0
            new byte[] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x03 },   // Power Off 1
            new byte[] { 0x02, 0x03, 0x00, 0x00, 0x02 /* DATA */ }, // Input change -> append the data in the data slot 2
            new byte[] { 0x02, 0x12, 0x00, 0x00, 0x00, 0x14 },   // Mute On 3
            new byte[] { 0x02, 0x13, 0x00, 0x00, 0x00, 0x15 },   // Mute Off 4
            new byte[] { 0x03, 0x10, 0x00, 0x00, 0x05, 0x05, 0x00, 0x00, /* Volume Byte */0x0A, 0x00, 0x27 }, // Volume Adjust -> append the data in the data slot 5
            new byte[] { 0x03, 0x8C, 0x00, 0x00, 0x00, 0x8F } // Lamp Information Request 6
        };

        // Constructor, creates a new connection
        public VirtualProjector() {
            conn = Connection.GetInstance();

            if(!conn.IsConnected) 
                projectorConnected = false;
            else
                projectorConnected = true;
        }

        // Get/Set for powerStatus
        public bool GetPowerStatus() => powerStatus;
        public void SetPowerStatus(bool ps) {
            this.powerStatus = ps;
            
            // Send the power message
            if(this.powerStatus) {
                conn.SendMessage(commands[0]);
            }
            else {
                conn.SendMessage(commands[1]);
            }
        }

        // Get/Set for activeInput
        public int GetActiveInput() => activeInput;
        public void SetActiveInput(int activeInput) {
            if(powerStatus)
                this.activeInput = activeInput;
        }

        // Get & increment/decrement for Volume
        // Power needs to be on, *can still change volume is the system is muted
        public int GetVolume() => volume;
        public void IncrementVolume() {
            if (powerStatus && volume != 100) {
                volume += 1;
                SetVolume();
            }
        }
        public void DecrementVolume() {
            if (powerStatus && volume != 0) {
                volume -= 1;
                SetVolume();
            }
        }
        // Set the volume
        private void SetVolume() {
            // Volume byte is found at 8
            byte[] volumeCommand = commands[5];

            // Set the volume byte and checksum to the correct volume
            if (volume > 0) {

                // Get volume byte
                string v = volume.ToString("X2");
                byte vol = Convert.ToByte(v, 16);
                volumeCommand[8] = vol;

                // Get checksum byte
                byte check = GetChecksum(volumeCommand);
                volumeCommand[10] = check;
                
                // Send message
                conn.SendMessage(volumeCommand);
            }
        }

        // Handle the muteStatus
        public void AlternateMute() {
            if(powerStatus) { 
                isMuted = !isMuted;
                if (isMuted)
                    conn.SendMessage(commands[3]);
                else
                    conn.SendMessage(commands[4]);
            }
        }
        public bool GetMuteStatus() => isMuted;

        // Get the connection status of the controller to the emulator
        public bool GetConnectionStatus() {
            if (conn.IsConnected)
                projectorConnected = true;
            else
                projectorConnected = false;

            return projectorConnected;
        }

        // Get the checksum
        private byte GetChecksum(byte[] command) {
            byte checksum = 0;
            for (int i = 0; i < command.Length - 1; i++)
                checksum += command[i];
            return checksum;
        }
    }
}
