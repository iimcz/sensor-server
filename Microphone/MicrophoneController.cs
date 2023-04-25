using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.Extensions.Creative.EnumerateAll;

namespace SensorServer.Microphone
{
    class MicrophoneController
    {
        private IEnumerable<string> _devices;
        private IEnumerable<string> _allDevices;
        private IEnumerable<string> _list;
        private List<string> _deviceNames;
        private Thread[] _threads;

        private bool _finished = false;

        private readonly ProtobufCommunication _dataSender;
        public MicrophoneController(ProtobufCommunication DataSender)
        {
            _dataSender = DataSender;

            _devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);
            Console.WriteLine($"Devices: {string.Join(", ", _devices)}");

            _allDevices = EnumerateAll.GetStringList(GetEnumerateAllContextStringList.AllDevicesSpecifier);
            Console.WriteLine($"All Devices: {string.Join(", ", _allDevices)}");

            Console.WriteLine("Available capture devices: ");
            _list = ALC.GetStringList(GetEnumerationStringList.CaptureDeviceSpecifier);
            _deviceNames = new List<string>();
            foreach (var item in _list)
            {
                Console.WriteLine("  " + item);
                if (item.StartsWith("FIFINE"))
                {
                    _deviceNames.Add(item);
                }
            }
        }
        public void Stop()
        {
            _finished = true;
            foreach (Thread t in _threads)
            {
                t.Join();
            }
        }
        public void Start()
        {
            float[] peaks = new float[_deviceNames.Count];
            _threads = new Thread[_deviceNames.Count];

            object[] locks = new object[_deviceNames.Count];
            Array.Fill(locks, new object());

            for (int i = 0; i < _deviceNames.Count; i++)
            {
                int index = i;
                _threads[i] = new Thread(() =>
                {
                    short[] buffer = new short[44100];
                    Console.WriteLine($"i: {index}");
                    Console.WriteLine($"Opening device \"{_deviceNames[index]}\" for capture.");

                    ALCaptureDevice device = ALC.CaptureOpenDevice(_deviceNames[index], 44100, ALFormat.Mono16, buffer.Length);
                    {
                        ALC.CaptureStart(device);

                        while (!_finished)
                        {
                            int samples = ALC.GetAvailableSamples(device);
                            ALC.CaptureSamples(device, buffer, samples);
                            short max = buffer.Max();
                            short min = buffer.Min();
                            float peak = (max - min) / 65536.0f;

                            lock (locks[index])
                            {
                                peaks[index] = peak;
                            }

                            for (int j = 0; j < buffer.Length; j++)
                            {
                                buffer[j] = 0;
                            }

                            Thread.Sleep(100);
                        }
                    }
                });
                _threads[i].Start();
            }

            while (!_finished)
            {
                Console.Write("Peaks: ");
                for (int i = 0; i < peaks.Length; i++)
                {
                    lock (locks[i])
                    {
                        Console.Write($"{peaks[i]} ");
                        _dataSender.SendMicrophonePeak(peaks[i], i);
                    }
                }
                Console.WriteLine();
                Thread.Sleep(100);
            }
        }
    }
}
