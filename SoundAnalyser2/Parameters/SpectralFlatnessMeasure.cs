using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class SpectralFlatnessMeasure
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            if (soundfile is null)
            {
                throw new ArgumentNullException (nameof (soundfile));
            }
            var f1 = Math.Max (soundfile.BEFrequencyStart, 0);
            var f2 = Math.Max (soundfile.BEFrequencyStop, 0);
            if (f2 < f1)
            {
                (f1, f2) = (f2, f1);
            }
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
