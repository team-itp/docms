using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Docms.Infrastructure.Queries
{
    public class DocumentHistoriesQueries : IDocumentHistoriesQueries
    {
        private DocmsContext ctx;

        public DocumentHistoriesQueries(DocmsContext ctx)
        {
            this.ctx = ctx;
        }

        public IQueryable<DocumentHistory> GetHistories(string path, DateTime? since = default(DateTime?))
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
            return query.OrderBy(e => e.Timestamp);
        }
    }
}
