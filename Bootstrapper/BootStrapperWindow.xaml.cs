using RivenSDK.Audio;
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
        public string Name { get; init; }
        public string Sound { get; init; }
        public Uri BackgroundImage { get; init; }
        public string FontFace { get; init; }
    }

    public delegate void ProgressTriggerEvent(double current);
    public delegate void ClientShutdownTriggerEvent();

    /// <summary>
    /// Interaction logic for BootStrapperWindow.xaml
    /// </summary>
    public partial class BootStrapperWindow : Window
    {
        private RivenFmodSystem Media = new();

        private string? RobloxDir;

        public event ProgressTriggerEvent ProgressTrigger;
        public event ClientShutdownTriggerEvent ClientShutdownTrigger;

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

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            timer.Tick += (_, _) =>
            {
                Media.UpdateSystem();
            };

            if (preset.Sound != "none")
            {
                Media.LoadSound("PresetSound", preset.Sound);
                Media.PlaySound("PresetSound");
            }

            BackgroundImage.Source = new BitmapImage(preset.BackgroundImage);
            StatusText.FontFamily = new(preset.FontFace);
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

        private void Timer_Tick(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
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
                if (curVersion == manifest!.ClientVersionUpload)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        Progress.Visibility = Visibility.Hidden;
                        StatusText.Content = "No Updates, Launching";
                        Hide();
                    });
                    Process proc = StartClient();
                    await Task.Run(() =>
                    {
                        proc.WaitForExit();
                        Dispatcher.Invoke(() => Close());
                        Media.Quit();
                    });
                    return;
                }
                // Delete Roblox Dir and reinstall
                Directory.CreateDirectory(RobloxDir!);
            }
            foreach (var zip in await Downloader.GetArchiveManifest(manifest!.ClientVersionUpload))
            {
                if (!CanDownload)
                {
                    break;
                }
                // File.WriteAllText($"{Directory.GetCurrentDirectory()}/DEBUG", CDN.GetPackage(manifest!.ClientVersionUpload, zip));
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
                            ProgressTrigger.Invoke(prog);
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
                Hide();
            });
            Process proc2 = StartClient();
            await Task.Run(() =>
            {
                proc2.WaitForExit();
                Dispatcher.Invoke(() => Close());
                Media.Quit();
            });
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            CanDownload = false;
        }
    }
}
