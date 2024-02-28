namespace SensorServer.Configuration
{
    public enum BackgroundModeType
    {
        Dynamic = 0,
        StaticFirstFrame = 1,
    }

    public enum GestureDetector
    {
        None = 0,
        AngleGestureDetector = 1,
    }

    public enum SkeletonFilter
    {
        None = 0,
        ConeSkeletonFilter = 1,
    }

    public class DepthCameraConfiguration
    {
        public BackgroundModeType BackgroundMode { get; set; } = BackgroundModeType.Dynamic; // static_first_frame, dynamic
        public GestureDetector GestureDetector { get; set; } = GestureDetector.AngleGestureDetector;
        public SkeletonFilter SkeletonFilter { get; set; } = SkeletonFilter.ConeSkeletonFilter;
        public int CalibrationFramesNumber { get; set; } = 20; // Number of frame to calibrate static backround
        public float JointMinConfidence { get; set; } = 0.75f; // Minimal joint confident
        public int MaxUsers { get; set; } = 6;
        public bool SendSkeletonData { get; set; } = false;
        public int BestUserChangeDelay { get; set; } = 1000; //In ms
        public int MinConfidenceDifference { get; set; } = 1;
        public AngleGestureDetectorConfiguration AngleGestureDetector { get; set; } = new();
        public ConeSkeletonFilterConfiguration ConeSkeletonFilter { get; set; } = new();
    }

    public class AngleGestureDetectorConfiguration
    {
        public int GestureDelay { get; set; } = 1000; // In ms
        public int HandPositionQueueLength { get; set; } = 30;
        public double HorizontalGestureLength { get; set; } = 0.2;
        public double VerticalGestureLength { get; set; } = 0.4;
        public int DeadZone { get; set; } = 10;
        public double UserMovement { get; set; } = 0.1;
    }

    public class ConeSkeletonFilterConfiguration
    {
        public double MaximumDistance { get; set; } = 500.0;
        public double MinimumDistance { get; set; } = 100.0;
        public double ConeAngle { get; set; } = 45.0;
    }
}