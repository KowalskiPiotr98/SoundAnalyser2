using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class BandEnergy
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
                var energy = new float [count];
                _ = Parallel.For (0, count, (i) =>
                {
                    float sumUp = 0, sumDown = 0;
                    for (int s = f1; s < Math.Min (f2 + 1, soundfile.GetFftPerFrame (i).Length); s++)
                    {
                        sumUp += soundfile.GetFftPerFrame (i) [s] * soundfile.GetFftPerFrame (i) [s];
                        sumDown++;
                    }
                    energy [i] = sumUp / sumDown;
                });
                return energy;
            });
        }
    }
}
