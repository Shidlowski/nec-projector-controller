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

        // Create the connection to the TCP Server
        private Connection conn = Connection.GetInstance();

        // Constructor
        public VirtualProjector() { }

        // Get/Set for powerStatus
        public bool GetPowerStatus() => powerStatus;
        public void SetPowerStatus(bool ps) { this.powerStatus = ps; }

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

        public void AlternateMute() {
            if(powerStatus)
                isMuted = !isMuted;
        }
        public bool GetMuteStatus() => isMuted;

        // Simply prints the state of the printer to the console
        public void PrintState() {
            Console.WriteLine("Power: " + powerStatus);
            Console.WriteLine("Active Input: " + activeInput);
            Console.WriteLine("Volume: " + volume);
            Console.WriteLine("Is Muted: " + isMuted);
        }

    }
}
