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
        private bool isPaused = true; //right after we authenticate, we will get corrrect value

        public SpotifyControl()
        {
            InitializeComponent();
            IsAuthenticated = new SpotifyAuthManager().IsAuthenticated();
            IsPaused = new SpotifyAuthManager().IsPaused();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        //This function covers both the play and pause buttons. 
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        { 
            System.Diagnostics.Debug.WriteLine("BUTTON WAS CLICKED AND HITTING METHOD");
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                //System.Diagnostics.Debug.WriteLine("IN TRY STATEMENT: " + client.Player.GetCurrentPlayback()); //debugging

                if (IsAuthenticated)
                {
                    //var client = await new SpotifyClientFactory().GetClientAsync();
                    if (!isPaused) //should be true
                    {
                        System.Diagnostics.Debug.WriteLine("In if"); 
                        var resumePlayback = await client.Player.PausePlayback();//ERROR WITH THIS LINE
                        System.Diagnostics.Debug.WriteLine("Pause playback: " + client.Player.PausePlayback()); //debugging
                        isPaused = false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("In else");  
                        var pausePlayback = await client.Player.ResumePlayback(); //ERROR WITH THIS LINE
                        System.Diagnostics.Debug.WriteLine("Resume playback: " + client.Player.ResumePlayback()); //debugged 
                        isPaused = true; 
                    }

                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
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
                        _ = authManager.GetCredentialsAsync();
                        IsAuthenticated = !authManager.IsAuthenticated();
                        IsAuthenticated = true;
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return isAuthenticated;
            }
            private set
            {
                isAuthenticated = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAuthenticated)));
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
                isPaused = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPaused)));
            }
        }

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                if (IsAuthenticated)
                {
                    var client = await new SpotifyClientFactory().GetClientAsync();
                    var skipPrev = await client.Player.SkipPrevious();
                    Console.WriteLine("Skip Previous Playback: " + skipPrev);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                //Console.Error.WriteLine("ERROR WITH PREV CLICK");
            }

        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsAuthenticated)
                {
                    //POST - tell spotify to skip to the next song 
                    var client = await new SpotifyClientFactory().GetClientAsync();
                    var skipNext = await client.Player.SkipNext();
                    Console.WriteLine("Skip Next Playback: " + skipNext);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                //Console.Error.WriteLine("ERROR WITH NEXT CLICK");
            }
        }

        private async void SongTitleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsAuthenticated)
                {
                    //needs to update if skip/prev is hit or if the song changes on its own or if song is changed in spotify app
                    var client = await new SpotifyClientFactory().GetClientAsync();
                    var currentPlayback = await client.Player.GetCurrentPlayback();
                    Console.WriteLine("Current song: " + currentPlayback);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                //Console.Error.WriteLine("ERROR WITH SONG TITLE CLICK");
            }
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAuthenticated)
            {
            }
        }
    }
}
