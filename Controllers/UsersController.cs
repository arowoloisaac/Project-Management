using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Task_Management_System.DTOs.UserDto;
using Task_Management_System.Services.UserService;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [EnableCors]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> RegisterUser([Required] RegisterDto dto)
        {
            try
            {
                return Ok(await _userService.RegisterUser(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> LoginUser(LoginDto dto)
        {
            try
            {
                return Ok(await _userService.LoginUser(dto));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("profile")]
        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var claimUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (claimUser == null)
                {
                    return NotFound("user not found");
                }
                else
                {
                    return Ok(await _userService.UserProfile(claimUser.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex.Message}");
            }
            throw new NotImplementedException();
        }


        [HttpPut]
        [Route("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(UpdateDto? dto, Guid? avatar)
        {
            try
            {
                var claimUser = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);

                if (claimUser == null)
                {
                    return NotFound("User not found");
                }

                else
                {
                    return Ok(await _userService.UpdateProfile(dto, avatar, claimUser.Value));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
