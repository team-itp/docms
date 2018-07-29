using Docms.Uploader.Common;
using System;
using System.Security;

namespace Docms.Uploader.Properties
{
    public static class SettingsExtension
    {
        internal static void SetPasswordHash(this Settings settings, SecureString password)
        {
            settings.PasswordHash = Cipher.Encrypt(password.ConvertToUnsecureString());
        }

        internal static SecureString GetPassword(this Settings settings)
        {
            try
            {
                return Cipher.Decrypt(settings.PasswordHash).ConvertToSecureString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
