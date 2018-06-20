using System.Collections.Generic;
using Docms.Web.Data;

namespace Docms.Web.Models
{
    public class SearchResultViewModel
    {
        public string SearchKeyword { get; set; }
        public IEnumerable<Tag> SearchTags { get; set; }
        public IEnumerable<SearchResultItemViewModel> Results { get; set; }
    }
}
