using System;

// Store Informations Returned From API Calls
// GetTokenFromCode(string code)
[Serializable]
public class ApiCodeTokenResponse
{
    public string access_token;
    public int expires_in;
    public string refresh_token;
    public string[] scope;
    public string token_type;
}

// ValidateToken(string token)
[Serializable]
public class ApiValidateTokenResponse
{
    public int status = 200;
    public string client_id;
    public string login;
    public string[] scope;
    public string user_id;
    public string expires_in;
}

// RefreshToken()
[Serializable]
public class ApiRefreshTokenResponse
{
    public string access_token;
    public string refresh_token;
    public string[] scope;
    public string token_type;
}

