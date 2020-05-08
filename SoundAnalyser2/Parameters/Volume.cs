using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    public static class Volume
    {

        public static Task<float []> Calculate (Soundfile soundfile)
        {
            if (soundfile is null)
            {
                throw new ArgumentNullException (nameof (soundfile));
            }
            return Task.Run (() =>
            {
                var volume = new List<float> ();
                for (int i = 0; i < soundfile.GetSamples ().Length / soundfile.FrameLength; i++)
                {
                    var singleVolume = FastFourierTransform.SelectedFrameFFT (soundfile.GetSamples (), soundfile.SampleRate, i, soundfile.FrameLength);
                    float sum = 0;
                    foreach (var j in singleVolume)
                    {
                        sum += j * j;
                    }
                    volume.Add (sum / soundfile.FrameLength);
                }
                return volume.ToArray ();
            });
        }
    }
}
