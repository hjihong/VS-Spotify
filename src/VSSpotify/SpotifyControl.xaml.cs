﻿using System;
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
        private bool isNotAuthenticated = true;

        public SpotifyControl()
        {
            InitializeComponent();
            isNotAuthenticated = !(new SpotifyAuthManager().IsAuthenticated());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
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
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var authManager = new SpotifyAuthManager())
                {
                    if (!IsNotAuthenticated)
                    {
                        authManager.ClearCredentials();
                        IsNotAuthenticated = true;
                    }
                    else
                    {
                        await authManager.GetCredentialsAsync();
                        IsNotAuthenticated = !authManager.IsAuthenticated();
                        IsNotAuthenticated = false;
                    }
                }
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync(ex.Message);
            }
        }

        public bool IsNotAuthenticated
        {
            get
            {
                return isNotAuthenticated;
            }
            private set
            {
                isNotAuthenticated = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNotAuthenticated)));
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
