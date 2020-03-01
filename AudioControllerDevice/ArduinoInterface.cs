using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AudioControllerDevice
{
    class ArduinoInterface
    {
        static SerialPort port;
        public bool hasError = false;
        public static int baud;
        public static Tuple<string,string> DeviceInfo;

        public enum Commands : byte
        {
            IMAGE = 0x00,
            PLAY = 0x01,
            PAUSE = 0x02,
            SETTIME = 0x03,
            NONE = 0x99,
        }

        public ArduinoInterface()
        {
            //9600, 14400,19200,28800,38400,57600,115200
            baud = 200000;
            DeviceInfo = AutodetectArduinoPort();
            if (DeviceInfo == null)
            {
                hasError = true;
                Console.WriteLine("Unable To Find Audio Device");
            }
        }

        public static bool retryConnection()
        {
            DeviceInfo = AutodetectArduinoPort();

            return DeviceInfo != null;
        }

        public static void BeginSerial()
        {
            port = new SerialPort(DeviceInfo.Item2, baud);
            try
            {
                port.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open Serial Connection");
            }
            port.DataReceived += dataIn;
        }

        private static void dataIn(object sender, SerialDataReceivedEventArgs e)
        {
            int command = port.ReadByte();
            Console.WriteLine("Command: " + command);
            switch (command)
            {
                case 0x01:
                    if (SpotifyLoad.connected)
                    {
                        Console.WriteLine("Pausing");
                        SpotifyLoad.pausePlay();
                    }
                 break;
                case 0x05:
                    Console.WriteLine("0x05");
                    SpotifyLoad.api.GetPlayback().Device.VolumePercent++;
                    break;
                case 0x06:
                    Console.WriteLine("0x06");
                    SpotifyLoad.api.GetPlayback().Device.VolumePercent--;
                    break;
            }
        }

        private static Tuple<string,string> AutodetectArduinoPort()
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();
                    Console.WriteLine(desc);
                    Console.WriteLine(deviceId);

                    if (desc.Contains("Arduino") || desc.Contains("Silicon Labs"))
                    {
                        return new Tuple<string, string>(desc,deviceId);
                    }
                }
            }
            catch (ManagementException e)
            {
                /* Do Nothing */
            }

            return null;
        }

        private static Bitmap LoadPicture(string url)
        {
            HttpWebRequest wreq;
            HttpWebResponse wresp;
            Stream mystream;
            Bitmap bmp;

            bmp = null;
            mystream = null;
            wresp = null;
            try
            {
                wreq = (HttpWebRequest)WebRequest.Create(url);
                wreq.AllowWriteStreamBuffering = true;

                wresp = (HttpWebResponse)wreq.GetResponse();

                if ((mystream = wresp.GetResponseStream()) != null)
                    bmp = new Bitmap(mystream);
            }
            finally
            {
                if (mystream != null)
                    mystream.Close();

                if (wresp != null)
                    wresp.Close();
            }
            return (bmp);
        }

        public static void sendFromImageUrl(string url)
        {
            var image = LoadPicture(url);
            sendImage(image);
        }

        public static void sendImage(Bitmap image)
        {
            Bitmap img = resizeBitmap(image);

            byte[] data = imageToByteArray(img);
            sendBytes(data, Commands.IMAGE);
        }

        public static void sendPlay(Int32 currTime, Int32 songLength)
        {
            byte[] curr = BitConverter.GetBytes(currTime);
            byte[] total = BitConverter.GetBytes(songLength);
            sendBytes(curr.Concat(total).ToArray(), Commands.PLAY);
        }

        public static void sendPause(Int32 currTime, Int32 songLength)
        {
            byte[] curr = BitConverter.GetBytes(currTime);
            byte[] total = BitConverter.GetBytes(songLength);
            sendBytes(curr.Concat(total).ToArray(), Commands.PAUSE);
        }

        private static Bitmap resizeBitmap(Bitmap image)
        {
            Bitmap animage = new Bitmap(240, 240, PixelFormat.Format16bppRgb565);
            using (Graphics gr = Graphics.FromImage(animage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.DrawImage(image, new Rectangle(0, 0, 240, 240));
            }
            return animage;
        }

        private static byte[] imageToByteArray(Bitmap bmp)
        {

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
             bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        public static void sendBytes(byte[] data, Commands command = Commands.NONE)
        {
            if (command != Commands.NONE)
            {
                byte[] sendBit = { (byte)command };
                port.Write(sendBit, 0, 1);
            }
            port.Write(data, 0, data.Length);
        }
    }
}
