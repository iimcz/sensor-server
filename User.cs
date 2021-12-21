using System;

namespace DepthCamera{
    /// <summary>
    /// Information about user (informations abou left and right hand, time whan last gesture was detected)
    /// </summary>
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