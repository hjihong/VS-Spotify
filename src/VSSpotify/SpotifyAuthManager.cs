using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace VSSpotify
{
    internal class SpotifyAuthManager : IDisposable
    {
        private const string PKCE_ACCESS_TOKEN_KEY = "PKCE_ACCESS_TOKEN";
        private const string PKCE_REFRESH_TOKEN_KEY = "PKCE_REFRESH_TOKEN";
        private const string PKCE_EXPIRY_KEY = "PKCE_EXPIRY";
        private const string CLIENT_ID = "CLIENT_ID_HERE";
        private readonly TempDictionary _cache;

        private string AccessToken
        {
            get => _cache.ContainsKey(PKCE_ACCESS_TOKEN_KEY) ? _cache[PKCE_ACCESS_TOKEN_KEY] : string.Empty;
            set => _cache[PKCE_ACCESS_TOKEN_KEY] = value;
        }

        private string RefreshToken
        {
            get => _cache.ContainsKey(PKCE_REFRESH_TOKEN_KEY) ? _cache[PKCE_REFRESH_TOKEN_KEY] : string.Empty;
            set => _cache[PKCE_REFRESH_TOKEN_KEY] = value;
        }

        private DateTimeOffset TokenExpiry
        {
            get => DateTimeOffset.TryParse(_cache.ContainsKey(PKCE_EXPIRY_KEY) ? _cache[PKCE_EXPIRY_KEY] : "", out var expiry) ? expiry : DateTimeOffset.MinValue;
            set => _cache[PKCE_EXPIRY_KEY] = value.ToString();
        }

        public SpotifyAuthManager()
        {
            _cache = new TempDictionary();
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(AccessToken) && TokenExpiry > DateTimeOffset.Now;
        }

        public void ClearCredentials()
        {
            _cache.Clear();
        }

        public async Task<string> GetCredentialsAsync()
        {
            try
            {
                if (!IsAuthenticated())
                {
                    await RefreshCredentialsAsync();
                }
            }
            catch(Exception)
            {
                ClearCredentials();
                await GetNewCredentialsAsync();
            }

            return AccessToken;
        }

        private async Task RefreshCredentialsAsync()
        {
            var newResponse = await new OAuthClient().RequestToken(new PKCETokenRefreshRequest(CLIENT_ID, RefreshToken));
            AccessToken = newResponse.AccessToken;
            RefreshToken = newResponse.RefreshToken;
            TokenExpiry = GetNewExpiry(newResponse.ExpiresIn);
        }

        private async Task GetNewCredentialsAsync()
        {
            var redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetCallbackPort());
            var (verifier, challenge) = PKCEUtil.GenerateCodes();
            var loginRequest = new LoginRequest(
                new Uri(redirectURI),
                CLIENT_ID,
                LoginRequest.ResponseType.Code)
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { 
                    Scopes.PlaylistReadPrivate, 
                    Scopes.UserReadPrivate,
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.UserModifyPlaybackState,
                    Scopes.UserModifyPlaybackState
                }
            };

            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            http.Start();

            var uri = loginRequest.ToUri();
            System.Diagnostics.Process.Start(uri.ToString());

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Please return to the app.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            var code = context.Request.QueryString.Get("code");
            var initialResponse = await new OAuthClient().RequestToken(new PKCETokenRequest(CLIENT_ID, code, new Uri(redirectURI), verifier));

            AccessToken = initialResponse.AccessToken;
            RefreshToken = initialResponse.RefreshToken;
            TokenExpiry = GetNewExpiry(initialResponse.ExpiresIn);
        }

        private DateTimeOffset GetNewExpiry(int offset)
        {
            return DateTimeOffset.Now.AddSeconds(offset - 15);
        }

        private int GetCallbackPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public void Dispose()
        {
            _cache.Dispose();
        }
    }
}
