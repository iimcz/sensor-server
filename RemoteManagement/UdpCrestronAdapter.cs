using Google.Protobuf;
using Naki3D.Common.Protocol;
using SensorServer.Configuration;
using SensorServer.ProjectorControl;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static Naki3D.Common.Protocol.ManagementRequest.Types;

namespace SensorServer.RemoteManagement
{
    class UdpCrestronAdapter
    {
        private UdpClient _client;
        private bool _finished;

        private readonly AppConfiguration _config;
        private readonly IIpwServiceManager _serviceManager;
        private readonly IProjectorController _projectorController;

        private readonly byte[] _ack = Encoding.ASCII.GetBytes("DRES00");
        private readonly byte[] _success = Encoding.ASCII.GetBytes("DRES01");
        private readonly byte[] _failUnknown = Encoding.ASCII.GetBytes("DRES99");

        public UdpCrestronAdapter(AppConfiguration config, IIpwServiceManager serviceManager, IProjectorController projectorController)
        {
            _config = config;
            _serviceManager = serviceManager;
            _projectorController = projectorController;

            _client = new UdpClient(_config.UdpCrestronAdapterConfiguration.Port);
        }

        public void Listen()
        {
            while (!_finished)
            {
                IPEndPoint endPoint = new(IPAddress.Any, 0);

                try
                {
                    byte[] data = _client.Receive(ref endPoint);
                    string command = Encoding.ASCII.GetString(data);
                    _client.Send(_ack, _ack.Length, endPoint);

                    ManagementType? managementType = command switch
                    {
                        "DCMD00" => ManagementType.Shutdown,
                        "DCMD01" => ManagementType.Start,
                        "DCMD02" => ManagementType.StartMute,
                        _ => null
                    };

                    var success = managementType == null ? UnknownCommand(command) : SendManagementMessage(managementType.Value);

                    if (success) _client.Send(_success, _success.Length, endPoint);
                    else // IPW did not respond, resort to manually changing service
                    {
                        if (ExecuteManagementMessage(managementType)) _client.Send(_success, _success.Length, endPoint);
                        else _client.Send(_failUnknown, _failUnknown.Length, endPoint);
                    }

                    Console.WriteLine($"Executed UDP command: {command}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to execute remote UDP command, stacktrace: {ex}");

                    try
                    {
                        _client.Send(_failUnknown, _failUnknown.Length, endPoint);
                    }
                    catch (Exception) // Network failure, we can't even tell the remote side we could not finish
                    {
                        _client.Close();
                        _client = new UdpClient(_config.UdpCrestronAdapterConfiguration.Port);
                    }
                }
            }
        }

        public void Stop()
        {
            _finished = true;
            _client?.Close();
        }

        private bool UnknownCommand(string command)
        {
            Console.WriteLine($"Invalid UDP remote control command '{command}', ignoring");
            return false;
        }

        // TODO: Split this into a standalone class when we have a proper management system in place
        private bool SendManagementMessage(ManagementType type)
        {
            try
            {
                var config = _config.CommunicationConfiguration;
                var client = new TcpClient(config.Host, config.ManagementPort);
                var stream = client.GetStream();

                var message = new ManagementRequest
                {
                    ConnectionId = "*",
                    ManagementType = type
                };

                message.WriteDelimitedTo(stream);
                var response = ManagementResponse.Parser.ParseDelimitedFrom(stream);
                return response.DeviceStatus == ManagementResponse.Types.DeviceStatus.Ok;
            }
            catch
            {
                // Most probably a network related reason, no need to spam the console with stacktraces
                Console.WriteLine($"Failed to send remote management command");
                return false;
            }
        }

        private bool ExecuteManagementMessage(ManagementType? type)
        {
            switch (type)
            {
                case ManagementType.Shutdown:
                    _serviceManager.Stop();
                    _projectorController?.PowerOff();
                    return true;
                case ManagementType.Start:
                    _projectorController?.PowerOn();
                    _serviceManager.Stop(); // To be absolutely sure we're not leaving some zombie process behind
                    _serviceManager.Start();
                    return true;
                case ManagementType.StartMute:
                    _projectorController?.PowerOn();
                    _serviceManager.Stop(); // To be absolutely sure we're not leaving some zombie process behind
                    _serviceManager.Start();
                    Thread.Sleep(_config.UdpCrestronAdapterConfiguration.UnityBootTimeout); // Wait for unity to load
                    return SendManagementMessage(ManagementType.StartMute);
                case null:
                default:
                    return false;
            }
        }
    }
}
