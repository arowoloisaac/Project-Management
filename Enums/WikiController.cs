using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Task_Management_System.DTOs.WikiDto;
using Task_Management_System.Services.WikiService;

namespace Task_Management_System.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    [EnableCors]
    public class WikiController : ControllerBase
    {
        private readonly IWikiService wiki;

        public WikiController(IWikiService wiki)
        {
            this.wiki = wiki;
        }

        [HttpPost]
        [Route("project/{projectId}/wiki/create")]
        public async Task<IActionResult> CreateWiki(Guid projectId, WikiDto wikiDto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.CreateWiki(projectId, wikiDto, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("project/{projectId}/wiki/{wikiId}")]
        public async Task<IActionResult> GetWiki(Guid projectId, Guid wikiId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.GetWiki(projectId, wikiId, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPut]
        [Route("project/{projectId}/wiki/{wikiId}/update")]
        public async Task<IActionResult> UpdateWiki(Guid projectId, Guid wikiId, WikiDto wikiDto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.UpdateWiki(projectId, wikiId, wikiDto, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpDelete]
        [Route("project/{projectId}/wiki/{wikiId}/delete")]
        public async Task<IActionResult> DeleteWiki(Guid projectId, bool deleteChildren, Guid wikiId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.DeleteWiki(projectId, deleteChildren, wikiId, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        [Route("project/{projectId}/wiki{parentWikiId}/create-child")]
        public async Task<IActionResult> CreateWikiChild(Guid projectId, Guid parentWikiId, WikiDto wikiDto)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.CreateWikiChild(projectId, wikiDto, parentWikiId, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        [Route("project/{projectId}/wiki")]
        public async Task<IActionResult> GetWikis(Guid projectId)
        {
            try
            {
                var user = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Authentication);
                if (user == null)
                {
                    return Unauthorized("User not found");
                }
                return Ok(await wiki.GetWikis(projectId, user.Value));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
