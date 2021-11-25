namespace DepthCamera.Configuration
{
    public class DepthCameraConfiguration
    {
        public string BackgroundMode { get; set; } = "dynamic"; //static_first_frame, dynamic
        public int CalibrationFramesNumber { get; set; } = 20;
        public int GestureDelay { get; set; } = 1000; //in ms
        public int GestureLength { get; set; } = 2;
        public float JointMinConfidence { get; set; } = 0.75f;
        public float HorizontalGridSize { get; set; } = 0.1f;
        public float VerticalGridSize { get; set; } = 0.1f;
    }
}