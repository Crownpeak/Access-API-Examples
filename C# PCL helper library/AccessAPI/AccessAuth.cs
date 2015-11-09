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
  public class AccessAuth
  {
    private cpHttpClient Client;
    public AccessAuth(cpHttpClient client)
    {
      Client = client;
    }

    public AuthAuthenticateWithCacheResponse AuthenticateWithCache(AuthAuthenticateWithCacheRequest request)
    {
      return process<AuthAuthenticateWithCacheResponse>("AuthenticateWithCache", request);
    }

    public AuthAuthenticateResponse Authenticate(AuthAuthenticateRequest request)
    {
      return process<AuthAuthenticateResponse>("Authenticate", request);
    }

    public AuthLogoutResponse Logout()
    {
      return process<AuthLogoutResponse>("Logout", string.Empty);
    }

    private TResponse process<TResponse>(string action, object postData)
    {
      Client.SetupAccessRequest("Auth", action, Newtonsoft.Json.JsonConvert.SerializeObject(postData).ToString());
      return Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(Client.CaptureToJsonString());
    }
  }
}
