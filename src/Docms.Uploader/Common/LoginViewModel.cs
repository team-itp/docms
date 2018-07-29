using Docms.Client;
using Docms.Uploader.Properties;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Docms.Uploader.Common
{
    public class LoginViewModel : ValidationViewModelBase
    {
        public event EventHandler<EventArgs> LoginSucceeded;

        private string _Username;
        private SecureString _Password;
        private string _ErrorMessage;
        private IDocmsClient _client;

        [Required]
        public string Username
        {
            get { return _Username; }
            set
            {
                SetProperty(ref _Username, value);
                ErrorMessage = null;
            }
        }

        [Required]
        public SecureString Password
        {
            get { return _Password; }
            set
            {
                SetProperty(ref _Password, value);
                ErrorMessage = null;
            }
        }

        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set
            {
                SetProperty(ref _ErrorMessage, value);
            }
        }

        public RelayCommand LoginCommand { get; }

        // Design-Time only
        [Obsolete]
        public LoginViewModel() { }

        public LoginViewModel(IDocmsClient client)
        {
            _client = client;
            LoginCommand = new RelayCommand(Login, () => !_isExecutingLogin && !HasErrors);
        }

        private bool _isExecutingLogin;

        public async void Login()
        {
            _isExecutingLogin = true;

            Validate();
            if (HasErrors)
            {
                _isExecutingLogin = false;
                return;
            }

            try
            {
                await _client.LoginAsync(Username, Password.ConvertToUnsecureString());
                Settings.Default.UserId = Username;
                Settings.Default.SetPasswordHash(Password.ConvertToUnsecureString());
                Settings.Default.Save();
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Password = "".ConvertToSecureString();
                ErrorMessage = ex.Message;
                Reset();
            }
            finally
            {
                _isExecutingLogin = false;
            }
        }
    }
}
