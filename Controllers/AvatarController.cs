using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Task_Management_System.Services.AvatarService;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    public class AvatarController : ControllerBase
    {
        private readonly IAvatarService _avatarService;

        public AvatarController(IAvatarService avatarService)
        {
            _avatarService = avatarService;
        }

        [HttpPost]
        [Route("avatar/add")]
        public async Task<IActionResult> AddAvatar(string avatarUrl)
        {
            try
            {
                await _avatarService.AddAvatar(avatarUrl);
                return Ok("CREATED");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("avatar/delete")]
        public async Task<IActionResult> DeleteAvatar(Guid Id)
        {
            try
            {
                await _avatarService.DeleteAvatar(Id);
                return Ok("deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("avatar/{Id}")]
        public async Task<IActionResult> GetAvatar(Guid Id)
        {
            try
            {
                //await _avatarService.GetAvatar(Id);
                return Ok(await _avatarService.GetAvatar(Id));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("avatar/get/all")]
        public async Task<IActionResult> GetAvatars()
        {
            try
            {
                return Ok(await _avatarService.GetAvatars());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
