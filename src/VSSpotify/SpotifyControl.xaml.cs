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
        public bool isPaused { get; private set; } = false; //right after we authenticate, we will get corrrect value

        public SpotifyControl()
        {
            InitializeComponent();
            IsAuthenticated = new SpotifyAuthManager().IsAuthenticated();

        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            //logic:
            //func covers pause and play 
            //if pause = true then change to false and play music POST 
            //if pause = false then change to true and pause music POST

            //combining the play button api and the pause button api 
            System.Diagnostics.Debug.WriteLine("BUTTON WAS CLICKED AND HITTING METHOD");
            try
            {
                var client = await new SpotifyClientFactory().GetClientAsync();
                System.Diagnostics.Debug.WriteLine("IN TRY STATEMENT: " + client.Player.GetCurrentPlayback()); //debugging

                if (IsAuthenticated)
                {
                    //var client = await new SpotifyClientFactory().GetClientAsync();

                    if (isPaused)
                    {
                        System.Diagnostics.Debug.WriteLine("In if");
                        //Tell api to actually play the song 
                        var resumePlayback = await client.Player.ResumePlayback();
                        System.Diagnostics.Debug.WriteLine("Pause playback: " + resumePlayback.ToString()); //debugging
                        isPaused = false;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("In else");
                        //Tell api to pause the song 
                        Task<bool> pausePlayback = client.Player.PausePlayback();
                        System.Diagnostics.Debug.WriteLine("Resume playback: " + pausePlayback.ToString()); //debugged 
                        isPaused = true;

                    }

                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine("ERROR: " + ex.Message);
                //Console.Error.WriteLine("ERROR WITH PLAYBUTTONCLICK");
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
                Console.Error.WriteLine(ex.Message);
                //Console.Error.WriteLine("ERROR WITH SIGNIN");
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

        private async void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsAuthenticated)
            {
            }
        }
    }
}
