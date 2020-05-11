using System;
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
                var volume = new float [soundfile.GetSamples ().Length / soundfile.FrameLength];
                _ = Parallel.For (0, soundfile.GetSamples ().Length / soundfile.FrameLength, (i) =>
                {
                    var singleVolume = soundfile.GetFftPerFrame (i);
                    float sum = 0;
                    foreach (var j in singleVolume)
                    {
                        sum += j * j;
                    }
                    volume [i] = (sum / soundfile.FrameLength);
                });
                return volume;
            });
        }
    }
}
