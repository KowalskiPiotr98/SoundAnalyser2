using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoundAnalyser2.Parameters
{
    internal static class FastFourierTransform
    {
#pragma warning disable S125
        //public enum WindowFunction
        // Sections of code should not be commented out
        //{
        //    Rectangular,
        //    Hamming,
        //    Hann
        //}
#pragma warning restore S125 // Might be removed later
        public static float [] FullFFT (float [] samples, int sampleRate)
        {
            if (samples is null)
            {
                throw new ArgumentNullException (nameof (samples));
            }
            return SelectedFrameFFT (samples, sampleRate, 0, samples.Length);
        }

        public static float [] SelectedFrameFFT (float [] samples, int sampleRate, int startFrame, int frameLength)
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
            MathNet.Numerics.IntegralTransforms.Fourier.ForwardReal (/*ApplyWindowFunction*/ (tempFFT), frameLength, MathNet.Numerics.IntegralTransforms.FourierOptions.NoScaling);
            var FFTD = new Dictionary<int, double> ();
            for (int i = 0; i < tempFFT.Length; i += 2)
            {
                var x = (int)(1.0 * sampleRate / frameLength * (i / 2));
                tempFFT [i] = Math.Sqrt (tempFFT [i] * tempFFT [i] + tempFFT [i + 1] * tempFFT [i + 1]);
                if (FFTD.ContainsKey (x) && FFTD [x] < 20 * Math.Log10 (tempFFT [i]))
                {
                    FFTD [x] = 20 * Math.Log10 (tempFFT [i]);
                }
                else if (!FFTD.ContainsKey (x))
                {
                    FFTD.Add (x, 20 * Math.Log10 (tempFFT [i]));
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
#pragma warning disable S125
        //private static double [] ApplyWindowFunction (double [] frame)
        //{
        //    switch (MainWindow.GetWindowFunction ())
        //    {
        //        case WindowFunction.Rectangular:
        //            return frame;
        //        case WindowFunction.Hamming:
        //        {
        //            _ = Parallel.For (0, frame.Length, (i) => frame [i] *= 0.54 + 0.46 * Math.Cos (Math.PI * i / frame.Length));
        //            return frame;
        //        }
        //        case WindowFunction.Hann:
        //        {
        //            _ = Parallel.For (0, frame.Length, (i) => frame [i] *= (1 + Math.Cos (Math.PI * i / frame.Length)) / 2);
        //            return frame;
        //        }
        //        default:
        //            throw new NotImplementedException ();
        //    }
        //}
#pragma warning restore S125 // Might be removed later
    }
}
