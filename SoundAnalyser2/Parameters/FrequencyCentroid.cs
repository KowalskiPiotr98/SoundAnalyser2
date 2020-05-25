using System;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class FrequencyCentroid
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            SoundfileValidator.ValidateBasics (soundfile);
            return Task.Run (() =>
            {
                var fc = new float [soundfile.GetSamples ().Length / soundfile.FrameLength];
                _ = Parallel.For (0, soundfile.GetSamples ().Length / soundfile.FrameLength, (i) =>
                {
                    var singleFc = soundfile.GetFftPerFrame (i);
                    float sumUp = 0, sumDown = 0;
                    for (int s = 0; s < singleFc.Length; s++)
                    {
                        sumUp += s * singleFc [s];
                        sumDown += singleFc [s];
                    }
                    var cent = sumUp / sumDown;
                    fc [i] = float.IsNaN (cent) || float.IsInfinity (cent) ? 0 : sumUp / sumDown;
                });
                return fc;
            });
        }
    }
}
