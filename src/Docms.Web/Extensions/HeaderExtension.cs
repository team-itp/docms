using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Docms.Web.Extensions
{
    public static class IHeaderDictionaryExtension
    {
        private struct Link
        {
            internal string Rel;
            internal string Url;
            internal int Page;
            internal int PerPage;

            internal string Create()
            {
                if (Url.Contains("?"))
                {
                    return $"<{Url}&page={Page}&per_page={PerPage}>; rel=\"{Rel}\"";
                }
                else
                {
                    return $"<{Url}?page={Page}&per_page={PerPage}>; rel=\"{Rel}\"";
                }
            }
        }
        public static void AddPaginationHeader(this IHeaderDictionary header, string url, int page, int perPage, int totalCount)
        {
            List<string> links = new List<string>();
            var lastPage = (totalCount / perPage) + 1;
            if (page < lastPage)
            {
                var next = new Link() { Rel = "next", Url = url, Page = page == lastPage ? lastPage : page + 1, PerPage = perPage };
                links.Add(next.Create());
            }
            var last = new Link() { Rel = "last", Url = url, Page = lastPage, PerPage = perPage };
            links.Add(last.Create());
            var first = new Link() { Rel = "first", Url = url, Page = 1, PerPage = perPage };
            links.Add(first.Create());
            var prev = new Link() { Rel = "prev", Url = url, Page = page == 1 ? 1 : page - 1, PerPage = perPage };
            if (page != 1)
            {
                links.Add(prev.Create());
            }
            header.Add("Link", string.Join(",", links));
        }
    }
}
