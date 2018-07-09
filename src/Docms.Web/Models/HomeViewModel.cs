﻿using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Web.Models
{
    public class HomeViewModel
    {
        public IEnumerable<HomeDocumentViewModel> RecentUploadedDocuments { get; set; }
        public IEnumerable<PreferredTagViewModel> PreferredTags { get; set; }

        public static HomeViewModel Create(
            IUrlHelper url, 
            IEnumerable<Document> recentUploadedDocuments, 
            IEnumerable<PreferredTagViewModel> preferredTags)
        {
            return new HomeViewModel()
            {
                RecentUploadedDocuments = recentUploadedDocuments.Select(e => HomeDocumentViewModel.Create(url, e)),
                PreferredTags = preferredTags
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
        public string Thumbnail { get; set; }

        public static HomeDocumentLinks Create(IUrlHelper url, Document document)
        {
            var vm = new HomeDocumentLinks()
            {
                Self = url.Action("Details", "Documents", new { id = document.Id }),
                Blob = url.Action("Get", "Blobs", new { blobName = document.BlobName }),
                Thumbnail = url.Action("Thumbnail", "Blobs", new { blobName = document.BlobName }),
            };
            return vm;
        }
    }
}