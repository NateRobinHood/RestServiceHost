using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsanaRestApi.DataObjects
{
    public class AsanaApiProject
    {
        public J_Project_Data data { get; set; }
    }

    public class AsanaApiTask
    {
        public J_Task_Data data { get; set; }
    }

    public class AsanaApiSubtask
    {
        public J_Subtask_Data data { get; set; }
    }

    public class AsanaApiProjects
    {
        public List<J_Ref> data { get; set; }
    }

    public class AsanaApiTasks
    {
        public List<J_Ref> data { get; set; }
    }

    public class AsanaApiSubtasks
    {
        public List<J_Ref> data { get; set; }
    }

    public class J_Ref
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    #region J_Task
    public class J_Task_Membership
    {
        public J_Ref project { get; set; }
        public J_Ref section { get; set; }
    }


    public class J_Task_Data
    {
        public long id { get; set; }
        public J_Ref assignee { get; set; }
        public string assignee_status { get; set; }
        public bool completed { get; set; }
        public object completed_at { get; set; }
        public DateTime created_at { get; set; }
        public object due_at { get; set; }
        public object due_on { get; set; }
        public List<J_Ref> followers { get; set; }
        public bool hearted { get; set; }
        public List<object> hearts { get; set; }
        public bool liked { get; set; }
        public List<object> likes { get; set; }
        public List<J_Task_Membership> memberships { get; set; }
        public DateTime modified_at { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public int num_hearts { get; set; }
        public int num_likes { get; set; }
        public object parent { get; set; }
        public List<J_Ref> projects { get; set; }
        public List<J_Ref> tags { get; set; }
        public J_Ref workspace { get; set; }
    }
    #endregion

    #region J_Subtask
    public class J_Subtask_Data
    {
        public long id { get; set; }
        public object assignee { get; set; }
        public string assignee_status { get; set; }
        public bool completed { get; set; }
        public DateTime? completed_at { get; set; }
        public DateTime? created_at { get; set; }
        public object due_at { get; set; }
        public object due_on { get; set; }
        public List<J_Ref> followers { get; set; }
        public bool hearted { get; set; }
        public List<object> hearts { get; set; }
        public bool liked { get; set; }
        public List<object> likes { get; set; }
        public List<object> memberships { get; set; }
        public DateTime? modified_at { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public int num_hearts { get; set; }
        public int num_likes { get; set; }
        public J_Ref parent { get; set; }
        public List<object> projects { get; set; }
        public List<object> tags { get; set; }
        public J_Ref workspace { get; set; }
    }
    #endregion

    #region J_Project
    public class J_Project_Data
    {
        public long id { get; set; }
        public bool archived { get; set; }
        public string color { get; set; }
        public DateTime created_at { get; set; }
        public object current_status { get; set; }
        public object due_date { get; set; }
        public List<J_Ref> followers { get; set; }
        public string layout { get; set; }
        public List<J_Ref> members { get; set; }
        public DateTime modified_at { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public J_Ref owner { get; set; }
        public bool @public { get; set; }
        public J_Ref team { get; set; }
        public J_Ref workspace { get; set; }
    }
    #endregion
}
