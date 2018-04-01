using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaRestApi.DataObjects
{
    public class AsanaProject
    {
        public string ProjectLink { get; set; }
        public string ProjectName { get; set; }
        public string ProjectID { get; set; }
        public List<AsanaTaskSummary> Tasks { get; set; }
    }

    public class AsanaProjects
    {
        public List<AsanaProject> Projects { get; set; }
    }
}
