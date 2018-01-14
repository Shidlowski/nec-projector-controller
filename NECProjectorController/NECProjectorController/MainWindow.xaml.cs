using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace NECProjectorController {

    // Contoller for the main application window

    public partial class MainWindow : Window {
        // Brush converter for hex values
        BrushConverter bc = new BrushConverter();

        private VirtualProjector vp = new VirtualProjector();

        // Initialization
        public MainWindow() {
            InitializeComponent();

            // Any label/component initialization that is needed
            volumeLabel.Content = "Volume: " + vp.GetVolume();

            if (!vp.GetPowerStatus())
                projectorStatusLabel.Visibility = Visibility.Visible;

            // Make the mutedLabel hidden on startup, could do this with XAML, but I needed to see it in the editor
            mutedLabel.Visibility = Visibility.Hidden;
        }

        // The event handler for the power button
        private void powerButton_Click(object sender, RoutedEventArgs e) {
            bool ps = vp.GetPowerStatus(); // Temporary holder for the powerStatus
            ps = !ps; // Alternate power status

            vp.SetPowerStatus(ps); // Change the powerStatus in the virtual object

            // Set the appearance of the power button (Brush)bc.ConvertFrom("#FFXXXXXX")
            if (ps) {
                powerButton.Background = (Brush)bc.ConvertFrom("#FF613737");
                powerButton.Content = "OFF";
                projectorStatusLabel.Visibility = Visibility.Hidden;
            } else {
                powerButton.Background = (Brush)bc.ConvertFrom("#FF376150");
                powerButton.Content = "ON";
                projectorStatusLabel.Visibility = Visibility.Visible;
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
    }
}
