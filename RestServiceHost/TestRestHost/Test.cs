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
    public class TestHost : RestHostableBase
    {
        [WebGet(UriTemplate = "Versions", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public VersionsPackage GetVersions()
        {
            return base.Versions();
        }

        [WebGet(UriTemplate = "v1", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public EndPointsPackage V1_EndPoints()
        {
            return base.GetEndpoints("v1");
        }

        [WebGet(UriTemplate = "v2", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public EndPointsPackage V2_EndPoints()
        {
            return base.GetEndpoints("v2");
        }

        public TestHost()
        {
            base.RegisterVersion("v1");
            base.RegisterVersion("v2");
        }

        //Version 1
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

        //Version 2
        [WebGet(UriTemplate = "v2/TestData/Incremental?start={startNumber}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData V2_Incremental(int startNumber)
        {
            return new TestData() { ValueA = (startNumber).ToString(), ValueB = (startNumber + 1).ToString(), ValueC = (startNumber + 2).ToString(), ValueD = (startNumber + 3).ToString() };
        }

        [WebGet(UriTemplate = "v2/TestData/Decremental?start={startNumber}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData V2_Decremental(int startNumber)
        {
            return new TestData() { ValueA = (startNumber).ToString(), ValueB = (startNumber - 1).ToString(), ValueC = (startNumber - 2).ToString(), ValueD = (startNumber - 3).ToString() };
        }

        //Internal Classes
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
    }
}
