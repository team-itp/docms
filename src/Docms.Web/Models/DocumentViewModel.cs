using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Models
{

    public class DocumentsByTagViewModel
    {
        public TagViewModel Tag { get; set; }
        public IEnumerable<DocumentViewModel> Documents { get; set; }

        public static DocumentsByTagViewModel Create(IUrlHelper url, Tag tag, List<Document> documents, List<UserFavoriteTag> favTags)
        {
            return new DocumentsByTagViewModel()
            {
                Tag = TagViewModel.Create(url, tag, favTags),
                Documents = documents.Select(e => DocumentViewModel.Create(url, e))
            };
        }
    }

    public class TagViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsFaved { get; set; }
        public int? FavId { get; set; }
        public TagLinks Links { get; set; }

        public static TagViewModel Create(IUrlHelper url, Tag data, List<UserFavoriteTag> favTags)
        {
            var fav = favTags.FirstOrDefault(f => f.DataId == data.Id);
            var vm = new TagViewModel()
            {
                Id = data.Id,
                Name = data.Name,
                IsFaved = fav != null,
                FavId = fav?.Id,
                Links = TagLinks.Create(url, data),
            };
            return vm;
        }
    }

    public class TagLinks
    {
        public string Self { get; set; }
        public string List { get; set; }
        public string Fav { get; set; }
        public static TagLinks Create(IUrlHelper url, Tag data)
        {
            var vm = new TagLinks()
            {
                Self = url.Action("Details", "Tags", new { id = data.Id }),
                List = url.Action("Index", "Search", new { t = new[] { data.Id } }),
                Fav = url.Action("AddFavorites", "Profile", new { type = Constants.FAV_TYPE_TAG, dataId = data.Id }),
            };
            return vm;
        }
    }

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
        public string EditFileName { get; set; }
        public string AddTag { get; set; }
        public string Delete { get; set; }

        public static DocumentLinks Create(IUrlHelper url, Document data)
        {
            var vm = new DocumentLinks()
            {
                Self = url.Action("Details", "Documents", new { id = data.Id }),
                EditFileName = url.Action("EditFileName", "Documents", new { id = data.Id }),
                AddTag = url.Action("AddTags", "Documents", new { id = data.Id }),
                Delete = url.Action("Delete", "Documents", new { id = data.Id }),
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
        public string Self { get; set; }
        public string List { get; set; }
        public string Delete { get; set; }

        public static DocumentTagLinks Create(IUrlHelper url, DocumentTag data)
        {
            var vm = new DocumentTagLinks()
            {
                Self = url.Action("Details", "Tags", new { id = data.TagId }),
                List = url.Action("Index", "Search", new { t = new[] { data.TagId } }),
                Delete = url.Action("DeleteTag", "Documents", new { id = data.DocumentId, tagId = data.TagId }),
            };
            return vm;
        }
    }
}