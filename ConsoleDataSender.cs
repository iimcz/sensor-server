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

        /// <summary>
        /// Print gesture data to the console if gesture is detected
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="gesture">Gesture type (swipe left, swipe right, swipe up, swipe down)</param>
        public void SendGestureData(string sensorId, ulong timestamp, Gesture gesture)
        {
            Console.WriteLine("Recognized {0} from user {1}", gesture.Type.ToString(), gesture.UserID);
        }

        /// <summary>
        /// Print hand position to the console
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="handType">Hand type (left, right)</param>
        /// <param name="hand">Hand data</param>
        public void SendHandMovement(string sensorId, int userId, ulong timestamp, HandType handType, HandContent hand)
        {
            Console.WriteLine("User {0} {1}: Position: [{2}; {3}; {4}], Click:{5}", userId, handType, hand.X, hand.Y, hand.ZReal, hand.Click);
        }
    }
}