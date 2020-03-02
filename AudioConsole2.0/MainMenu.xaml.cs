using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AudioConsole2._0
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : Page
    {
        public MainMenu()
        {
            this.InitializeComponent();
            var settings = EqSettings.getSettings();
            settings.Wait();
            BaseSlider.Value = settings.Result[0];
            midSlider.Value = settings.Result[1];
            trebSlider.Value = settings.Result[2];
        }


        private void BaseSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Debug.WriteLine(e.NewValue);
            EqSettings.changeBase(e.NewValue);
        }

        private void midSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Debug.WriteLine(e.NewValue);
            EqSettings.changeMid(e.NewValue);
        }

        private void trebSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Debug.WriteLine(e.NewValue);
            EqSettings.changeTreble(e.NewValue);
        }
    }
}
