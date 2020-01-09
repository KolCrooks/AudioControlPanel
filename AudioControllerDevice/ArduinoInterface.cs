using System;
using System.Collections.Generic;
using System.Drawing;
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
        public static string name;
        public ArduinoInterface()
        {
            //9600, 14400,19200,28800,38400,57600,115200
            baud = 115200;
            name = AutodetectArduinoPort();
            if (name == null)
            {
                hasError = true;
                Console.WriteLine("Unable To Find Audio Device");
            }
        }

        public static bool retryConnection()
        {
            name = AutodetectArduinoPort();

            return name != null;
        }

        public static void BeginSerial()
        {
            port = new SerialPort(name, baud);
            port.Open();
        }

        private static string AutodetectArduinoPort()
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

                    if (desc.Contains("Arduino"))
                    {
                        return deviceId;
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
            sendBitmap(image);
        }

        public static void sendBitmap(Bitmap image)
        {
            Byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, ImageFormat.Bmp);

                data = memoryStream.ToArray();
            }

            port.Write(data, 0, data.Length);
        }
    }
}
