using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Queries.Blobs;
using Docms.Application.Commands;
using Docms.Web.Filters;
using Docms.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Docms.Web.Api.V1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/v1/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IBlobsQueries _queries;
        private readonly IDataStore _storage;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public FilesController(IDataStore storage, IBlobsQueries queries)
        {
            _storage = storage;
            _queries = queries;
        }


        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string path = "",
            [FromQuery] bool download = false)
        {
            var entry = await _queries.GetEntryAsync(path ?? "").ConfigureAwait(false);
            if (entry == null)
            {
                return NotFound();
            }
            if (download && entry is Blob blob)
            {
                if (Request.Headers.Keys.Contains("If-None-Match") && Request.Headers["If-None-Match"].ToString() == "\"" + blob.Hash + "\"")
                {
                    return new StatusCodeResult(304);
                }

                var data = await _storage.FindAsync(blob.StorageKey ?? blob.Path).ConfigureAwait(false);
                return File(await data.OpenStreamAsync().ConfigureAwait(false), blob.ContentType, blob.Name, new DateTimeOffset(blob.LastModified), new EntityTagHeaderValue("\"" + blob.Hash + "\""));
            }
            else
            {
                return Ok(entry);
            }
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [RequestFormLimits(MultipartBodyLengthLimit = 4_294_967_294)] // 4GB
        public async Task<IActionResult> Post([FromServices]  IMediator mediator)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }

            // Used to accumulate all the form url encoded key value pairs in the 
            // request.
            var formAccumulator = new KeyValueAccumulator();
            IData data = null;

            var boundary = MultipartRequestHelper.GetBoundary(
                MediaTypeHeaderValue.Parse(Request.ContentType),
                int.MaxValue);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync().ConfigureAwait(false);
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        data = await _storage.CreateAsync(_storage.CreateKey(), section.Body);
                    }
                    else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                    {
                        // Content-Disposition: form-data; name="key"
                        //
                        // value

                        // Do not limit the key name length here because the 
                        // multipart headers length limit is already in effect.
                        var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                        var encoding = GetEncoding(section);
                        using (var streamReader = new StreamReader(
                            section.Body,
                            encoding,
                            detectEncodingFromByteOrderMarks: true,
                            bufferSize: 1024,
                            leaveOpen: true))
                        {
                            // The value length limit is enforced by MultipartBodyLengthLimit
                            var value = await streamReader.ReadToEndAsync();
                            if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                            {
                                value = string.Empty;
                            }
                            formAccumulator.Append(key.Value, value);

                            if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                            {
                                throw new InvalidDataException($"Form key count limit {_defaultFormOptions.ValueCountLimit} exceeded.");
                            }
                        }
                    }
                }

                // Drains any remaining section body that has not been consumed and
                // reads the headers for the next section.
                section = await reader.ReadNextSectionAsync().ConfigureAwait(false);
            }

            // Bind form data to a model
            var formValueProvider = new FormValueProvider(
                BindingSource.Form,
                new FormCollection(formAccumulator.GetResults()),
                CultureInfo.CurrentCulture);

            var request = new UploadRequest
            {
                File = data
            };
            var bindingSuccessful = await TryUpdateModelAsync(request, prefix: "", valueProvider: formValueProvider).ConfigureAwait(false);
            if (!bindingSuccessful)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }
            return await PostInternal(request, mediator).ConfigureAwait(false);
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);
            // UTF-7 is insecure and should not be honored. UTF-8 will succeed in 
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        private async Task<IActionResult> PostInternal(
            UploadRequest request,
            IMediator mediator)
        {
            if (!FilePath.TryParse(request.Path, out var filepath))
            {
                return BadRequest("invalid path name.");
            }

            var command = new CreateOrUpdateDocumentCommand
            {
                Path = filepath,
                Data = request.File,
                Created = request.Created?.ToUniversalTime(),
                LastModified = request.LastModified?.ToUniversalTime()
            };
            await mediator.Send(command).ConfigureAwait(false);
            return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.Path.ToString()) });
        }

        [HttpPost("move")]
        public async Task<IActionResult> Move(
            [FromForm] MoveRequest request,
            [FromServices] IMediator mediator)
        {
            if (!FilePath.TryParse(request.OriginalPath, out var originalFilePath)
                || !FilePath.TryParse(request.DestinationPath, out var destinationFilePath))
            {
                return BadRequest("invalid path name.");
            }

            var command = new MoveDocumentCommand
            {
                OriginalPath = originalFilePath,
                DestinationPath = destinationFilePath
            };
            await mediator.Send(command).ConfigureAwait(false);
            return CreatedAtAction("Get", new { path = HttpUtility.UrlEncode(command.DestinationPath.ToString()) });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(
            [FromQuery] string path,
            [FromServices] IMediator mediator)
        {
            if (!FilePath.TryParse(path, out var filepath))
            {
                return BadRequest("invalid path name.");
            }

            try
            {
                var command = new DeleteDocumentCommand
                {
                    Path = filepath
                };
                var response = await mediator.Send(command);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}