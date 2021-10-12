using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpoifyControl
{
    /// <summary>
    /// To use:
    /// - Pass your client id
    /// - Add http://127.0.0.1 to your redirect URLs
    /// </summary>
    internal class SpotifyClientFactory
    {
        private readonly string _clientId;

        public SpotifyClientFactory(string clientId)
        {
            _clientId = clientId;
        }

        public async Task<SpotifyClient> GetClientAsync()
        {
            string clientID = _clientId;
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, GetRandomUnusedPort());

            var (verifier, challenge) = PKCEUtil.GenerateCodes();
            var loginRequest = new LoginRequest(
                new Uri(redirectURI),
                clientID,
                LoginRequest.ResponseType.Code)
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.PlaylistReadPrivate, Scopes.UserReadPrivate }
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
            var incoming_state = context.Request.QueryString.Get("state");

            var initialResponse = await new OAuthClient().RequestToken(new PKCETokenRequest(clientID, code, new Uri(redirectURI), verifier));
            var authenticator = new PKCEAuthenticator(clientID, initialResponse);

            var config = SpotifyClientConfig.CreateDefault()
              .WithAuthenticator(authenticator);
            var spotify = new SpotifyClient(config);

            return spotify;
        }


        private int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
