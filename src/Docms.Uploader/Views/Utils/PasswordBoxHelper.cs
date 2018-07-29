using Docms.Uploader.Common;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Docms.Uploader.Views.Utils
{
    public class PasswordBoxHelper : DependencyObject
    {
        public static readonly DependencyProperty BindPasswordProperty = DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(false, BindPasswordProperty_Changed));

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
            "Password",
            typeof(SecureString),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(default(SecureString), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PasswordProperty_Changed));

        public static bool GetBindPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(BindPasswordProperty);
        }

        public static SecureString GetPassword(DependencyObject dp)
        {
            return (SecureString)dp.GetValue(PasswordProperty);
        }

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(BindPasswordProperty, value);
        }

        public static void SetPassword(DependencyObject dp, SecureString value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        private static void BindPasswordProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if ((bool)e.OldValue)
            {
                passwordBox.PreviewLostKeyboardFocus -= PasswordBox_PreviewLostKeyboardFocus;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PreviewLostKeyboardFocus += PasswordBox_PreviewLostKeyboardFocus;
            }
        }

        private static void PasswordBox_PreviewLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            SetPassword(passwordBox, passwordBox.SecurePassword);
        }

        private static void PasswordProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var newPassword = (SecureString)e.NewValue;

            if (!GetBindPassword(passwordBox))
            {
                SetBindPassword(passwordBox, true);
            }

            if (passwordBox.Password == newPassword?.ConvertToUnsecureString())
            {
                return;
            }

            passwordBox.PreviewLostKeyboardFocus -= PasswordBox_PreviewLostKeyboardFocus;
            passwordBox.Password = newPassword?.ConvertToUnsecureString();
            passwordBox.PreviewLostKeyboardFocus += PasswordBox_PreviewLostKeyboardFocus;
        }
    }
}
