using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Roblox
{
    public class WindowsPlayerManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("clientVersionUpload")]
        public string ClientVersionUpload { get; set; }

        [JsonPropertyName("bootstrapperVersion")]
        public string BootstrapperVersion { get; set; }
    }
}
