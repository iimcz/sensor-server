namespace SensorServer.Configuration
{
    public class AppConfiguration
    {
        public CommunicationConfiguration CommunicationConfiguration { get; set; }
        public DepthCameraConfiguration DepthCameraConfiguration { get; set; }
        public bool ProjectorControl { get; set; } = false; 
        public bool DepthCamera { get; set; } = false;

        public AppConfiguration()
        {
            CommunicationConfiguration = new CommunicationConfiguration();
            DepthCameraConfiguration = new DepthCameraConfiguration();
        }
    }
}