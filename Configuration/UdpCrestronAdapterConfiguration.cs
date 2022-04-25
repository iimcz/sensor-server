namespace SensorServer.Configuration
{
    public class UdpCrestronAdapterConfiguration
    {
        public bool Enabled { get; set; }
        public int Port { get; set; } = 41897;

        /// <summary>
        /// How long should the server wait for the Unity app to start listening for TCP connections in ms
        /// </summary>
        public int UnityBootTimeout = 5000;

        /// <summary>
        /// Type of projector control to apply OUTSIDE of the Unity app, so that projectors can be ready before starting the app.
        /// </summary>
        public ProjectorControlType ProjectorControl { get; set; } = ProjectorControlType.None;
    }
}
