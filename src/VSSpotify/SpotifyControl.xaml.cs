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
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using SpotifyAPI.Web;

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
        private string currentlyPlayingItemTitle;
        private readonly JoinableTaskFactory joinableTaskFactory;
        private readonly VSSpotifyPackage package;

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

                    if (!isAuthenticated)
                    {
                        // User just signed out, clean up controls
                        OnUserSignedOut();
                    }
                }
            }
        }

        private void OnUserSignedOut()
        {
            this.CurrentlyPlayingItemTitle = "";
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

        public string CurrentlyPlayingItemTitle
        {
            get
            {
                return currentlyPlayingItemTitle;
            }
            private set
            {
                if (currentlyPlayingItemTitle != value)
                {
                    currentlyPlayingItemTitle = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentlyPlayingItemTitle)));
                }
            }
        }

        public SpotifyControl(JoinableTaskFactory joinableTaskFactory, VSSpotifyPackage package)
        {
            this.joinableTaskFactory = joinableTaskFactory;
            this.package = package;

            InitializeComponent();

            // Start a loop of refreshing controls
            this.joinableTaskFactory.RunAsync(async () =>
            {
                using (var authManager = new SpotifyAuthManager())
                {
                    // Try to silently get credentials first. We only want to show signed
                    // out if we're not able to refresh credentials silently.
                    if (!authManager.IsAuthenticated())
                    {
                        try
                        {
                            await authManager.RefreshCredentialsAsync();
                        }
                        catch (Exception) { }
                    }

                    IsAuthenticated = authManager.IsAuthenticated();
                }

                while (!this.package.DisposalToken.IsCancellationRequested)
                {
                    await RefreshControlsAsync();
                    await Task.Delay(1000);
                }

            }).FileAndForget("vs/spotify/failure");
        }

        private async Task RefreshControlsAsync()
        {
            if (IsAuthenticated)
            {
                // Switch to background thread
                await TaskScheduler.Default;

                var client = await new SpotifyClientFactory().GetClientAsync();
                var currentPlayback = await client.Player.GetCurrentPlayback();
                var currentPlayingItem = currentPlayback.Item;

                string song = "";
                if (currentPlayingItem is FullTrack track)
                {
                    song = $"{track.Artists.FirstOrDefault().Name} - {track.Name}";
                }

                // Switch back to UI thread to update UI
                await this.joinableTaskFactory.SwitchToMainThreadAsync(this.package.DisposalToken);

                this.CurrentlyPlayingItemTitle = song;
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
