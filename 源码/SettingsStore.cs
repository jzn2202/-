using System;
using System.IO;
using System.Xml.Serialization;

namespace ReminderMini
{
    public sealed class AppSettings
    {
        public string Hours { get; set; }
        public string Minutes { get; set; }
        public string Seconds { get; set; }
        public string Message { get; set; }
        public bool SoundEnabled { get; set; }

        public AppSettings()
        {
            Hours = "0";
            Minutes = "40";
            Seconds = "0";
            Message = "";
            SoundEnabled = true;
        }
    }

    internal static class SettingsStore
    {
        internal static string PathOverride { get; set; }

        private static string PathName
        {
            get
            {
                if (!string.IsNullOrEmpty(PathOverride)) return PathOverride;
                string folder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ReminderMini");
                return System.IO.Path.Combine(folder, "settings.xml");
            }
        }

        public static AppSettings Load()
        {
            try
            {
                if (!File.Exists(PathName)) return new AppSettings();
                using (FileStream stream = File.OpenRead(PathName))
                {
                    return (AppSettings)new XmlSerializer(typeof(AppSettings)).Deserialize(stream);
                }
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            string path = PathName;
            string directory = System.IO.Path.GetDirectoryName(path);
            Directory.CreateDirectory(directory);
            string temporary = path + ".tmp";
            using (FileStream stream = File.Create(temporary))
            {
                new XmlSerializer(typeof(AppSettings)).Serialize(stream, settings);
            }
            File.Copy(temporary, path, true);
            File.Delete(temporary);
        }
    }
}
