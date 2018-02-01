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
            Video = 0x06,
            Component = 0x10, // AKA YPbPr
            HDMI1 = 0x1A,
            HDMI2 = 0x1B,
            LAN = 0x20        // AKA HDBaseT
        };

        // List of byte arrays for the commands
        private List<byte[]> commands = new List<byte[]>() {
            new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x02 },   // Power On 
            new byte[] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x03 },   // Power Off 
            new byte[] { 0x02, 0x03, 0x00, 0x00, 0x02, 0x01, 0x00, 0x00 }, // Input change 
            new byte[] { 0x02, 0x12, 0x00, 0x00, 0x00, 0x14 },   // Mute On 
            new byte[] { 0x02, 0x13, 0x00, 0x00, 0x00, 0x15 },   // Mute Off 
            new byte[] { 0x03, 0x10, 0x00, 0x00, 0x05, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00 }, // Volume Adjust
            new byte[] { 0x03, 0x9B, 0x00, 0x00, 0x03, 0x00, 0x00, 0x01, 0xA2 }, // Lamp Information Request 
            new byte[] { 0x00, 0xC0, 0x00, 0x00, 0x00, 0xC0 } // Common information Request
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
            if (powerStatus) {
                this.activeInput = activeInput;
                byte[] message = commands[2];
                switch(activeInput) {
                    case 0: // VGA 1 
                        message[6] = (byte)InputList.VGA1;
                        break;
                    case 1: // VGA 2
                        message[6] = (byte)InputList.VGA2;
                        break;
                    case 2: // Video
                        message[6] = (byte)InputList.Video;
                        break;
                    case 3: // Component
                        message[6] = (byte)InputList.Component;
                        break;
                    case 4: // HDMI 1
                        message[6] = (byte)InputList.HDMI1;
                        break;
                    case 5: // HDMI 2
                        message[6] = (byte)InputList.HDMI2;
                        break;
                    case 6: // LAN/Network
                        message[6] = (byte)InputList.LAN;
                        break;
                    default:
                        break;
                        
                }

                // Set checksum, send message
                message[7] = GetChecksum(message);
                conn.SendMessage(message);
            }
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
        // Slider's set volume function
        public void SliderVolume(int vol) {
            if (powerStatus) {
                volume = vol;
                SetVolume();
            }
        }
        // Set the volume
        private void SetVolume() {
            // Volume byte is found at 8
            byte[] volumeCommand = commands[5];

            // Set the volume byte and checksum to the correct volume
            if (volume >= 0 && !isMuted) {

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
                else {
                    conn.SendMessage(commands[4]);

                    // Since we're unmuting, we want to set the volume in case any changes were made during the mute process
                    SetVolume();
                }
            }
        }
        public bool GetMuteStatus() => isMuted;

        // Get the Lamp Hours
        public int PollLampHours() {
            int LampHours = 0;
            byte[] message = null;

            if (powerStatus) {
                conn.SendMessage(commands[6]);
                
                // Found message passing worked better if I slept the process
                System.Threading.Thread.Sleep(500);
                message = conn.RecieveMessage();
                LampHours = ParseLampPoll(message);
            }
            return LampHours;
        }

        // Get the connection status of the controller to the emulator
        public bool GetConnectionStatus() {
            if (conn.GetConnectionStatus())
                projectorConnected = true;
            else
                projectorConnected = false;

            return projectorConnected;
        }

        // Poll common information request 
        public int[] PollGeneralInfo() {
            byte[] message = null;

            conn.SendMessage(commands[7]);

            // Found message passing worked better if I slept the process
            System.Threading.Thread.Sleep(500);

            message = conn.RecieveMessage();

            return ParseGeneralPoll(message);
        }

        // Convert the Lamp Hour Poll to the Number of Hours
        private int ParseLampPoll(byte[] message) {
            byte[] data = new byte[] { message[8], message[9], message[10], message[11] };
            int LampHours = BitConverter.ToInt32(data, 0) / 3600;

            return LampHours;
        }

        // Parse the general information 
        private int[] ParseGeneralPoll(byte[] message) {

            // If the projector is not connected, send an error
            if (message.Length == 1)
                return new int[] { 0x00, 0x00, 0x00 };

            byte projId = message[5];
            byte power = message[8];
            byte[] input = new byte[] { message[11], message[12] };
            int inputId = 999;

            // Get the correct input based on the pairs
            if(input[0] == 0x01) {
                if (input[1] == 0x01)
                    inputId = 0;
                else if (input[1] == 0x04)
                    inputId = 2;
                else
                    inputId = 4;
            }
            else if(input[0] == 0x02) {
                if (input[1] == 0x01)
                    inputId = 1;
                else if (input[1] == 0x04)
                    inputId = 3;
                else
                    inputId = 5;
            }
            else {
                inputId = 6;
            }

            return new int[] { (ushort)projId, (ushort)power, inputId };
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
