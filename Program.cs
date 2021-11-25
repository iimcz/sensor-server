using System;

namespace DepthCamera
{
    class Program
    {
        private static DataSender _dataSender;
        private static CameraController _cameraController;
        static void Main()
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleEventHandler);
            //_dataSender = new ProtobufDataSender("localhost", 5000);
            _dataSender = new ConsoleDataSender();
            _cameraController = new(_dataSender);
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
    