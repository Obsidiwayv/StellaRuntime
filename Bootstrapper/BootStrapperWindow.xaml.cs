using Roblox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StellaBootstrapper
{
    public class BootstrapPreset
    {
        public Uri Sound { get; init; }
        public Uri BackgroundImage { get; init; }
        //public Uri FontFace { get; init; }
    }

    public delegate void ProgressTriggerEvent(int current);

    /// <summary>
    /// Interaction logic for BootStrapperWindow.xaml
    /// </summary>
    public partial class BootStrapperWindow : Window
    {
        private MediaPlayer media = new();

        private string? RobloxDir;

        public event ProgressTriggerEvent ProgressTrigger;

        private bool CanDownload = true;

        private const string AppSettings =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
            "<Settings>\r\n" +
            "	<ContentFolder>content</ContentFolder>\r\n" +
            "	<BaseUrl>http://www.roblox.com</BaseUrl>\r\n" +
            "</Settings>\r\n";

        public BootStrapperWindow(BootstrapPreset preset, string robloxDir)
        {
            InitializeComponent();

            media.Open(preset.Sound);
            media.Play();
            media.Close();

            BackgroundImage.Source = new BitmapImage(preset.BackgroundImage);
            // Fix at some point 
            //Dispatcher.Invoke(() =>
            //{
            //    StatusText.FontFamily = new FontFamily(preset.FontFace, "./#");
            //});

            StatusText.Content = "Updating";

            RobloxDir = robloxDir;

            StatusText.UpdateLayout();
            StatusText.InvalidateVisual();

            Downloader.AddUserAgent();
        }

        public Process StartClient() 
        {
            if (RobloxDir == null)
            {
                MessageBox.Show("Invalid roblox directory", "Runtime Error");
            }
            return Process.Start($"{RobloxDir}/RobloxPlayerBeta.exe");
        }

        public async Task DownloadClient(string directory)
        {
            var manifest = await Downloader.GetVersionBuild();
            var versionFile = $"{directory}/VERSION";
            if (manifest == null)
            {
                MessageBox.Show("Unable to download Roblox: Manifest is null");
                Close();
            }
            if (File.Exists(versionFile))
            {
                var curVersion = File.ReadAllText(versionFile);
                if (curVersion != manifest!.ClientVersionUpload)
                {
                    // Just return for now
                    return;
                }
            }
            foreach (var zip in await Downloader.GetArchiveManifest(manifest!.ClientVersionUpload))
            {
                if (!CanDownload)
                {
                    break;
                }
            //    File.WriteAllText($"{Directory.GetCurrentDirectory()}/DEBUG", CDN.GetPackage(manifest!.ClientVersionUpload, zip));
                using var res = await Downloader.client.GetAsync(CDN.GetPackage(manifest!.ClientVersionUpload, zip));
                res.EnsureSuccessStatusCode();

                var totalBytes = res.Content.Headers.ContentLength ?? -1L;
                var canReport = totalBytes != -1 && Progress != null;

                using var stream = await res.Content.ReadAsStreamAsync();
                using var fileStream = File.Create($"{directory}/{zip}");

                var buffer = new byte[8192];
                long total = 0;
                int read;

                while ((read = await stream.ReadAsync(buffer)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, read));
                    total += read;
                    if (canReport)
                    {
                        double prog = (double)total / totalBytes * 100;
                        Dispatcher.Invoke(() =>
                        {
                            Progress!.Value = prog;
                        });
                    }
                }
            }
            File.WriteAllText(versionFile, manifest!.ClientVersionUpload);
            File.WriteAllText($"{RobloxDir}/AppSettings.xml", AppSettings);

            await Dispatcher.InvokeAsync(() => 
            {
                Progress.IsIndeterminate = true;
                StatusText.Content = "Extracting";
            }, DispatcherPriority.Render);

            await Task.Run(() =>
            {
                ContentPipeline.ExtractZipFiles(directory, RobloxDir!);
            });

            await Dispatcher.InvokeAsync(() =>
            {
                Progress.Visibility = Visibility.Hidden;
                StatusText.Content = "Done!, Launching";
                Thread.Sleep(2);
                Hide();
            });
            Process proc = StartClient();
            await Task.Run(() =>
            {
                proc.WaitForExit();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            CanDownload = false;
        }
    }
}
