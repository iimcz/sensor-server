namespace SensorServer.Configuration
{
    public class PIRConfiguration
    {
        public int PIRPin { get; set; } = 6;
        public int ReadInterval { get; set; } = 100; // in ms
    }
}
