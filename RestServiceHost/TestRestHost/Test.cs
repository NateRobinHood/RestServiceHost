using RestServiceHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace TestRestHost
{
    [ServiceContract]
    public class TestHost : IRestHostable
    {
        [WebGet(UriTemplate = "/TestData/Incremental", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData Incremental()
        {
            return new TestData() { ValueA = "1", ValueB = "2", ValueC = "3", ValueD = "4" };
        }

        [WebGet(UriTemplate = "/TestData/Decremental", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public TestData decremental()
        {
            return new TestData() { ValueA = "4", ValueB = "3", ValueC = "2", ValueD = "1" };
        }

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
