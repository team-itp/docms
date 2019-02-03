using Docms.Client.Uploading;
using System;
using System.Threading.Tasks;

namespace Docms.Client
{
    public interface IApplicationContext : IDisposable
    {
        ILocalFileUploader Uploader { get; }
        Task InitializeAsync();
    }
}