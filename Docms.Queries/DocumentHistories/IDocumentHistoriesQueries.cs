using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Queries.DocumentHistories
{
    public interface IDocumentHistoriesQueries
    {
        Task<IEnumerable<DocumentHistory>> GetHistoriesAsync(string path, DateTime? since = default(DateTime?));
    }
}
