using CrownPeak.AccessAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CrownPeakPublic.AccessAPI
{
  /// <summary>
  /// Pass the Client property from a AccessSession object into the constructor.
  /// </summary>
  public class AccessWorkflow
  {
    private cpHttpClient Client;
    public AccessWorkflow(cpHttpClient client)
    {
      Client = client;
    }

    public WorkflowReadResponse Read()
    {
      return process<WorkflowReadResponse>("Read", string.Empty);
    }

    public WorkflowReadByIdResponse Read(int id)
    {
      return process<WorkflowReadByIdResponse>("Read/" + id.ToString(), string.Empty);
    }

    private TResponse process<TResponse>(string action, object postData)
    {
      Client.SetupAccessRequest("Workflow", action, Newtonsoft.Json.JsonConvert.SerializeObject(postData).ToString());
      return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(Client.CaptureToJsonString());
    }
  }
}
