using System;
using System.IO;
using DepthCamera.Configuration;
using Newtonsoft.Json;

namespace DepthCamera
{
    class Program
    {
        private static DataSender _dataSender;
        private static CameraController _cameraController;
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
            
            if (config.DataSenderConfiguration.DataSenderType == "console")
            {
                _dataSender = new ConsoleDataSender();
            }
            else
            {
                _dataSender = new ProtobufDataSender(config.DataSenderConfiguration.Host, config.DataSenderConfiguration.Port);
            }
            
            _cameraController = new(_dataSender, config.DepthCameraConfiguration);
            _cameraController.Start();
        }
        public static void ConsoleEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _cameraController.Stop();
                _cameraController.Dispose();
                _dataSender.Dispose();
            }
        }
    }
}
    