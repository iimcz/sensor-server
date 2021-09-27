using nuitrack;
using Naki3D.Common.Protocol;
using System;

namespace DepthCamera{
    interface DataSender : IDisposable
    {
        public void SendGestureData(string sensorId, ulong timestamp, Gesture gesture);
        public void SendHandMovement(string sensorId, int userId, ulong timestamp, HandType handType, HandContent hand);
    }
}