using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class SpectralFlatnessMeasure
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            SoundfileValidator.ValidateBasics (soundfile);
            var f1 = soundfile.BEFrequencyStart;
            var f2 = soundfile.BEFrequencyStop;
            return Task.Run (() =>
            {
                var count = soundfile.GetSamples ().Length / soundfile.FrameLength;
                var sfm = new float [count];
                _ = Parallel.For (0, count, (i) =>
                {
                    double up = 1, down = 0;
                    var max = Math.Min (f2 + 1, soundfile.GetFftPerFrame (i).Length);
                    double n = max - f1;
                    for (int s = f1; s < max; s++)
                    {
                        if (soundfile.GetFftPerFrame (i) [s] == 0)
                        {
                            continue;
                        }
                        up *= Math.Pow (soundfile.GetFftPerFrame (i) [s], 2.0 / n);
                        down += soundfile.GetFftPerFrame (i) [s] * soundfile.GetFftPerFrame (i) [s];
                    }
                    down /= n;
                    var add = (float)(up / down);
                    if (!float.IsNaN (add) && !float.IsInfinity (add))
                    {
                        sfm [i] = add;
                    }
                });
                return sfm;
            });
        }
    }
}
