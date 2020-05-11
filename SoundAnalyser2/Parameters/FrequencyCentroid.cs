using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class FrequencyCentroid
    {
        public static Task<float []> Calculate (Soundfile soundfile)
        {
            if (soundfile is null)
            {
                throw new ArgumentNullException (nameof (soundfile));
            }
            return Task.Run (() =>
            {
                var fc = new List<float> ();
                for (int i = 0; i < soundfile.GetSamples ().Length / soundfile.FrameLength; i++)
                {
                    var singleFc = FastFourierTransform.SelectedFrameFFT (soundfile.GetSamples (), soundfile.SampleRate, i, soundfile.FrameLength);
                    float sumUp = 0, sumDown = 0;
                    for (int s = 0; s < singleFc.Length; s++)
                    {
                        sumUp += s * singleFc [s];
                        sumDown += singleFc [s];
                    }
                    var cent = sumUp / sumDown;
                    if (float.IsNaN (cent) || float.IsInfinity (cent))
                    {
                        fc.Add (0);
                    }
                    else
                    {
                        fc.Add (sumUp / sumDown);
                    }
                }
                return fc.ToArray ();
            });
        }
    }
}
