using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class DocumentHistoriesQueries : IDocumentHistoriesQueries
    {
        private readonly DocmsContext ctx;

        public DocumentHistoriesQueries(DocmsContext ctx)
        {
            this.ctx = ctx;
        }

        public Task<DocumentHistory> FindByIdAsync(Guid id)
        {
            return ctx.DocumentHistories.FirstOrDefaultAsync(e => e.Id == id);
        }

        public IQueryable<DocumentHistory> GetHistories(string path, DateTime? since = default, Guid? lastHistoryId = default)
        {
            var query = ctx.DocumentHistories as IQueryable<DocumentHistory>;
            if (!string.IsNullOrEmpty(path))
            {
                query = query.Where(e => e.Path == path || EF.Functions.Like(e.Path, path + "/%"));
            }
            if (since != null)
            {
                var sinceUtc = since.Value.ToUniversalTime();
                query = query.Where(e => e.Timestamp >= sinceUtc);
            }
            if (lastHistoryId != null)
            {
                var lastHistory = ctx.DocumentHistories.Find(lastHistoryId.Value);
                if (lastHistory == null)
                {
                    throw new SpecifiedDocumentHistoryNotExistsException("指定された履歴は存在しません。");
                }
                var histories = ctx.DocumentHistories
                    .Where(e => e.Timestamp == lastHistory.Timestamp)
                    .OrderBy(h => h.Id)
                    .Select(h => h.Id)
                    .ToList()
                    .TakeWhile(h => h != lastHistoryId.Value)
                    .ToList();
                histories.Add(lastHistory.Id);
                query = query.Where(e => e.Timestamp >= lastHistory.Timestamp && !histories.Contains(e.Id));
            }

            return query;
        }
    }
}
