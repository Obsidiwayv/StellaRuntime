using ICSharpCode.SharpZipLib.Zip;
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

            new(@"WebView2.zip", @""),
            new(@"WebView2RuntimeInstaller.zip", @"WebView2RuntimeInstaller"),
        ];

        public static void ExtractZipFiles(string from, string to)
        {
            foreach (ContentMap map in Map)
            {
                string zip = $"{from}/{map.OriginalZip}";
                using var fs = File.OpenRead(zip);
                using var zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(fs);

                foreach (ZipEntry entry in zipFile)
                {
                    if (!entry.IsFile)
                    {
                        continue;
                    }

                    string entryFileName = entry.Name;
                    string fullPath = Path.Combine($"{to}/{map.OutputDirectory}", entryFileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                    using var zipStream = zipFile.GetInputStream(entry);
                    using var streamWriter = File.Create(fullPath);

                    zipStream.CopyTo(streamWriter);
                }
                fs.Close();
                zipFile.Close();
                File.Delete(zip);
            }
        }
    }
}