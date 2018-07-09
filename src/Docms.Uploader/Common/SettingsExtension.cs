namespace Docms.Uploader.Properties
{
    public static class SettingsExtension
    {
        internal static void SetPasswordHash(this Settings settings, string password)
        {
            settings.PasswordHash = password;
        }

        internal static string GetPassword(this Settings settings)
        {
            return settings.PasswordHash;
        }
    }
}
