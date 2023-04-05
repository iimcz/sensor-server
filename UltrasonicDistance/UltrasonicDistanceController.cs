using System;
using System.Device.Gpio;
using System.Threading;
using System.Diagnostics;
using SensorServer.Configuration;

namespace SensorServer.UltrasonicDistance
{
    class UltrasonicDistanceController : IDisposable
    {
        private bool _finished = false;

        private readonly ProtobufCommunication _dataSender;
        private readonly UltrasonicDistanceConfiguration _configuration;
        private GpioController _controller;

        private Stopwatch _sleepWatch = new Stopwatch();
        private Stopwatch _measureWatch = new Stopwatch();
        private Stopwatch _timeoutWatch = new Stopwatch();

        public UltrasonicDistanceController(ProtobufCommunication DataSender, UltrasonicDistanceConfiguration configuration)
        {
            _dataSender = DataSender;
            _configuration = configuration;
            _controller = new GpioController();
            _controller.OpenPin(_configuration.UltrasonicDistancePin);
        }
        public void Start()
        {
            while (!_finished)
            {
                float? dist = GetDistance();
                if(dist is float distance)
                {
                    _dataSender.SendDistance(distance);
                }

                Thread.Sleep(_configuration.ReadInterval);
            }
        }
        public void Stop()
        {
            _finished = true;
        }
        public void Dispose()
        {
            _controller.Dispose();
        }

        private void MicroSleep(long microseconds)
        {
            var waiter = new SpinWait();
            _sleepWatch.Restart();
            while (((double)_sleepWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0 < microseconds)
            {
                waiter.SpinOnce();
            }
        }

        private float? GetDistance()
        {
            _measureWatch.Restart();
            Console.WriteLine("Starting distance reading.");
            _controller.SetPinMode(_configuration.UltrasonicDistancePin, PinMode.Output);
            _controller.Write(_configuration.UltrasonicDistancePin, PinValue.Low);
            MicroSleep(2);
            _controller.Write(_configuration.UltrasonicDistancePin, PinValue.High);
            MicroSleep(10);
            _controller.Write(_configuration.UltrasonicDistancePin, PinValue.Low);

            _controller.SetPinMode(_configuration.UltrasonicDistancePin, PinMode.Input);
            float t0 = ((float)_measureWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0f;

            _timeoutWatch.Restart();
            while(true)
            {
                PinValue value = _controller.Read(_configuration.UltrasonicDistancePin);
                if(value == PinValue.High)
                {
                    break;
                }
                if(((double)_timeoutWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0 > _configuration.Timeout1)
                {
                    return null;
                }
            }

            float t1 = ((float)_measureWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0f;

            _timeoutWatch.Restart();
            while (true)
            {
                PinValue value = _controller.Read(_configuration.UltrasonicDistancePin);
                if (value == PinValue.Low)
                {
                    break;
                }
                if (((double)_timeoutWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0 > _configuration.Timeout2)
                {
                    return null;
                }
            }

            float t2 = ((float)_measureWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0f;

            float dt = (t1 - t0);
            if (dt > 530)
            {
                return null;
            }

            Console.WriteLine($"Final distance value: {(t2 - t1) / 29 / 2}");

            return (t2 - t1) / 29 / 2;
        }
    }
}
