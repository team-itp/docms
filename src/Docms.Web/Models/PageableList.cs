using System.Collections.Generic;

namespace Docms.Web.Models
{
    public class PageableList<T>
    {
        public PageableList(List<T> list, int page, int perPage, int totalCount)
        {
            List = list;
            CurrentPage = page;
            LastPage = totalCount % perPage == 0 ? (totalCount / perPage) : (totalCount / perPage) + 1;
            PerPage = perPage;
        }

        public List<T> List { get; }
        public int CurrentPage { get; }
        public int PerPage { get; }
        public int LastPage { get; }
        public bool HasNext => CurrentPage != LastPage;
        public bool HasPrev => CurrentPage != 1;
    }
}
