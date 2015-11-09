using CrownPeak.AccessAPI;
using CrownPeakPublic.AccessAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

// ***********************************************************************************
//
// Summary: Uploads files from a local folder to the destination CMS instance folder.
//
// Note:  configure settings in App.config first.
//
// Example usage: SyncFolder.exe c:\temp\Source-Sync-Folder /DevSite/sync-root
//
// **********************************************************************************
namespace SyncFolder
{
  class Program
  {
    private static Dictionary<string, int> folderCache = new Dictionary<string, int>(); //folder id cache, so we don't have excessive asset path lookups
    private static AccessAsset accessAsset; //for accessability, used in multiple methods
    
    static void Main(string[] args)
    {
      try
      {
        #region handle arguments and validation
        if (args.Length < 2)
        {
          Console.WriteLine("Usage: syncFolder [local folder to synchronize] [destination CMS folder]");
        }
        string localFolder = args[0];
        string destFolder = args[1];

        if (!Directory.Exists(localFolder))
        {
          Console.WriteLine("Could not find local folder: {0}", localFolder);
          return;
        }
        #endregion

        #region get password
        string username = ConfigurationSettings.AppSettings["username"];
        Console.Write("Enter pw for {0}: ", username);
        string pw = getSecuredString();
        Console.WriteLine("");
        Console.WriteLine("Authenticating: {0} ", username);
        #endregion

        Console.WriteLine("Communicating with {0}/{1}", ConfigurationSettings.AppSettings["host"], ConfigurationSettings.AppSettings["instance"]);
        var session = new AccessSession( ConfigurationSettings.AppSettings["host"],
                                         ConfigurationSettings.AppSettings["instance"],
                                         username,
                                         pw,
                                         ConfigurationSettings.AppSettings["apiKey"]);
          accessAsset = new AccessAsset(session.Client);
          processFolder(localFolder.ToLower(), destFolder);
          var accessAuth = new AccessAuth(session.Client);
          accessAuth.Logout();

      }
      catch (Exception ex)
      {
        Console.WriteLine("Error: {0}", ex.Message);
      }
      Console.Write("Press, almost, any key to continue...");
      Console.ReadLine();
    }

    //get a list of all files in a folder, including files in subfolders, and process them
    private static void processFolder(string localFolder, string cmsDestinationPath)
    {
      var files = Directory.GetFiles(localFolder, "*.*", SearchOption.AllDirectories);
      if (files != null && files.Length > 0)
      {
        foreach (var file in files)
        {
          CheckOrUploadFile(localFolder, cmsDestinationPath, accessAsset, file);
        }
      }
    }

    //checks if a file already exists, uploads it if not
    private static void CheckOrUploadFile(string localFolder, string cmsDestinationPath, AccessAsset accessAsset, string file)
    {
      string cmsPathToCheck = "";
      //1 - check if file already exists
      cmsPathToCheck = file.ToLower().Replace(localFolder, cmsDestinationPath).Replace("\\", "/");
      var existsResponse = accessAsset.Exists(new AssetExistsRequest(cmsPathToCheck));
      if (existsResponse.exists)
      {
        Console.WriteLine("Skipping, asset {0} already exists.  (id: {1})", cmsPathToCheck, existsResponse.assetId);
      }
      else
      {
        //file was not found in the CMS instance, so we will upload it, but first make sure we do have an existing folder structure in the CMS
        //2 - check if folder exists, create otherwise        
        string path = Path.GetDirectoryName(file);
        int folderId = -1;
        if (folderCache.ContainsKey(path))
        {
          folderId = folderCache[path];
        }
        else
        {
          //attempt to create folder and cache new assetId as the folderId
          cmsPathToCheck = path.ToLower().Replace(localFolder, cmsDestinationPath).Replace("\\", "/");
          existsResponse = accessAsset.Exists(new AssetExistsRequest(cmsPathToCheck));
          if (existsResponse.exists)
          {
            folderId = existsResponse.assetId;
            folderCache.Add(path, folderId);
          }
          else
          {
            //could not find folder, split and build path
            folderId = BuildFolderTree(cmsPathToCheck);
            if (folderId < 0)
            {
              //something went wrong, exit app, so we don't upload files to wrong folders, or have excessive # of errors.
              Console.WriteLine("Error, Unable to create folder {0} ", cmsPathToCheck);              
              Environment.Exit(-1);
            }
            folderCache.Add(path, folderId);            
          }
        }

        //upload asset here
        AssetUploadRequest req = new AssetUploadRequest(Path.GetFileName(file), folderId);
        req.bytes = File.ReadAllBytes(file);
        var resp = accessAsset.Upload(req);
        if (resp.IsSuccessful)
        {
          Console.WriteLine("upload successful: {0} in {1}", file, path, resp.asset.id);
        }
        else
        {
          Console.WriteLine("Error uploading file: {0} in {1}: {2}", file, path, resp.ErrorMessage);
        }
      }
    }

    #region helper functions
    //found a missing folder, recreate from all parts, since we don't know what 
    private static int BuildFolderTree(string cmsPathToCheck)
    {
      int id = -1;
      string[] parts = cmsPathToCheck.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
      string pathToCheck = "/";
      foreach (var part in parts)
      {
        pathToCheck += part + "/";        
        var existsResponse = accessAsset.Exists(new AssetExistsRequest(pathToCheck));
        if (existsResponse.exists)
        {
          Console.WriteLine("base folder exists {0}", pathToCheck);
          id = existsResponse.assetId;
        }
        else
        {          
          Console.WriteLine("need to create {0}", pathToCheck);
          //var resp = accessAsset.Create(new AssetCreateRequest(part, id, AssetType.Folder));
          var resp = accessAsset.Create(part, id, AssetType.Folder);
          if (resp.IsSuccessful)
          {
            id = resp.asset.id;
          }
          else
          {
            //create, assign tid, if failed, set to -1
            id = -1;
            break;
          }
        }        
      }
      return id;
    }

    //based on http://stackoverflow.com/questions/3404421/password-masking-console-application
    private static string getSecuredString()
    {
      ConsoleKeyInfo key;
      string pw = "";
      do
      {
        key = Console.ReadKey(true);
        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
          pw += key.KeyChar;
          Console.Write("*");
        }
        else
        {
          if (key.Key == ConsoleKey.Backspace && pw.Length > 0)
          {
            pw = pw.Substring(0, (pw.Length - 1));
            Console.Write("\b \b");
          }
        }
      }
      // Stops Receving Keys Once Enter is Pressed
      while (key.Key != ConsoleKey.Enter);
      return pw;
    }
    #endregion
  }
}
