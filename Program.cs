using System;

namespace DepthCamera
{
    class Program
    {
        private static ProtobufDataSender _protobufDataSender;
        private static CameraController _cameraController;
        static void Main()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleEventHandler);
            _protobufDataSender = new("localhost", 5000);
            _cameraController = new(_protobufDataSender);
            _cameraController.Start();
        }
        public static void ConsoleEventHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _cameraController.Stop();
                _cameraController.Dispose();
                _protobufDataSender.Dispose();
            }
        }
    }
}
