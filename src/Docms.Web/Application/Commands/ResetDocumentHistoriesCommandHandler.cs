using Docms.Domain.Documents;
using Docms.Infrastructure;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class ResetDocumentHistoriesCommandHandler : IRequestHandler<ResetDocumentHistoriesCommand, bool>
    {
        private readonly DocmsContext _context;
        private readonly IDocumentRepository _documentRepository;
        private readonly IDataStore _dataStore;

        public ResetDocumentHistoriesCommandHandler(
            DocmsContext context,
            IDocumentRepository documentRepository,
            IDataStore dataStore)
        {
            _context = context;
            _documentRepository = documentRepository;
            _dataStore = dataStore;
        }

        public async Task<bool> Handle(ResetDocumentHistoriesCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            _context.Entries.RemoveRange(_context.Entries);
            _context.DocumentHistories.RemoveRange(_context.DocumentHistories);
            await _context.SaveChangesAsync();
            await RecreateAllFilesAsync();
            await _context.SaveEntitiesAsync();

            return true;
        }

        private async Task RecreateAllFilesAsync()
        {
            foreach (var document in (await _documentRepository.GetDocumentsAsync()).ToList())
            {
                if (document.Path == null)
                {
                    if (document.StorageKey != null)
                    {
                        await _dataStore.DeleteAsync(document.StorageKey);
                    }
                    continue;
                }

                var data = await _dataStore.FindAsync(document.StorageKey ?? document.Path);
                if (data == null)
                {
                    _context.Documents.Remove(document);
                }
                else
                {
                    document.Recreate(data);
                }
            }
        }
    }
}

