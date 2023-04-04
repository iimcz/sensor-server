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
        public UdpCrestronAdapterConfiguration UdpCrestronAdapterConfiguration { get; set; }
        public ShellIpwServiceConfiguration ShellIpwServiceConfiguration { get; set; }
        public bool LightSensor { get; set; } = false;
        public bool UltrasonicDistance { get; set; } = false;
        public int UltrasonicDistancePin { get; set; } = 10;

        public AppConfiguration()
        {
            CommunicationConfiguration = new CommunicationConfiguration();
            DepthCameraConfiguration = new DepthCameraConfiguration();
            PjlinkConfiguration = new PjlinkConfiguration();
        }
    }
}