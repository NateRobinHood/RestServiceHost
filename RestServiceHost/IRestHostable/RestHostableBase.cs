using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace RestHostable
{
    public class RestHostableBase : IRestHostable
    {
        private ServiceHostData m_ServiceHostData = null;
        private List<VersionDetails> m_VersionDetails = new List<VersionDetails>();

        public RestHostableBase()
        {
        }

        //public Properties
        public ServiceHostData HostData
        {
            get
            {
                return m_ServiceHostData;
            }
        }

        //Public Methods
        public void RegisterVersion(string versionKey)
        {
            if (m_ServiceHostData != null)
            {
                m_VersionDetails.Add(new VersionDetails() { VersionKey = versionKey, Link = string.Format("{0}/{1}", m_ServiceHostData.ServiceHostUri, versionKey) });
            }
            else
            {
                m_VersionDetails.Add(new VersionDetails() { VersionKey = versionKey });
            }
        }

        public VersionsPackage Versions()
        {
            VersionsPackage versions = new VersionsPackage();
            foreach (VersionDetails details in m_VersionDetails)
            {
                versions.Add(new VersionData() { Version = details.VersionKey, Link = details.Link });
            }
            return versions;
        }

        public EndPointsPackage GetEndpoints(string versionKey)
        {
            EndPointsPackage endPoints = new EndPointsPackage();

            //Get All WebGets
            {
                List<MethodInfo> methods = this.GetType().GetMethods().Where(c => c.GetCustomAttributes(typeof(WebGetAttribute), false).Count() > 0).ToList();
                foreach (MethodInfo method in methods)
                {
                    WebGetAttribute webGet = method.GetCustomAttributes(typeof(WebGetAttribute), false)[0] as WebGetAttribute;
                    if (webGet != null && webGet.UriTemplate.StartsWith(versionKey))
                    {
                        EndPointData newEndPoint = new EndPointData();
                        newEndPoint.Type = "Get";
                        newEndPoint.Link = string.Format("{0}/{1}", m_ServiceHostData.ServiceHostUri, webGet.UriTemplate);
                        newEndPoint.Format = webGet.ResponseFormat.ToString();

                        endPoints.Add(newEndPoint);
                    }
                }
            }

            //Get all WebInvokes
            {
                List<MethodInfo> methods = this.GetType().GetMethods().Where(c => c.GetCustomAttributes(typeof(WebInvokeAttribute), false).Count() > 0).ToList();
                foreach (MethodInfo method in methods)
                {
                    WebInvokeAttribute webInvoke = method.GetCustomAttributes(typeof(WebGetAttribute), false)[0] as WebInvokeAttribute;
                    if (webInvoke != null && webInvoke.UriTemplate.StartsWith(versionKey))
                    {
                        EndPointData newEndPoint = new EndPointData();
                        newEndPoint.Type = webInvoke.Method;
                        newEndPoint.Link = string.Format("{0}/{1}", m_ServiceHostData.ServiceHostUri, webInvoke.UriTemplate);
                        newEndPoint.Format = webInvoke.ResponseFormat.ToString();

                        endPoints.Add(newEndPoint);
                    }
                }
            }

            return endPoints;
        }

        #region IRestHostable

        public void ServiceHostInjection(ServiceHostData data)
        {
            m_ServiceHostData = data;

            if (m_VersionDetails.Count > 0)
            {
                foreach (VersionDetails details in m_VersionDetails)
                {
                    details.Link = string.Format("{0}/{1}", data.ServiceHostUri, details.VersionKey);
                }
            }
        }

        #endregion

        //Internal Classes
        public class VersionDetails
        {
            public string VersionKey { get; set; }
            public string Link { get; set; }
        }

        [DataContract(Name = "VersionsPackage")]
        public class VersionsPackage
        {
            [DataMember(Name = "Versions")]
            public List<VersionData> Versions = new List<VersionData>();

            public void Add(VersionData version)
            {
                Versions.Add(version);
            }
        }

        [DataContract(Name = "VersionData")]
        public class VersionData
        {
            [DataMember(Name = "Version")]
            public string Version;
            [DataMember(Name = "Link")]
            public string Link;
        }

        [DataContract(Name = "EndPointPackage")]
        public class EndPointsPackage
        {
            [DataMember(Name = "EndPoints")]
            public List<EndPointData> EndPoints = new List<EndPointData>();

            public void Add(EndPointData endPoint)
            {
                EndPoints.Add(endPoint);
            }
        }

        [DataContract(Name = "EndPointData")]
        public class EndPointData
        {
            [DataMember(Name = "Link")]
            public string Link;
            [DataMember(Name = "Type")]
            public string Type;
            [DataMember(Name = "Format")]
            public string Format;
        }
    }
}
