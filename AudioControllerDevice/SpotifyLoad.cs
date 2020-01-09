using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace AudioControllerDevice
{
    public delegate void AudioChangeHandler(object source, AudioChangeArgs e);

    //This is a class which describes the event to the class that recieves it.
    //An EventArgs class must always derive from System.EventArgs.
    public class AudioChangeArgs : EventArgs
    {
        private FullTrack newTrack;
        public AudioChangeArgs(FullTrack newTrack )
        {
            this.newTrack = newTrack;
        }

        public FullTrack getTrack()
        {
            return newTrack;
        }
    }

    class SpotifyLoad
    {
        private static string _clientId = "0d2bd0a545cb4ac88f44b89d0d2207ab";
        private static string _clientSecret = "5a4938d377c142439b4df33c89fe8951";

        public event AudioChangeHandler OnAudioChange;

        SpotifyWebAPI api;
        Token token;
        AuthorizationCodeAuth auth;
        private string currentlyPlaying = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";


        public SpotifyLoad()
        {
            auth = new AuthorizationCodeAuth(
              _clientId,
              _clientSecret,
              "http://localhost:4002",
              "http://localhost:4002",
              Scope.UserReadCurrentlyPlaying
            );

            auth.AuthReceived += async (sender, payload) =>
            {
                auth.Stop();
                token = await auth.ExchangeCode(payload.Code);
                api = new SpotifyWebAPI()
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };
                Thread t = new Thread(spotifyDataLoop);
                t.Start();
            };
            auth.Start(); // Starts an internal HTTP Server
            auth.OpenBrowser();
        }

        public async void spotifyDataLoop()
        {

            while (true)
            {
                if (token.IsExpired())
                {
                    token = await auth.RefreshToken(token.RefreshToken);
                    api.AccessToken = token.AccessToken;
                    api.TokenType = token.TokenType;
                }
                var playback = api.GetPlayingTrack();
                if (playback.HasError()) Console.WriteLine(playback.Error.Message);
                else if (playback.Item == null)
                {
                    if(currentlyPlaying != "")
                    {
                        currentlyPlaying = "";
                        OnAudioChange(this, new AudioChangeArgs(playback.Item));
                        //picture.Image = Properties.Resources.NoSong;
                        //playingLabel.Text = currentlyPlaying;
                    }
                }
                else if (currentlyPlaying != playback.Item.Id)
                {
                    Console.WriteLine(playback.Item.Name + " " + playback.Item.PreviewUrl);
                    currentlyPlaying = playback.Item.Id;
                    OnAudioChange(this, new AudioChangeArgs(playback.Item));
                }
                Thread.Sleep(1000);
            }
        }

    }
}
