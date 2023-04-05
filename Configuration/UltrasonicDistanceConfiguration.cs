namespace SensorServer.Configuration
{
    public class UltrasonicDistanceConfiguration
    {
        public int UltrasonicDistancePin { get; set; } = 5;
        public int ReadInterval { get; set; } = 100; // in ms
        public int Timeout1 { get; set; } = 100000; // in microseconds
        public int Timeout2 { get; set; } = 10000000; // in microseconds
    }
}
