using System;
using System.Collections.Generic;
using System.Linq;
using Docms.Web.Data;
using Microsoft.AspNetCore.Mvc;

namespace Docms.Web.Models
{
    public class SearchResultViewModel
    {
        public string SearchKeyword { get; set; }
        public IEnumerable<SearchTagViewModel> SearchTags { get; set; }
        public IEnumerable<SearchResultItemViewModel> Results { get; set; }

        public static SearchResultViewModel Create(IUrlHelper url, string keyword, IEnumerable<Tag> searchTags, IEnumerable<Document> results)
        {
            var vm = new SearchResultViewModel()
            {
                SearchKeyword = keyword,
                SearchTags = searchTags.Select(t => SearchTagViewModel.Create(url, t, searchTags)),
                Results = results.Select(d => SearchResultItemViewModel.Create(url, d)),
            };
            return vm;
        }
    }

    public class SearchTagViewModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public SearchTagLinks Links { get; private set; }

        public static SearchTagViewModel Create(IUrlHelper url, Tag tag, IEnumerable<Tag> searchTags)
        {
            var vm = new SearchTagViewModel()
            {
                Id = tag.Id,
                Name = tag.Name,
                Links = SearchTagLinks.Create(url, tag, searchTags),
            };
            return vm;
        }
    }

    public class SearchTagLinks
    {
        public string Delete { get; set; }

        public static SearchTagLinks Create(IUrlHelper url, Tag tag, IEnumerable<Tag> searchTags)
        {
            var searchTagsExcludeSelf = searchTags.Except(new[] { tag });
            var vm = new SearchTagLinks()
            {
                Delete = url.Action("Index", "Search", new { t = searchTagsExcludeSelf.Select(t => t.Id) }),
            };
            return vm;
        }
    }

    public class SearchResultItemViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public SearchResultItemLinks Links { get; set; }

        public static SearchResultItemViewModel Create(IUrlHelper url, Document document)
        {
            var vm = new SearchResultItemViewModel()
            {
                Id = document.Id,
                FileName = document.FileName,
                UploadedAt = document.UploadedAt,
                Links = SearchResultItemLinks.Create(url, document),
            };
            return vm;
        }
    }

    public class SearchResultItemLinks
    {
        public string Self { get; set; }
        public string Blob { get; set; }
        public string Thumbnail { get; set; }

        public static SearchResultItemLinks Create(IUrlHelper url, Document document)
        {
            var vm = new SearchResultItemLinks()
            {
                Self = url.Action("Details", "Documents", new { id = document.Id }),
                Blob = url.Action("Thumubnail", "Blobs", new { blobName = document.BlobName }),
                Thumbnail = url.Action("Thumubnail", "Blobs", new { blobName = document.BlobName }),
            };
            return vm;
        }
    }
}
