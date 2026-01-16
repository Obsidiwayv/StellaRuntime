using System;
using System.Collections.Generic;
using System.Text;

namespace Roblox
{
    public class CDN
    {
        public static string WindowsBinaryManifest { get; } 
            = "https://clientsettingscdn.roblox.com/v2/client-version/WindowsPlayer";

        public static string GetVersionManifest(string version)
        {
            return GetPackage(version, "rbxPkgManifest.txt");
        }

        public static string GetPackage(string version, string package)
        {
            return $"https://setup.rbxcdn.com/channel/common/{version}-{package}";
        }
    }
}
