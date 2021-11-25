namespace DepthCamera.Configuration 
{
    public class DataSenderConfiguration
    {
        public string DataSenderType { get; set; } = "console"; //console, protobuf
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
    }
}