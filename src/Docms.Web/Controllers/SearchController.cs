using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace Docms.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SearchController : Controller
    {
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(SearchResponse))]
        public SearchResponse Get(int[] tagIds)
        {
            return new SearchResponse()
            {
                SearchTags = new TagResponse[0],
                Documents = new List<DocumentResponse>()
                {
                    new DocumentResponse()
                    {
                        Id = 1,
                        MediaType = MediaTypeNames.Image.Jpeg,
                        Size = 4520,
                        Name = "何とか現場の写真.jpg",
                        Path = "何とか現場の写真.jpg",
                        Tags = new TagResponse[0],
                        Links = new DocumentLinks()
                        {
                            Self = new Link()
                            {
                                Href = "http://www.google.com/"
                            },
                            File = new Link()
                            {
                                Href = "http://www.google.com/"
                            }
                        }
                    }
                }
            };
        }
    }

    public class SearchResponse
    {
        public TagResponse[] SearchTags { get; internal set; }
        public List<DocumentResponse> Documents { get; internal set; }
    }
}