using Google.Protobuf;
using Naki3D.Common.Protocol;
using nuitrack;
using SensorServer.ProjectorControl;
using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace SensorServer
{
    class ProtobufCommunication
    {
        private readonly string _ip;
        private readonly int _port;
        private readonly int _maxUsers;

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private IProjectorController _projectorController;
        private bool _finished = false;

        private readonly bool _lightSensor;
        private readonly bool _ultrasonicDistance;

        public IProjectorController ProjectorController { get => _projectorController; set => _projectorController = value; }

        public ProtobufCommunication(string ip, int port, int maxUsers, bool lightSensor, bool ultrasonicDistance)
        {
            _ip = ip;
            _port = port;
            _maxUsers = maxUsers;
            _lightSensor = lightSensor;
            _ultrasonicDistance = ultrasonicDistance;
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
                    SendAllDiscovery();
                    break;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine($"Connection failed: {e.Message}\nRetry in 1s.");
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
        public void SendGestureData(int Id, ulong timestamp, Gesture gesture, DepthCamera.CameraController.HandSide handSide)
        {
            GestureType type = gesture.Type;
            SensorDataMessage data = new SensorDataMessage()
            {
                Timestamp = timestamp,
                Void = new Google.Protobuf.WellKnownTypes.Empty()
            };

            string path = $"nuitrack/handtracking/user/0/hand/{handSide.ToString().ToLower()}/gestures"; //Max one user with id 0
            switch (type)
            {
                case GestureType.GestureSwipeLeft:
                    data.Path = path + "/swipe_left";
                    break;

                case GestureType.GestureSwipeRight:
                    data.Path = path + "/swipe_right";
                    break;

                case GestureType.GestureSwipeUp:
                    data.Path = path + "/swipe_up";
                    break;

                case GestureType.GestureSwipeDown:
                    data.Path = path + "/swipe_down";
                    break;
            }

            SensorMessage message = new SensorMessage()
            { 
                Data = data 
            };
            System.Console.WriteLine(type);
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
        public void SendHandMovement(int userId, ulong timestamp, DepthCamera.CameraController.HandSide handSide, HandContent hand)
        {
            Vector3Data vector = new Vector3Data()
            {
                X = hand.X,
                Y = hand.Y,
                Z = hand.ZReal
            };

            SensorDataMessage data = new SensorDataMessage()
            {
                Path = $"nuitrack/handtracking/user/0/hand/{handSide.ToString().ToLower()}/center_position", //Max one user with id 0
                Timestamp = timestamp,
                Vector3 = vector
            };

            SensorMessage message = new SensorMessage()
            {
                Data = data
            };
            SendMessage(message);
        }
        private void SendMessage(SensorMessage sensorMessage)
        {
            try
            {
                sensorMessage.WriteDelimitedTo(_networkStream);
                _networkStream.Flush();
            }
            catch(System.Exception e)
            {
                Console.WriteLine($"Message sendig failed: {e.Message}");
                if(!IsConnected())
                {
                    Console.WriteLine("Reconnecting...");
                    Connect();
                }
            }
        }

        private void SendAllDiscovery()
        {
            Dictionary<string, DataType> typeMap = new Dictionary<string, DataType>()
            {
                {"/center_position", DataType.Vector3 },

                {"/gestures/swipe_left", DataType.Void },
                {"/gestures/swipe_right", DataType.Void },
                {"/gestures/swipe_up", DataType.Void },
                {"/gestures/swipe_down", DataType.Void }
            };

            for(int i = 0; i < _maxUsers; i++)
            {
                foreach(KeyValuePair<string, DataType> type in typeMap)
                {
                    SendDiscovery($"nuitrack/handtracking/user/{i}/hand/left" + type.Key, type.Value);
                    SendDiscovery($"nuitrack/handtracking/user/{i}/hand/right" + type.Key, type.Value);
                }
            }

            if (_lightSensor)
            {
                SendDiscovery($"lightsensor/value", DataType.Float);
            }

            if (_ultrasonicDistance)
            {
                SendDiscovery($"ultrasonicdistance/value", DataType.Float);
            }
        }

        private void SendDiscovery(string path, DataType dataType)
        {
            SensorDescriptor descriptor = new SensorDescriptor()
            {
                Model = "Nuitrack v",
                Path = path,
                DataType = dataType
            };
            
            SensorMessage message = new SensorMessage()
            {
                Descriptor_ = descriptor
            };
            SendMessage(message);
        }
        public void SendLightValue(double value)
        {
            DateTimeOffset now = DateTime.UtcNow;
            ulong time = (ulong)now.ToUnixTimeSeconds();
            SensorDataMessage data = new SensorDataMessage()
            {
                Path = $"lightsensor/value",
                Timestamp = time,
                Float = (float)value
            };
            SensorMessage message = new SensorMessage()
            {
                Data = data
            };
            SendMessage(message);
        }

        public void SendDistance(float value)
        {
            DateTimeOffset now = DateTime.UtcNow;
            ulong time = (ulong)now.ToUnixTimeSeconds();
            SensorDataMessage data = new SensorDataMessage()
            {
                Path = $"ultrasonicdistance/value",
                Timestamp = time,
                Float = value
            };
            SensorMessage message = new SensorMessage()
            {
                Data = data
            };
            SendMessage(message);
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
                            _projectorController?.PowerOn();
                            break;

                        case CECAction.PowerOff:
                            _projectorController?.PowerOff();
                            break;
                    }
                }
                catch(System.Exception e)
                {
                    Console.WriteLine($"Invalid message: {e.Message}\n Assuming disconnected socket...");
                    break;
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
