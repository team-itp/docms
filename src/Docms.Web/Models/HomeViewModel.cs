using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Models
{
    public class HomeViewModel
    {
        public IEnumerable<HomeDocumentViewModel> RecentUploadedDocuments { get; set; }
        public IEnumerable<FavoriteTagViewModel> Favorites { get; set; }

        public static HomeViewModel Create(
            IUrlHelper url,
            IEnumerable<Document> recentUploadedDocuments,
            IEnumerable<FavoriteTagViewModel> favorites)
        {
            return new HomeViewModel()
            {
                RecentUploadedDocuments = recentUploadedDocuments.Select(e => HomeDocumentViewModel.Create(url, e)),
                Favorites = favorites
            };
        }
    }

    public class HomeDocumentViewModel
    {
        public int Id { get; private set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public HomeDocumentLinks Links { get; private set; }

        public static HomeDocumentViewModel Create(IUrlHelper url, Document document)
        {
            var vm = new HomeDocumentViewModel()
            {
                Id = document.Id,
                FileName = document.FileName,
                UploadedAt = document.UploadedAt,
                Links = HomeDocumentLinks.Create(url, document),
            };
            return vm;
        }
    }

    public class HomeDocumentLinks
    {
        public string Self { get; set; }
        public string Blob { get; set; }
        public string SmallThumb { get; set; }
        public string LargeThumb { get; set; }

        public static HomeDocumentLinks Create(IUrlHelper url, Document document)
        {
            var vm = new HomeDocumentLinks()
            {
                Self = url.Action("Details", "Documents", new { id = document.Id }),
                Blob = url.Action("Get", "Blobs", new { blobName = document.BlobName }),
                SmallThumb = url.Action("GetThumbnail", "Blobs", new { blobName = document.BlobName, size = "small" }),
                LargeThumb = url.Action("GetThumbnail", "Blobs", new { blobName = document.BlobName, size = "large" }),
            };
            return vm;
        }
    }
}
