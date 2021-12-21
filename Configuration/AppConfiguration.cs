namespace DepthCamera.Configuration
{
    public class AppConfiguration
    {
        public DataSenderConfiguration DataSenderConfiguration { get; set; }
        public DepthCameraConfiguration DepthCameraConfiguration { get; set; }
        public AppConfiguration()
        {
            DataSenderConfiguration = new DataSenderConfiguration();
            DepthCameraConfiguration = new DepthCameraConfiguration();
        }
    }
}