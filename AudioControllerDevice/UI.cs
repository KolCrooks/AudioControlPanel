using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioControllerDevice
{

    public partial class UI : Form
    {

        public const int VK_MEDIA_NEXT_TRACK = 0xB0;
        public const int VK_MEDIA_PREV_TRACK = 0xB1;
        public const int VK_MEDIA_PLAY_PAUSE = 0xB3;
        private delegate void SafeCallDelegate(string text);
        public UI()
        {
            InitializeComponent();
            setupCustomFont();
            SpotifyLoad spotify = new SpotifyLoad();
            spotify.OnAudioChange += trackChanged;

            ArduinoInterface arduinoInterface = new ArduinoInterface();
            AudioPassthrough ap = new AudioPassthrough();
            if (arduinoInterface.hasError)
            {
                RefreshButton.Visible = true;
            }
            else
            {
                ArduinoInterface.BeginSerial();
                ConnectionLabel.Text = ArduinoInterface.name;
            }
        }

        private void setupCustomFont()
        {
            PrivateFontCollection modernFont = new PrivateFontCollection();

            int fontLength = Properties.Resources.HelveticaNeue_Light.Length;

            // create a buffer to read in to
            byte[] fontdata = Properties.Resources.HelveticaNeue_Light;

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, fontLength);

            // pass the font to the font collection
            modernFont.AddMemoryFont(data, fontLength);
            this.SongLabel.Font = new Font(modernFont.Families[0], 16);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //KeyBoard.SendKeyPress(0xB5);
            showwSTMC();
        }

        private void showwSTMC()
        {
            KeyBoard.SendKeyPress(0xAE);
            KeyBoard.SendKeyPress(0xAF);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            //if the form is minimized  
            //hide it from the task bar  
            //and show the system tray icon (represented by the NotifyIcon control)  
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseSingleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            Show();
            this.notifyIcon1.Visible = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trackChanged(object sender, AudioChangeArgs e)
        {
            if (e.getTrack() != null)
            {
                FullTrack track = e.getTrack();
                pictureBox1.Load(track.Album.Images[2].Url);
                WriteTextSafe(track.Name);
            }
            else
            {
                pictureBox1.Image = Properties.Resources.NoSong;
                WriteTextSafe("No Song Playing");
            }
        }

        private void WriteTextSafe(string text)
        {
            if (SongLabel.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteTextSafe);
                SongLabel.Invoke(d, new object[] { text });
            }
            else
            {
                SongLabel.Text = text;
            }
        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            if (ArduinoInterface.retryConnection())
            {
                RefreshButton.Visible = false;
                ConnectionLabel.Text = ArduinoInterface.name;
                ArduinoInterface.BeginSerial();
            }
        }
    }
}
