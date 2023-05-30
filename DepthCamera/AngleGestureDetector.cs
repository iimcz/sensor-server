using nuitrack;
using System;

namespace SensorServer.DepthCamera
{
    class AngleGestureDetector : IGestureDetector
    {
        private HandContent _lastPosition;
        private double _minGestureDistance = 0.2;
        private int _minGestureDelay = 1000; //ms
        private DateTimeOffset _lastGesture;
        public AngleGestureDetector()
        {
            _lastGesture = DateTime.Now;
        }
        public bool Update(int userId, CameraController.HandSide handType, HandContent handContent, out Gesture outGesture)
        {
            Gesture gesture = new Gesture();
            gesture.UserID = userId;
            DateTimeOffset now = DateTime.Now;
            TimeSpan duration = now - _lastGesture;
            if (duration.TotalMilliseconds > _minGestureDelay)
            {
                double distance = GetDistance(handContent);
                if (distance > _minGestureDistance)
                {
                    double angle = RadianToDegree(GetAngle(handContent));
                    if (angle >= -45 && angle < 45)
                    {
                        Console.WriteLine("Swipe right");
                        _lastGesture = now;
                        gesture.Type = GestureType.GestureSwipeRight;
                        outGesture = gesture;
                        return true;
                    }
                    else if (angle >= 45 && angle < 135)
                    {
                        Console.WriteLine("Swipe up");
                        _lastGesture = now;
                        gesture.Type = GestureType.GestureSwipeUp;
                        outGesture = gesture;
                        return true;
                    }
                    else if (angle >= 135 || angle < -135)
                    {
                        Console.WriteLine("Swipe left");
                        _lastGesture = now;
                        gesture.Type = GestureType.GestureSwipeLeft;
                        outGesture = gesture;
                        return true;
                    }
                    else if (angle >= -135 && angle < -45)
                    {
                        Console.WriteLine("Swipe down");
                        _lastGesture = now;
                        gesture.Type = GestureType.GestureSwipeDown;
                        outGesture = gesture;
                        return true;
                    }
                }
            }
            _lastPosition = handContent;

            outGesture = gesture;
            return false;
        }

        private double GetDistance(HandContent handContent)
        {
            return Math.Sqrt(Math.Pow(handContent.X - _lastPosition.X, 2) + Math.Pow(handContent.Y - _lastPosition.Y, 2));
        }
        private double GetAngle(HandContent handContent)
        {
            double ab_x = _lastPosition.X - handContent.X;
            double ab_y = _lastPosition.Y - handContent.Y;
            double cb_x = _lastPosition.X - _lastPosition.X + 0.1;
            double cb_y = _lastPosition.Y - _lastPosition.Y;

            return Math.Atan2((ab_x * cb_y - ab_y * cb_x), (ab_x * cb_x + ab_y * cb_y));
        }
        private int RadianToDegree(double rad)
        {
            return (int)Math.Floor(rad * 180 / Math.PI + 0.5);
        }
    }
}
