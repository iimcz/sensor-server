using System;
using Naki3D.Common.Protocol;
using nuitrack;

namespace DepthCamera{
    class ConsoleDataSender : DataSender
    {
        public ConsoleDataSender(){
            Console.WriteLine("Tracking started");
        }

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
            //Console.WriteLine("User {0} left hand: Position: [{1}, {2}, {3}], Click:{4}", userId, hand.X, hand.Y, hand.ZReal, hand.Click);
            Console.WriteLine("[{0}, {1}]", hand.XReal , hand.YReal);
        }
    }
}