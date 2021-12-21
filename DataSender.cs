using nuitrack;
using Naki3D.Common.Protocol;
using System;

namespace DepthCamera{
    interface DataSender : IDisposable
    {
        /// <summary>
        /// Send gesture data if gesture is detected
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="gesture">Gesture type (swipe left, swipe right, swipe up, swipe down)</param>
        public void SendGestureData(string sensorId, ulong timestamp, Gesture gesture);

        /// <summary>
        /// Send hand position
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="handType">Hand type (left, right)</param>
        /// <param name="hand">Hand data</param>
        public void SendHandMovement(string sensorId, int userId, ulong timestamp, HandType handType, HandContent hand);
    }
}