using System.Collections.Generic;
using nuitrack;
using System;

namespace DepthCamera
{
    class GestureDetector
    {
        private Dictionary<int, User> _users;
        public GestureDetector()
        {
            _users = new Dictionary<int, User>();
        }
        public bool Update(int userId, Naki3D.Common.Protocol.HandType handType, HandContent handContent, out Gesture outGesture)
        {
            Hand hand = new Hand(handContent.X, handContent.Y);
            GestureType gestureType;
            bool gestureDetected = false;

            User user;
            if(!_users.TryGetValue(userId ,out user))
            {
                user = new User();
                _users.Add(userId, user);
            }

            if(handType == Naki3D.Common.Protocol.HandType.HandRight)
            {
                gestureDetected = DetectGesture(user.RightHand, hand, out gestureType);
            }
            else
            {
                gestureDetected = DetectGesture(user.LeftHand, hand, out gestureType);
            }

            Gesture gesture = new Gesture();
            gesture.UserID = userId;
            gesture.Type = gestureType;
            outGesture = gesture;
            return gestureDetected;
        }
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

                        if(userHand.HandStepCounter >= 3)
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