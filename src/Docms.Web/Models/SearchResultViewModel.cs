using System.Collections.Generic;
using Docms.Web.Data;

namespace Docms.Web.Models
{
    public class SearchResultViewModel
    {
        public IEnumerable<SearchResultItemViewModel> Results { get; set; }
    }
}
