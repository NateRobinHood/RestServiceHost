using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaRestApi.DataObjects
{
    public class AsanaTaskSummary
    {
        public List<AsanaSubtaskSummary> Subtasks { get; set; }
        public string TaskID { get; set; }
        public string TaskName { get; set; }
        public string TaskLink { get; set; }
        public bool TaskComplete { get; set; }
        public string SectionName { get; set; }
        public string SectionID { get; set; }
    }

    public class AsanaSubtaskSummary
    {
        public string SubtaskID { get; set; }
        public string SubtaskName { get; set; }
        public string SubtaskLink { get; set; }
        public bool SubtaskComplete { get; set; }
    }
}
