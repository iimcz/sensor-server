using static SensorServer.LightSensor.AnalogPorts;

namespace SensorServer.Configuration
{
    public class LightSensorConfiguration
    {
        public AnalogPort LightSensonPin { get; set; } = AnalogPort.A0;
        public int ReadInterval { get; set; } = 100; // in ms
    }
}
