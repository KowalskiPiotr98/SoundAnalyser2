using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class EffectiveBandwidth
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            SoundfileValidator.ValidateBasics (soundfile);
            var centroid = soundfile.GetFrequencyCentroid ();
            if (centroid is null || centroid.Length == 0)
            {
                throw new InvalidOperationException ($"{nameof (soundfile)} does not have frequency centroid calculated. Calculate it first.");
            }
            return Task.Run (() =>
            {
                var count = soundfile.GetSamples ().Length / soundfile.FrameLength;
                var bandwidth = new float [count];
                _ = Parallel.For (0, count, (i) =>
                {
                    var singleFrame = soundfile.GetFftPerFrame (i);
                    float sumUp = 0, sumDown = 0;
                    for (int s = 0; s < singleFrame.Length; s++)
                    {
                        sumUp += (s - centroid [i]) * (s - centroid [i]) * singleFrame [s] * singleFrame [s];
                        sumDown += singleFrame [s] * singleFrame [s];
                    }
                    var band = sumUp / sumDown;
                    bandwidth [i] = float.IsInfinity (band) || float.IsNaN (band) ? 0 : sumUp / sumDown;
                });
                return bandwidth;
            });
        }
    }
}
