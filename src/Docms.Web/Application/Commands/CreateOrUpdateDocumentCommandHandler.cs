using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class CreateOrUpdateDocumentCommandHandler : IRequestHandler<CreateOrUpdateDocumentCommand, bool>
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorage _fileStorage;

        public CreateOrUpdateDocumentCommandHandler(
            IDocumentRepository documentRepository,
            IFileStorage fileStorage)
        {
            _documentRepository = documentRepository;
            _fileStorage = fileStorage;
        }

        public async Task<bool> Handle(CreateOrUpdateDocumentCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var path = request.Path;
            var entry = await _fileStorage.GetEntryAsync(request.Path);
            if (entry is Docms.Infrastructure.Files.Directory)
            {
                throw new InvalidOperationException();
            }

            var file = entry as Docms.Infrastructure.Files.File;
            if (file == null)
            {
                var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
                var utcNow = DateTime.UtcNow;
                var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, request.Stream);
                var document = new Document(new DocumentPath(request.Path.ToString()), fileProps.ContentType, fileProps.Size, fileProps.Hash, request.Created ?? utcNow, request.LastModified ?? request.Created ?? utcNow);
                await _documentRepository.AddAsync(document);
            }
            else
            {
                var props = await file.GetPropertiesAsync();
                using (var ms = new MemoryStream())
                {
                    await request.Stream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var hash = Hash.CalculateHash(ms);
                    if (hash == props.Hash)
                    {
                        return true;
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    var dir = file.Parent;
                    var utcNow = DateTime.UtcNow;
                    var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, ms);
                    var document = await _documentRepository.GetAsync(request.Path.ToString());
                    document.Update(fileProps.ContentType, fileProps.Size, fileProps.Hash, request.Created ?? document.Created, request.LastModified ?? utcNow);
                }
            }
            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
