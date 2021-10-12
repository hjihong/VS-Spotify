using System;
using System.Collections.Generic;
using System.Linq;
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

namespace VSSpotify
{
    /// <summary>
    /// Interaction logic for SpotifyControl.xaml
    /// </summary>
    public partial class SpotifyControl : UserControl
    {
        public SpotifyControl()
        {
            InitializeComponent();
        }

        private async Task TestSpotifyAsync()
        {
            var client = await new SpotifyClientFactory("CLIENT_ID_HERE").GetClientAsync();
            var profile = await client.UserProfile.Current();
            Console.WriteLine(profile.DisplayName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var _ = TestSpotifyAsync().ConfigureAwait(false);
        }
    }
}
