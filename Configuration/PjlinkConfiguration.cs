using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorServer.Configuration
{
    public class PjlinkConfiguration
    {
        public List<string> Hosts { get; set; }

        public PjlinkConfiguration()
        {
            Hosts = new List<string>();
        }
    }
}
