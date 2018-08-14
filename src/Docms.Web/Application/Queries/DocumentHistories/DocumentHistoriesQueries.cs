using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Application.Queries.DocumentHistories
{
    public class DocumentHistoriesQueries
    {
        private DocmsQueriesContext ctx;

        public DocumentHistoriesQueries(DocmsQueriesContext ctx)
        {
            this.ctx = ctx;
        }

        public async Task<IEnumerable<DocumentHistory>> GetHistoriesAsync(string path, DateTime? since = default(DateTime?))
        {
            var query = ctx.DocumentHistories as IQueryable<DocumentHistory>;
            if (!string.IsNullOrEmpty(path))
            {
                query = query.Where(e => e.Path.StartsWith(path + "/") || e.Path == path);
            }
            if (since != null)
            {
                query = query.Where(e => e.Timestamp > since);
            }
            return await query.ToListAsync();
        }
    }
}
