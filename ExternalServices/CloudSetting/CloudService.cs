using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Options;
using Task_Management_System.Configurations;
using Task_Management_System.Data;
using Task_Management_System.Models;

namespace Task_Management_System.ExternalServices.CloudSetting
{
    public class CloudService : ICloudService
    {
        private readonly YandexCloudSetting _setting;
        private readonly IAmazonS3 _s3Client;
        private readonly ApplicationDbContext _context;

        public CloudService(IOptionsSnapshot<YandexCloudSetting> cloudSetting, IAmazonS3 s3, ApplicationDbContext context)
        {
            _setting = cloudSetting.Value;
            _s3Client = s3;
            _context = context;
        }

        public async Task<List<string>> GetObjects()
        {
            var objectsList = new List<string>();
            var objectCloud = new ListObjectsV2Request
            {
                BucketName = _setting.BucketName,
            };

            var response = await _s3Client.ListObjectsV2Async(objectCloud);

            //var respo = await _s3Client.ListObjectsAsync(objectCloud);


            foreach (var obj in response.S3Objects)
            {
                objectsList.Add(obj.BucketName);
            }

            return objectsList;
        }

        public async Task<string> UploadObject(ObjectUpload model)
        {
            using var stream = new MemoryStream();
            await model.File.CopyToAsync(stream);

            stream.Position = 0;

            var counter = await GetCounter();
            counter.Value++;
            _context.Counters.Update(counter);
            await _context.SaveChangesAsync();

            string prefix = "pic-";
            var getExtension = Path.GetExtension(model.File.FileName);


            string fileName = $"{prefix}{counter.Value}{getExtension}";

            var contentType = GetContentType(fileName);


            var request = new PutObjectRequest
            {
                BucketName = _setting.BucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = contentType,
            };

            try
            {
                var response = await _s3Client.PutObjectAsync(request);

                //return response.ToString();

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    return $"File {model.FileName} uploaded successfully.";
                }
                else
                {
                    return $"Failed to upload file. Status code: {response.HttpStatusCode}";
                }
            }
            catch (AmazonS3Exception ex)
            {
                return $"Error uploading file: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"An error occurred: {ex.Message}";
            }
        }

        public Task<byte[]> GetObject(string objectName)
        {
            throw new NotImplementedException();
        }


        private async Task<Counter> GetCounter()
        {
            var retrieveValue = await _context.Counters.FindAsync(1);
            if (retrieveValue == null)
            {
                _context.Counters.Add(new Counter { Value = 1 });
                await _context.SaveChangesAsync();

            }
            return retrieveValue;
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                default:
                    return "application/octet-stream"; // Default if unknown
            }
        }
    }
}
