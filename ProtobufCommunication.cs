using Google.Protobuf;
using Naki3D.Common.Protocol;
using nuitrack;
using SensorServer.ProjectorControl;
using System;
using System.Net.Sockets;

namespace SensorServer
{
    class ProtobufCommunication
    {
        private readonly string _ip;
        private readonly int _port;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private IProjectorController _projectorController;
        private bool _finished = false;

        public ProtobufCommunication(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Connect()
        {
            while (true)
            {
                if (_finished) break;

                try
                {
                    _tcpClient = new TcpClient(_ip, _port);
                    _networkStream = _tcpClient.GetStream();
                    _projectorController = new ProjectorController();
                    break;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Connection failed. Retry in 1s.");
                }
                System.Threading.Thread.Sleep(1000);
            }
        }

        public void Dispose()
        {
            if (_networkStream != null) _networkStream.Close();
            if ( _tcpClient != null) _tcpClient.Close();
        }

        /// <summary>
        /// Send gesture data if gesture is detected
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="gesture">Gesture type (swipe left, swipe right, swipe up, swipe down)</param>
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
            System.Console.WriteLine(gesture.Type);
            SendMessage(message);
        }

        /// <summary>
        /// Send hand position
        /// </summary>
        /// <param name="sensorId">ID of the sensor that detected the gesture</param>
        /// <param name="userId">ID of the user</param>
        /// <param name="timestamp">Time when the gesture was detected</param>
        /// <param name="handType">Hand type (left, right)</param>
        /// <param name="hand">Hand data</param>
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

        public void Start()
        {
            while (!_finished)
            {
                try
                {
                    if (!_tcpClient.Connected)
                    {
                        break;
                    }

                    SensorControlMessage message = SensorControlMessage.Parser.ParseDelimitedFrom(_networkStream);
                    Console.WriteLine(message.CecMessage);

                    switch (message.CecMessage.Action)
                    {
                        case CECAction.PowerOn:
                            _projectorController.PowerOn();
                            break;

                        case CECAction.PowerOff:
                            _projectorController.PowerOff();
                            break;
                    }
                }
                catch(System.Exception e)
                {
                    Console.WriteLine("Invalid message");
                }
            }
        }

        public void Stop()
        {
            _finished = true;
        }

        public bool IsConnected()
        {
            return _tcpClient.Connected;
        }
    }
}
