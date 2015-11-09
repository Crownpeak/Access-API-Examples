using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrownPeak.AccessAPI
{
  public partial class AssetUploadRequest
  {
    public AssetUploadRequest()
    {
      newName = string.Empty;
      destinationFolderId = -1;
      modelId = -1;
      workflowId = -1;
      bytes = null;
    }

    public AssetUploadRequest(string newName, int destinationFolderId)
    {
      this.newName = newName;
      this.destinationFolderId = destinationFolderId;
      modelId = -1;
      workflowId = -1;
      bytes = null;
    }
  }

  public partial class AssetExistsRequest
  {
    public AssetExistsRequest()
    {
      assetIdOrPath = string.Empty;
    }

    public AssetExistsRequest(int assetId)
    {
      assetIdOrPath = assetId.ToString();
    }

    public AssetExistsRequest(string path)
    {
      assetIdOrPath = path;
    }
  }

  public partial class AssetCreateRequest
  {
    public AssetCreateRequest()
    {
      destinationFolderId = -1;
      newName = string.Empty;
      type = -1;
      modelId = -1;
      workflowId = -1;
      templateId = -1;
    }

    public AssetCreateRequest(string newName, int destinationFolderId, AssetType type)
    {
      this.destinationFolderId = destinationFolderId;
      this.newName = newName;
      this.type = (int)type;
      modelId = -1;
      workflowId = -1;
      templateId = -1;
    }
  }

  public partial class ResultClass
  {
    public bool IsSuccessful
    {
      get
      {
        return this.ResultCode == eResultCodes.conWS_Success;
      }
    }
  }
}
