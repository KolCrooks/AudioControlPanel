using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            this.InitializeComponent();
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            pathtext.Text = localSettings.Values["eqconfigPath"] as string;
        }


        private async void ConfigChange_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.FileTypeFilter.Add(".txt");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            this.pathtext.Text = file.Path;
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            saveconfigbutton.IsEnabled = localSettings.Values["eqconfigPath"] != pathtext.Text;
        }

        private void saveSettings()
        {
            // Save a setting locally on the device
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["eqconfigPath"] = this.pathtext.Text;
        }

        private void saveconfigbutton_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
            saveconfigbutton.IsEnabled = false;
        }

        private void pathtext_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            saveconfigbutton.IsEnabled = localSettings.Values["eqconfigPath"] != pathtext.Text;
        }
    }
}
