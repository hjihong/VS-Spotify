﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.VisualStudio;
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
        private bool isShuffled = false;
        private int volume;
        private bool isVolumeExpanded = false;
        private string currentlyPlayingItemTitle;
        private readonly JoinableTaskFactory joinableTaskFactory;
        private readonly VSSpotifyPackage package;
        private Timer refreshTimer;
        private bool isVisualStudioActivated;
        private string currentlyPlayingItemUrl;

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

                    if (isAuthenticated)
                    {
                        OnUserSignedIn();
                    }
                    else
                    {
                        // User just signed out, clean up controls
                        OnUserSignedOut();
                    }
                }
            }
        }

        public bool IsShuffled
        {
            get
            {
                return IsShuffled;
            }
            private set
            {
                if (isShuffled != value)
                {
                    isShuffled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShuffled)));
                }
            }
        }

        private void OnUserSignedIn()
        {
            // User just logged in, refresh controls
            BeginControlRefresh(state: null);
        }

        private async void OnUserSignedOut()
        {
            if (!this.isPaused)
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                await client.Player.PausePlayback();
                this.isPaused = true;
                this.CurrentlyPlayingItemTitle = "";
                this.currentlyPlayingItemUrl = null;
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

        public int Volume 
        {
            get
            {
                return volume;
            }
            private set
            {
                if (volume != value)
                {
                   volume = value;
                   PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Volume)));
                }
                
            }
        }

        public bool IsVolumeExpanded
        {
            get
            {
                return isVolumeExpanded;
            }
            private set
            {
                if (isVolumeExpanded != value)
                {
                    isVolumeExpanded = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVolumeExpanded)));
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

        public string CurrentlyPlayingItemUrl
        {
            get
            {
                return currentlyPlayingItemUrl;
            }
            private set
            {
                if (currentlyPlayingItemUrl != value)
                {
                    currentlyPlayingItemUrl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentlyPlayingItemUrl)));
                }
            }
        }

        public SpotifyControl(JoinableTaskFactory joinableTaskFactory, VSSpotifyPackage package)
        {
            this.joinableTaskFactory = joinableTaskFactory;
            this.package = package;

            InitializeComponent();

            // If teh control is being created, VS is active
            this.isVisualStudioActivated = true;
            // Kick in initial refresh
            this.refreshTimer = new Timer(BeginControlRefresh, state: null, dueTime:0, Timeout.Infinite);

            Application.Current.Activated += OnApplicationActivated;
            Application.Current.Deactivated += OnApplicationDeactivated;
        }

        private void BeginControlRefresh(object state)
        {
            if (this.isVisualStudioActivated && !this.package.DisposalToken.IsCancellationRequested)
            {

                this.joinableTaskFactory.RunAsync(async () =>
                {
                    if (!this.package.DisposalToken.IsCancellationRequested)
                    {
                        await RefreshControlsAsync();
                    }

                // If user is authenticated and VS is active, tick again in 5s
                if (this.isVisualStudioActivated && this.IsAuthenticated)
                    {
                        this.refreshTimer.Change(dueTime: 5000, period: Timeout.Infinite);
                    }

                }).FileAndForget("vs/spotify/failure");
            }
        }

        private void OnApplicationDeactivated(object sender, EventArgs e)
        {
            this.isVisualStudioActivated = false;
        }

        private void OnApplicationActivated(object sender, EventArgs e)
        {
            this.isVisualStudioActivated = true;
            if (this.IsAuthenticated)
            {
                BeginControlRefresh(state: null);
            }
        }

        private async Task RefreshControlsAsync()
        {
            // If not authenticated, we want to try to refresh the credentials silently
            // first before showing that the user is logged out.
            if (!IsAuthenticated)
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
            }

            if (IsAuthenticated)
            {
                // Switch to background threadn
                await TaskScheduler.Default;

                var client = await new SpotifyClientFactory().GetClientAsync();
                var currentPlayback = await client.Player.GetCurrentPlayback();
                // If there's no playback, skip the updates.
                if (currentPlayback != null)
                {
                    var currentPlayingItem = currentPlayback.Item;
                    int currentVolume = (int)currentPlayback.Device.VolumePercent;

                    string song = null;
                    string songImageUrl = null;
                    if (currentPlayingItem is FullTrack track)
                    {
                        song = $"{track.Artists.FirstOrDefault().Name} - {track.Name}";
                        var image = track.Album.Images.FirstOrDefault();
                        if (image != null)
                        {
                            songImageUrl = image.Url;
                        }
                    }

                    // Switch back to UI thread to update UI
                    await this.joinableTaskFactory.SwitchToMainThreadAsync(this.package.DisposalToken);

                    this.CurrentlyPlayingItemTitle = song;
                    this.CurrentlyPlayingItemUrl = songImageUrl;
                    this.IsPaused = !currentPlayback.IsPlaying;
                    this.isShuffled = currentPlayback.ShuffleState;
                    this.Volume = currentVolume;

                    //Manually update Volume Slider UI
                    VolumeSlider.Value = currentVolume;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string PlayNextTooltip
        {
            get
            {
                var shortcut = KeyBindingHelper.GetKeyBinding(SpotifyPlayNextCommand.CommandSet, (int)SpotifyPlayNextCommand.CommandId, VSConstants.GUID_VSStandardCommandSet97);
                return string.IsNullOrEmpty(shortcut) ? "Spotify Play Next" : $"Spotify Play Next ({shortcut})";
            }
        }

        public string PlayPreviousTooltip
        {
            get
            {
                var shortcut = KeyBindingHelper.GetKeyBinding(SpotifyPlayPreviousCommand.CommandSet, (int)SpotifyPlayPreviousCommand.CommandId, VSConstants.GUID_VSStandardCommandSet97);
                return string.IsNullOrEmpty(shortcut) ? "Spotify Play Previous" : $"Spotify Play Previous ({shortcut})";
            }
        }

        public string PlayTooltip
        {
            get
            {
                var shortcut = KeyBindingHelper.GetKeyBinding(SpotifyPlayOrPauseCommand.CommandSet, (int)SpotifyPlayOrPauseCommand.CommandId, VSConstants.GUID_VSStandardCommandSet97);
                return string.IsNullOrEmpty(shortcut) ? "Spotify Play" : $"Spotify Play ({shortcut})";
            }
        }

        public string PauseTooltip
        {
            get
            {
                var shortcut = KeyBindingHelper.GetKeyBinding(SpotifyPlayOrPauseCommand.CommandSet, (int)SpotifyPlayOrPauseCommand.CommandId, VSConstants.GUID_VSStandardCommandSet97);
                return string.IsNullOrEmpty(shortcut) ? "Spotify Pause" : $"Spotify Pause ({shortcut})";
            }
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayOrPauseAsync();
        }

        internal async Task PlayOrPauseAsync()
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();

                if (isPaused)
                {
                    if (await client.Player.ResumePlayback())
                    {
                        IsPaused = false;
                    }
                }
                else
                {
                    if (await client.Player.PausePlayback())
                    {
                        IsPaused = true;
                    }
                }
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

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            await PlayPreviousAsync();
        }

        internal async Task PlayPreviousAsync()
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                await client.Player.SkipPrevious();
                // Spotify needs a bit of delay to actually switch to next song
                this.refreshTimer.Change(dueTime: 500, period: Timeout.Infinite);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await PlayNextAsync();
        }

        internal async Task PlayNextAsync()
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                await client.Player.SkipNext();
                // Spotify needs a bit of delay to actually switch to next song
                this.refreshTimer.Change(dueTime: 500, period: Timeout.Infinite);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                await client.Player.SetShuffle(new PlayerShuffleRequest(!isShuffled)); 
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void SongTitleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                var currentPlaying = await client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
                var link = currentPlaying.Context.ExternalUrls["spotify"];
                System.Diagnostics.Process.Start(link);
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void VolumeSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            //Part 1 is to get the new volume as an int
            int volumeLevel = 0;
            if (sender is Slider volumeSlider) 
            { 
                volumeLevel = (int)Math.Round(volumeSlider.Value); 
            }

            //Part 2 is to change the current volume
            try
             {
                 var vol = new PlayerVolumeRequest(volumeLevel); 
                 var client = await new SpotifyClientFactory().GetClientAsync();
                 await client.Player.SetVolume(vol);
             }
             catch (Exception ex)
             {
                 await Console.Error.WriteLineAsync(ex.Message);
             }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            IsVolumeExpanded = !isVolumeExpanded;
        }

        private void OpenInAppButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://open.spotify.com");
        }
    }
}
