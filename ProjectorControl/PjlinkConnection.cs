using System;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace SensorServer.Pjlink
{
    public class PjlinkConnection : IDisposable
    {
        private readonly TcpClient _client;
        private readonly string _hostname;
        private readonly int _port;
        private bool _auth;
        private string _pwd;
        private string _pjlinkKey;

        public PjlinkConnection(string hostname, int port = 4352, string pwd = "")
        {
            _client = new TcpClient();
            _auth = !String.IsNullOrEmpty(pwd);
            _pwd = pwd;
            _hostname = hostname;
            _port = port;
        }

        private void InitConnection()
        {
            if (_client.Connected)
                return;

            _client.Connect(_hostname, _port);

            var stream = _client.GetStream();
            var buffer = new byte[_client.ReceiveBufferSize];
            var read = stream.Read(buffer, 0, _client.ReceiveBufferSize);

            if (read <= 0 || buffer[read - 1] != 0x0d)
            {
                throw new NotSupportedException("Invalid protocol: does not end with CR.");
            }
            buffer[read - 1] = 0;

            var message = Encoding.ASCII.GetString(buffer);
            if (message.StartsWith("PJLINK 0"))
            {
                _auth = false;
            }
            else if (message.StartsWith("PJLINK 1 "))
            {
                _pjlinkKey = message.Substring("PJLINK 1 ".Length);
            }
            else
            {
                throw new NotSupportedException("Invalid protocol: does not start with PJLINK 0 or PJLINK 1");
            }
        }

        public TRes SendCommand<TRes>(PjlinkCommand<TRes> cmd) where TRes : PjlinkResponse, new()
        {
            InitConnection();

            var stream = _client.GetStream();
            var msg = cmd.Message;

            if (_auth)
            {
                msg = ComputeMD5(_pjlinkKey + _pwd) + msg;
            }

            var bytes = Encoding.ASCII.GetBytes(msg);
            stream.Write(bytes);

            var resp = new byte[_client.ReceiveBufferSize];
            var read = stream.Read(resp, 0, _client.ReceiveBufferSize);

            if (resp[read - 1] != 0x0d)
            {
                throw new NotSupportedException("Invalid protocol: does not end with CR.");
            }
            resp[read - 1] = 0;

            return (TRes)cmd.ParseResponse(Encoding.ASCII.GetString(resp));
        }

        private string ComputeMD5(string v)
        {
            var data = Encoding.ASCII.GetBytes(v);
            var hash = MD5.HashData(data);
            return String.Join("", hash.Select(b => b.ToString("x2")));
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        // Convenience methods
        public void PowerOn()
        {
            SendCommand(new PjlinkPowerCommand(PjlinkPowerCommand.Power.On));
        }

        public void PowerOff()
        {
            SendCommand(new PjlinkPowerCommand(PjlinkPowerCommand.Power.Off));
        }

        public PowerResponse PowerQuery()
        {
            var resp = SendCommand(new PjlinkPowerCommand(PjlinkPowerCommand.Power.Query));
            return resp.Power;
        }
    }

    public enum ResponseType
    {
        UndefinedCommand,      // ERR1 
        OutOfParameter,       // ERR2
        UnavailableTime,       // ERR3
        ProjectorFailure,      // ERR4
        AuthenticationFailure, // ERRA
        Success,                // OK
    }

    public enum PowerResponse
    {
        Standby,
        LampOn,
        Cooling,
        WarmUp,
        Unknown
    }

    public abstract class PjlinkCommand<TRes> where TRes : PjlinkResponse, new()
    {
        protected abstract string Cls { get; }
        protected abstract string Cmd { get; }
        protected abstract string Param { get; }

        public string Message
        {
            get
            {
                return $"%{Cls}{Cmd} {Param}\r";
            }
        }

        public virtual PjlinkResponse ParseResponse(string rsp)
        {
            if (!rsp.StartsWith($"%{Cls}{Cmd}"))
            {
                throw new NotSupportedException("Invalid protocol: response does not belong to command.");
            }

            PjlinkResponse response = new TRes();
            if (rsp.EndsWith("=ERR1"))
            {
                response.Response = ResponseType.UndefinedCommand;
            }
            else if (rsp.EndsWith("=ERR2"))
            {
                response.Response = ResponseType.OutOfParameter;
            }
            else if (rsp.EndsWith("=ERR3"))
            {
                response.Response = ResponseType.UnavailableTime;
            }
            else if (rsp.EndsWith("=ERR4"))
            {
                response.Response = ResponseType.ProjectorFailure;
            }
            else if (rsp.EndsWith(" ERRA"))
            {
                response.Response = ResponseType.AuthenticationFailure;
            }
            else
            {
                response.Response = ResponseType.Success;
            }

            return response;
        }
    }

    public class PjlinkResponse
    {
        public ResponseType Response { get; set; }

        public PjlinkResponse() { }
    }

    public class PjlinkPowerCommand : PjlinkCommand<PjlinkPowerResponse>
    {
        public enum Power
        {
            Query,
            On,
            Off
        }

        protected override string Cls => "1";

        protected override string Cmd => "POWR";

        protected override string Param
        {
            get
            {
                switch (_power)
                {
                    default:
                    case Power.Query:
                        return "?";
                    case Power.On:
                        return "1";
                    case Power.Off:
                        return "0";
                }
            }
        }

        private Power _power = Power.Query;

        public PjlinkPowerCommand(Power type)
        {
            _power = type;
        }

        public override PjlinkResponse ParseResponse(string rsp)
        {
            PjlinkPowerResponse response = (PjlinkPowerResponse)base.ParseResponse(rsp);
            Console.WriteLine(response.Response);

            if (response.Response == ResponseType.Success && _power == Power.Query)
            {
                int value = int.Parse(rsp.Substring(rsp.IndexOf('=') + 1));
                response.Power = (PowerResponse)value;
            }
            else
            {
                response.Power = PowerResponse.Unknown;
            }

            return response;
        }
    }

    public class PjlinkPowerResponse : PjlinkResponse
    {
        public PowerResponse Power { get; set; }

        public PjlinkPowerResponse() { }
    }
}