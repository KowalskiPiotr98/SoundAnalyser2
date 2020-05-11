using NAudio.Wave;
using SoundAnalyser2.Parameters;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoundAnalyser2
{
    public class Soundfile
    {
        public int FrameLength { get; private set; }

        private readonly float [] samples;

        public float [] GetSamples () => samples;

        public int SampleRate { get; }

        private readonly string filename;
        private float [] volume;
        private float [] frequencyCentroid;

        public Soundfile (string filename, int frameLength = 256)
        {
            using var wav = new WaveFileReader (filename);
            if (wav.WaveFormat.SampleRate != 22050 || wav.WaveFormat.BitsPerSample != 16 || wav.WaveFormat.Channels != 1)
            {
                throw new FileFormatException (new Uri (filename), "Wav file has to have a sample rate of 22050, 16 bits per sample and mono channel.");
            }
            this.FrameLength = frameLength;
            this.SampleRate = wav.WaveFormat.SampleRate;
            this.filename = filename;
            samples = (new float [wav.SampleCount]);
            wav.ToSampleProvider ().Read (samples, 0, samples.Length);
            RefreshCalculations ();
        }

        public void DrawSignalTimePlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            plot.plt.Clear ();
            plot.plt.Title (filename.Split ('\\').Last (), true);
            plot.plt.XLabel ("sample");
            plot.plt.PlotSignalConst (GetSamples ());
            plot.Render ();
        }

        public void RefreshCalculations (int? frameLength = null)
        {
            if (frameLength.HasValue)
            {
                this.FrameLength = frameLength.Value;
            }
            var taskList = new Task<float []> []
            {
                Volume.Calculate (this),
                FrequencyCentroid.Calculate (this)
            };
            Task.WaitAll (taskList);
            volume = taskList [0].Result;
            frequencyCentroid = taskList [1].Result;
        }

        public void DrawVolumePlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (volume is null || volume.Length == 0)
            {
                throw new InvalidOperationException ($"{nameof (volume)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Volume", true);
            plot.plt.PlotSignalConst (volume);
            plot.Render ();
        }

        public void DrawFrequencyCentroidPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (frequencyCentroid is null || frequencyCentroid.Length == 0)
            {
                throw new InvalidOperationException ($"{nameof (frequencyCentroid)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Frequency centroid", true);
            plot.plt.PlotSignalConst (frequencyCentroid);
            plot.Render ();
        }
    }
}
