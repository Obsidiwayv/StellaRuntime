using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace StellaBootstrapper
{
    public record ContentMap(string OriginalZip, string OutputDirectory);

    public class ContentPipeline
    {
        public static List<ContentMap> Map = [
            new(@"RobloxApp.zip", @""),
            new(@"content-avatar.zip", @"content/avatar"),
            new(@"content-configs.zip", @"content/configs"),
            new(@"content-fonts.zip", @"content/fonts"),
            new(@"content-sky.zip", @"content/sky"),
            new(@"content-sounds.zip", @"content/sounds"),
            new(@"content-textures2.zip", @"content/textures"),
            new(@"content-models.zip", @"content/models"),

            new(@"content-textures3.zip", @"PlatformContent/pc/textures"),
            new(@"content-terrain.zip", @"PlatformContent/pc/terrain"),
            new(@"content-platform-fonts.zip", @"PlatformContent/pc/fonts"),
            new(@"content-platform-dictionaries.zip", @"PlatformContent/pc/shared_compression_dictionaries"),

            new(@"extracontent-luapackages.zip", @"ExtraContent/LuaPackages"),
            new(@"extracontent-translations.zip", @"ExtraContent/translations"),
            new(@"extracontent-models.zip", @"ExtraContent/models"),
            new(@"extracontent-textures.zip", @"ExtraContent/textures"),
            new(@"extracontent-places.zip", @"ExtraContent/places"),

            new(@"shaders.zip", @"shaders"),
            new(@"ssl.zip", @"ssl"),
            new(@"redist.zip", @""),

            new(@"WebView2.zip", @""),
            new(@"WebView2RuntimeInstaller.zip", @"WebView2RuntimeInstaller"),
        ];

        public static void ExtractZipFiles(string from, string to)
        {
            foreach (ContentMap map in Map)
            {
                var zip = $"{from}/{map.OriginalZip}";
                ZipFileEx.ExtractToDirectory(zip, $"{to}/{map.OutputDirectory}");
                File.Delete(zip);
            }
        }
    }
}