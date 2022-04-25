using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using SensorServer.Configuration;
using SensorServer.DepthCamera;
using SensorServer.ProjectorControl;
using SensorServer.RemoteManagement;

namespace SensorServer
{
    class Program
    {
        private static CameraController _cameraController = null;
        private static ProtobufCommunication _protobufCommunication = null;
        private static UdpCrestronAdapter _udpCrestronAdapter = null;

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
                Console.WriteLine("Config file not found");
            }

            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleEventHandler);

            _protobufCommunication = new(config.CommunicationConfiguration.Host, config.CommunicationConfiguration.Port);
            
            if (config.UdpCrestronAdapterConfiguration?.Enabled ?? false)
            {
                IIpwServiceManager ipwServiceManager = new ShellIpwServiceManager(config.ShellIpwServiceConfiguration);
                IProjectorController projectorController = null;
                switch (config.UdpCrestronAdapterConfiguration.ProjectorControl)
                {
                    case ProjectorControlType.HdmiCec:
                        projectorController = new HdmiProjectorController();
                        break;
                    case ProjectorControlType.Pjlink:
                        projectorController = new PjlinkProjectorController(config.PjlinkConfiguration);
                        break;
                }


                _udpCrestronAdapter = new(config, ipwServiceManager, projectorController);
            }

            while (true)
            {
                if (_finished) break;

                _protobufCommunication.Connect();

                Thread readThread = null;
                Thread crestronAdapterThread = null;
                if (_udpCrestronAdapter != null)
                {
                    crestronAdapterThread = new(_udpCrestronAdapter.Listen);
                    crestronAdapterThread.Start();
                }

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
                if (crestronAdapterThread != null) crestronAdapterThread.Join();
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

                if (_udpCrestronAdapter != null)
                {
                    _udpCrestronAdapter.Stop();
                }
            }
        }
    }
}
