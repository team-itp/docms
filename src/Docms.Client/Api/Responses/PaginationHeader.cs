using System.Linq;

namespace Docms.Client.Api.Responses
{
    public class PaginationHeader
    {
        public string First { get; private set; }
        public string Last { get; private set; }
        public string Next { get; private set; }
        public string Prev { get; private set; }

        public static PaginationHeader Parse(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }

            var linkHeader = new PaginationHeader();
            var links = s.Split(',').Select(v => v.Trim());
            foreach (var link in links)
            {
                var tmp = link.Split(';');
                var url = tmp[0].TrimStart('<').TrimEnd('>');
                var rel = tmp[1].Trim().Split('=')[1].Trim('"');
                switch (rel)
                {
                    case "prev":
                        linkHeader.Prev = url;
                        break;
                    case "next":
                        linkHeader.Next = url;
                        break;
                    case "first":
                        linkHeader.First = url;
                        break;
                    case "last":
                        linkHeader.Last = url;
                        break;
                }
            }
            return linkHeader;
        }
    }
}