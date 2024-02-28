using nuitrack;

namespace SensorServer.DepthCamera
{
    class NoGestureDetector : IGestureDetector
    {
        public bool Update(int userId, CameraController.HandSide handType, HandContent handContent, Joint torso, out Gesture outGesture)
        {
            outGesture = new();
            return false;
        }
    }
}