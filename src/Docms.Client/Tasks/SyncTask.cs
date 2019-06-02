﻿using Docms.Client.Operations;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    public class SyncTask : ITask
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private ApplicationContext context;
        private object prevResult;

        public bool IsCompleted { get; private set; }

        public SyncTask(ApplicationContext context)
        {
            this.context = context;
        }

        public void Next(object result)
        {
            prevResult = result;
        }

        public async Task ExecuteOperationAsync(IOperation operation)
        {
            prevResult = null;
            await context.App.Invoke(operation).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            context.CurrentTask = this;
            await ExecuteOperationAsync(new LocalDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            await ExecuteOperationAsync(new ChangesIntoOperationsOperation(context)).ConfigureAwait(false);
            if (prevResult == null || !(prevResult is ChangesIntoOperationsOperationResult operationsResult) || operationsResult.Operations.Count == 0)
            {
                IsCompleted = true;
                return;
            }
            var errorOperations = new List<IOperation>();
            foreach (var op in operationsResult.Operations)
            {
                try
                {
                    await ExecuteOperationAsync(op).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            IsCompleted = true;
        }
    }
}
