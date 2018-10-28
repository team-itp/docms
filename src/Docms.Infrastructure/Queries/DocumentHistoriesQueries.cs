﻿using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class DocumentHistoriesQueries : IDocumentHistoriesQueries
    {
        private DocmsContext ctx;

        public DocumentHistoriesQueries(DocmsContext ctx)
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
            return await query.OrderBy(e => e.Timestamp).ToListAsync();
        }
    }
}