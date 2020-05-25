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

        public static bool ShowHiddenParameters { get; set; } = false;
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
                    ParseAllInput (out int frameLength, out int beStart, out int beStop);
                    file = new Soundfile (ofd.FileName, frameLength, beStart, beStop);
                    RedrawAll ();
                }
                catch (FileFormatException err)
                {
                    _ = MessageBox.Show (err.Message, "File error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (ArgumentException err)
                {
                    _ = MessageBox.Show (err.Message, "Loading error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    _ = MessageBox.Show ("Unknown error has occured.", "Catastrophic failure", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }
            }
        }

        private void HelpMenu_Click (object sender, RoutedEventArgs e) => _ = MessageBox.Show ("Plot help is available on plot right-click.\n", "Help", MessageBoxButton.OK, MessageBoxImage.Question);


        private void RedrawAll ()
        {
            if (file is null)
            {
                return;
            }
            file.DrawSignalTimePlot (SignalTimePlot);
            file.DrawVolumePlot (VolumePlot);
            file.DrawFrequencyCentroidPlot (FrequencyCentroidPlot);
            file.DrawEffectiveBandwidthPlot (EffectiveBandwidthPlot);
            file.DrawBandEnergyPlot (BandEnergyPlot);
            file.DrawBandEnergyRatioPlot (BandEnergyRatioPlot);
            file.DrawSpectralFlatnessMeasurePlot (SpectralFlatnessMeasurePlot);
            file.DrawSpectralCrestFactorPlot (SpectralCrestFactorPlot);
        }

        private void Refresh_Click (object sender, RoutedEventArgs e)
        {
            try
            {
                ParseAllInput (out int frameLength, out int beStart, out int beStop);
                if (file is null)
                {
                    return;
                }
                file.RefreshCalculations (frameLength, beStart, beStop);
                RedrawAll ();
            }
            catch (ArgumentException err)
            {
                _ = MessageBox.Show (err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception)
            {
                _ = MessageBox.Show ("Unknown error has occured.", "Catastrophic failure", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void ParseAllInput (out int frameLength, out int beStart, out int beStop)
        {
            frameLength = ParseFrameLength () ?? throw new ArgumentException ("Parsing failed");
            beStart = ParseBEStart () ?? throw new ArgumentException ("Parsing failed");
            beStop = ParseBEStop () ?? throw new ArgumentException ("Parsing failed");
        }

        private int? ParseFrameLength ()
        {
            var frameLengthString = FrameLength;
            if (!Int32.TryParse (frameLengthString.Text, out int frameLength))
            {
                _ = MessageBox.Show ($"Frame length of {frameLengthString.Text} is not valid. Please input a valid integer.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                frameLengthString.Text = file is null ? "256" : file.FrameLength.ToString ();
                return null;
            }
            return frameLength;
        }

        private int? ParseBEStart ()
        {
            var beStartString = BEStart;
            if (!Int32.TryParse (beStartString.Text, out int beStart))
            {
                _ = MessageBox.Show ($"Start frequency of {beStartString.Text} is not valid. Please input a valid integer.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                beStartString.Text = file is null ? "0" : file.BEFrequencyStart.ToString ();
                return null;
            }
            return beStart;
        }

        private int? ParseBEStop ()
        {
            var beStopString = BEStop;
            if (!Int32.TryParse (beStopString.Text, out int beStop))
            {
                _ = MessageBox.Show ($"Start frequency of {beStopString.Text} is not valid. Please input a valid integer.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                beStopString.Text = file is null ? "630" : file.BEFrequencyStop.ToString ();
                return null;
            }
            return beStop;
        }
    }
}
