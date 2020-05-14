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
        public int BEFrequencyStart { get; private set; }
        public int BEFrequencyStop { get; private set; }

        private readonly float [] samples;


        public int SampleRate { get; }

        private readonly string filename;
        private float [] volume;
        private float [] frequencyCentroid;
        private float [] effectiveBandwidth;
        private float [] bandEnergy;
        private float [] bandEnergyRatio;
        private float [] spectralFlatnessMeasure;
        private float [] spectralCrestFactor;
        private float [][] fftPerFrame;

        public float [] GetSamples () => samples;
        public float [] GetVolume () => volume;
        public float [] GetFrequencyCentroid () => frequencyCentroid;
        public float [] GetEffectiveBandwidth () => effectiveBandwidth;
        public float [] GetBandEnergy () => bandEnergy;
        public float [] GetBandEnergyRatio () => bandEnergyRatio;
        public float [] GetSpectralFlatnessMeasure () => spectralFlatnessMeasure;
        public float [] GetSpectralCrestFactor () => spectralCrestFactor;
        internal float [] GetFftPerFrame (int n) => fftPerFrame [n] ?? Array.Empty<float> ();

        public Soundfile (string filename, int frameLength = 256, int beStart = 0, int beStop = 630)
        {
            using var wav = new WaveFileReader (filename);
            if (wav.WaveFormat.SampleRate != 22050 || wav.WaveFormat.BitsPerSample != 16 || wav.WaveFormat.Channels != 1)
            {
                throw new FileFormatException (new Uri (filename), "Wav file has to have a sample rate of 22050, 16 bits per sample and mono channel.");
            }
            this.FrameLength = frameLength;
            this.SampleRate = wav.WaveFormat.SampleRate;
            this.filename = filename;
            this.BEFrequencyStart = beStart;
            this.BEFrequencyStop = beStop;
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

        public void RefreshCalculations (int? frameLength = null, int? beStart = null, int? beStop = null)
        {
            if (frameLength.HasValue)
            {
                this.FrameLength = frameLength.Value;
            }
            if (beStart.HasValue)
            {
                this.BEFrequencyStart = beStart.Value;
            }
            if (beStop.HasValue)
            {
                this.BEFrequencyStop = beStop.Value;
            }
            BEFrequencyStart = Math.Max (BEFrequencyStart, 0);
            BEFrequencyStop = Math.Max (BEFrequencyStop, 0);
            if (BEFrequencyStop < BEFrequencyStart)
            {
                (BEFrequencyStart, BEFrequencyStop) = (BEFrequencyStop, BEFrequencyStart);
            }
            //Eagerly calculate FFT per frame as it's needed in every parameter
            fftPerFrame = new float [samples.Length / FrameLength] [];
            _ = Parallel.For (0, samples.Length / FrameLength, (i) => fftPerFrame [i] = FastFourierTransform.SelectedFrameFFT (GetSamples (), SampleRate, i, FrameLength, false));
            //First batch of tasks - independent from one another
            var taskList = new Task<float []> []
            {
                Volume.Calculate (this),
                FrequencyCentroid.Calculate (this),
                BandEnergy.Calculate (this),
                SpectralFlatnessMeasure.Calculate (this),
                SpectralCrestFactor.Calculate (this)
            };
            Task.WaitAll (taskList);
            volume = taskList [0].Result;
            frequencyCentroid = taskList [1].Result;
            bandEnergy = taskList [2].Result;
            spectralFlatnessMeasure = taskList [3].Result;
            spectralCrestFactor = taskList [4].Result;
            //Second batch - previously calculated parameters are needed here
            taskList = new Task<float []> []
            {
                EffectiveBandwidth.Calculate (this),
                BandEnergyRatio.Calculate (this)
            };
            Task.WaitAll (taskList);
            effectiveBandwidth = taskList [0].Result;
            bandEnergyRatio = taskList [1].Result;
        }

        public void DrawVolumePlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (volume is null)
            {
                throw new InvalidOperationException ($"{nameof (volume)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Volume", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.PlotSignalConst (volume);
            plot.Render ();
        }

        public void DrawFrequencyCentroidPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (frequencyCentroid is null)
            {
                throw new InvalidOperationException ($"{nameof (frequencyCentroid)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Frequency centroid", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.YLabel ("Hz", enable: true);
            plot.plt.PlotSignalConst (frequencyCentroid);
            plot.Render ();
        }

        public void DrawEffectiveBandwidthPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (effectiveBandwidth is null)
            {
                throw new InvalidOperationException ($"{nameof (effectiveBandwidth)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Effective bandwidth", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.YLabel ("Hz", enable: false);
            plot.plt.PlotSignalConst (effectiveBandwidth);
            plot.Render ();
        }

        public void DrawBandEnergyPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (bandEnergy is null)
            {
                throw new InvalidOperationException ($"{nameof (bandEnergy)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Band energy", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.YLabel ("Hz", enable: false);
            plot.plt.PlotSignalConst (bandEnergy);
            plot.Render ();
        }

        public void DrawBandEnergyRatioPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (bandEnergyRatio is null)
            {
                throw new InvalidOperationException ($"{nameof (bandEnergyRatio)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Band energy ratio", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.PlotSignalConst (bandEnergyRatio);
            plot.Render ();
        }

        public void DrawSpectralFlatnessMeasurePlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (spectralFlatnessMeasure is null)
            {
                throw new InvalidOperationException ($"{nameof (spectralFlatnessMeasure)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Spectral flatness measure", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.PlotSignalConst (spectralFlatnessMeasure);
            plot.Render ();
        }

        public void DrawSpectralCrestFactorPlot (ScottPlot.WpfPlot plot)
        {
            if (plot is null)
            {
                throw new ArgumentNullException (nameof (plot));
            }
            if (spectralCrestFactor is null)
            {
                throw new InvalidOperationException ($"{nameof (spectralCrestFactor)} must be calculated before being drawn. Call {nameof (RefreshCalculations)} before drawing.");
            }
            plot.plt.Clear ();
            plot.plt.Title ("Spectral crest factor", true);
            plot.plt.XLabel ("Frame", enable: true);
            plot.plt.PlotSignalConst (spectralCrestFactor);
            plot.Render ();
        }
    }
}
