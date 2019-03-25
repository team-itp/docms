using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public interface IApplicationEngine
    {
        void Start();
    }
}
