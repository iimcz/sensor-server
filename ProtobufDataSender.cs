using Google.Protobuf;
using Naki3D.Common.Protocol;
using nuitrack;
using System.Net.Sockets;

namespace DepthCamera
{
    class ProtobufDataSender : DataSender
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _networkStream;
        public ProtobufDataSender(string ip, int port)
        {
            _tcpClient = new TcpClient(ip, port);
            _networkStream = _tcpClient.GetStream();
        }
        public void Dispose()
        {
            _networkStream.Close();
            _tcpClient.Close();
        }
        public void SendGestureData(string sensorId, ulong timestamp, Gesture gesture)
        {
            Naki3D.Common.Protocol.GestureData gestureData = new()
            {
                UserId = gesture.UserID,
                Type = (Naki3D.Common.Protocol.GestureType)gesture.Type
            };
            SensorMessage message = new()
            {
                SensorId = sensorId,
                Timestamp = timestamp,
                Gesture = gestureData
            };
            SendMessage(message);
        }
        public void SendHandMovement(string sensorId, int userId, ulong timestamp, HandType handType, HandContent hand)
        {
            Naki3D.Common.Protocol.Vector3 vector3 = new()
            {
                X = hand.X,
                Y = hand.Y,
                Z = hand.ZReal
            };
            HandMovementData handMovementData = new()
            {
                Hand = handType,
                ProjPosition = vector3,
                OpenHand = !hand.Click,
                UserId = userId
            };
            SensorMessage message = new()
            {
                SensorId = sensorId,
                Timestamp = timestamp,
                HandMovement = handMovementData
            };
            SendMessage(message);
        }
        private void SendMessage(SensorMessage sensorMessage)
        {
            sensorMessage.WriteDelimitedTo(_networkStream);
            _networkStream.Flush();
        }
    }
}
