﻿using Docms.Domain.Documents;
using Docms.Domain.Documents.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.Blobs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.DomainEventHandlers
{
    public class BlobsQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentDeletedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentMovedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentUpdatedEvent>>
    {
        private readonly DocmsContext _db;

        public BlobsQueriesEventHandler(DocmsContext db)
        {
            _db = db;
        }

        private async Task AddParentContainerAsync(DocumentPath parent)
        {
            while (parent != null)
            {
                if (!await _db.BlobContainers.AnyAsync(c => c.Path == parent.Value))
                {
                    _db.BlobContainers.Add(new BlobContainer()
                    {
                        Path = parent.Value,
                        Name = parent.Name,
                        ParentPath = parent.Parent?.Value
                    });
                }
                parent = parent.Parent;
            }
        }

        private async Task RemoveEmptyContainerAsync(DocumentPath parent)
        {
            while (parent != null)
            {
                if (!await _db.Blobs.AnyAsync(e => e.ParentPath == parent.Value)
                    && !await _db.BlobContainers.AnyAsync(e => e.ParentPath == parent.Value))
                {
                    var container = await _db.BlobContainers.FirstOrDefaultAsync(e => e.Path == parent.Value);
                    if (container != null)
                    {
                        _db.BlobContainers.Remove(container);
                        await _db.SaveChangesAsync();
                    }
                }
                parent = parent.Parent;
            }
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;

            if (_db.Blobs.Any(e => e.Path == ev.Path.Value))
            {
                throw new InvalidOperationException();
            }

            var blob = new Blob()
            {
                Path = ev.Path.Value,
                Name = ev.Path.Name,
                ParentPath = ev.Path.Parent?.Value,
                DocumentId = ev.Entity.Id,
                StorageKey = ev.StorageKey,
                ContentType = ev.ContentType,
                FileSize = ev.Data.Length,
                Hash = ev.Data.Hash,
                Created = ev.Created,
                LastModified = ev.LastModified,
            };
            _db.Blobs.Add(blob);
            await AddParentContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentDeletedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;

            var blob = await _db.Blobs.FirstOrDefaultAsync(e => e.Path == ev.Path.Value);
            if (blob == null)
            {
                throw new InvalidOperationException();
            }

            _db.Blobs.Remove(blob);
            await _db.SaveChangesAsync();
            await RemoveEmptyContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentMovedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;

            var oldBlob = await _db.Blobs.FirstOrDefaultAsync(e => e.Path == ev.Path.Value);
            _db.Blobs.Remove(oldBlob);
            await _db.SaveChangesAsync();
            await RemoveEmptyContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();

            var blob = new Blob()
            {
                Path = ev.NewPath.Value,
                Name = ev.NewPath.Name,
                ParentPath = ev.NewPath.Parent?.Value,
                DocumentId = ev.Entity.Id,
                StorageKey = oldBlob.StorageKey,
                ContentType = oldBlob.ContentType,
                FileSize = oldBlob.FileSize,
                Hash = oldBlob.Hash,
                Created = oldBlob.Created,
                LastModified = oldBlob.LastModified,
            };

            _db.Blobs.Add(blob);

            await AddParentContainerAsync(ev.NewPath.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentUpdatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;

            var blob = await _db.Blobs.FirstOrDefaultAsync(e => e.Path == ev.Path.Value);
            blob.DocumentId = ev.Entity.Id;
            blob.StorageKey = ev.Data.StorageKey;
            blob.ContentType = ev.ContentType;
            blob.FileSize = ev.Data.Length;
            blob.Hash = ev.Data.Hash;
            blob.Created = ev.Created;
            blob.LastModified = ev.LastModified;

            _db.Update(blob);
            await _db.SaveChangesAsync();
        }
    }
}
