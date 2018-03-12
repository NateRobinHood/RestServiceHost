using RestHostable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace TestRestHost
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [ServiceContract]
    public class TestHost : IRestHostable
    {
        private ServiceHostData m_ServiceHostData = null;

        [WebGet(UriTemplate = "Versions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public VersionsPackage Versions()
        {
            VersionsPackage versions = new VersionsPackage();
            versions.Add(new VersionData() { Version = "v1", Link = string.Format("{0}/{1}", m_ServiceHostData.ServiceHostUri, "v1") });
            return versions;
        }

        [WebGet(UriTemplate = "v1", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public EndPointsPackage V1_EndPoints()
        {
            EndPointsPackage endPoints = new EndPointsPackage();
            string versions1 = "v1";

            //Get All WebGets
            List<MethodInfo> methods = this.GetType().GetMethods().Where(c => c.GetCustomAttributes(typeof(WebGetAttribute), false).Count() > 0).ToList();
            foreach (MethodInfo method in methods)
            {
                WebGetAttribute webGet = method.GetCustomAttributes(typeof(WebGetAttribute), false)[0] as WebGetAttribute;
                if (webGet != null && webGet.UriTemplate.StartsWith(versions1))
                {
                    EndPointData newEndPoint = new EndPointData();
                    newEndPoint.Type = "Get";
                    newEndPoint.Link = string.Format("{0}/{1}", m_ServiceHostData.ServiceHostUri, webGet.UriTemplate);
                    newEndPoint.Format = webGet.ResponseFormat.ToString();

                    endPoints.Add(newEndPoint);
                }
            }

            return endPoints;
        }

        [WebGet(UriTemplate = "v1/TestData/Incremental", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData V1_Incremental()
        {
            return new TestData() { ValueA = "1", ValueB = "2", ValueC = "3", ValueD = "4" };
        }

        [WebGet(UriTemplate = "v1/TestData/Decremental", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData V1_Decremental()
        {
            return new TestData() { ValueA = "4", ValueB = "3", ValueC = "2", ValueD = "1" };
        }

        #region IRestHostable

        public void ServiceHostInjection(ServiceHostData data)
        {
            m_ServiceHostData = data;
        }

        #endregion

        [DataContract(Name = "TestData", Namespace = "")]
        public class TestData
        {
            [DataMember(Name = "ValueA", Order = 1)]
            public string ValueA;
            [DataMember(Name = "ValueB", Order = 2)]
            public string ValueB;
            [DataMember(Name = "ValueC", Order = 3)]
            public string ValueC;
            [DataMember(Name = "ValueD", Order = 4)]
            public string ValueD;
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
