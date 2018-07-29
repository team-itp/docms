using Docms.Client;
using Docms.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Uploader.Common
{
    public class MockDocmsClient : IDocmsClient
    {
        public Task LoginAsync(string username, string password)
        {
            if (username != "validusername" || password != "validpassword")
            {
                throw new Exception();
            }
            return Task.CompletedTask;
        }

        public Task LogoutAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task VerifyTokenAsync()
        {
            throw new System.NotImplementedException();
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
    }
}
