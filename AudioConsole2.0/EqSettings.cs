using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AudioConsole2._0
{
    class EqSettings
    {
        public const string defaultEQ = "GraphicEQ: 25 0; 40 0; 63 0; 100 0; 160 0; 250 0; 400 0; 630 0; 1000 0; 1600 0; 2500 0; 4000 0; 6300 0; 10000 0; 16000 0";

        public static void changeBase(double newVal)
        {
            changeBands(0, 5, newVal);
        }

        public static void changeMid(double newVal)
        {
            changeBands(5, 5, newVal);
        }

        public static void changeTreble(double newVal)
        {
            changeBands(10, 5, newVal);
        }


        public static string replaceEQIndex(string input, int index, double newVal)
        {
            var parts = Regex.Split(input, "\\:\\s|\\;\\s");
            string output = parts[0] + ": ";
            parts[index + 1] = Regex.Replace(parts[index + 1], "\\s([0-9.\\-]*)", " " + newVal);
            for (int i = 1; i < parts.Length - 1; i++)
                output += parts[i] + "; ";
            output += parts[parts.Length - 1];
            return output;
        }

        internal async static Task<double[]> getSettings()
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var lines = await File.ReadAllLinesAsync(localSettings.Values["eqconfigPath"] as string);
            if (lines.Length == 0) lines = new string[]{ defaultEQ };
            var parts = Regex.Split(lines[0], "\\:\\s|\\;\\s");
            double[] ret = new double[3];
            for(int i = 0; i < 3; i++)
            {
                string str = Regex.Split(parts[(i * 5) + 1], "\\s")[1];
                ret[i] = Double.Parse(str);
            }

            return ret;
        }

        public static async void changeBands(int startI, int cnt, double val)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var file = await StorageFile.GetFileFromPathAsync(localSettings.Values["eqconfigPath"] as string);
            try
            {
                var lines = await FileIO.ReadLinesAsync(file);
                if (lines.Count == 0) lines.Add(defaultEQ);
                string txt = lines[0];
                for (int i = 0; i < cnt; i++)
                {
                    txt = replaceEQIndex(txt, i+startI, val);
                }
                lines[0] = txt;
                await FileIO.WriteLinesAsync(file, lines);
                Debug.WriteLine("Changed: " + val + "db");
            }
            catch (Exception e) { Debug.WriteLine(e); }
        }
    }
}
