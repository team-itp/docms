using Docms.Web.Data;
using Docms.Web.Services;
using Docms.Web.VisualizationSystem.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.VisualizationSystem.Services
{
    public class VisualizationSystemTagsService : TagsService
    {
        private DocmsDbContext _docms;
        private VisualizationSystemDBContext _vs;

        public VisualizationSystemTagsService(
            DocmsDbContext docms,
            VisualizationSystemDBContext vs) : base(docms)
        {
            _docms = docms;
            _vs = vs;
        }

        public override async Task<Tag> FindOrCreateAsync(string tagname, string category)
        {
            var tag = await _docms.Tags.FirstOrDefaultAsync(e => e.Name == tagname);
            if (tag == null)
            {
                tag = new Tag() { Name = tagname };
                var user = await _vs.Users.FirstOrDefaultAsync(e => e.Name == tagname);
                if (user != null)
                {
                    tag[VSConstants.TAG_KEY_USER_ID] = user.Id;
                    tag[Constants.TAG_KEY_CATEGORY] = Constants.TAG_CATEGORY_PERSON_IN_CHARGE;
                }
                var customer = await _vs.Customers.FirstOrDefaultAsync(e => e.Name == tagname);
                if (customer != null)
                {
                    tag[VSConstants.TAG_KEY_CUSTOMER_ID] = customer.Id;
                    tag[Constants.TAG_KEY_CATEGORY] = Constants.TAG_CATEGORY_CUSTOMER;
                }
                var project = await _vs.SalesOrders.FirstOrDefaultAsync(e => e.Name == tagname);
                if (project != null)
                {
                    tag[VSConstants.TAG_KEY_CUSTOMER_ID] = project.CustomerId;
                    tag[VSConstants.TAG_KEY_SALES_ORDER_NO] = project.No;
                    tag[Constants.TAG_KEY_CATEGORY] = Constants.TAG_CATEGORY_PROJECT;
                }
            }
            if (!string.IsNullOrEmpty(category))
            {
                tag[Constants.TAG_KEY_CATEGORY] = category;
            }
            return tag;
        }
    }
}