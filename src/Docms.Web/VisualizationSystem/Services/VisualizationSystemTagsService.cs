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

        public override async Task<Tag> FindOrCreateAsync(string tagname)
        {
            var tag = await _docms.Tags.FirstOrDefaultAsync(e => e.Name == tagname);
            if (tag == null)
            {
                tag = new Tag() { Name = tagname };
                var user = await _vs.Users.FirstOrDefaultAsync(e => e.Name == tagname);
                if (user != null)
                {
                    tag["vs_user_id"] = user.Id;
                    tag["category"] = "担当者";
                }
                var customer = await _vs.Customers.FirstOrDefaultAsync(e => e.Name == tagname);
                if (customer != null)
                {
                    tag["vs_customer_id"] = customer.Id;
                    tag["category"] = "顧客";
                }
                var project = await _vs.SalesOrders.FirstOrDefaultAsync(e => e.Name == tagname);
                if (project != null)
                {
                    tag["vs_customer_id"] = project.CustomerId;
                    tag["vs_sales_order_no"] = project.No;
                    tag["category"] = "案件";
                }
            }
            return tag;
        }
    }
}