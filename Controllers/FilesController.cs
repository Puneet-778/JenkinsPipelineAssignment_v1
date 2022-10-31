using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using AWS_Asssignment_1.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AWS_Asssignment_1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;
        public FilesController()
        {
            var credentials = new BasicAWSCredentials("Access key ID", "Secret access key");
            _s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFilesAsync( string? prefix)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync("linuxbucketv1");
            if (!bucketExists) return NotFound($"Bucket linuxbucketv1 does not exist.");
            var request = new ListObjectsV2Request()
            {
                BucketName = "linuxbucketv1",
                Prefix = prefix
            };
            var result = await _s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = "linuxbucketv1",
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = _s3Client.GetPreSignedURL(urlRequest),
                };
            });
            return Ok(s3Objects);
        }
        [HttpGet("get-by-key")]
        public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
            var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }
     }
}
