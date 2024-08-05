using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using WholesaleDistributionApp.Controllers;

namespace WholesaleDistributionApp.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3Service> _logger;
        private readonly string _bucketName;
        private readonly string _imageFolderPath = "images/"; // Ensure this is correctly set

        public S3Service(IConfiguration configuration, ILogger<S3Service> logger)
        {
            _logger = logger;
            // Read AWS options from configuration
            var awsOptions = configuration.GetAWSOptions();
            _s3Client = awsOptions.CreateServiceClient<IAmazonS3>();

            // Alternatively, you could initialize the S3 client with credentials manually:
            // var awsAccessKey = configuration["AWS:AccessKey"];
            // var awsSecretKey = configuration["AWS:SecretKey"];
            // var awsRegion = configuration["AWS:Region"];
            // var regionEndpoint = RegionEndpoint.GetBySystemName(awsRegion);
            // _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, regionEndpoint);

            // Get bucket name from configuration
            _bucketName = configuration["AWS:BucketName"];
        }

        public string GetFileUrl(string keyName)
        {
            string fullPath = $"{_imageFolderPath}{keyName}";
            return $"https://{_bucketName}.s3.amazonaws.com/{fullPath}";
        }

        public async Task<bool> UploadFileAsync(string keyName, string filePath)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"{_imageFolderPath}{keyName}", // Ensure the key includes the folder path
                    FilePath = filePath,
                };

                var response = await _s3Client.PutObjectAsync(putRequest);
                Console.WriteLine("File uploaded successfully.");
                return true;
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e.Message);
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when writing an object");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                Console.WriteLine($"Unknown error encountered. Message:'{e.Message}' when writing an object");
                return false;
            }
        }

        public async Task DownloadFileAsync(string keyName, string destinationPath)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = keyName
                };

                using (var response = await _s3Client.GetObjectAsync(request))
                using (var responseStream = response.ResponseStream)
                using (var fs = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    await responseStream.CopyToAsync(fs);
                }

                Console.WriteLine("File downloaded successfully.");
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e.Message);
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when reading an object");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                Console.WriteLine($"Unknown error encountered. Message:'{e.Message}' when reading an object");
            }
        }
    }
}