#if (!ACCESS_API_HELPER)
using CrownPeakApp.Model;
using CrownPeakApp.App;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CrownPeak.AccessAPI
{
  #region AssetController
  #region AssetRead
  [DataContract]
  public partial class AssetReadResponse : ResultClass
  {
    public AssetReadResponse() { }

    public AssetReadResponse(ResultClass result)
      : base(result) {}
  }
  #endregion

  #region AssetReadById
  [DataContract]
  public partial class AssetReadByIdResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    public AssetReadByIdResponse() { }

    public AssetReadByIdResponse(ResultClass result, WorklistAsset asset)
      : base(result)
    {
      this.asset = asset;
    }
  }
  #endregion

  #region AssetPaged
  [DataContract]
  public partial class AssetPagedRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public int currentPage;

    [DataMember]
    public int pageSize;

    [DataMember]
    public string sortColumn;

    [DataMember]
#if (!ACCESS_API_HELPER)
    public cpAsset.OrderType orderType;
#else
    public cpAssetOrderType orderType;
#endif

    [DataMember]
#if (!ACCESS_API_HELPER)
    public cpAsset.VisibilityType visibilityType;
#else
    public cpAssetVisibilityType visibilityType;
#endif
    
    [DataMember]
    public bool ignoreFilter;

    [DataMember]
    public bool ignoreSort;
  }

  [DataContract]
  public partial class AssetPagedResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset[] assets;

    [DataMember]
    public int normalCount;

    [DataMember]
    public int hiddenCount;

    [DataMember]
    public int deletedCount;

    public AssetPagedResponse() { }

    public AssetPagedResponse(ResultClass result, WorklistAsset[] assets, int normalCount, int hiddenCount, int deletedCount)
      : base(result)
    {
      this.assets = assets;
      this.normalCount = normalCount;
      this.hiddenCount = hiddenCount;
      this.deletedCount = deletedCount;
    }
  }
  #endregion

  #region AssetCreate
  [DataContract]
  public partial class AssetCreateRequest
  {
    [DataMember]
    public string newName;
    [DataMember]
    public int destinationFolderId;
    [DataMember]
    public int modelId;
    [DataMember]
    public int type;
    [DataMember]
    public int devTemplateLanguage;
    [DataMember]
    public int templateId;
    [DataMember]
    public int workflowId;
  }

  [DataContract]
  public partial class AssetCreateResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    public AssetCreateResponse() { }

    public AssetCreateResponse(ResultClass result, WorklistAsset asset)
      : base(result)
    {
      this.asset = asset;
    }
  }
  #endregion

  #region AssetUpdate
  [DataContract]
  public partial class AssetUpdateRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public Dictionary<string, string> fields;
  }

  [DataContract]
  public partial class AssetUpdateResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    public AssetUpdateResponse() { }

    public AssetUpdateResponse(ResultClass result, WorklistAsset asset)
      : base(result)
    {
      this.asset = asset;
    }
  }
  #endregion

  #region AssetDelete
  [DataContract]
  public partial class AssetDeleteResponse : ResultClass
  {
    public AssetDeleteResponse() { }

    public AssetDeleteResponse(ResultClass result)
      : base(result) { }
  }
  #endregion

  #region AssetUndelete
  [DataContract]
  public partial class AssetUndeleteResponse : ResultClass
  {
    public AssetUndeleteResponse() { }

    public AssetUndeleteResponse(ResultClass result)
      : base(result) { }
  }
  #endregion

  #region AssetRename
  [DataContract]
  public partial class AssetRenameRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public string newName;
  }

  [DataContract]
  public partial class AssetRenameResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    public AssetRenameResponse() { }

    public AssetRenameResponse(ResultClass result, WorklistAsset asset)
      : base(result)
    {
      this.asset = asset;
    }
  }
  #endregion

  #region AssetRoute
  [DataContract]
  public partial class AssetRouteRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public int stateId;
  }

  [DataContract]
  public partial class AssetRouteResponse : ResultClass
  {
    public AssetRouteResponse() { }

    public AssetRouteResponse(ResultClass result)
      : base(result) {}
  }
  #endregion

  #region AssetBranch
  [DataContract]
  public partial class AssetBranchResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    public AssetBranchResponse() { }

    public AssetBranchResponse(ResultClass result, WorklistAsset asset)
      : base(result)
    {
      this.asset = asset;
    }
  }
  #endregion

  #region AssetFields
  [DataContract]
  [KnownType(typeof(AssetsField))]
  public partial class AssetFieldsResponse : ResultClass
  {
    [DataMember]
    public List<AssetsField> fields { get; set; }

    public AssetFieldsResponse() { }

    public AssetFieldsResponse(ResultClass result, List<AssetsField> fields)
      : base(result)
    {
      this.fields = fields;
    }
  }
  #endregion

  #region AssetUpload
  [DataContract]
  public partial class AssetUploadRequest
  {
    [DataMember]
    public string newName;
    [DataMember]
    public int destinationFolderId;
    [DataMember]
    public int modelId;
    [DataMember]
    public int workflowId;
    [DataMember]
    public byte[] bytes;
  }

  [DataContract]
  public partial class AssetUploadResponse : ResultClass
  {
    [DataMember]
    public WorklistAsset asset;

    [DataMember]
    public string displayUrl;

    public AssetUploadResponse() { }

    public AssetUploadResponse(ResultClass result, WorklistAsset asset, string displayUrl)
      : base(result)
    {
      this.asset = asset;
      this.displayUrl = displayUrl;
    }
  }
  #endregion

  #region AssetAttach
  [DataContract]
  public partial class AssetAttachRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public string originalFilename;

    [DataMember]
    public byte[] bytes;
  }

  [DataContract]
  public partial class AssetAttachResponse : ResultClass
  {
    [DataMember]
    public string displayUrl;

    public AssetAttachResponse() { }

    public AssetAttachResponse(ResultClass result, string displayUrl)
      : base(result) 
    {
      this.displayUrl = displayUrl;
    }
  }
  #endregion

  #region AssetExecuteWorkflowCommand
  [DataContract]
  public partial class AssetExecuteWorkflowCommandRequest
  {
    [DataMember]
    public int assetId;

    [DataMember]
    public int commandId;

    [DataMember]
    public bool skipDependencies;
  }

  [DataContract]
  public partial class AssetExecuteWorkflowCommandResponse : ResultClass
  {
    [DataMember]
    public cpWorkflowActionRequired requiredAction;
    [DataMember]
    public WorklistAsset asset;
    [DataMember]
    public int publishingSessionId;

    public AssetExecuteWorkflowCommandResponse()
    {
    }
    public AssetExecuteWorkflowCommandResponse(ResultClass result, cpWorkflowActionRequired requiredAction, int publishingSessionId, WorklistAsset asset)
      : base(result)
    {
      this.requiredAction = requiredAction;
      this.publishingSessionId = publishingSessionId;
      this.asset = asset;
    }
  }
  #endregion

  #region AssetExists
  [DataContract]
  public partial class AssetExistsRequest
  {
    [DataMember]
    public string assetIdOrPath;
  }

  [DataContract]
  public partial class AssetExistsResponse : ResultClass
  {
    [DataMember]
    public bool exists;

    [DataMember]
    public int assetId;

    public AssetExistsResponse()
    {
    }
    public AssetExistsResponse(ResultClass result, bool assetExists, int assetId)
      : base(result)
    {
      this.exists = assetExists;
      this.assetId = assetId;
    }
  }
  #endregion
  #endregion

  #region WorkflowController
  #region WorkflowRead
  [DataContract]
  [KnownType(typeof(WorkflowData))]
  public partial class WorkflowReadResponse : ResultClass
  {
    [DataMember]
    public Dictionary<int, WorkflowData> workflows;

    public WorkflowReadResponse() { }

    public WorkflowReadResponse(ResultClass result, Dictionary<int, WorkflowData> workflows)
      : base(result)
    {
      this.workflows = workflows;
    }
  }
  #endregion

  #region WorkflowReadById
  [DataContract]
  [KnownType(typeof(WorkflowData))]
  public partial class WorkflowReadByIdResponse : ResultClass
  {
    [DataMember]
    public WorkflowData workflow;

    public WorkflowReadByIdResponse() { }

    public WorkflowReadByIdResponse(ResultClass result, WorkflowData workflow)
      : base(result)
    {
      this.workflow = workflow;
    }
  }
  #endregion

  #region WorkflowCreate
  [DataContract]
  [KnownType(typeof(WorkflowData))]
  public partial class WorkflowCreateResponse : ResultClass
  {
    public WorkflowCreateResponse() { }

    public WorkflowCreateResponse(ResultClass result)
      : base(result) {}
  }
  #endregion

  #region WorkflowUpdate
  [DataContract]
  [KnownType(typeof(WorkflowData))]
  public partial class WorkflowUpdateResponse : ResultClass
  {
    public WorkflowUpdateResponse() { }

    public WorkflowUpdateResponse(ResultClass result)
      : base(result) {}
  }
  #endregion

  #region WorkflowDelete
  [DataContract]
  [KnownType(typeof(WorkflowData))]
  public partial class WorkflowDeleteResponse : ResultClass
  {
    public WorkflowDeleteResponse() { }

    public WorkflowDeleteResponse(ResultClass result)
      : base(result) {}
  }
  #endregion

  #endregion

  #region AuthController
  #region AuthAuthenticate
  [DataContract]
  public partial class AuthAuthenticateRequest
  {
    [DataMember]
    public string instance;
    [DataMember]
    public string username;
    [DataMember]
    public string password;
    [DataMember]
    public bool remember_me;
    [DataMember]
    public int timeZoneOffsetMinutes;
  }

  [DataContract]
  public partial class AuthAuthenticateResponse : ResultClass
  {
    public AuthAuthenticateResponse() { }

    public AuthAuthenticateResponse(ResultClass result)
      : base(result) {}
  }
  #endregion
  
  #region AuthAuthenticateWithCache
  [DataContract]
  public partial class AuthAuthenticateWithCacheRequest
  {
    [DataMember]
    public string instance;
    [DataMember]
    public string username;
    [DataMember]
    public string password;
    [DataMember]
    public bool remember_me;
    [DataMember]
    public int timeZoneOffsetMinutes;
  }

  [DataContract]
  [KnownType(typeof(System.Collections.Generic.KeyValuePair<string, string>))]
  [KnownType(typeof(System.Collections.Generic.KeyValuePair<int, ActionData>))]
  public partial class AuthAuthenticateWithCacheResponse : WSResultClass
  {
    [DataMember]
    public UserData user;

    [DataMember]
    public int systemTemplatesPathId;

    [DataMember]
    public int systemModelsPathId;

    [DataMember]
    public int daysToExpire;

    [DataMember]
    public bool needsExpirationWarning;

    [DataMember]
    public int idleTimeoutMinutes;

    [DataMember]
    public int taskCount;

    [DataMember]
    public int workflowTaskCount;

    [DataMember]
    public int tasksFolderId;

    [DataMember]
    public int taskBaseModelId;

    [DataMember]
    public Dictionary<int, ActionData> actions;

    [DataMember]
    public WysiwygEditorType instanceWysiwygEditor;

    [DataMember]
#if (!ACCESS_API_HELPER)
    public List<cpLists.cpKeyValuePair> uiConfiguration;
#else
    public List<cpListscpKeyValuePair> uiConfiguration;
#endif

    [DataMember]
    public Dictionary<int, WorkflowData> workflowData;

    [DataMember]
    public Dictionary<int, StatusData> statusData;

    [DataMember]
    public List<WCOBeaconSiteData> wcoBeaconSites;

    public AuthAuthenticateWithCacheResponse()
    {
    }

    public AuthAuthenticateWithCacheResponse(AuthenticateResponseWCF result)
      : base((WSResultClass)result)
    {
      this.user = result.user;
      this.systemTemplatesPathId = result.SystemTemplatesPathId;
      this.systemModelsPathId = result.SystemModelsPathId;
      this.daysToExpire = result.DaysToExpire;
      this.needsExpirationWarning = result.NeedsExpirationWarning;
      this.idleTimeoutMinutes = result.IdleTimeoutMinutes;
      this.taskCount = result.TaskCount;
      this.workflowTaskCount = result.WorkflowTaskCount;
      this.tasksFolderId = result.TasksFolderId;
      this.taskBaseModelId = result.TaskBaseModelId;
      this.actions = result.Actions;
      this.instanceWysiwygEditor = result.InstanceWysiwygEditor;
      this.uiConfiguration = result.UIConfiguration.ToList();
      this.workflowData = result.WorkflowData;
      this.statusData = result.StatusData;
      this.wcoBeaconSites = result.WCOBeaconSites.ToList();
    }
  }
  #endregion

  #region AuthLogout
  [DataContract]
  public partial class AuthLogoutResponse : ResultClass
  {
    public AuthLogoutResponse() { }

    public AuthLogoutResponse(ResultClass result)
      : base(result) {}
  }
  #endregion
  #endregion
}