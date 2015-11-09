using CrownPeak.AccessAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CrownPeakPublic.AccessAPI
{
  public class AccessSession : IDisposable
  {
    private string _Server;
    public string Server
    {
      get
      {
        return _Server;
      }
      set
      {
        _Server = value;
      }
    }

    private string _Instance;
    public string Instance
    {
      get
      {
        return _Instance;
      }
      set
      {
        _Instance = value;
      }
    }

    private string _Username;
    public string Username
    {
      get
      {
        return _Username;
      }
      set
      {
        _Username = value;
      }
    }

    private string _Password;
    public string Password
    {
      get
      {
        return _Password;
      }
      set
      {
        _Password = value;
      }
    }

    private string _Secret;
    public string Secret
    {
      get
      {
        return _Secret;
      }
      set
      {
        _Secret = value;
      }
    }

    private string _Key;
    public string Key
    {
      get
      {
        return _Key;
      }
      set
      {
        _Key = value;
      }
    }

    private AuthAuthenticateWithCacheResponse _Cache;
    public AuthAuthenticateWithCacheResponse Cache
    {
      get
      {
        return _Cache;
      }
      set
      {
        _Cache = value;
      }
    }

    private cpHttpClient _Client;
    public cpHttpClient Client
    {
      get
      {
        return _Client;
      }
      set
      {
        _Client = value;
      }
    }

    private AccessAuth authController;

    /// <summary>
    /// Starts a session for the user on the given server/instance.
    /// This in intended to be called with a using block. 
    /// All AccessControllers used within will be automatically authenticated.
    /// The Dispose method will Logout the user when complete.
    /// </summary>
    /// <param name="server">Domain of the server to use, ex: cms.crownpeak.com</param>
    /// <param name="instance">Name of the instance to use, ex: CPQA</param>
    /// <param name="username">Username of a user with active account in the CMS</param>
    /// <param name="password">Password of user</param>
    /// <param name="apiKey">Public developer API key supplied by CrownPeak</param>
    /// <param name="apiSecret">(Optional)Developer API Secret Key, ideally this will come from a secure database. Do not store key within application. Supplying a secret key will trigger Signature authentication method</param>
    /// <param name="requestCache">(Optional)When set to true the Cache property will be populated with data that can be used to cache information that does not often change. Cache property will be null otherwise.</param>
    public AccessSession(string server, string instance, string username, string password, string apiKey, string apiSecret = "", bool requestCache = false)
    {
      Client = new cpHttpClient(server, instance, apiKey, apiSecret);
      Server = server;
      Instance = instance;
      Username = username;
      Password = password;
      Key = apiKey;
      Secret = apiSecret;
      authController = new AccessAuth(Client);

      Authenticate(requestCache);
    }

    private void Authenticate(bool withCache)
    {
      
      if (withCache)
      {
        var request = new AuthAuthenticateWithCacheRequest
        {
          instance = Instance,
          password = Password,
          remember_me = false,
          timeZoneOffsetMinutes = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes,
          username = Username,
        };
        var authResponse = authController.AuthenticateWithCache(request);
        if (authResponse == null || authResponse.ResultCode != eResultCodes.conWS_Success)
        {
          throw new Exception("Failed to AuthenticateWithCache: " + authResponse == null ? "" : authResponse.ErrorMessage.ToString());
        }
        else
        {
          Cache = authResponse;
        }
      }
      else
      {
        var request = new AuthAuthenticateRequest
        {
          instance = Instance,
          password = Password,
          remember_me = false,
          timeZoneOffsetMinutes = (int)TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).TotalMinutes,
          username = Username,
        };
        var authResponse = authController.Authenticate(request);
        if (authResponse == null || authResponse.ResultCode != eResultCodes.conWS_Success)
        {
          throw new Exception("Failed to Authenticate: " + authResponse == null ? "" : authResponse.ErrorMessage.ToString());
        }
      }
    }

    public void Dispose()
    {
      Logout();
      if (Client != null) Client.Dispose();
    }

    private void Logout()
    {
      var logoutResponse = authController.Logout();
      if (logoutResponse != null && logoutResponse.ResultCode == eResultCodes.conWS_Success)
      {
        throw new Exception("Failed to Logout: " + logoutResponse == null ? "" : logoutResponse.ErrorMessage.ToString());
      }
    }
  }
}
