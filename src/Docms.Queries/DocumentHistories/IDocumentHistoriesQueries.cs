using System;
using System.Linq;

namespace Docms.Queries.DocumentHistories
{
    public interface IDocumentHistoriesQueries
    {
        IQueryable<DocumentHistory> GetHistories(string path, DateTime? since = default(DateTime?), Guid? lastHistoryId = default(Guid?));
    }
}
