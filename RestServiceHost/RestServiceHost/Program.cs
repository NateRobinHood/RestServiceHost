using RestServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace RestServiceHost
{
    class Program
    {
        static void Main(string[] args)
        {
            List<EndpointHost> m_Hosts = new List<EndpointHost>();
            string m_HostUri = "http://localhost:5051";

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string[] folders = Directory.GetDirectories(string.Format("{0}/{1}", baseDir, "EndPoints"));
            foreach (string fodler in folders)
            {
                DirectoryInfo folderInfo = new DirectoryInfo(fodler);

                foreach (FileInfo dllFile in folderInfo.GetFiles("*.dll"))
                {
                    Assembly assembly = Assembly.LoadFrom(dllFile.FullName);

                    //using types didn't work becuase the they are coming from different dlls
                    List<Type> hostableTypes = assembly.GetTypes().Where(c => c.GetInterfaces().Any(iface => iface.Name == "IRestHostable")).ToList();

                    foreach (Type hostableType in hostableTypes)
                    {
                        EndpointHost newHost = new EndpointHost(hostableType, m_HostUri, folderInfo);
                        m_Hosts.Add(newHost);
                    }
                }
            }

            Console.WriteLine("Starting service hosts");
            foreach (EndpointHost host in m_Hosts)
            {
                host.Start();
            }

            if (m_Hosts.All(c => c.ServiceHostState == CommunicationState.Opened))
            {
                Console.WriteLine("All service hosts started, press Enter to stop");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Failed to start all service hosts");
            }

            foreach (EndpointHost host in m_Hosts)
            {
                host.Stop();
            }
        }
    }
}
