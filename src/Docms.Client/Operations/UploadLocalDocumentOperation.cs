﻿using Docms.Client.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class UploadLocalDocumentOperation : AsyncOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}