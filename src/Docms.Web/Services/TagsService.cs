using Docms.Web.Data;
using Docms.Web.VisualizationSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class TagsService
    {
        private DocmsDbContext _docms;

        public TagsService(DocmsDbContext docms)
        {
            _docms = docms;
        }

        public virtual async Task<Tag> FindOrCreateAsync(string tagname)
        {
            var tag = await _docms.Tags.FirstOrDefaultAsync(e => e.Name == tagname);
            if (tag == null)
            {
                tag = new Tag() { Name = tagname };
            }
            return tag;
        }
    }
}