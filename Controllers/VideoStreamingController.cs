using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using System.IO;

namespace ELearningPlatform.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoStreamingController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VideoStreamingController> _logger;
        private const int BufferSize = 64 * 1024;

        public VideoStreamingController(IWebHostEnvironment environment, ILogger<VideoStreamingController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetVideo([FromQuery] string filename)
        {
            var path = Path.Combine(_environment.ContentRootPath, "Videos", filename);

            if (!System.IO.File.Exists(path))
            {
                _logger.LogWarning($"File not found: {path}");
                return NotFound($"File {filename} not found.");
            }

            var fileInfo = new FileInfo(path);
            var contentType = GetContentType(path);
            var rangeHeader = Request.Headers[HeaderNames.Range].ToString();

            if (string.IsNullOrEmpty(rangeHeader))
            {
                _logger.LogInformation($"Streaming entire file: {filename}");
                return PhysicalFile(path, contentType, enableRangeProcessing: true);
            }

            var (rangeStart, rangeEnd) = GetRange(rangeHeader, fileInfo.Length);
            var contentLength = rangeEnd - rangeStart + 1;

            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers.Add(HeaderNames.ContentType, contentType);
            Response.Headers.Add(HeaderNames.ContentLength, contentLength.ToString());
            Response.Headers.Add(HeaderNames.AcceptRanges, "bytes");
            Response.Headers.Add(HeaderNames.ContentRange, $"bytes {rangeStart}-{rangeEnd}/{fileInfo.Length}");

            _logger.LogInformation($"Streaming file {filename} from {rangeStart} to {rangeEnd}");

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(rangeStart, SeekOrigin.Begin);
                await StreamToClientAsync(stream, contentLength);
            }

            return new EmptyResult();
        }

        [HttpGet("list")]
        public IActionResult ListVideos()
        {
            var videoFolder = Path.Combine(_environment.ContentRootPath, "Videos");
            var videosFiles = Directory.GetFiles(videoFolder)
                .Select(Path.GetFileName)
                .ToList();
            _logger.LogInformation($"Listing {videosFiles.Count} videos");
            return Ok(videosFiles);
        }

        private async Task StreamToClientAsync(Stream stream, long contentLength)
        {
            var buffer = new byte[BufferSize];
            var totalBytesRead = 0L;
            while (totalBytesRead < contentLength)
            {
                var bytesRemaining = contentLength - totalBytesRead;
                var bytesToRead = (int)Math.Min(bytesRemaining, buffer.Length);
                var bytesRead = await stream.ReadAsync(buffer, 0, bytesToRead);
                if (bytesRead == 0)
                    break;
                await Response.Body.WriteAsync(buffer, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out string contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }

        private (long Start, long End) GetRange(string rangeHeader, long fileLength)
        {
            var ranges = rangeHeader.Replace("bytes=", "").Split('-');
            var start = ranges.Length > 0 && long.TryParse(ranges[0], out long parsedStart) ? parsedStart : 0;
            var end = ranges.Length > 1 && long.TryParse(ranges[1], out long parsedEnd) ? parsedEnd : fileLength - 1;

            return (Math.Min(start, fileLength), Math.Min(end, fileLength - 1));
        }
    }
}