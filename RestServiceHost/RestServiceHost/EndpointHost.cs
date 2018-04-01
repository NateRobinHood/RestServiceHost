using RestHostable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace RestServiceHost
{
    public class EndpointHost
    {
        //Private Variables
        protected ServiceHost m_ServiceHost;
        protected DirectoryInfo m_Directory;
        private string m_Uri;

        //Constructor
        public EndpointHost(Type hostType, string uri, DirectoryInfo hostFolder)
        {
            object hostingService = Activator.CreateInstance(hostType);
            IRestHostable hostableService = hostingService as IRestHostable;

            m_Uri = uri;
            m_ServiceHost = new ServiceHost(hostableService,
                                             new Uri(uri));

            m_ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>().HttpHelpPageEnabled = false;
            m_ServiceHost.Description.Behaviors.Find<ServiceDebugBehavior>().HttpsHelpPageEnabled = false;

            m_Directory = hostFolder;

            WebHttpBinding binding = new WebHttpBinding();
            binding.CrossDomainScriptAccessEnabled = true;
            ServiceEndpoint endpoint = m_ServiceHost.AddServiceEndpoint(hostType, binding, hostFolder.Name);
            endpoint.EndpointBehaviors.Add(new WebHttpBehavior());
        }

        //Public Properties
        public CommunicationState ServiceHostState
        {
            get
            {
                return m_ServiceHost.State;
            }
        }

        public IRestHostable Instance
        {
            get
            {
                return (IRestHostable)m_ServiceHost.SingletonInstance;
            }
        }

        public string HostPoint
        {
            get
            {
                return string.Format("{0}/{1}", m_Uri, m_Directory.Name);
            }
        }

        //Public Methods
        public bool Start()
        {
            bool openSucceeded = false;
            try
            {
                m_ServiceHost.Open();
                openSucceeded = true;

                ServiceHostData injectionData = new ServiceHostData();
                injectionData.ServiceHostUri = HostPoint;
                Instance.ServiceHostInjection(injectionData);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Service host failed to open {0}",
                                  ex.ToString());
            }
            finally
            {
                if (!openSucceeded)
                {
                    m_ServiceHost.Abort();
                }
            }

            return openSucceeded;
        }

        public bool Stop()
        {
            bool closeSucceed = false;
            try
            {
                m_ServiceHost.Close();
                closeSucceed = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Service failed to close. Exception {0}",
                                  ex.ToString());
            }
            finally
            {
                if (!closeSucceed)
                {
                    m_ServiceHost.Abort();
                }
            }

            return closeSucceed;
        }
    }
}
