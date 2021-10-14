using System;
using System.ComponentModel;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "<Pending>")]
    public partial class SpotifyControl : UserControl, INotifyPropertyChanged
    {
        private bool isAuthenticated = false;
        private bool isPaused = false;

        public bool IsAuthenticated
        {
            get
            {
                return isAuthenticated;
            }
            private set
            {
                if (isAuthenticated != value)
                {
                    isAuthenticated = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAuthenticated)));
                }
            }
        }

        public bool IsPaused
        {
            get
            {
                return isPaused;
            }
            private set
            {
                if (isPaused != value)
                {
                    isPaused = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPaused)));
                }
            }
        }

        public SpotifyControl()
        {
            InitializeComponent();
            using (var authManager = new SpotifyAuthManager())
            {
                IsAuthenticated = authManager.IsAuthenticated();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (isPaused)
                {
                    // call Play method
                }
                else
                {
                    // call pause method
                }
                IsPaused = !IsPaused;
                //var client = await new SpotifyClientFactory().GetClientAsync();
                //var profile = await client.UserProfile.Current();
                //Console.WriteLine(profile.DisplayName);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var authManager = new SpotifyAuthManager())
                {
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
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
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

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
