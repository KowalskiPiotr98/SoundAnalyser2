using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;

namespace SoundAnalyser2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Soundfile file = null;
        public MainWindow () => InitializeComponent ();

        private void LoadWAV_Click (object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "WAV files (*.wav)|*.wav"
            };
            var res = ofd.ShowDialog ();
            if (res.HasValue && res.Value)
            {
                try
                {
                    file = new Soundfile (ofd.FileName);
                }
                catch (FileFormatException err)
                {
                    _ = MessageBox.Show (err.Message, "File error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                RedrawAll ();
            }
        }

        private void HelpMenu_Click (object sender, RoutedEventArgs e) => _ = MessageBox.Show ("Plot help is available on plot right-click.\n", "Help", MessageBoxButton.OK, MessageBoxImage.Question);


        private void RedrawAll ()
        {
            if (file is null)
            {
                return;
            }
            file.DrawSignalTimePlot (ref SignalTimePlot);
            file.DrawVolumePlot (ref VolumePlot);
        }

        private void Refresh_Click (object sender, RoutedEventArgs e)
        {
            var frameLengthString = FrameLength;
            if (!Int32.TryParse (frameLengthString.Text, out int frameLength))
            {
                _ = MessageBox.Show ($"{frameLengthString.Text} is not a valid integer", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                frameLengthString.Text = file is null ? "256" : file.FrameLength.ToString ();
                return;
            }
            if (file is null)
            {
                return;
            }
            file.RefreshCalculations (frameLength);
            RedrawAll ();
        }
    }
}
