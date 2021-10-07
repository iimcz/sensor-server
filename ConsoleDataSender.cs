using System;
using Naki3D.Common.Protocol;
using nuitrack;

namespace DepthCamera{
    class ConsoleDataSender : DataSender
    {
        public void Dispose()
        {
            Console.WriteLine("Exit");
        }
        public void SendGestureData(string sensorId, ulong timestamp, Gesture gesture)
        {
            Console.WriteLine("Recognized {0} from user {1}", gesture.Type.ToString(), gesture.UserID);
        }
        public void SendHandMovement(string sensorId, int userId, ulong timestamp, HandType handType, HandContent hand)
        {
            Console.WriteLine("User {0} {1}: Position: [{2}; {3}; {4}], Click:{5}", userId, handType, hand.X, hand.Y, hand.ZReal, hand.Click);
        }
    }
}