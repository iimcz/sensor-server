using Google.Protobuf;
using Naki3D.Common.Protocol;
using SensorServer.Configuration;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static Naki3D.Common.Protocol.ManagementRequest.Types;

namespace SensorServer.RemoteManagement
{
    class UdpCrestronAdapter
    {
        private UdpClient _client;
        private bool _finished;

        private readonly AppConfiguration _config;

        private readonly byte[] _ack = Encoding.ASCII.GetBytes("DRES00");
        private readonly byte[] _success = Encoding.ASCII.GetBytes("DRES01");
        private readonly byte[] _failUnknown = Encoding.ASCII.GetBytes("DRES99");

        public UdpCrestronAdapter(AppConfiguration config)
        {
            _config = config;
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

                    var success = command switch
                    {
                        "DCMD00" => SendManagementMessage(ManagementType.Shutdown),
                        "DCMD01" => SendManagementMessage(ManagementType.Start),
                        "DCMD02" => SendManagementMessage(ManagementType.StartMute),
                        _ => UnknownCommand(command)
                    };

                    if (success) _client.Send(_success, _success.Length, endPoint);
                    else _client.Send(_failUnknown, _failUnknown.Length, endPoint);

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
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send remote management command, stacktrace: {ex}");
                return false;
            }
        }
    }
}
