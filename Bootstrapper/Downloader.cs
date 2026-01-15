using Roblox.JSON;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StellaBootstrapper
{
    internal class Downloader
    {
        private static readonly HttpClient client = new();

        public static async Task<WindowsPlayerManifest?> GetVersionBuild() 
        {
            return await GetManifest<WindowsPlayerManifest?>(RobloxUrls.Binary);
        }

        public static async Task<List<string>> GetArchiveManifest()
        {
            var builds = await GetVersionBuild();
            if (builds == null)
            {
                // Return an empty array due to network error
                return ["EMPTY"];
            }
            var manifest = await client.GetStringAsync(RobloxUrls.GetVersionManifest(builds.ClientVersionUpload));
            if (manifest != null)
            {
                var zips = new List<string>();
                // Invalid Manifest
                if (!manifest.StartsWith("v0"))
                {
                    return ["INVALID_MANIFEST"];
                }
                foreach (var line in manifest.Split("\n"))
                {
                    if (line.EndsWith(".zip"))
                    {
                        zips.Add(line);
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
            var res = await client.GetStringAsync(url);
            return JsonSerializer.Deserialize<T>(res);
        }
    }
}
