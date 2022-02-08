using System.Diagnostics;

namespace SensorServer.ProjectorControl
{
    class ProjectorController : IProjectorController
    {
        public void PowerOff()
        {
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"cec-ctl -d/dev/cec1 --to 0 --standby\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }

        public void PowerOn()
        {
            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"cec-ctl -d/dev/cec1 --playback -S\"",
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
