using System.Collections.Generic;
using nuitrack;
using System;
using SensorServer.Configuration;

namespace SensorServer.DepthCamera
{
    /// <summary>
    /// Handle gesture detection
    /// </summary>
    class GestureDetector
    {
        private readonly int _gestureDelay;
        private readonly int _gestureLength;
        private Dictionary<int, User> _users;
        private readonly DepthCameraConfiguration _config;

        public GestureDetector(DepthCameraConfiguration config)
        {
            _config = config;
            _gestureDelay = _config.GestureDelay;
            _gestureLength = _config.GestureLength;
            _users = new Dictionary<int, User>();
        }

        /// <summary>
        /// Update gesture detection whan hand movement is detected
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="handType">Hand type (left, right)</param>
        /// <param name="handContent">Information abou new hand position</param>
        /// <param name="outGesture">Output paremeter will contain geture type if gesture is detected</param>
        /// <returns>True if gesture was detected, False otherwise</returns>
        public bool Update(int userId, Naki3D.Common.Protocol.HandType handType, HandContent handContent, out Gesture outGesture)
        {
            Hand hand = new Hand(handContent.X, handContent.Y, _config.HorizontalGridSize, _config.VerticalGridSize);
            GestureType gestureType;
            bool gestureDetected = false;

            if(!_users.TryGetValue(userId, out User user))
            {
                user = new User();
                _users.Add(userId, user);
            }

            Gesture gesture = new Gesture();

            if(handType == Naki3D.Common.Protocol.HandType.HandRight)
            {
                //Console.WriteLine($"[{hand.RealX}, {hand.RealY}]");
                gestureDetected = DetectGesture(user.RightHand, hand, out gestureType);

                if(gestureDetected)
                {
                    long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if(timestamp - user.LastGestureDetected <= _gestureDelay){
                        outGesture = gesture;
                        return false;
                    }
                    else{
                        user.LastGestureDetected = timestamp;
                        gesture.UserID = userId;
                        gesture.Type = gestureType;
                        outGesture = gesture;
                        return true;
                    }
                }
            }
            else
            {
                gestureDetected = DetectGesture(user.LeftHand, hand, out gestureType);

                if(gestureDetected)
                {
                    long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    if(timestamp - user.LastGestureDetected <= _gestureDelay){
                        outGesture = gesture;
                        return false;
                    }
                    else{
                        user.LastGestureDetected = timestamp;
                        gesture.UserID = userId;
                        gesture.Type = gestureType;
                        outGesture = gesture;
                        return true;
                    }
                }
            }

            outGesture = gesture;
            return false;
        }

        /// <summary>
        /// Detect gestures
        /// </summary>
        /// <param name="userHand">Information abou hand position</param>
        /// <param name="hand">New hand position</param>
        /// <param name="gestureType">Output paremeter will contain geture type if gesture is detected</param>
        /// <returns>True if gesture was detected, False otherwise</returns>
        private bool DetectGesture(UserHand userHand, Hand hand, out GestureType gestureType)
        {
            if(userHand.LastHandPosition == null)
            {
                userHand.LastHandPosition = hand;
            }
            else
            {
                HandMovement handMovement = userHand.LastHandPosition.CalculateMovement(hand);

                if(handMovement != HandMovement.None)
                {
                    userHand.LastHandPosition = hand;

                    if(handMovement == userHand.HandMovement)
                    {
                        userHand.HandStepCounter++;

                        if(userHand.HandStepCounter >= _gestureLength)
                        {
                            userHand.HandMovement = HandMovement.None;
                            userHand.HandStepCounter = 0;

                            switch(handMovement)
                            {
                                case HandMovement.Left:
                                    gestureType = GestureType.GestureSwipeLeft;
                                    return true;

                                case HandMovement.Right:
                                    gestureType = GestureType.GestureSwipeRight;
                                    return true;

                                case HandMovement.Up:
                                    gestureType = GestureType.GestureSwipeUp;
                                    return true;
                                    
                                case HandMovement.Down:
                                    gestureType = GestureType.GestureSwipeDown;
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        userHand.HandMovement = handMovement;
                        userHand.HandStepCounter = 1;
                    }
                }
            }

            gestureType = GestureType.GestureSwipeLeft;
            return false;
        }
    }
}