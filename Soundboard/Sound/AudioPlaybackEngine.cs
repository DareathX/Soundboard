using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundboard.Sound
{
    class AudioPlaybackEngine : IDisposable
    {
        public readonly IWavePlayer outputDevice;
        public static AudioFileReader fileReaderInput;
        private readonly MixingSampleProvider mixer;
        public float Volume
        {
            get
            {
                return 1;
            }
            set
            {
                outputDevice.Volume = value;
            }
        }

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount))
            {
                ReadFully = true
            };
            outputDevice.Init(mixer);
        }

        public void PlaySound(VarispeedSampleProvider input)
        {
            AddMixerInput(new AutoDisposeFileReader(input));
            if (outputDevice.PlaybackState != PlaybackState.Playing)
            {
                outputDevice.Play();
            }
        }

        public void StopSound()
        {
            mixer.RemoveAllMixerInputs();
            outputDevice.Stop();
            outputDevice.Dispose();
            Instance.Dispose();
            outputDevice.Init(mixer);
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }
            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        private void AddMixerInput(ISampleProvider input)
        {
            VolumeSampleProvider volumeSample = new VolumeSampleProvider(ConvertToRightChannelCount(input))
            {
                Volume = this.Volume
            };
            mixer.AddMixerInput(volumeSample);
        }

        public void Dispose()
        {
            mixer.RemoveAllMixerInputs();
            outputDevice.Stop();
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }
}
