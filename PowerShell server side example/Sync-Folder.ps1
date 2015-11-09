# -------------------------------------------------------------------------------------------------------
# Example code for using the AccessAPI Restlike endpoint
# This code will show how to import binary assets from a Source Folder, mantaining the folder structure.
#
# Example usage: .\sync-folder.ps1 c:\temp\Source-Sync-Folder '/DevSite/sync-root' -recurse -whatif
#
# -------------------------------------------------------------------------------------------------------
param([System.String]$startingFolder, [System.String]$destCMSPath, [System.Management.Automation.SwitchParameter]$recurse, [System.Management.Automation.SwitchParameter]$whatif)
Import-Module .\CP_AccessAPI.psm1 -Force

#region 'script setup'
set-CMS_Config '.\config.json'  #sets server/instance/accesskey settings

$cred = get-CMS_Credentials #
#endregion

#region helper functions
function GetCachedFolderId([string]$path) {
  if ($folderCache.ContainsKey($path)) {
    $folderId = $folderCache[$path]
  }
  return $folderId
}
function SetCachedFolderId([string]$path, [int]$assetId) {
  if (!$folderCache.ContainsKey($path)) {
    $folderCache.Add($path, $assetId)
  }
}
function GetFolderTree([string]$thePathToCheck, [string]$path, [string]$name) {
  $part = $thePathToCheck.Replace($path,'')
  if ($name -ne '') {
   $part = $part.Replace($name.ToLower(),'')
  }
  if ($part.EndsWith('/')) {
    $part = $part.Substring(0,$part.Length-1)
  }
  $folderTree = $part.split('/')
  if ($folderTree.Length -eq 2 -and $folderTree[0] -eq '' -and $folderTree[1] -eq '') {
    $folderTree = @()
  }
  return $folderTree
}
#endregion

Write-Host "Sync: $startingFolder => $destCMSPath" -foregroundcolor green

if (!(Test-Path $startingFolder))
{
  Write-Host "Unable to find $startingFolder" -ForegroundColor red
  return
}

#get list of files/folders to upload
if ($recurse)
{
  $items = Get-ChildItem $startingFolder -Recurse
}
else
{
  $items = Get-ChildItem $startingFolder
}

#only process if we found something to synchronize
if ($items -ne $null -and $items.length -gt 0)
{
  $authResp = Invoke-CMS_Authenticate $cred
  if ($authResp -ne $null)
  {
    #make sure destination CMS path exists
    $result = Invoke-CMS_AssetExists $destCMSPath
    if (!$result.Exists)
    {
      #todo: create folder structure if missing
      Write-Host "Unable to find destination CMS path: $destCMSPath" -ForegroundColor red
      return
    }
    $folderId = $result.assetId
    $startingFolderId = $folderId
    Write-Host "Synchronizing with CMS path: $destCMSPath ($folderId)" -ForegroundColor Yellow

    $folderCache = @{}
    $folderCache.Add($destCMSPath, $folderId)
    $parentPath = $destCMSPath
    $startingFolder = $startingFolder.ToLower()
    foreach ($item in $items)
    {
      $thePathToCheck = "$($item.FullName.ToLower().Replace($startingFolder,$destCMSPath).Replace('\','/'))"
      if ($item.PSIsContainer)
      {
        #found a folder, traverse the volder
        $folderTree = GetFolderTree $thePathToCheck $destCMSPath ''
        $rootPath = $destCMSPath
        foreach ($folder in $folderTree)
        {
          $thePathToCheck = (Join-Path $rootPath $folder).Replace('\','/')
          $rootPath = $thePathToCheck
          $folderId = GetCachedFolderId $rootPath
          $result = Invoke-CMS_AssetExists $thePathToCheck
          if ($result.Exists)
          {
            SetCachedFolderId $rootPath $result.assetId
            Write-Host "Folder already exists: $thePathToCheck - $($result.assetId)" -ForegroundColor yellow
          }
          else
          {
            if (!$whatif)
            {
              Write-Host "Creating Folder: $thePathToCheck in $folderId"
              $result = Invoke-CMS_CreateAsset $item.Name $folderId -1 $conFolderType              
              SetCachedFolderId $thePathToCheck $result.asset.id
            }
            else
            {
              Write-Host "Would create Folder: $thePathToCheck in $folderId"
            }
          }
        }
      }
      else
      {
        $folderTree = GetFolderTree $thePathToCheck $destCMSPath $item.Name
        $result = Invoke-CMS_AssetExists $thePathToCheck
        if ($result.Exists)
        {
          Write-Host "File already exists: $thePathToCheck - $($result.assetId)" -ForegroundColor yellow
        }
        else
        {
          #todo: refactor this part to make it more readable
          $rootPath = $destCMSPath
          $thePathToCheck1  = $rootPath
          foreach ($folder in $folderTree)
          {
            $thePathToCheck1 = (Join-Path $thePathToCheck1 $folder).Replace('\','/')
            $folderId = GetCachedFolderId $thePathToCheck1
          }
          #end todo
          if (!$whatif)
          {
            Write-Host "Creating File: $thePathToCheck in folder $folderId"
            $bytesRaw = Get-Content $item.FullName -Encoding byte
            $bytes = [System.Convert]::ToBase64String($bytesRaw)
            $result = Invoke-CMS_UploadAsset $item.Name $folderId -1 0 $bytes
          }
          else
          {
            Write-Host "Would create File: $thePathToCheck in folder id = $folderId"
          }
        }
      }
    }
    
    #completed the sync, now display the assets in the top folder    
    $result = Invoke-CMS_GetPagedAssets $startingFolderId
    $assets = $result.assets 
    $assets | ft -AutoSize -Property fullpath, iconType

    #logout
    Invoke-CMS_Logout
    }
}