using System;
using System.IO;
using System.Text.Json;

namespace CursorSwitcher
{
    public class AppSettings
    {
        public string? Style1Path { get; set; }
        public string? Style2Path { get; set; }
        public string? Hotkey { get; set; } = "F9";
        public string CurrentStyle { get; set; } = "Default";

        public string FilePath { get; private set; }

        public AppSettings(string filePath)
        {
            FilePath = filePath;
        }

        public static AppSettings Load(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings is not null)
                    {
                        settings.FilePath = filePath;
                        return settings;
                    }
                }
                catch
                {
                    // ignore invalid file and recreate defaults
                }
            }

            var defaultSettings = new AppSettings(filePath)
            {
                Style1Path = null,
                Style2Path = null,
                Hotkey = "F9",
                CurrentStyle = "Default"
            };

            defaultSettings.Save();
            return defaultSettings;
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
