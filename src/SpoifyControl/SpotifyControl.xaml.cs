﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpotifyAPI.Web;

namespace SpoifyControl
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SpotifyPlayerControl : UserControl
    {

        public SpotifyPlayerControl()
        {
            InitializeComponent();
            TestSpotify().ConfigureAwait(false);
        }

        private async Task TestSpotify()
        {
            var client = await new SpotifyClientFactory("CLIENT_ID_HERE").GetClientAsync();
            var profile = await client.UserProfile.Current();
            Console.WriteLine(profile.DisplayName);
        }
    }
}
