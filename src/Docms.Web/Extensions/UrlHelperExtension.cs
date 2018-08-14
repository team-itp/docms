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

            var escapedPath = string.Join('/', path.Split('/').Select(HttpUtility.UrlEncode));
            return Url.Content("~/files/view/" + escapedPath);
        }
    }
}