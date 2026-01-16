using Roblox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StellaBootstrapper
{
    internal class Downloader
    {
        public static readonly HttpClient client = new();

        public static void AddUserAgent()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "RobloxPlayer/Win32");
        }

        public static async Task<WindowsPlayerManifest?> GetVersionBuild() 
        {
            return await GetManifest<WindowsPlayerManifest?>(CDN.WindowsBinaryManifest);
        }

        public static async Task<List<string>> GetArchiveManifest(string version)
        {
            var builds = await GetVersionBuild();
            if (builds == null)
            {
                // Return an empty array due to network error
                return ["EMPTY"];
            }
            var manifest = await client.GetAsync(CDN.GetVersionManifest(version));
            var stringManifest = await manifest.Content.ReadAsStringAsync();
            var debug = stringManifest;
            if (manifest != null)
            {
                var zips = new List<string>();
                // Invalid Manifest
                if (!stringManifest.StartsWith("v0"))
                {
                    return ["INVALID_MANIFEST"];
                }
                using StringReader reader = new(stringManifest);
                while (true)
                {
                    string? fileName = reader.ReadLine();
                    

                    if (string.IsNullOrEmpty(fileName)) break;
                    if (fileName.EndsWith(".zip"))
                    {
                        zips.Add(fileName);
                    }
                }
                if (zips.Count != 0)
                {
                    return zips;
                } else
                {
                    return ["EMPTY"];
                }
            } else
            {
                return ["NETWORK_ERROR"];
            }
        }

        private static async Task<T?> GetManifest<T>(string url) 
        {
            var res = await client.GetAsync(url);
            return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync());
        }
    }
}
