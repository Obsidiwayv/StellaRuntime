using RivenSDK.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StellaBootstrapper
{
    public class PresetManifest
    {
        [JsonPropertyName("presetName")]
        public string PresetName { get; set; }

        [JsonPropertyName("font")]
        public string? Font { get; set; }

        [JsonPropertyName("background")]
        public string BackgroundImage { get; set; }

        [JsonPropertyName("sound")]
        public string? Sound { get; set; }
    }

    public class BootstrapperPresets
    {
        public static List<BootstrapPreset> Get()
        {
            List<BootstrapPreset> presets = [];
            foreach (var preset in Directory.GetFiles(
                $"{Directory.GetCurrentDirectory()}/Presets", "*.json", SearchOption.AllDirectories))
            {
                var json = JsonSerializer.Deserialize<PresetManifest>(File.ReadAllText(preset));

                if (json == null) continue;
                presets.Add(new()
                {
                    BackgroundImage = new Uri($"{Directory.GetCurrentDirectory()}/Presets/{json.BackgroundImage}"),
                    FontFace = json.Font == null ? "None" : $"Presets/{json.Font}",
                    Sound = json.Sound == null ? "None": $"Presets/{json.Sound}",
                    Name = json.PresetName
                });
            }
            return presets;
        }
    }
}
