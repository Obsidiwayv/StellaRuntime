using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace StellaBootstrapper
{
    public class BootstrapPreset
    {
        public Uri Sound { get; init; }
        public Uri BackgroundImage { get; init; }
        public Uri FontFace { get; init; }
    }

    /// <summary>
    /// Interaction logic for BootStrapperWindow.xaml
    /// </summary>
    public partial class BootStrapperWindow : Window
    {
        private MediaPlayer media = new();

        public BootStrapperWindow(BootstrapPreset preset)
        {
            InitializeComponent();

            BackgroundImage.Source = new BitmapImage(preset.BackgroundImage);
            StatusText.FontFamily = new FontFamily(preset.FontFace, "DefaultFont");

            media.Open(preset.Sound);
            media.Play();
        }

        public void UpdateBootstrapStatus(string stat)
        {
            StatusText.Content = stat;
        }

        public void DownloadClient(string extractionDirectory)
        {

        }
    }
}
