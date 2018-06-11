using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mime;

namespace Docms.Web.Controllers
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(IEnumerable<DocumentResponse>))]
        public IEnumerable<DocumentResponse> Get()
        {
            return new[] {
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
            };
        }

        /// <summary>
        /// ドキュメントの情報を取得する
        /// </summary>
        /// <param name="id">ドキュメントID</param>
        /// <returns>ドキュメント情報</returns>
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(DocumentResponse))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult Get(int id)
        {
            return Ok(new DocumentResponse()
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
            });
        }

        /// <summary>
        /// 新規にドキュメントをアップロードする
        /// </summary>
        /// <param name="document">ドキュメント情報</param>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult Post([FromBody] UploadDocumentRequest document)
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
        public IActionResult Put(int id, [FromBody] UploadDocumentRequest document)
        {
            return RedirectToAction(nameof(Get), new { id });
        }

        /// <summary>
        /// 文書情報を削除する
        /// </summary>
        /// <param name="id">ドキュメント ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public IActionResult Delete(int id)
        {
            return Ok();
        }
    }
    /// <summary>
    /// ドキュメント
    /// </summary>
    public class DocumentResponse
    {
        /// <summary>
        /// ドキュメントID
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }

        /// <summary>
        /// MIME タイプ
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        [JsonProperty("size")]
        public long Size { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        [JsonProperty("tags")]
        public TagResponse[] Tags { get; set; }

        /// <summary>
        /// リンク
        /// </summary>
        [JsonProperty("_links")]
        public DocumentLinks Links { get; set; }
    }

    /// <summary>
    /// アップロード文書
    /// </summary>
    public class UploadDocumentRequest
    {
        /// <summary>
        /// MIME タイプ
        /// </summary>
        [JsonProperty("mediaType")]
        public string MediaType { get; set; }

        /// <summary>
        /// コンテンツの Encoding
        /// </summary>
        [JsonProperty("encoding")]
        public string Encoding { get; set; }

        /// <summary>
        /// コンテンツ
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; set; }

        /// <summary>
        /// ファイル名
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// ファイルパス
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        /// <summary>
        /// タグリスト
        /// </summary>
        [JsonProperty("tags")]
        public TagResponse[] Tags { get; set; }
    }

    /// <summary>
    /// ドキュメントのリンク
    /// </summary>
    public class DocumentLinks : Links
    {
        /// <summary>
        /// ファイルのダウンロード先
        /// </summary>
        public Link File { get; set; }
    }
}
