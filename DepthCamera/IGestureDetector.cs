using nuitrack;

namespace SensorServer.DepthCamera
{
    interface IGestureDetector
    {
        public bool Update(int userId, CameraController.HandSide handType, HandContent handContent, Joint torso, out Gesture outGesture);
    }
}
