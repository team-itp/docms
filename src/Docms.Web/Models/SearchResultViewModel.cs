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
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public IEnumerable<SearchResultItemViewModel> Results { get; set; }
        public SearchResultLinks Links { get; set; }

        public static SearchResultViewModel Create(
            IUrlHelper url,
            string keyword,
            IEnumerable<Tag> searchTags,
            int page,
            int totalPages,
            IEnumerable<Document> results)
        {
            var vm = new SearchResultViewModel()
            {
                SearchKeyword = keyword,
                SearchTags = searchTags.Select(t => SearchTagViewModel.Create(url, t, searchTags)),
                Page = page,
                TotalPages = totalPages,
                Results = results.Select(d => SearchResultItemViewModel.Create(url, d)),
                Links = SearchResultLinks.Create(url, keyword, searchTags, page, totalPages),
            };
            return vm;
        }
    }

    public class SearchResultLinks
    {
        private string _pageFormatTemplate;
        public string Self { get; set; }
        public string First { get; set; }
        public string Next { get; set; }
        public string Prev { get; set; }
        public string ForPage(int i)
        {
            return string.Format(_pageFormatTemplate, i);
        }

        public static SearchResultLinks Create(IUrlHelper url, string keyword, IEnumerable<Tag> searchTags, int page, int totalPages)
        {
            var tags = searchTags.Select(t => t.Id).ToArray();
            var baseUrl = url.Action("Index", "Search", new
            {
                q = keyword,
                t = tags
            });
            int idx = baseUrl.IndexOf('?');
            var template = baseUrl + (idx >= 0 ? "&p={0}" : "?p={0}");
            var vm = new SearchResultLinks();
            vm._pageFormatTemplate = template;
            vm.Self = page == 1 ? baseUrl : vm.ForPage(page);
            vm.First = baseUrl;
            vm.Prev = page > 2 ? vm.ForPage(page - 1) : null;
            vm.Next = page < totalPages ? vm.ForPage(page + 1) : null;
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
        public string SmallThumb { get; set; }
        public string LargeThumb { get; set; }

        public static SearchResultItemLinks Create(IUrlHelper url, Document document)
        {
            var vm = new SearchResultItemLinks()
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
