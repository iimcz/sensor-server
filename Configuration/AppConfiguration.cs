namespace SensorServer.Configuration
{
    public enum ProjectorControlType
    {
        None = 0,
        HdmiCec = 1,
        Pjlink = 2
    }

    public class AppConfiguration
    {
        public CommunicationConfiguration CommunicationConfiguration { get; set; }
        public DepthCameraConfiguration DepthCameraConfiguration { get; set; }
        public ProjectorControlType ProjectorControl { get; set; } = ProjectorControlType.None; 
        public PjlinkConfiguration PjlinkConfiguration { get; set; }
        public bool DepthCamera { get; set; } = false;

        public AppConfiguration()
        {
            CommunicationConfiguration = new CommunicationConfiguration();
            DepthCameraConfiguration = new DepthCameraConfiguration();
            PjlinkConfiguration = new PjlinkConfiguration();
        }
    }
}