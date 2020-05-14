using System;

namespace SoundAnalyser2.Parameters
{
    internal static class SoundfileValidator
    {
        internal static void ValidateBasics (Soundfile soundfile)
        {
            if (soundfile is null)
            {
                throw new ArgumentNullException (nameof (soundfile));
            }
            if (soundfile.GetSamples () is null)
            {
                throw new InvalidOperationException ($"No samples loaded in {nameof (soundfile)}, load a file first.");
            }
        }
    }
}
