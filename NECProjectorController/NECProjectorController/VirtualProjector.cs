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

        // Byte array commands
        private byte[] powerOn = new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x02 };
        private byte[] powerOff = new byte[] { 0x02, 0x01, 0x00, 0x00, 0x00, 0x03 };

        // Create the connection to the TCP Server
        private Connection conn;

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
                conn.SendMessage(powerOn);
            }
            else {
                conn.SendMessage(powerOff);
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
            if(powerStatus)
                volume += 1;
        }
        public void DecrementVolume() {
            if(powerStatus)
                volume -= 1;
        }
        
        // Handle the muteStatus
        public void AlternateMute() {
            if(powerStatus)
                isMuted = !isMuted;
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
    }
}
