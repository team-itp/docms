using Docms.Web.Services;
using Docms.Web.VisualizationSystem.Data;
using Docms.Web.VisualizationSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.VisualizationSystem.Controllers
{
    /// <summary>
    /// VS顧客コントローラー
    /// </summary>
    [Produces("application/json")]
    [Route("api/vs/customers")]
    public class CustomersController : Controller
    {
        private VisualizationSystemDBContext _context;

        public CustomersController(VisualizationSystemDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IEnumerable<CustomerResponse> GetCustomers()
        {
            return _context.Customers
                .GroupJoin(
                    _context.SalesOrders.DefaultIfEmpty(),
                    lm => lm.Id,
                    rm => rm.CustomerId,
                    (lm, rm) => new { c = lm, so = rm.Where(m => m != null) }
                )
                .OrderBy(cso => cso.c.Kana)
                .Select(cso => new CustomerResponse()
                {
                    Id = cso.c.Id,
                    Name = cso.c.Name,
                    Projects = cso.so.Select(m => new ProjectResponse()
                    {
                        No = m.No,
                        CustomerId = m.CustomerId,
                        Name = m.Name,
                    }).ToArray()
                }).ToList();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .SingleOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(new CustomerResponse()
            {
                Id = customer.Id,
                Name = customer.Name,
                Projects = await _context.SalesOrders
                    .Where(m => m.CustomerId == id)
                    .OrderByDescending(m => m.EndDate)
                    .Select(m => new ProjectResponse
                    {
                        No = m.No,
                        CustomerId = id,
                        Name = m.Name
                    }).ToArrayAsync(),
            });
        }

        [HttpGet("{id}/projects")]
        public async Task<IActionResult> GetProjects(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .SingleOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(await _context.SalesOrders
                .Where(m => m.CustomerId == id)
                .OrderByDescending(m => m.EndDate)
                .Select(m => new ProjectResponse
                {
                    No = m.No,
                    CustomerId = id,
                    Name = m.Name
                }).ToArrayAsync());
        }

    }
}
