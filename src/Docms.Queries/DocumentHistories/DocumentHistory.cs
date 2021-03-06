﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.DocumentHistories
{
    public enum DocumentHistoryDiscriminator
    {
        DocumentCreated,
        DocumentDeleted,
        DocumentUpdated
    }

    public class DocumentHistory
    {
        [Column("Id")]
        [Key]
        public Guid Id { get; set; }
        [Column("Timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }
        [Column("Discriminator")]
        public DocumentHistoryDiscriminator Discriminator { get; set; }
        [Column("DocumentId")]
        public int DocumentId { get; set; }
        [Column("Path")]
        [Required]
        [MaxLength(800)]
        public string Path { get; set; }
        [Column("StorageKey")]
        [MaxLength(800)]
        public string StorageKey { get; set; }
        [Column("ContentType")]
        public string ContentType { get; set; }
        [Column("FileSize")]
        public long? FileSize { get; set; }
        [Column("Hash")]
        public string Hash { get; set; }
        [Column("Created")]
        public DateTime? Created { get; set; }
        [Column("LastModified")]
        public DateTime? LastModified { get; set; }

        public static DocumentHistory DocumentCreated(
            DateTime timestamp,
            int documentId,
            string path, 
            string storageKey, 
            string contentType,
            long fileSize,
            string hash,
            DateTime created,
            DateTime lastModified)
        {
            return new DocumentHistory()
            {
                Id = Guid.NewGuid(),
                Discriminator = DocumentHistoryDiscriminator.DocumentCreated,
                Timestamp = timestamp,
                DocumentId = documentId,
                Path = path,
                StorageKey = storageKey,
                ContentType = contentType,
                FileSize = fileSize,
                Hash = hash,
                Created = created,
                LastModified = lastModified
            };
        }

        public static DocumentHistory DocumentUpdated(
            DateTime timestamp,
            int documentId,
            string path,
            string storageKey,
            string contentType,
            long fileSize,
            string hash,
            DateTime created,
            DateTime lastModified)
        {
            return new DocumentHistory()
            {
                Id = Guid.NewGuid(),
                Discriminator = DocumentHistoryDiscriminator.DocumentUpdated,
                Timestamp = timestamp,
                DocumentId = documentId,
                Path = path,
                StorageKey = storageKey,
                ContentType = contentType,
                FileSize = fileSize,
                Hash = hash,
                Created = created,
                LastModified = lastModified
            };
        }
        public static DocumentHistory DocumentDeleted(
            DateTime timestamp,
            int documentId,
            string path)
        {
            return new DocumentHistory()
            {
                Id = Guid.NewGuid(),
                Discriminator = DocumentHistoryDiscriminator.DocumentDeleted,
                Timestamp = timestamp,
                DocumentId = documentId,
                Path = path
            };
        }
    }
}
