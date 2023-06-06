namespace SensorServer.Configuration
{
    public class DepthCameraConfiguration
    {
        public string BackgroundMode { get; set; } = "dynamic"; // static_first_frame, dynamic
        public int CalibrationFramesNumber { get; set; } = 20; // Number of frame to calibrate static backround
        public int GestureDelay { get; set; } = 1000; // In ms
        public float JointMinConfidence { get; set; } = 0.75f; // Minimal joint confident
        public int MaxUsers { get; set; } = 6;
        public bool SendSkeletonData { get; set; } = false;
        public int BestUserChangeDelay { get; set; } = 1000; //In ms
        public int MinConfidenceDifference { get; set; } = 1;
        public int HandPositionQueueLength { get; set; } = 30;
        public double HorizontalGestureLength { get; set; } = 0.2;
        public double VerticalGestureLength { get; set; } = 0.4;
        public int DeadZone { get; set; } = 10;
        public double UserMovement { get; set; } = 0.1;
    }
}