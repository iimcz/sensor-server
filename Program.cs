using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using SensorServer.Configuration;
using SensorServer.DepthCamera;
using SensorServer.LightSensor;
using SensorServer.Microphone;
using SensorServer.PIR;
using SensorServer.ProjectorControl;
using SensorServer.UltrasonicDistance;

namespace SensorServer
{
    class Program
    {
        private static CameraController _cameraController = null;
        private static ProtobufCommunication _protobufCommunication = null;
        private static LightSensorController _lightSensorController = null;
        private static UltrasonicDistanceController _ultrasonicDistanceController = null;
        private static PIRController _pirController = null;
        private static MicrophoneController _microphoneController = null;

        private static bool _finished = false;

        /// <summary>
        /// Read configuration file, initialize data sender and depth camera controller
        /// </summary>
        static void Main()
        {
            AppConfiguration config = new();

            try
            {
                string json = File.ReadAllText("config.json");
                config = JsonConvert.DeserializeObject<AppConfiguration>(json);
            }
            catch
            {
                Console.WriteLine("Config file not found, writing our default.");
                File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleEventHandler);

            _protobufCommunication = new(config);

            while (true)
            {
                if (_finished) break;

                _protobufCommunication.Connect();

                Thread readThread = null;

                switch (config.ProjectorControl)
                {
                    case ProjectorControlType.HdmiCec:
                        _protobufCommunication.ProjectorController = new HdmiProjectorController();
                        break;
                    case ProjectorControlType.Pjlink:
                        _protobufCommunication.ProjectorController = new PjlinkProjectorController(config.PjlinkConfiguration);
                        break;

                }
                if (config.ProjectorControl != ProjectorControlType.None)
                {
                    readThread = new(_protobufCommunication.Start);
                    readThread.Start();
                }

                Thread lightSensorThread = null;
                if (config.LightSensor)
                {
                    _lightSensorController = new LightSensorController(_protobufCommunication, config.LightSensorConfiguration);
                    lightSensorThread = new(_lightSensorController.Start);
                    lightSensorThread.Start();
                }

                Thread ultrasonicDistanceThread = null;
                if (config.UltrasonicDistance)
                {
                    _ultrasonicDistanceController = new UltrasonicDistanceController(_protobufCommunication, config.UltrasonicDistanceConfiguration);
                    ultrasonicDistanceThread = new(_ultrasonicDistanceController.Start);
                    ultrasonicDistanceThread.Start();
                }

                Thread pirThread = null;
                if (config.PIR)
                {
                    _pirController = new PIRController(_protobufCommunication, config.PIRConfiguration);
                    pirThread = new(_pirController.Start);
                    pirThread.Start();
                }

                Thread microphoneThread = null;
                if (config.Microphones)
                {
                    _microphoneController = new MicrophoneController(_protobufCommunication);
                    microphoneThread = new(_microphoneController.Start);
                    microphoneThread.Start();
                }

                if (config.DepthCamera)
                {
                    _cameraController = new(_protobufCommunication, config.DepthCameraConfiguration);
                    try
                    {
                        _cameraController.Start();
                    }
                    catch(Exception e)
                    {
                        if (!_protobufCommunication.IsConnected())
                        {
                            if(readThread != null)
                            {
                                _protobufCommunication.Stop();
                                _protobufCommunication.Dispose();
                            }
                            continue;
                        }
                        else
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    
                }

                if (readThread != null) readThread.Join();
                if (lightSensorThread != null) lightSensorThread.Join();
                if (ultrasonicDistanceThread != null) ultrasonicDistanceThread.Join();
                if (pirThread != null) pirThread.Join();
                if (microphoneThread != null) microphoneThread.Join();
            }
        }

        /// <summary>
        /// Added EventHandler for stoping program
        /// </summary>
        public static void ConsoleEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _finished = true;
                _protobufCommunication.Stop();
                _protobufCommunication.Dispose();
                if (_cameraController != null)
                {
                    _cameraController.Stop();
                    _cameraController.Dispose();
                }
                if(_lightSensorController != null)
                {
                    _lightSensorController.Stop();
                    _lightSensorController.Dispose();
                }
                if(_ultrasonicDistanceController != null)
                {
                    _ultrasonicDistanceController.Stop();
                    _ultrasonicDistanceController.Dispose();
                }
                if(_pirController != null)
                {
                    _pirController.Stop();
                    _pirController.Dispose();
                }
                if(_microphoneController != null)
                {
                    _microphoneController.Stop();
                }
            }
        }
    }
}
