using System;
using System.Linq;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class SpectralCrestFactor
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            if (!MainWindow.ShowHiddenParameters)
            {
                return Task.Run (() => Array.Empty<float> ());
            }
            SoundfileValidator.ValidateBasics (soundfile);
            var f1 = soundfile.BEFrequencyStart;
            var f2 = soundfile.BEFrequencyStop;
            return Task.Run (() =>
            {
                var count = soundfile.GetSamples ().Length / soundfile.FrameLength;
                var csf = new float [count];
                _ = Parallel.For (0, count, (i) =>
                {
                    var maxUp = soundfile.GetFftPerFrame (i).Max (f => f * f);
                    double down = 0;
                    for (int s = f1; s < Math.Min (f2 + 1, soundfile.GetFftPerFrame (i).Length); s++)
                    {
                        down += soundfile.GetFftPerFrame (i) [s] * soundfile.GetFftPerFrame (i) [s];
                    }
                    down /= f2 - f1 + 1;
                    var add = (float)(maxUp / down);
                    if (!float.IsNaN (add) && !float.IsInfinity (add))
                    {
                        csf [i] = add;
                    }
                });
                return csf;
            });
        }
    }
}
