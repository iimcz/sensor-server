using SensorServer.Configuration;
using System.Diagnostics;

namespace SensorServer.RemoteManagement
{
    public class ShellIpwServiceManager : IIpwServiceManager
    {
        private readonly ShellIpwServiceConfiguration _configuration;

        public ShellIpwServiceManager(ShellIpwServiceConfiguration config)
        {
            _configuration = config;
        }

        public void Start()
        {
            RunCommand(_configuration.StartCommand);
        }

        public void Stop()
        {
            RunCommand(_configuration.StopCommand);
        }

        private void RunCommand(string command)
        {
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
