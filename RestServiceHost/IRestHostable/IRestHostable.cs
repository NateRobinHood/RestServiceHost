using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace RestHostable
{
    public interface IRestHostable
    {
        void ServiceHostInjection(ServiceHostData data);
    }

    //Internal Classes
    public class ServiceHostData
    {
        public ServiceHostData()
        {
        }

        public string ServiceHostUri { get; set; }
    }
}
