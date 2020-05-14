using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    internal static class FastFourierTransform
    {
        public static float [] FullFFT (float [] samples, int sampleRate)
        {
            if (samples is null)
            {
                throw new ArgumentNullException (nameof (samples));
            }
            return SelectedFrameFFT (samples, sampleRate, 0, samples.Length);
        }

        public static float [] SelectedFrameFFT (float [] samples, int sampleRate, int startFrame, int frameLength, bool scaleToDb = true)
        {
            if (samples is null)
            {
                throw new ArgumentNullException (nameof (samples));
            }
            if (frameLength < 0 || frameLength > samples.Length)
            {
                frameLength = 256;
            }
            var tempFFT = new double [frameLength % 2 == 0 ? frameLength + 2 : frameLength + 1];
            var startSample = startFrame * frameLength;
            if (startSample < 0 || startSample > samples.Length)
            {
                startSample = 0;
            }
            if (startSample + frameLength > samples.Length)
            {
                frameLength = samples.Length - startSample;
            }
            Array.Copy (samples, startSample, tempFFT, 0, frameLength);
            MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal (tempFFT, frameLength, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);
            var FFTD = new Dictionary<int, double> ();
            for (int i = 0; i < tempFFT.Length; i += 2)
            {
                var x = (int)(1.0 * sampleRate / frameLength * (i / 2));
                tempFFT [i] = Math.Sqrt (tempFFT [i] * tempFFT [i] + tempFFT [i + 1] * tempFFT [i + 1]);
                var y = scaleToDb ? 20 * Math.Log10 (tempFFT [i]) : tempFFT [i];
                if (FFTD.ContainsKey (x) && FFTD [x] < y)
                {
                    FFTD [x] = y;
                }
                else if (!FFTD.ContainsKey (x))
                {
                    FFTD.Add (x, y);
                }
            }
            var FFT = new float [FFTD.Keys.Max () + 1];
            Parallel.ForEach (FFTD, (item) => FFT [item.Key] = (float)item.Value);

            if (FFTD.Keys.Count != FFT.Length)
            {
                FFT = FillFFT (FFT, FFTD);
            }
            FFT [0] = 0;
            return FFT;
        }

        private static float [] FillFFT (float [] FFT, Dictionary<int, double> FFTD)
        {
            _ = Parallel.For (0, FFT.Length, (i) =>
            {
                if (FFTD.ContainsKey (i))
                {
                    return;
                }
                int prev = i - 1, next = i + 1;
                while (!FFTD.ContainsKey (prev) && prev >= 0)
                {
                    prev--;
                }
                if (prev == -1)
                {
                    return;
                }
                while (!FFTD.ContainsKey (next) && next < FFT.Length)
                {
                    next++;
                }
                if (next == FFT.Length)
                {
                    return;
                }
                var fftdPrev = FFTD [prev];
                var step = FFTD [next] - fftdPrev;
                var oneDiff = step / (next - prev);
                FFT [i] = (float)(fftdPrev + oneDiff * (i - prev));
            });
            return FFT;
        }
    }
}
