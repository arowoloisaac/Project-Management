using Task_Management_System.Models;

namespace Task_Management_System.ExternalServices.CloudSetting
{
    public interface ICloudService
    {
        Task<List<string>> GetObjects();

        Task<byte[]> GetObject(string objectName);

        Task<string> UploadObject(ObjectUpload model);
    }
}
