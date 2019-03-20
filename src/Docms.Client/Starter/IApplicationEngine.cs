using System;

namespace Docms.Client.Starter
{
    public interface IApplicationEngine
    {
        void Start(ApplicationContext context);
        void FailInitialization(Exception ex);
    }
}