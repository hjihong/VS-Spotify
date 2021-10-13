using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace VSSpotify
{
    /// <summary>
    /// To use:
    /// - Pass your client id
    /// - Add http://127.0.0.1 to your redirect URLs
    /// </summary>
    internal class SpotifyClientFactory
    {
        public async Task<SpotifyClient> GetClientAsync()
        {
            using (var authManager = new SpotifyAuthManager())
            {
                var creds = await authManager.GetCredentialsAsync();
                return new SpotifyClient(creds);
            }
        }
    }
}
