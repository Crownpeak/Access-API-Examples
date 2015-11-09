using CrownPeak.AccessAPI;
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
  public class AccessAsset
  {
    private cpHttpClient Client;
    public AccessAsset(cpHttpClient client)
    {
      Client = client;
    }

    public AssetReadByIdResponse Read(int id)
    {
      return process<AssetReadByIdResponse>("Read/" + id.ToString(), string.Empty);
    }

    public AssetPagedResponse Paged(AssetPagedRequest request)
    {
      return process<AssetPagedResponse>("Paged", request);
    }

    public AssetCreateResponse Create(AssetCreateRequest request)
    {
      return process<AssetCreateResponse>("Create", request);
    }

    public AssetCreateResponse Create(string newName, int destinationFolderId, AssetType type)
    {
      return process<AssetCreateResponse>("Create", new AssetCreateRequest(newName, destinationFolderId, type));
    }

    public AssetUpdateResponse Update(AssetUpdateRequest request)
    {
      return process<AssetUpdateResponse>("Update", request);
    }

    public AssetDeleteResponse Delete(int id)
    {
      return process<AssetDeleteResponse>("Delete/" + id.ToString(), string.Empty);
    }

    public AssetUndeleteResponse Undelete(int id)
    {
      return process<AssetUndeleteResponse>("Undelete/" + id.ToString(), string.Empty);
    }

    public AssetRenameResponse Rename(AssetRenameRequest request)
    {
      return process<AssetRenameResponse>("Rename", request);
    }

    public AssetRouteResponse Route(AssetRouteRequest request)
    {
      return process<AssetRouteResponse>("Route", request);
    }

    public AssetBranchResponse Branch(int id)
    {
      return process<AssetBranchResponse>("Branch/" + id.ToString(), string.Empty);
    }

    public AssetFieldsResponse Fields(int id)
    {
      return process<AssetFieldsResponse>("Fields/" + id.ToString(), string.Empty);
    }

    public AssetUploadResponse Upload(AssetUploadRequest request)
    {
      return process<AssetUploadResponse>("Upload", request);
    }

    public AssetAttachResponse Attach(AssetAttachRequest request)
    {
      return process<AssetAttachResponse>("Attach", request);
    }

    public AssetExecuteWorkflowCommandResponse ExecuteWorkflowCommand(AssetExecuteWorkflowCommandRequest request)
    {
      return process<AssetExecuteWorkflowCommandResponse>("ExecuteWorkflowCommand", request);
    }

    public AssetExistsResponse Exists(AssetExistsRequest request)
    {
      return process<AssetExistsResponse>("Exists", request);
    }

    private TResponse process<TResponse>(string action, object postData)
    {
      Client.SetupAccessRequest("Asset", action, Newtonsoft.Json.JsonConvert.SerializeObject(postData).ToString());
      return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(Client.CaptureToJsonString());
    }
  }
}
