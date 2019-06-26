using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;

namespace Docms.Client.Syncing
{
    public class SyncManager
    {
        private readonly Dictionary<string, SyncHistory> histories;
        private readonly IResourceOperationDispatcher<SyncHistoryDbContext> dispatcher;
        public SyncManager(IResourceOperationDispatcher<SyncHistoryDbContext> dispatcher)
        {
            this.dispatcher = dispatcher;
            this.histories = new Dictionary<string, SyncHistory>();
            Load();
        }

        private void Load()
        {
            dispatcher.Execute(async db =>
            {
                foreach (var history in await db
                    .SyncHistories
                    .AsNoTracking()
                    .ToListAsync()
                    .ConfigureAwait(false))
                {
                    if (this.histories.TryGetValue(history.Path, out var latestHistory))
                    {
                        if (latestHistory.Timestamp < history.Timestamp)
                        {
                            this.histories[history.Path] = history;
                        }
                    }
                    else
                    {
                        this.histories.Add(history.Path, history);
                    }
                }
            });
        }

        public void AddHistories(IEnumerable<SyncHistory> histories)
        {
            foreach (var history in histories)
            {
                if (this.histories.TryGetValue(history.Path, out var latestHistory))
                {
                    if (latestHistory.Timestamp < history.Timestamp)
                    {
                        this.histories[history.Path] = history;
                    }
                }
                else
                {
                    this.histories.Add(history.Path, history);
                }
            }
            dispatcher.Execute(db =>
            {
                db.SyncHistories.AddRange(histories);
                return db.SaveChangesAsync();
            });
        }

        public void AddHistory(SyncHistory history)
        {
            if (this.histories.TryGetValue(history.Path, out var latestHistory))
            {
                if (latestHistory.Timestamp < history.Timestamp)
                {
                    this.histories[history.Path] = history;
                    dispatcher.Execute(db =>
                    {
                        db.SyncHistories.Add(history);
                        return db.SaveChangesAsync();
                    });

                }
            }
            else
            {
                this.histories.Add(history.Path, history);
                dispatcher.Execute(db =>
                {
                    db.SyncHistories.Add(history);
                    return db.SaveChangesAsync();
                });
            }
        }

        public SyncHistory FindLatestHistory(PathString path)
        {
            if (this.histories.TryGetValue(path.ToString(), out var history))
            {
                return history;
            }
            return null;
        }
    }
}
