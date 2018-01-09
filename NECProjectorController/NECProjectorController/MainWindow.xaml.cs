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

    // activeInput color: #FF5F7D8B
    // inactiveInput color: #FF41494D

    public partial class MainWindow : Window {
        // Brush converter for hex values
        BrushConverter bc = new BrushConverter();

        private VirtualProjector vp = new VirtualProjector();

        // Initialization
        public MainWindow() {
            InitializeComponent();

            // Any label/component initialization that is needed
            volumeLabel.Content = "Volume: " + vp.GetVolume();
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
                powerLabel.Content = "Power: On";
            } else {
                powerButton.Background = (Brush)bc.ConvertFrom("#FF376150");
                powerButton.Content = "ON";
                powerLabel.Content = "Power: Off";
            }
        }

        // Handler for any of the input presses
        private void input_Click(object sender, RoutedEventArgs e) {
            
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

    }
}
