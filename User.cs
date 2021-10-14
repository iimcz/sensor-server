using System;

namespace DepthCamera{
    class User{
        public UserHand LeftHand;
        public UserHand RightHand;
        public long LastGestureDetected;
        public User()
        {
            LeftHand = new UserHand();
            RightHand = new UserHand();
            LastGestureDetected = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}