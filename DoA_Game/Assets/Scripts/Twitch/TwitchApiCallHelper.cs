using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// Small Helper Class To Help With API Things
public class TwitchApiCallHelper : MonoBehaviour
{
    private HttpClient _httpClient = new HttpClient();
    private string _twitchClientId;
    private string _twitchAuthToken;


    public string TwitchClientId
    {
        set { _twitchClientId = value; }
    }

    public string TwitchAuthToken
    {
        set { _twitchAuthToken = value; }
    }

    private void Start()
    {
        _twitchClientId = "";
        _twitchAuthToken = "";
    }

    /// <summary>
    /// Simple Helper Method To Call A Twitch API Endpoint And Return Its Response Data.
    /// </summary>
    /// <remarks>
    /// This BLOCKS The Current Thread Till The API Response Has Been Recieved, So In A Production Environment You
    /// Either Want A different/async Method Or Simply Run This In An Own thread/job/task.
    /// </remarks>
    /// <param name="endpoint">API Endpoint To Call (full URL)</param>
    /// <param name="method">(optional) HTTP Method To Use - Defaults To GET</param>
    /// <param name="body">(optional) Any Body Data To Send</param>
    /// <param name="headers">(optional) Any Additional Headers (the standard API headers are always added)</param>
    /// <returns>Returns The Response Body Of The API Call, If Any Was Recieved.</returns>
    public string CallApi(string endpoint, string method = "GET", string body = "", string[] headers = null)
    {
        HttpRequestMessage httpRequest;
        string[] headerParts;
        string returnData;

        // Init Some Things
        _httpClient.BaseAddress = null;
        _httpClient.DefaultRequestHeaders.Clear();

        // Set Http Client Method As Requested
        switch (method.ToLower())
        {
            case "get":
                httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
                break;

            case "post":
                httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
                break;

            case "put":
                httpRequest = new HttpRequestMessage(HttpMethod.Put, endpoint);
                break;

            case "patch":
                httpRequest = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint);
                break;

            case "delete":
                httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);
                break;

            default:
                httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);
                break;
        }

        // Set Http Client Request Body, If Any Was Supplied
        if (body.Length > 0)
        {
            httpRequest.Content = new StringContent(body, Encoding.UTF8, "application/json");
        }

        // Set Default Headers
        if (_twitchAuthToken.Length > 0)
        {
            httpRequest.Headers.TryAddWithoutValidation("Authorization", "Bearer " + _twitchAuthToken);
        }

        if (_twitchClientId.Length > 0)
        {
            httpRequest.Headers.TryAddWithoutValidation("Client-Id", _twitchClientId);
        }

        httpRequest.Headers.TryAddWithoutValidation("Content-Type", "application/json");

        // Set Additional Headers, If Any Were Supplied
        if (headers != null)
        {
            foreach (string header in headers)
            {
                headerParts = header.Split(':');
                if (headerParts.Length >= 2)
                {
                    if (headerParts[1] != "")
                    {
                        httpRequest.Headers.TryAddWithoutValidation(headerParts[0].Trim(), headerParts[1].Trim());
                    }
                }
            }
        }

        // Send Request And Wait For It To Complete
        Task<HttpResponseMessage> httpRespose = _httpClient.SendAsync(httpRequest);
        while (!httpRespose.IsCompleted)
        {
            // NOP - Keep Waiting....
        }

        // Fetch Response Content
        Task<string> httpResponseContent = httpRespose.Result.Content.ReadAsStringAsync();
        while (!httpResponseContent.IsCompleted)
        {
            // NOP - Keep Waiting....
        }

        // Return The Response Content And Be Done 
        returnData = httpResponseContent.Result;
        return returnData;
    }
}
