using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task_Management_System.ExternalServices.CloudSetting;
using Task_Management_System.Models;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    public class CloudController : ControllerBase
    {
        private readonly ICloudService _s3Service;

        public CloudController(ICloudService cloudService)
        {
            _s3Service = cloudService;
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListObjects()
        {
            try
            {
                var objects = await _s3Service.GetObjects();
                return Ok(objects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddObject(ObjectUpload model)
        {
            try
            {
                var objects = await _s3Service.UploadObject(model);
                return Ok(objects);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
