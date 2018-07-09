using Docms.Client;
using Docms.Uploader.Properties;
using System;
using System.ComponentModel.DataAnnotations;

namespace Docms.Uploader.Common
{
    public class LoginViewModel : ValidationViewModelBase
    {
        public event EventHandler<EventArgs> LoginSucceeded;

        private string _Username;
        private string _Password;
        private string _ErrorMessage;
        private DocmsClient _client;

        [Required]
        public string Username
        {
            get => _Username;
            set
            {
                SetProperty(ref _Username, value);
            }
        }

        [Required]
        public string Password
        {
            get => _Password;
            set
            {
                SetProperty(ref _Password, value);
            }
        }

        public string ErrorMessage
        {
            get => _ErrorMessage;
            set
            {
                SetProperty(ref _ErrorMessage, value);
            }
        }

        public RelayCommand LoginCommand { get; }

        // Design-Time only
        [Obsolete]
        public LoginViewModel() { }

        public LoginViewModel(DocmsClient client)
        {
            _client = client;
            LoginCommand = new RelayCommand(Login, () => !_isExecutingLogin && !HasErrors);
        }

        private bool _isExecutingLogin;

        public async void Login()
        {
            _isExecutingLogin = true;
            try
            {
                await _client.LoginAsync(Username, Password);
                Settings.Default.UserId = Username;
                Settings.Default.SetPasswordHash(Password);
                Settings.Default.Save();
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Password = "";
            }
            finally
            {
                _isExecutingLogin = false;
            }
        }
    }
}
