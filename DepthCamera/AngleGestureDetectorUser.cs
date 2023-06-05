using nuitrack;
using System;
using System.Collections.Generic;

namespace SensorServer.DepthCamera
{
    class AngleGestureDetectorUser
    {
        public Queue<HandContent> LeftHand;
        public Queue<HandContent> RightHand;
        public DateTimeOffset LastGesture;

        public AngleGestureDetectorUser()
        {
            LeftHand = new();
            RightHand = new();
            LastGesture = DateTime.Now;
        }

    }
}
