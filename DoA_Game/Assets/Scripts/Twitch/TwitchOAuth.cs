using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class TwitchOAuth : MonoBehaviour
{
    TwitchConnect twitchConnect;

    // Inputfields
    public InputField inputSeAccountId;
    public InputField inputJwtToken;
    public InputField inputTwitchChannelName;
    public InputField inputTwitchClientId;
    public InputField inputTwitchClientSecret;

    public Button loginButton;

    // Canvas
    public GameObject loginCanvas;
    public GameObject gameCanvas;

    // Login 
    private string twitchAuthUrl = "https://id.twitch.tv/oauth2/authorize";
    private string twitchRedirectUrl = "http://localhost:8080/";
    public static string seAccountId = "";
    public static string jwtToken = "";
    public static string twitchChannelName = "";
    [SerializeField] private string twitchClientId = "";
    [SerializeField] private string twitchClientSecret = "";

    public bool isAuthReady = false;
    public bool autoLogin = false;

    // API
    [SerializeField] private TwitchApiCallHelper twitchApiCallHelper;
    private string _twitchAuthStateVerify;
    public static string _authToken;
    public static string _refreshToken;


    void Start()
    {
        // Check If There Is A Stored Auth & Refresh Token
        if(PlayerPrefs.HasKey("pref_authToken") && PlayerPrefs.HasKey("pref_refreshToken"))   
        {
            _authToken = PlayerPrefs.GetString("pref_authToken");                                           
            _refreshToken = PlayerPrefs.GetString("pref_refreshToken");
            autoLogin = true;                                                                   
        }
        else
        {
           _authToken = "";
           _refreshToken = "";
        }

        // Loads from PlayerPrefs
        inputTwitchChannelName.text = PlayerPrefs.GetString("twitchChannelName");
        inputSeAccountId.text = PlayerPrefs.GetString("seAccountId");
        inputJwtToken.text = PlayerPrefs.GetString("jwtToken");
        inputTwitchClientId.text = PlayerPrefs.GetString("twitchClientId");
        inputTwitchClientSecret.text = PlayerPrefs.GetString("twitchClientSecret");

        // Sets The InputFields
        twitchChannelName = PlayerPrefs.GetString("twitchChannelName");
        seAccountId = PlayerPrefs.GetString("seAccountId");
        jwtToken = PlayerPrefs.GetString("jwtToken");
        twitchClientId = PlayerPrefs.GetString("twitchClientId");
        twitchClientSecret = PlayerPrefs.GetString("twitchClientSecret");

        twitchConnect = GameObject.FindGameObjectWithTag("TwitchConnect").GetComponent<TwitchConnect>();
    }


    // Check If Login Was Successful 
    void Update()
    {
        if(isAuthReady)
        {
            PlayerPrefs.SetString("pref_authToken", _authToken);
            PlayerPrefs.SetString("pref_refreshToken", _refreshToken);

            loginCanvas.SetActive(false);
            gameCanvas.SetActive(true);
            twitchConnect.ConnectToTwitch();

            isAuthReady = false;
        }

        if(string.IsNullOrWhiteSpace(inputTwitchChannelName.text) 
            || string.IsNullOrWhiteSpace(inputTwitchClientId.text) 
            || string.IsNullOrWhiteSpace(inputTwitchClientSecret.text) 
            || string.IsNullOrWhiteSpace(inputSeAccountId.text) 
            || string.IsNullOrWhiteSpace(inputJwtToken.text))
        {
            loginButton.interactable = false;
        }
        else
        {
            loginButton.interactable = true;
        }

    }


    // Set And Save The Login Data
    public void OnButtonClick()
    {
        twitchChannelName = inputTwitchChannelName.text;
        seAccountId = inputSeAccountId.text;
        jwtToken = inputJwtToken.text;
        twitchClientId = inputTwitchClientId.text;
        twitchClientSecret = inputTwitchClientSecret.text;

        PlayerPrefs.SetString("twitchChannelName",inputTwitchChannelName.text);
        PlayerPrefs.SetString("seAccountId",inputSeAccountId.text);
        PlayerPrefs.SetString("jwtToken",inputJwtToken.text);
        PlayerPrefs.SetString("twitchClientId",inputTwitchClientId.text);
        PlayerPrefs.SetString("twitchClientSecret",inputTwitchClientSecret.text);   
    }


    /// Starts The Twitch OAuth Flow By Constructing The Twitch Auth URL Based On The Scopes You Want/Need.
    public void InitiateTwitchAuth()
    {
        string[] scopes;
        string s;

        // List Of Scopes We Want
        scopes = new[]
        {
            "user:read:email",
            "chat:edit",
            "chat:read",
            "channel:read:redemptions",
            "channel_subscriptions",
            "user:read:broadcast",
            "user:edit:broadcast",
            "channel:manage:redemptions"
        };

        // Generate Something For The "state" Parameter.
        // This Can Be Whatever You Want It To Be, It's Gonna Be "echoed back" To Us As Is And Should Be Used To
        // Verify The Redirect Back From Twitch Is Valid.
        _twitchAuthStateVerify = ((Int64) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();

        // Query Parameters For The Twitch Auth URL
        s = "client_id=" + twitchClientId + "&" +
            "redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl) + "&" +
            "state=" + _twitchAuthStateVerify + "&" +
            "response_type=code&" +
            "scope=" + String.Join("+", scopes);

        // Start Our Local Webserver To Receive The Redirect Back After Twitch Authenticated
        StartLocalWebserver();

        // Open The Users Browser And Send Them To The Twitch Auth URL
        Application.OpenURL(twitchAuthUrl + "?" + s);
    }


    /// Opens A Simple "webserver" On localhost:8080 For The Auth Redirect To Land On.
    private void StartLocalWebserver()
    {
        HttpListener httpListener = new HttpListener();

        httpListener.Prefixes.Add(twitchRedirectUrl);
        httpListener.Start();
        httpListener.BeginGetContext(new AsyncCallback(IncomingHttpRequest), httpListener);
    }


    /// Handles The Incoming HTTP Request
    /// <param name="result"></param>
    private void IncomingHttpRequest(IAsyncResult result)
    {
        string code;
        string state;
        HttpListener httpListener;
        HttpListenerContext httpContext;
        HttpListenerRequest httpRequest;
        HttpListenerResponse httpResponse;
        string responseString;

        // Get Back The Reference To Our Http Listener
        httpListener = (HttpListener) result.AsyncState;

        // Fetch The Context Object
        httpContext = httpListener.EndGetContext(result);

        // The Context Object Has The Request Object For Us, That Holds Details About The Incoming Request
        httpRequest = httpContext.Request;

        code = httpRequest.QueryString.Get("code");
        state = httpRequest.QueryString.Get("state");

        // Check That We Got A Code Value And The State Value Matches Our Remembered One
        if ((code.Length > 0) && (state == _twitchAuthStateVerify))
        {
            // If All Checks Out, Use The Code To Exchange It For The Actual Auth Token At The API
            GetTokenFromCode(code);
            isAuthReady = true;
        }

        // Build A Response To Send An "ok" Back To The Browser For The User To See
        httpResponse = httpContext.Response;
        responseString = "<html><body><b>DONE!</b><br>(You can close this tab/window now)</body></html>";
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

        // Send The Output To The Client Browser
        httpResponse.ContentLength64 = buffer.Length;
        System.IO.Stream output = httpResponse.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();

        // The HTTP Listener Has Served It's Purpose, Shut It Down
        httpListener.Stop();
    }


    /// Makes The API Call To Exchange The Ceceived Code For The Actual Auth Token
    /// <param name="code">The Code Parameter Received In The Callback HTTP Request</param>
    private void GetTokenFromCode(string code)
    {
        string apiUrl;
        string apiResponseJson;
        ApiCodeTokenResponse apiResponseData;

        // Construct Full URL For API Call
        apiUrl = "https://id.twitch.tv/oauth2/token" +
                 "?client_id=" + twitchClientId +
                 "&client_secret=" + twitchClientSecret +
                 "&code=" + code +
                 "&grant_type=authorization_code" +
                 "&redirect_uri=" + UnityWebRequest.EscapeURL(twitchRedirectUrl);

        // Make Sure Our API Helper Knows Our Client ID (It Needed For The HTTP Headers)
        twitchApiCallHelper.TwitchClientId = twitchClientId;

        // Make The Call
        apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "POST");

        // Parse The Return JSON Into A More Usable Data Object
        apiResponseData = JsonUtility.FromJson<ApiCodeTokenResponse>(apiResponseJson);

        // Fetch The Token From The Response Data
        _authToken = apiResponseData.access_token;
        _refreshToken = apiResponseData.refresh_token;
    }


    // https://dev.twitch.tv/docs/authentication/validate-tokens/
    public bool ValidateToken(string token)
    {
        string apiUrl = "https://id.twitch.tv/oauth2/validate";
        string apiResponseJson;
        ApiValidateTokenResponse apiResponseData;

        twitchApiCallHelper.TwitchAuthToken = token;

        // Make The Call
        apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "GET");

        apiResponseData = JsonUtility.FromJson<ApiValidateTokenResponse>(apiResponseJson);

        if(apiResponseData.status == 200)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    
    // https://dev.twitch.tv/docs/authentication/refresh-tokens/
    public void RefreshToken()
    {
        string apiUrl;
        string apiResponseJson;
        ApiRefreshTokenResponse apiResponseData;

       // Construct Full URL For API Call
        apiUrl = "https://id.twitch.tv/oauth2/token" +
                 "?client_id=" + twitchClientId +
                 "&client_secret=" + twitchClientSecret +
                 "&grant_type=refresh_token" +
                 "&refresh_token=" + _refreshToken;;

        // Make Sure Our API Helper Knows Our Client ID (It Needed For The HTTP Headers)
        twitchApiCallHelper.TwitchClientId = twitchClientId;

        // Make The Call
        apiResponseJson = twitchApiCallHelper.CallApi(apiUrl, "POST");

        // Parse The Return JSON Into A More Usable Data Object
        apiResponseData = JsonUtility.FromJson<ApiRefreshTokenResponse>(apiResponseJson);

        _authToken = apiResponseData.access_token;
        _refreshToken = apiResponseData.refresh_token;

        PlayerPrefs.SetString("pref_authToken", _authToken);
        PlayerPrefs.SetString("pref_refreshToken", _refreshToken);
    }


    public void AutoLogin()
    {
        // Check If Auth And Refresh Stored
        if(autoLogin)
        {   
            Debug.Log("AutoLogin");          
            if(!ValidateToken(_authToken))
            {
                RefreshToken();
                Debug.Log("Refreshed");
            }
            isAuthReady = true;
        }
        else
        {
            Debug.Log("Not AutoLogin");
            InitiateTwitchAuth();
        }
    }
}
