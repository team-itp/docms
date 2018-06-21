using System.Collections.Generic;
using System.Linq;
using Docms.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Docms.Web.Models
{
    public class EditFileNameViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string EditedFileName { get; set; }
    }

    public class AddTagsViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string[] Tags { get; set; }
    }

    public class DeleteTagViewModel
    {
        public int Id { get; set; }
        public int TagId { get; set; }
        public string Name { get; set; }
    }
}