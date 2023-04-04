using System;
using System.Device.Gpio;
using System.Threading;

namespace SensorServer.UltrasonicDistance
{
    class UltrasonicDistanceController : IDisposable
    {
        private bool _finished = false;

        private readonly ProtobufCommunication _dataSender;
        private GpioController _controller;
        private readonly int _pin;
        private long _timeout1 = 1000;
        private long _timeout2 = 10000;
        public UltrasonicDistanceController(ProtobufCommunication DataSender, int Pin)
        {
            _dataSender = DataSender;
            _pin = Pin;
            _controller = new GpioController();
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
                Thread.Sleep(100);
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

        private float? GetDistance()
        {
            _controller.OpenPin(_pin, PinMode.Output);
            _controller.Write(_pin, PinValue.Low);
            Thread.Sleep(2);
            _controller.Write(_pin, PinValue.High);
            Thread.Sleep(10);
            _controller.Write(_pin, PinValue.Low);

            _controller.OpenPin(_pin, PinMode.Input);
            DateTimeOffset now = DateTime.UtcNow;
            long t0 = now.ToUnixTimeSeconds();
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

            now = DateTime.UtcNow;
            long t1 = now.ToUnixTimeSeconds();
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

            now = DateTime.UtcNow;
            long t2 = now.ToUnixTimeSeconds();

            long dt = (t1 - t0) * 1000000;
            if (dt > 530)
            {
                return null;
            }

            return (t2 - t1) * 1000000 / 29 / 2;
        }
    }
}
