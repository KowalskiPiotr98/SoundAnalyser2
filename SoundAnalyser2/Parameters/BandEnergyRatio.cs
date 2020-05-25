using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class BandEnergyRatio
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            if (!MainWindow.ShowHiddenParameters)
            {
                return Task.Run (() => Array.Empty<float> ());
            }
            SoundfileValidator.ValidateBasics (soundfile);
            if (soundfile.GetVolume () is null || soundfile.GetBandEnergy () is null)
            {
                throw new InvalidOperationException ("Volumen and Band Energy have to be calculated before calling this function.");
            }
            return Task.Run (() =>
            {
                var be = soundfile.GetBandEnergy ();
                var vol = soundfile.GetVolume ();
                var count = be.Length;
                var ber = new float [count];
                _ = Parallel.For (0, count, (i) => ber [i] = be [i] / vol [i]);
                return ber;
            });
        }
    }
}
