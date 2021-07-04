using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Queries.DocumentHistories
{
    public interface IDocumentHistoriesQueries
    {
        Task<DocumentHistory> FindByIdAsync(Guid id);
        IQueryable<DocumentHistory> GetHistories(string path, DateTime? since = default(DateTime?), Guid? lastHistoryId = default(Guid?));
    }
}
