using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Models
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<DocumentTagViewModel> Tags { get; set; }
        public DocumentLinks Links { get; set; }

        public static DocumentViewModel Create(IUrlHelper url, Document data)
        {
            var vm = new DocumentViewModel()
            {
                Id = data.Id,
                FileName = data.FileName,
                UploadedAt = data.UploadedAt,
                Links = DocumentLinks.Create(url, data),
                Tags = data.Tags
                    .Select(t => DocumentTagViewModel.Create(url, t))
                    .ToList(),
            };
            return vm;
        }
    }

    public class DocumentLinks
    {
        public string Self { get; set; }
        public string Blob { get; set; }
        public string Thumbnail { get; set; }
        public string Edit { get; set; }
        public string Delete { get; set; }

        public static DocumentLinks Create(IUrlHelper url, Document data)
        {
            var vm = new DocumentLinks()
            {
                Self = url.Action("Details", "Documents", new { id = data.Id }),
                Edit = url.Action("Details", "Edit", new { id = data.Id }),
                Delete = url.Action("Details", "Delete", new { id = data.Id }),
                Blob = url.Action("Get", "Blobs", new { blobName = data.BlobName }),
                Thumbnail = url.Action("Thumbnail", "Blobs", new { blobName = data.BlobName }),
        };
            return vm;
        }
}

public class DocumentTagViewModel
{
    public int Id { get; set; }
    public int DoucmentId { get; set; }
    public string Name { get; set; }
    public DocumentTagLinks Links { get; set; }

    public static DocumentTagViewModel Create(IUrlHelper url, DocumentTag data)
    {
        var vm = new DocumentTagViewModel()
        {
            Id = data.TagId,
            DoucmentId = data.DocumentId,
            Name = data.Tag.Name,
            Links = DocumentTagLinks.Create(url, data),
        };
        return vm;
    }
}

public class DocumentTagLinks
{
    public string Delete { get; set; }

    public static DocumentTagLinks Create(IUrlHelper url, DocumentTag data)
    {
        var vm = new DocumentTagLinks()
        {
            Delete = url.Action("DeleteTag", "Documents", new { id = data.DocumentId, tagId = data.TagId })
        };
        return vm;
    }
}
}