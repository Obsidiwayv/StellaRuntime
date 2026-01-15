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
        [JsonPropertyName("preset")]
        public string PresetName { get; }

        [JsonPropertyName("font")]
        public string Font { get; }

        [JsonPropertyName("background")]
        public string BackgroundImage { get; }

        [JsonPropertyName("sound")]
        public string Sound { get; }
    }

    internal class BootstrapperPresets
    {
        public static List<BootstrapPreset> Get()
        {
            List<BootstrapPreset> presets = [];
            foreach (var preset in Directory.GetFiles("Presets", ".json"))
            {
                var directory = Path.GetDirectoryName(preset);
                var json = JsonSerializer.Deserialize<PresetManifest>(File.ReadAllText(preset));

                if (json == null) continue;
                var presetDir = $"{Directory.GetCurrentDirectory()}/Presets/{directory}";
                presets.Add(new()
                {
                    BackgroundImage = new Uri($"{presetDir}/{json.BackgroundImage}"),
                    FontFace = new Uri($"{presetDir}/{json.Font}"),
                    Sound = new Uri($"{presetDir}/{json.Sound}")
                });
            }
            return presets;
        }
    }
}
