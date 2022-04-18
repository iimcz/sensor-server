using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SensorServer.Configuration;
using SensorServer.Pjlink;

namespace SensorServer.ProjectorControl
{
    public class PjlinkProjectorController : IProjectorController
    {
        private List<string> _hosts;

        public PjlinkProjectorController(PjlinkConfiguration configuration)
        {
            _hosts = configuration.Hosts;
        }

        public void PowerOff()
        {
            var tasks = _hosts.Select(host =>
            {
                return Task.Run(async () =>
                {
                    using (var client = new PjlinkConnection(host))
                    {
                        client.PowerOff();
                        for (int i = 0; i < 10; ++i)
                        {
                            var status = client.PowerQuery();
                            if (status == PowerResponse.Standby || status == PowerResponse.Unknown)
                            {
                                break;
                            }
                            await Task.Delay(5000);
                        }
                    }
                });
            }).ToArray();

            try
            {
                Task.WaitAll(tasks, 1000);
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                {
                    Console.WriteLine(ie.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void PowerOn()
        {
            var tasks = _hosts.Select(host =>
            {

                return Task.Run(async () =>
                {
                    using (var client = new PjlinkConnection(host, 4352, "test"))
                    {
                        client.PowerOn();
                        for (int i = 0; i < 10; ++i)
                        {
                            var status = client.PowerQuery();
                            if (status == PowerResponse.LampOn || status == PowerResponse.Unknown)
                            {
                                break;
                            }
                            await Task.Delay(5000);
                        }
                    }
                });
            }).ToArray();
            
            try
            {
                Task.WaitAll(tasks, 1000);
            }
            catch (AggregateException e)
            {
                foreach (var ie in e.InnerExceptions)
                {
                    Console.WriteLine(ie.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}
