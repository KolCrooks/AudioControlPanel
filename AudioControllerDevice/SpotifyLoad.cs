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
        private PlaybackContext playback;
        public AudioChangeArgs(PlaybackContext playback )
        {
            this.playback = playback;
        }

        public PlaybackContext getPlayback()
        {
            return playback;
        }
    }

    class SpotifyLoad
    {
        private static string _clientId = "0d2bd0a545cb4ac88f44b89d0d2207ab";
        private static string _clientSecret = "5a4938d377c142439b4df33c89fe8951";

        public event AudioChangeHandler OnAudioChange;

        public static SpotifyWebAPI api;
        Token token;
        AuthorizationCodeAuth auth;
        private string currentlyPlaying = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        public static bool connected = false;

        public SpotifyLoad()
        {
            auth = new AuthorizationCodeAuth(
              _clientId,
              _clientSecret,
              "http://localhost:4002",
              "http://localhost:4002",
              Scope.UserReadCurrentlyPlaying | Scope.UserModifyPlaybackState
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

        public static async void nextTrack() {
            await api.SkipPlaybackToNextAsync();
        }

        public static async void prevTrack()
        {
            await api.SkipPlaybackToPreviousAsync();
        }

        public static bool isPlaying()
        {
            return api.GetPlayback().IsPlaying;
        }

        public static void pausePlay()
        {
            if (isPlaying())
            {
                api.PausePlayback();
                ArduinoInterface.sendPause(api.GetPlayback().ProgressMs, api.GetPlayback().Item.DurationMs);
            }
            else
            {
                api.ResumePlayback(offset: "");
                ArduinoInterface.sendPlay(api.GetPlayback().ProgressMs, api.GetPlayback().Item.DurationMs);
            }
        }

        public void mixer()
        {
            //api.
        }

        private async void spotifyDataLoop()
        {
            connected = true;
            bool playbackState = false;
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
                        OnAudioChange(this, new AudioChangeArgs(playback));
                        //picture.Image = Properties.Resources.NoSong;
                        //playingLabel.Text = currentlyPlaying;
                    }
                }
                else if (currentlyPlaying != playback.Item.Id)
                {
                    Console.WriteLine(playback.Item.Name + " " + playback.Item.PreviewUrl);
                    currentlyPlaying = playback.Item.Id;
                    OnAudioChange(this, new AudioChangeArgs(playback));
                }
                if (playback.IsPlaying != playbackState)
                {
                    if (playback.IsPlaying)
                        ArduinoInterface.sendPlay(playback.ProgressMs, playback.Item.DurationMs);
                    else
                        ArduinoInterface.sendPause(playback.ProgressMs, playback.Item.DurationMs);
                }


                playbackState = playback.IsPlaying;
                Thread.Sleep(1000);
            }
        }

    }
}
