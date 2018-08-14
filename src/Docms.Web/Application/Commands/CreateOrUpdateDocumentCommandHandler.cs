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
            var file = (await _fileStorage.GetEntryAsync(request.Path)) as Infrastructure.Files.File;
            if (file == null)
            {
                var dir = await _fileStorage.GetDirectoryAsync(path.DirectoryPath);
                var utcNow = DateTime.UtcNow;
                var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, request.Stream, request.Created ?? utcNow, request.LastModified ?? utcNow);
                var document = new Document(new DocumentPath(request.Path.ToString()), fileProps.ContentType, fileProps.Size, fileProps.Hash, fileProps.Created, fileProps.LastModified);
                await _documentRepository.AddAsync(document);
            }
            else
            {
                var props = await file.GetPropertiesAsync();
                using (var ms = new MemoryStream())
                {
                    await request.Stream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var hash = Hash.CalculateHashString(ms);
                    if (hash == Hash.ConvertHashString(props.Hash))
                    {
                        return true;
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    var dir = file.Parent;
                    var utcNow = DateTime.UtcNow;
                    var fileProps = await _fileStorage.SaveAsync(dir, path.FileName, ms, request.Created ?? props.Created, request.LastModified ?? utcNow);
                    var document = await _documentRepository.GetAsync(request.Path.ToString());
                    document.Update(fileProps.ContentType, fileProps.Size, fileProps.Hash, fileProps.Created, fileProps.LastModified);
                }
            }
            await _documentRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
