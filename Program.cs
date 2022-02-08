using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using SensorServer.Configuration;
using SensorServer.DepthCamera;

namespace SensorServer
{
    class Program
    {
        private static CameraController _cameraController;
        private static ProtobufCommunication _protobufCommunication;

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
                Console.WriteLine("Config file not found");
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleEventHandler);

            _protobufCommunication = new(config.CommunicationConfiguration.Host, config.CommunicationConfiguration.Port);

            if (config.ProjectorControl)
            {
                Thread readThread = new(_protobufCommunication.Start);
                readThread.Start();
            }

            if (config.DepthCamera)
            {
                _cameraController = new(_protobufCommunication, config.DepthCameraConfiguration);
                _cameraController.Start();
            }
        }

        /// <summary>
        /// Added EventHandler for stoping program
        /// </summary>
        public static void ConsoleEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _cameraController.Stop();
                _cameraController.Dispose();
                _protobufCommunication.Stop();
                _protobufCommunication.Dispose();
            }
        }
    }
}
