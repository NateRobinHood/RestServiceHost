using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaRestApi.DataObjects
{
    public class AsanaProjectProgress
    {
        public string ProjectLink { get; set; }
        public string ProjectName { get; set; }
        public double ProjectProgress { get; set; }
    }
}
