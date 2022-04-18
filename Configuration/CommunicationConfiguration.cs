namespace SensorServer.Configuration 
{
    public class CommunicationConfiguration
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
        public int ManagementPort { get; set; } = 5001;
    }
}