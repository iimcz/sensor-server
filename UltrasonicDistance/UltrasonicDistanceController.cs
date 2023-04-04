using System;
using System.Device.Gpio;
using System.Threading;
using System.Diagnostics;

namespace SensorServer.UltrasonicDistance
{
    class UltrasonicDistanceController : IDisposable
    {
        private bool _finished = false;

        private readonly ProtobufCommunication _dataSender;
        private GpioController _controller;
        private readonly int _pin;

        // TODO: use Stopwatch for these timeouts as well to make them not
        // hardware dependent
        private long _timeout1 = 100000;
        private long _timeout2 = 10000000;
        private Stopwatch _sleepWatch = new Stopwatch();
        private Stopwatch _measureWatch = new Stopwatch();

        public UltrasonicDistanceController(ProtobufCommunication DataSender, int Pin)
        {
            _dataSender = DataSender;
            _pin = Pin;
            _controller = new GpioController();
            _controller.OpenPin(_pin);
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

                // TODO: maybe make this configurable
                Thread.Sleep(500);
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
            _controller.SetPinMode(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.Low);
            MicroSleep(2);
            _controller.Write(_pin, PinValue.High);
            MicroSleep(10);
            _controller.Write(_pin, PinValue.Low);

            _controller.SetPinMode(_pin, PinMode.Input);
            float t0 = ((float)_measureWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0f;
            long count = 0;

            while(count < _timeout1)
            {
                PinValue value = _controller.Read(_pin);
                if(value == PinValue.High)
                {
                    break;
                }
                count++;
                if(count >= _timeout1)
                {
                    return null;
                }
            }

            float t1 = ((float)_measureWatch.ElapsedTicks / Stopwatch.Frequency) * 1000000.0f;
            count = 0;

            while (count < _timeout2)
            {
                PinValue value = _controller.Read(_pin);
                if (value == PinValue.Low)
                {
                    break;
                }
                count++;
                if (count >= _timeout2)
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
