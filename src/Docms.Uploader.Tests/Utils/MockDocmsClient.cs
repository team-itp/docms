using Docms.Client;
using Docms.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Uploader.Utils
{
    public class MockDocmsClient : IDocmsClient
    {
        private bool _isLogin = false;
        private Dictionary<string, Delegate> delegates = new Dictionary<string, Delegate>();

        public void SetHandler(string methodName, Delegate delg)
        {
            delegates.Add(methodName, delg);
        }

        public Task LoginAsync(string username, string password)
        {
            if (!TryInvokeHandler(nameof(LoginAsync), username, password))
            {
                if (username != "validusername" || password != "validpassword")
                {
                    throw new InvalidLoginException();
                }
            }
            _isLogin = true;
            return Task.CompletedTask;
        }

        public Task LogoutAsync()
        {
            if (!TryInvokeHandler(nameof(LogoutAsync)))
            {
            }
            _isLogin = false;
            return Task.CompletedTask;
        }

        public Task VerifyTokenAsync()
        {
            if (!TryInvokeHandler(nameof(VerifyTokenAsync)))
            {
                if (!_isLogin)
                {
                    throw new TokenVerificationException();
                }
            }
            return Task.CompletedTask;
        }

        public Task CreateDocumentAsync(string localFilePath, string name, string personInCharge, string customer, string project, string[] tags)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<CustomerResponse>> GetCustomersAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TagResponse>> GetTagsAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<UserResponse>> GetUsersAsync()
        {
            throw new System.NotImplementedException();
        }

        private bool TryInvokeHandler(string methodName, params object[] param)
        {
            if (delegates.TryGetValue(methodName, out var @delegate))
            {
                @delegate.DynamicInvoke(param);
                return true;
            }
            return false;
        }
    }
}
