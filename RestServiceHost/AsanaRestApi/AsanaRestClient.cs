using AsanaRestApi.DataObjects;
using Newtonsoft.Json;
using RestHostable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace AsanaRestApi
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    [ServiceContract]
    public class AsanaRestClient : RestHostableBase
    {
        private const string SECTION_ID_BACKLOG = "613254468923882";
        private const string SECTION_ID_TO_DO = "613254468923889";
        private const string SECTION_ID_IN_PROGRESS = "613254468923890";
        private const string SECTION_ID_COMPLETED = "613254468923891";

        private const string ASANA_API_BASE_URL = "https://app.asana.com/api/1.0";
        private const string ASANA_API_TOEKN = "0/993ceb62879f7270e9b34f1c1a5525a2";

        //Private Methods
        private HttpWebRequest NewGetRequest(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Authorization", string.Format("{0} {1}", "Bearer", ASANA_API_TOEKN));
            request.Method = "GET";
            request.Accept = "*/*";

            return request;
        }

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

        public AsanaRestClient()
        {
            base.RegisterVersion("v1");

            //required to hit https for asana, otherwise get a connection forcably closed error
            ServicePointManager.ServerCertificateValidationCallback = (s, cert, chain, ssl) => true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        //Version 1
        [WebGet(UriTemplate = "v1/progress/{projectId}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public AsanaProjectProgress GetProgress(string projectId)
        {
            AsanaProjectProgress retProgress = new AsanaProjectProgress();

            AsanaProject asanaProject = GetProject(projectId);

            retProgress.ProjectLink = asanaProject.ProjectLink;
            retProgress.ProjectName = asanaProject.ProjectName;
            List<bool> completedStates = asanaProject.Tasks.Where(c => c.SectionID == SECTION_ID_TO_DO || c.SectionID == SECTION_ID_IN_PROGRESS || c.SectionID == SECTION_ID_COMPLETED)
                                                           .Select(c => c.TaskComplete).ToList();
            completedStates.AddRange(asanaProject.Tasks.SelectMany(c => c.Subtasks.Select(st => st.SubtaskComplete)));
            retProgress.ProjectProgress = (double)completedStates.Where(c => c == true).Count() / (double)completedStates.Count;

            return retProgress;
        }

        [WebGet(UriTemplate = "v1/project/{projectId}", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public AsanaProject GetProject(string projectId)
        {
            AsanaProject retProject = new AsanaProject();

            //Project
            HttpWebRequest request = NewGetRequest(string.Format("{0}/{1}", "https://app.asana.com/api/1.0/projects", projectId));
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = string.Empty;
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        content = sr.ReadToEnd();
                    }
                }

                AsanaApiProject apiProject = JsonConvert.DeserializeObject<AsanaApiProject>(content);

                retProject.Tasks = new List<AsanaTaskSummary>();
                retProject.ProjectID = apiProject.data.id.ToString();
                retProject.ProjectLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/projects", apiProject.data.id);
                retProject.ProjectName = apiProject.data.name;

                //Tasks
                request = NewGetRequest(string.Format("{0}/{1}/{2}", "https://app.asana.com/api/1.0/projects", projectId, "tasks"));
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    content = string.Empty;
                    using (Stream stream = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }

                    AsanaApiTasks apiTasks = JsonConvert.DeserializeObject<AsanaApiTasks>(content);

                    foreach (J_Ref task in apiTasks.data)
                    {
                        //Task
                        request = NewGetRequest(string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", task.id));
                        response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            content = string.Empty;
                            using (Stream stream = response.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    content = sr.ReadToEnd();
                                }
                            }

                            AsanaApiTask apiTask = JsonConvert.DeserializeObject<AsanaApiTask>(content);

                            AsanaTaskSummary retTask = new AsanaTaskSummary();
                            retTask.Subtasks = new List<AsanaSubtaskSummary>();
                            retTask.TaskID = task.id.ToString();
                            retTask.TaskLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", task.id);
                            retTask.TaskName = apiTask.data.name;
                            retTask.TaskComplete = apiTask.data.completed;
                            if (apiTask.data.memberships.Count > 0)
                            {
                                retTask.SectionName = apiTask.data.memberships[0].section.name;
                                retTask.SectionID = apiTask.data.memberships[0].section.id.ToString();
                            }

                            //Subtasks
                            request = NewGetRequest(string.Format("{0}/{1}/{2}", "https://app.asana.com/api/1.0/tasks", task.id, "subtasks"));
                            response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                content = string.Empty;
                                using (Stream stream = response.GetResponseStream())
                                {
                                    using (StreamReader sr = new StreamReader(stream))
                                    {
                                        content = sr.ReadToEnd();
                                    }
                                }

                                AsanaApiSubtasks apiSubtasks = JsonConvert.DeserializeObject<AsanaApiSubtasks>(content);

                                foreach (J_Ref subtask in apiSubtasks.data)
                                {
                                    //Subtask
                                    request = NewGetRequest(string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", subtask.id));
                                    response = (HttpWebResponse)request.GetResponse();
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        content = string.Empty;
                                        using (Stream stream = response.GetResponseStream())
                                        {
                                            using (StreamReader sr = new StreamReader(stream))
                                            {
                                                content = sr.ReadToEnd();
                                            }
                                        }

                                        AsanaApiSubtask apiSubtask = JsonConvert.DeserializeObject<AsanaApiSubtask>(content);

                                        AsanaSubtaskSummary retSubtask = new AsanaSubtaskSummary();
                                        retSubtask.SubtaskLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", subtask.id);
                                        retSubtask.SubtaskID = subtask.id.ToString();
                                        retSubtask.SubtaskName = apiSubtask.data.name;
                                        retSubtask.SubtaskComplete = apiSubtask.data.completed;

                                        retTask.Subtasks.Add(retSubtask);
                                    }
                                }
                            }

                            retProject.Tasks.Add(retTask);
                        }
                    }
                }
            }

            return retProject;
        }

        [WebGet(UriTemplate = "v1/projects", ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        public AsanaProjects GetProjects()
        {
            AsanaProjects retProjects = new AsanaProjects();
            retProjects.Projects = new List<AsanaProject>();

            //Projects
            HttpWebRequest request = NewGetRequest("https://app.asana.com/api/1.0/projects");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string content = string.Empty;
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        content = sr.ReadToEnd();
                    }
                }

                AsanaApiProjects apiProjects = JsonConvert.DeserializeObject<AsanaApiProjects>(content);

                foreach (J_Ref project in apiProjects.data)
                {
                    //Project
                    request = NewGetRequest(string.Format("{0}/{1}","https://app.asana.com/api/1.0/projects", project.id));
                    response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        content = string.Empty;
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                content = sr.ReadToEnd();
                            }
                        }

                        AsanaApiProject apiProject = JsonConvert.DeserializeObject<AsanaApiProject>(content);

                        AsanaProject retProject = new AsanaProject();
                        retProject.Tasks = new List<AsanaTaskSummary>();
                        retProject.ProjectID = apiProject.data.id.ToString();
                        retProject.ProjectLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/projects", apiProject.data.id);
                        retProject.ProjectName = apiProject.data.name;

                        //Tasks
                        request = NewGetRequest(string.Format("{0}/{1}/{2}", "https://app.asana.com/api/1.0/projects", project.id, "tasks"));
                        response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            content = string.Empty;
                            using (Stream stream = response.GetResponseStream())
                            {
                                using (StreamReader sr = new StreamReader(stream))
                                {
                                    content = sr.ReadToEnd();
                                }
                            }

                            AsanaApiTasks apiTasks = JsonConvert.DeserializeObject<AsanaApiTasks>(content);

                            foreach (J_Ref task in apiTasks.data)
                            {
                                //Task
                                request = NewGetRequest(string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", task.id));
                                response = (HttpWebResponse)request.GetResponse();
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    content = string.Empty;
                                    using (Stream stream = response.GetResponseStream())
                                    {
                                        using (StreamReader sr = new StreamReader(stream))
                                        {
                                            content = sr.ReadToEnd();
                                        }
                                    }

                                    AsanaApiTask apiTask = JsonConvert.DeserializeObject<AsanaApiTask>(content);

                                    AsanaTaskSummary retTask = new AsanaTaskSummary();
                                    retTask.Subtasks = new List<AsanaSubtaskSummary>();
                                    retTask.TaskID = task.id.ToString();
                                    retTask.TaskLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", task.id);
                                    retTask.TaskName = apiTask.data.name;
                                    retTask.TaskComplete = apiTask.data.completed;
                                    if (apiTask.data.memberships.Count > 0)
                                    {
                                        retTask.SectionName = apiTask.data.memberships[0].section.name;
                                        retTask.SectionID = apiTask.data.memberships[0].section.id.ToString();
                                    }

                                    //Subtasks
                                    request = NewGetRequest(string.Format("{0}/{1}/{2}", "https://app.asana.com/api/1.0/tasks", task.id, "Subtasks"));
                                    response = (HttpWebResponse)request.GetResponse();
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        content = string.Empty;
                                        using (Stream stream = response.GetResponseStream())
                                        {
                                            using (StreamReader sr = new StreamReader(stream))
                                            {
                                                content = sr.ReadToEnd();
                                            }
                                        }

                                        AsanaApiSubtasks apiSubtasks = JsonConvert.DeserializeObject<AsanaApiSubtasks>(content);

                                        foreach (J_Ref subtask in apiSubtasks.data)
                                        {
                                            //Subtask
                                            request = NewGetRequest(string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", subtask.id));
                                            response = (HttpWebResponse)request.GetResponse();
                                            if (response.StatusCode == HttpStatusCode.OK)
                                            {
                                                content = string.Empty;
                                                using (Stream stream = response.GetResponseStream())
                                                {
                                                    using (StreamReader sr = new StreamReader(stream))
                                                    {
                                                        content = sr.ReadToEnd();
                                                    }
                                                }

                                                AsanaApiSubtask apiSubtask = JsonConvert.DeserializeObject<AsanaApiSubtask>(content);

                                                AsanaSubtaskSummary retSubtask = new AsanaSubtaskSummary();
                                                retSubtask.SubtaskLink = string.Format("{0}/{1}", "https://app.asana.com/api/1.0/tasks", subtask.id);
                                                retSubtask.SubtaskID = subtask.id.ToString();
                                                retSubtask.SubtaskName = apiSubtask.data.name;
                                                retSubtask.SubtaskComplete = apiSubtask.data.completed;

                                                retTask.Subtasks.Add(retSubtask);
                                            }
                                        }
                                    }

                                    retProject.Tasks.Add(retTask);
                                }
                            }
                        }

                        retProjects.Projects.Add(retProject);
                    }
                }
            }

            return retProjects;
        }
    }
}
