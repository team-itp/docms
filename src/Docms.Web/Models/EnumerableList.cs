using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Models
{
    public class EnumerableList<T>
    {
        public EnumerableList(List<T> list, int page, int perPage)
        {
            List = list;
            CurrentPage = page;
            PerPage = perPage;
        }

        public List<T> List { get; }
        public int CurrentPage { get; }
        public int PerPage { get; }
        public bool HasNext => List.Count >= PerPage;
        public bool HasPrev => CurrentPage != 1;
    }
}
