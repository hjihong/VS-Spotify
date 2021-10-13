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
using Microsoft.VisualStudio.Shell.Interop;

namespace VSSpotify
{
    /// <summary>
    /// Interaction logic for SpotifyControl.xaml
    /// </summary>
    public partial class SpotifyControl : UserControl
    {
        public bool IsAuthenticated { get; private set; } = false;

        public SpotifyControl()
        {
            InitializeComponent();
            IsAuthenticated = new SpotifyAuthManager().IsAuthenticated();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                var profile = await client.UserProfile.Current();
                Console.WriteLine(profile.DisplayName);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var authManager = new SpotifyAuthManager();

                if (IsAuthenticated)
                {
                    authManager.ClearCredentials();
                    IsAuthenticated = false;
                }
                else
                {
                    await authManager.GetCredentialsAsync();
                    IsAuthenticated = authManager.IsAuthenticated();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SongTitleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
