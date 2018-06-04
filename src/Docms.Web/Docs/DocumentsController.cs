using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace Docms.Web.Docs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DocumentsController : ControllerBase
    {
        /// <summary>
        /// ドキュメント情報の一覧を返す
        /// </summary>
        /// <returns>ドキュメント情報の一覧</returns>
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<Document>))]
        public IEnumerable<Document> Get()
        {
            return new Document[] {
                new Document()
                {
                    Id = 1,
                    MediaType = MediaTypeNames.Image.Jpeg,
                    Size = 4520,
                    Name = "何とか現場の写真.jpg",
                    Path = "何とか現場の写真.jpg",
                    Tags = new Tag[0],
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
            };
        }

        /// <summary>
        /// ドキュメントの情報を取得する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns>ドキュメント情報</returns>
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(Document))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public Document Get(int id)
        {
            return new Document()
            {
                Id = 1,
                MediaType = MediaTypeNames.Image.Jpeg,
                Size = 4520,
                Name = "何とか現場の写真.jpg",
                Path = "何とか現場の写真.jpg",
                Tags = new Tag[0],
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
            };
        }

        /// <summary>
        /// 新規にドキュメントをアップロードする
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Post([FromBody] UploadDocument document)
        {
            var id = new Random().Next();
            return RedirectToAction(nameof(Get), new { id });
        }

        /// <summary>
        /// ドキュメントを更新する
        /// </summary>
        /// <param name="id">ドキュメント ID</param>
        /// <param name="document">ドキュメント情報</param>
        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Put(int id, [FromBody] UploadDocument document)
        {
            return RedirectToAction(nameof(Get), new { id });
        }

        /// <summary>
        /// 文書情報を削除する
        /// </summary>
        /// <param name="id">ドキュメント ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}
