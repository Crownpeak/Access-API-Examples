using CrownPeak.AccessAPI;
using CrownPeakPublic.AccessAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CrownPeakPublic
{
  public class cpHttpClient : HttpClient
  {
    public string Server { get; private set; }
    public string Instance { get; private set; }
    public string Key { get; private set; }
    public string Secret { get; private set; }
    public HttpResponseMessage Response { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }
    public HttpMethod Method { get; set; }
    private Uri uri;
    private StringContent postContent;
    private bool useSignatureAuth;

    /// <summary>
    /// Supplying a secret key will trigger Signature authentication method
    /// </summary>
    /// <param name="server"></param>
    /// <param name="instance"></param>
    /// <param name="key"></param>
    /// <param name="secret"></param>
    public cpHttpClient(string server, string instance, string key, string secret = "")
    {
      Server = CleanupServer(server);
      Instance = instance;
      Key = key;
      Secret = secret;
      useSignatureAuth = !string.IsNullOrWhiteSpace(Secret);
      Method = HttpMethod.Post;
    }

    public cpHttpClient() { }

    public void SetupAccessRequest(string controller, string action, string postData = "", string postDataMediaType = "application/json", string accept = "application/json")
    {
      uri = new Uri(string.Format(@"{0}/{1}/cpt_webservice/accessapi/{2}/{3}", Server, Instance, controller, action));
      SetPostData(postData, Encoding.UTF8, postDataMediaType);
      AddAccessAPIHeaders(accept);
    }

    private string CleanupServer(string server)
    {
      if (string.IsNullOrWhiteSpace(server)) return string.Empty;
      if (!server.StartsWith("http", StringComparison.OrdinalIgnoreCase)) server = "https://" + server;
      var uri = new Uri(server);
      return uri.ToString();
    }

    //set uri and post data before calling this method with signature auth enabled
    private void AddAccessAPIHeaders(string acceptHeaderValue)
    {
      RemoveHeader("Authorization");
      RemoveHeader("cp-datetime");
      RemoveHeader("x-api-key");
      SetAcceptHeader(acceptHeaderValue);

      if (useSignatureAuth)
      {
        var dateTimeSent = DateTime.UtcNow.ToString("o");//Round-trip date/time pattern. ISO 8601
        AddHeader("Authorization", "CP " + Key + ":" + GenerateSignature(Method, uri, postContent.ReadAsStringAsync().Result, dateTimeSent));
        AddHeader("cp-datetime", dateTimeSent);
      }
      else
      {
        AddHeader("x-api-key", Key);
      }
    }

    public void SetUri(string sUri)
    {
      uri = new Uri(sUri);
    }

    public void SetUri(Uri uri)
    {
      this.uri = uri;
    }

    public void SetPostData(string content, Encoding encoding, string mediaType)
    {
      postContent = new StringContent(content, encoding, mediaType);
    }

    public void SetAcceptHeader(string mediaType)
    {
      DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
    }

    public void AddHeader(string name, string value)
    {
      DefaultRequestHeaders.Add(name, value);
    }

    public void RemoveHeader(string name)
    {
      DefaultRequestHeaders.Remove(name);
    }

    public string CaptureToJsonString()
    {
      try
      {
        // by calling .Result you are performing a synchronous call
        switch (Method.Method)
        {
          case "GET":
            Response = GetAsync(uri).Result;
            break;
          case "POST":
            Response = PostAsync(uri, postContent).Result;
            break;
          case "PUT":
            Response = PutAsync(uri, postContent).Result;
            break;
          case "DELETE":
            Response = DeleteAsync(uri).Result;
            break;
          case "HEAD":
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Head, uri);
            Response = SendAsync(req).Result;
            break;
          default:
            Response = GetAsync(uri).Result;
            break;
        }
      }
      catch(Exception ex)
      {
        string errors = ex.Message;
        while(ex.InnerException != null)
        {
          errors += Environment.NewLine + ex.InnerException.Message;
          ex = ex.InnerException;
        }
        return Newtonsoft.Json.JsonConvert.SerializeObject(new ResultClass { ResultCode = eResultCodes.conWS_GeneralError, ErrorMessage = errors });
      }

      // by calling .Result you are synchronously reading the result
      string responseString = Response.Content.ReadAsStringAsync().Result;

      if(!responseString.StartsWith("{"))//ensure response is in Json format
      {
        if ((int)Response.StatusCode == 429)
        {
          //TODO: add retry logic
        }
        responseString = Newtonsoft.Json.JsonConvert.SerializeObject(new ResultClass { ResultCode = eResultCodes.conWS_GeneralError, InternalCode = (int)Response.StatusCode, ErrorMessage = responseString });
      }
      return responseString;
    }

    public string GenerateSignature(HttpMethod method, Uri requestUri, string postData, string dateTimeSent)
    {
      var stringToSign = method.ToString().ToUpper() + "\n" +
                         requestUri.AbsolutePath + "\n" +
                         postData + "\n" +
                         dateTimeSent;
      var secretBytes = Encoding.UTF8.GetBytes(Secret);
      System.Security.Cryptography.HMACSHA1 hmacsha1 = new System.Security.Cryptography.HMACSHA1(secretBytes);
      return Convert.ToBase64String(hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
    }
  }
}
