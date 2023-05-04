namespace SensorServer.Configuration
{
    public class DepthCameraConfiguration
    {
        public string BackgroundMode { get; set; } = "dynamic"; // static_first_frame, dynamic
        public int CalibrationFramesNumber { get; set; } = 20; // Number of frame to calibrate static backround
        public int GestureDelay { get; set; } = 1000; // In ms
        public int GestureLength { get; set; } = 2; 
        public float JointMinConfidence { get; set; } = 0.75f; // Minimal joint confident
        public float HorizontalGridSize { get; set; } = 0.1f; 
        public float VerticalGridSize { get; set; } = 0.1f;
        public int MaxUsers { get; set; } = 6;
        public bool SendSkeletonData { get; set; } = false;
    }
}