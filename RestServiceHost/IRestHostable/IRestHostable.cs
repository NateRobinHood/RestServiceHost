using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestHostable
{
    public interface IRestHostable
    {
        void ServiceHostInjection(ServiceHostData data);
    }

    public class ServiceHostData
    {
        public ServiceHostData()
        {
        }

        public string ServiceHostUri { get; set; }
    }
}
