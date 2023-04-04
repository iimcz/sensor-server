using System;
using System.Device.I2c;
using System.Threading;

namespace SensorServer.LightSensor
{
    class LightSensorController : IDisposable
    {
        private bool _finished = false;

        private const int _busId = 1;
        private readonly I2cConnectionSettings _i2cConnectionSettings;
        private readonly I2cDevice _i2cDevice;
        private readonly AnalogPorts _analogPorts;

        private readonly ProtobufCommunication _dataSender;
        public LightSensorController(ProtobufCommunication DataSender)
        {
            _dataSender = DataSender;
            _i2cConnectionSettings = new(_busId, AnalogPorts.DefaultI2cAddress);
			_i2cDevice = I2cDevice.Create(_i2cConnectionSettings);
            _analogPorts = new AnalogPorts(_i2cDevice);
		}
        public void Start()
        {
            while (!_finished)
            {
                double value = _analogPorts.Read(AnalogPorts.AnalogPort.A0);
                _dataSender.SendLightValue(value);
                Thread.Sleep(100);
            }
        }
        public void Stop()
        {
            _finished = true;
        }

        public void Dispose()
        {
            _i2cDevice.Dispose();
            _analogPorts.Dispose();
        }
    }
}
