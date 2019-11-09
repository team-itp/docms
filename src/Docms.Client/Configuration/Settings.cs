using System.Configuration;
using System.IO;

namespace Docms.Client.Configuration
{
    public class Settings
    {
        private static ClientSettingsSection settings;

        static Settings()
        {
            var exe = Path.GetFullPath("docmssync.exe");
            var configuration = ConfigurationManager.OpenExeConfiguration(exe);
            settings = configuration.GetSectionGroup("applicationSettings")?
                .Sections["docmssync.Properties.Settings"] as ClientSettingsSection;
        }

        private static string GetConfig(string v)
        {
            return settings.Settings.Get(v).Value.ValueXml.InnerXml;
        }

        public static string AppPath => GetConfig("AppPath");
        public static string WatchPath => GetConfig("WatchPath");
        public static string ServerUrl => GetConfig("ServerUrl");
        public static string UploadClientId => GetConfig("UploadClientId");
        public static string UploadUserName => GetConfig("UploadUserName");
        public static string UploadUserPassword => GetConfig("UploadUserPassword");
    }
}
