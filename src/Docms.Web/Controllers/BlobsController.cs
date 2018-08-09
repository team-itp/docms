using Docms.Infrastructure.Files;
using Microsoft.AspNetCore.Mvc;

namespace Docms.Web.Controllers
{
    [Route("blobs")]
    public class BlobsController : Controller
    {
        public IFileStorage _storage;

        public BlobsController(IFileStorage storage)
        {
            _storage = storage;
        }


    }
}
