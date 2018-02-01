using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NECProjectorController {

    // Contoller for the main application window

    public partial class MainWindow : Window {

        // Brush converter for hex values
        BrushConverter bc = new BrushConverter();
        private static VirtualProjector vp;
        private DispatcherTimer dispatcherTimer;

        // Used when polling for general info
        // General information [projectorId, powerStatus, InputIndex]
        private int[] generalInformation = new int[3];

        // Initialization
        public MainWindow() {
            InitializeComponent();

            // Create the virtual projector
            vp = new VirtualProjector();

            // Any label/component initialization that is needed
            volumeLabel.Content = "Volume: " + vp.GetVolume();

            // isMuted is Off normally -- set here so that I can see the label in the editor
            mutedLabel.Visibility = Visibility.Hidden;
        }

        // On window load
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Check for a TCP Connection
            CheckConnection();

            // Initialize the timer
            dispatcherTimer = new DispatcherTimer();

            // Check every second for a connection (if there isn't one)
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

        }

        // Timer event handler -- Only serves to check for a connection
        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            CheckConnection();
        }

        // Check the connection status
        private void CheckConnection() {
            // For Debug
            //Console.WriteLine("Connection Status: {0}", vp.GetConnectionStatus());

            if (!vp.GetConnectionStatus()) {
                projectorStatusLabel.Visibility = Visibility.Visible;
                projectorStatusLabel.Content = "No Connection Detected (Start Projector)";
            }
            else {
                if(!vp.GetPowerStatus()) {
                    projectorStatusLabel.Visibility = Visibility.Visible;
                    projectorStatusLabel.Content = "Projector is Off";
                }
                else {
                    projectorStatusLabel.Visibility = Visibility.Hidden;
                }
            }
        }

        // The event handler for the power button
        private void powerButton_Click(object sender, RoutedEventArgs e) {
            if(vp.GetConnectionStatus()) {
                bool ps = vp.GetPowerStatus(); // Temporary holder for the powerStatus
                ps = !ps; // Alternate power status

                // Change the powerStatus in the virtual object
                vp.SetPowerStatus(ps);

                // Set the appearance of the power button (Brush)bc.ConvertFrom("#FFXXXXXX")
                if (ps) {
                    powerButton.Background = (Brush)bc.ConvertFrom("#FF613737");
                    powerButton.Content = "OFF";
                    projectorStatusLabel.Visibility = Visibility.Hidden;

                    // We also want to poll lamp hours when the projector is turned on
                    lampHoursLabel.Content = "Lamp Hours: " + vp.PollLampHours();

                    // Poll for generalInfo when the power is turned on
                    generalInformation = vp.PollGeneralInfo();
                    vp.SetActiveInput(generalInformation[2]);
                    
                    // Since we are setting the active input based on our poll, we also need to update our UI
                    // Change the color of all inputs to the deselected input color
                    foreach (var children in inputWrap.Children) {
                        (children as Button).Background = (Brush)bc.ConvertFrom("#FF41494D");
                    }
                    // Change selected input to active input
                    (inputWrap.Children[generalInformation[2]] as Button).Background = (Brush)bc.ConvertFrom("#FF5F7D8B");

                } else {
                    powerButton.Background = (Brush)bc.ConvertFrom("#FF376150");
                    powerButton.Content = "ON";
                    projectorStatusLabel.Visibility = Visibility.Visible;
                }
            }
        }

        // Handler for any of the input presses
        private void input_Click(object sender, RoutedEventArgs e) {

            // If the projector is powered on, allow control of inputs
            if (vp.GetPowerStatus()) {

                // Get the Name of the pressed input button
                string input = (sender as Button).Content.ToString();

                // Change the color of all inputs to the deselected input color
                foreach (var children in inputWrap.Children) {
                    (children as Button).Background = (Brush)bc.ConvertFrom("#FF41494D");
                }
                // Change selected input to active input
                (sender as Button).Background = (Brush)bc.ConvertFrom("#FF5F7D8B");

                // Check which input option was pressed, and execute correct commands based on
                // selected input
                switch (input) {
                    case "VGA1":
                        vp.SetActiveInput(0);
                        break;
                    case "VGA2":
                        vp.SetActiveInput(1);
                        break;
                    case "Video":
                        vp.SetActiveInput(2);
                        break;
                    case "Component":
                        vp.SetActiveInput(3);
                        break;
                    case "HDMI1":
                        vp.SetActiveInput(4);
                        break;
                    case "HDMI2":
                        vp.SetActiveInput(5);
                        break;
                    case "LAN/Network":
                        vp.SetActiveInput(6);
                        break;
                    default:
                        break;
                }

                // Print active input to the console
                Console.WriteLine(input + ": " + vp.GetActiveInput());
            }
        }

        // Volume Up Handler
        private void volumeUp_Click(object sender, RoutedEventArgs e) {
            vp.IncrementVolume(); // Increment volume, then change the label
            volumeLabel.Content = "Volume: " + vp.GetVolume();
        }

        // Volume Down Handler
        private void volumeDown_Click(object sender, RoutedEventArgs e) {
            vp.DecrementVolume(); // Decrement volume, then change the label
            volumeLabel.Content = "Volume: " + vp.GetVolume();
        }

        // Volume Mute Handler
        private void mute_Click(object sender, RoutedEventArgs e) {
            vp.AlternateMute();
            if (vp.GetMuteStatus())
                mutedLabel.Visibility = Visibility.Visible;
            else
                mutedLabel.Visibility = Visibility.Hidden;
        }

        // Refresh get lamp hours, get general information
        private void refreshButton_Click(object sender, RoutedEventArgs e) {
            
            if (vp.GetPowerStatus())
                // Polling specifically the lamp hours
                lampHoursLabel.Content = "Lamp Hours: " + vp.PollLampHours();
            
            // Poll everything else
            generalInformation = vp.PollGeneralInfo();

            Console.WriteLine("General Information");
            for (int i = 0; i < generalInformation.Length; i++)
                Console.Write(generalInformation[i] + " ");
        }
    }
}
