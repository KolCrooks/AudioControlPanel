using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AudioControllerDevice
{
    class AudioPassthrough
    {
        private WaveOutEvent waveOut;
        private BufferedWaveProvider bufferedWaveProvider;

        const string cableName = "VB-Audio";

        public AudioPassthrough()
        {

            var enumerator = new MMDeviceEnumerator();
            MMDevice device;
            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.All))
            {
                if (wasapi.DeviceFriendlyName.StartsWith(cableName))
                {
                    device = wasapi;
                    break;
                }
            }
            WasapiIn soundIn;

            if (device != null)
                soundIn = new WasapiIn(device);

            waveOut = new WaveOutEvent();
            int deviceNum = -1;
            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                var caps = WaveIn.GetCapabilities(n);
                if (caps.ProductName.StartsWith("CABLE"))
                {
                    deviceNum = n;
                    break;
                }
            }
            var waveIn = new WaveInEvent() { DeviceNumber = deviceNum, WaveFormat = new WaveFormat(192000,24,2), BufferMilliseconds = 1};
            waveIn.DataAvailable += OnDataAvailable;

            for (int n = 0; n < WaveOut.DeviceCount; n++)
            {
                var caps = WaveOut.GetCapabilities(n);
                if (caps.ProductName.StartsWith("Speakers"))
                {
                    waveOut.DeviceNumber = n;
                    break;
                }
            }
            bufferedWaveProvider = new BufferedWaveProvider(waveIn.WaveFormat);


            waveOut.Init(bufferedWaveProvider);
            waveIn.StartRecording();

            waveOut.Play();
        }
        private void OnDataAvailable(object sender, WaveInEventArgs args)
        {
            bufferedWaveProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);
        }
    }

}
