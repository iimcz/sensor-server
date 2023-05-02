using SensorServer.Configuration;
using System;
using System.Device.Gpio;
using System.Threading;

namespace SensorServer.PIR
{
    class PIRController : IDisposable
    {
        private readonly ProtobufCommunication _dataSender;
        private readonly PIRConfiguration _configuration;

        private bool _finished = false;
        private PinValue _lastValue = PinValue.Low;

        private GpioController _controller;
        public PIRController(ProtobufCommunication DataSender, PIRConfiguration configuration)
        {
            _dataSender = DataSender;
            _configuration = configuration;
            _controller = new GpioController();
            _controller.OpenPin(_configuration.PIRPin, PinMode.Input);
        }
        public void Dispose()
        {
            _controller.Dispose();
        }
        public void Start()
        {
            PinValue value;
            while (!_finished)
            {
                value = _controller.Read(_configuration.PIRPin);
                if (value == PinValue.High && _lastValue == PinValue.Low)
                {
                    _lastValue = PinValue.High;
                    _dataSender.SendPIRData();
                }
                else if (value == PinValue.Low) 
                {
                    _lastValue = PinValue.Low;
                }
                Thread.Sleep(_configuration.ReadInterval);
            }
        }
        public void Stop()
        {
            _finished = true;
        }
    }
}
