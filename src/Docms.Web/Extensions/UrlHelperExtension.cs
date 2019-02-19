using System;
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

        public static string FileHistory(this IUrlHelper Url, string path)
        {
            if (path == null)
                return Url.Content("~/files/histories/");

            var escapedPath = string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
            return Url.Content("~/files/histories/" + escapedPath);
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