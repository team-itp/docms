using System;
using System.Collections.Generic;
using System.Text;

namespace Docms.Client
{
    public class ApplicationInitializer
    {
        private string watchPath;
        private string serverUrl;
        private string uploadClientId;
        private string uploadUserName;
        private string uploadUserPassword;

        public ApplicationInitializer()
        {

        }

        public ApplicationInitializer(string watchPath, string serverUrl, string uploadClientId, string uploadUserName, string uploadUserPassword)
        {
            this.watchPath = watchPath;
            this.serverUrl = serverUrl;
            this.uploadClientId = uploadClientId;
            this.uploadUserName = uploadUserName;
            this.uploadUserPassword = uploadUserPassword;
        }

        public void Initialize(Application application)
        {
        }
    }
}
