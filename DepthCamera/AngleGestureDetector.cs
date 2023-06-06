using nuitrack;
using SensorServer.Configuration;
using System;
using System.Collections.Generic;

namespace SensorServer.DepthCamera
{
    class AngleGestureDetector : IGestureDetector
    {
        private readonly DepthCameraConfiguration _config;
        private Dictionary<int, AngleGestureDetectorUser> _usersHands;
        private int _deadZone = 10;
        private double _userMovement = 0.1;
        public AngleGestureDetector(DepthCameraConfiguration config)
        {
            _config = config;
            _usersHands = new();
        }
        public bool Update(int userId, CameraController.HandSide handType, HandContent handContent, Joint torso, out Gesture outGesture)
        {
            Gesture gesture = new Gesture();
            gesture.UserID = userId;

            if (!_usersHands.TryGetValue(userId, out AngleGestureDetectorUser user))
            {
                user = new AngleGestureDetectorUser();
                _usersHands.Add(userId, user);
            }

            DateTimeOffset lastGesture;
            HandContent lastPosition;
            Joint lastTorso;

            if (handType == CameraController.HandSide.Left)
            {
                if(user.LeftHand.Count < _config.HandPositionQueueLength)
                {
                    user.LeftHand.Enqueue(handContent);
                    user.Torso.Enqueue(torso);
                    outGesture = gesture;
                    return false;
                }

                user.LeftHand.Enqueue(handContent);
                user.Torso.Enqueue(torso);
                lastGesture = user.LastGesture;
                lastPosition = user.LeftHand.Dequeue();
                lastTorso = user.Torso.Dequeue();
            }
            else
            {
                if (user.RightHand.Count < _config.HandPositionQueueLength)
                {
                    user.RightHand.Enqueue(handContent);
                    user.Torso.Enqueue(torso);
                    outGesture = gesture;
                    return false;
                }

                user.RightHand.Enqueue(handContent);
                user.Torso.Enqueue(torso);
                lastGesture = user.LastGesture;
                lastPosition = user.RightHand.Dequeue();
                lastTorso = user.Torso.Dequeue();
            }

            DateTimeOffset now = DateTime.Now;
            TimeSpan duration = now - lastGesture;
            if (duration.TotalMilliseconds > _config.GestureDelay)
            {
                double distance = GetDistance(handContent, lastPosition);
                if (GetJointDistance(torso, lastTorso) > _userMovement)
                {
                    outGesture = gesture;
                    return false;
                }
                if (distance > _config.HorizontalGestureLength)
                {
                    double angle = RadianToDegree(GetAngle(handContent, lastPosition));
                    if (angle >= -45 + _deadZone && angle < 45 - _deadZone)
                    {
                        user.LastGesture = now;
                        gesture.Type = GestureType.GestureSwipeRight;
                        outGesture = gesture;
                        return true;
                    }
                    else if (angle >= 45 + _deadZone && angle < 135 - _deadZone)
                    {
                        if(distance > _config.VerticalGestureLength)
                        {
                            user.LastGesture = now;
                            gesture.Type = GestureType.GestureSwipeDown;
                            outGesture = gesture;
                            return true;
                        }
                        else
                        {
                            outGesture = gesture;
                            return false;
                        }
                        
                    }
                    else if (angle >= 135 + _deadZone || angle < -135 - _deadZone)
                    {
                        user.LastGesture = now;
                        gesture.Type = GestureType.GestureSwipeLeft;
                        outGesture = gesture;
                        return true;
                    }
                    else if (angle >= -135 + _deadZone && angle < -45 - _deadZone)
                    {
                        if (distance > _config.VerticalGestureLength)
                        {
                            user.LastGesture = now;
                            gesture.Type = GestureType.GestureSwipeUp;
                            outGesture = gesture;
                            return true;
                        }
                        else
                        {
                            outGesture = gesture;
                            return false;
                        }
                        
                    }
                }
            }
            else
            {
                if (handType == CameraController.HandSide.Left)
                {
                    user.LeftHand.Dequeue();
                    user.LeftHand.Enqueue(handContent);
                }
                else
                {
                    user.RightHand.Dequeue();
                    user.RightHand.Enqueue(handContent);
                }
                user.Torso.Dequeue();
                user.Torso.Enqueue(torso);
            }

            outGesture = gesture;
            return false;
        }
        private double GetDistance(HandContent handContent, HandContent lastPosition)
        {
            return Math.Sqrt(Math.Pow(handContent.X - lastPosition.X, 2) + Math.Pow(handContent.Y - lastPosition.Y, 2));
        }
        private double GetJointDistance(Joint current, Joint last)
        {
            return Math.Sqrt(Math.Pow(current.Proj.X - last.Proj.X, 2) + Math.Pow(current.Proj.Y - last.Proj.Y, 2));
        }
        private double GetAngle(HandContent handContent, HandContent lastPosition)
        {
            double ab_x = lastPosition.X - handContent.X;
            double ab_y = lastPosition.Y - handContent.Y;
            double cb_x = lastPosition.X - lastPosition.X + 0.1;
            double cb_y = lastPosition.Y - lastPosition.Y;

            return Math.Atan2((ab_x * cb_y - ab_y * cb_x), (ab_x * cb_x + ab_y * cb_y));
        }
        private int RadianToDegree(double rad)
        {
            return (int)Math.Floor(rad * 180 / Math.PI + 0.5);
        }
    }
}
