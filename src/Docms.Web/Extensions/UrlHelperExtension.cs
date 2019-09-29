using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Docms.Web.Extensions
{
    public static class UrlHelperExtension
    {
        public static string ViewFile(this IUrlHelper Url, string path)
        {
            if (path == null)
                return Url.Content("~/files/view/");

            var escapedPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
            return Url.Content("~/files/view/" + escapedPath);
        }

        public static string ShowFileInContainer(this IUrlHelper Url, string path)
        {
            if (path == null)
                return Url.Content("~/files/view/");

            var escapedPathComponents = path.Split('/').Select(Uri.EscapeDataString).ToArray();
            var escapedPath = string.Join('/', escapedPathComponents.Take(escapedPathComponents.Length - 1));
            return Url.Content("~/files/view/" + escapedPath + "#" + escapedPathComponents.Last());
        }

        public static string FileHistory(this IUrlHelper Url, string path, int page = 1, int per_page = 100)
        {
            var url = default(string);
            var param = new List<string>();
            if (path == null)
            {
                url = Url.Content("~/files/histories/");
            }
            else
            {
                var escapedPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
                url = Url.Content("~/files/histories/" + escapedPath);
            }
            if (page != 1)
            {
                param.Add($"page={page}");
            }
            if (per_page != 100)
            {
                param.Add($"per_page={per_page}");
            }
            if (param.Count != 0)
            {
                url = url + "?" + string.Join("&", param);
            }
            return url;
        }

        public static string DownloadFile(this IUrlHelper Url, string path)
        {
            if (path == null)
                new ArgumentNullException();

            var escapedPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
            return Url.Content("~/files/download/" + escapedPath);
        }

        public static string UploadFile(this IUrlHelper Url, string path)
        {
            var escapedPath = string.Join('/', (path ?? "").Split('/').Select(Uri.EscapeDataString));
            return Url.Content("~/files/upload/" + escapedPath);
        }

        public static string GetDirectoriesApi(this IUrlHelper Url, string path)
        {
            var escapedPath = string.Join('/', (path ?? "").Split('/').Select(Uri.EscapeDataString));
            return Url.Content("~/api/directories/" + escapedPath);
        }
    }
}