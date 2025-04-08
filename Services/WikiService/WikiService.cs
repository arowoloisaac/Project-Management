using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.WikiDto;
using Task_Management_System.Models;
using Task_Management_System.Services.UserConfiguration;

namespace Task_Management_System.Services.WikiService
{
    public class WikiService : IWikiService
    {
        private readonly ApplicationDbContext context;
        private readonly IUserConfig config;

        public WikiService(ApplicationDbContext context, IUserConfig config)
        {
            this.context = context;
            this.config = config;
        }

        public async Task<string> CreateWiki(Guid projectId, WikiDto wikiDto, string userId)
        {
            var user = await config.GetUserById(userId);

            var getProject = await context.Projects.FindAsync(projectId);

            var getWikiProject = await context.Wikis
                .Where(prj => prj.Project.Id == projectId && prj.Title != wikiDto.Title).SingleOrDefaultAsync();

            if (getWikiProject != null)
            {
                throw new Exception("Project with title already exist");
            }

            else
            {
                var addWiki = new Wiki
                {
                    Id = Guid.NewGuid(),
                    Title = wikiDto.Title,
                    Content = wikiDto.Content,
                    CreatedTime = DateTime.UtcNow,
                    //CreatedBy = user,
                    Project = getProject,
                    User = user,
                };

                await context.Wikis.AddAsync(addWiki);

                await context.SaveChangesAsync();

                return "Saved successfully";
            }
        }

        public async Task<string> CreateWikiChild(Guid projectId, WikiDto wikiDto, Guid parentWikiId, string userId)
        {
            var user = await config.GetUserById(userId);

            var getProject = await context.Projects.FindAsync(projectId);

            var getWikiProject = await context.Wikis
                .Where(prj => prj.Project.Id == projectId && prj.Id == parentWikiId).SingleOrDefaultAsync();

            if (getWikiProject != null)
            {
                throw new Exception("Project with title already exist");
            }

            else
            {
                var addWiki = new Wiki
                {
                    Id = Guid.NewGuid(),
                    Title = wikiDto.Title,
                    Content = wikiDto.Content,
                    CreatedTime = DateTime.UtcNow,
                    //CreatedBy = user,
                    Project = getProject,
                    User = user,
                    ParentWiki = getWikiProject
                };

                await context.Wikis.AddAsync(addWiki);

                await context.SaveChangesAsync();

                return "Saved successfully";
            }
        }

        public async Task<string> DeleteWiki(Guid projectId, bool isDeleteChild, Guid wikiId, string userId)
        {
            var user = await config.GetUserById(userId);

            var getProject = await context.Projects.FindAsync(projectId);

            var getWiki = await context.Wikis
                .Where(wk => wk.Id == wikiId && wk.Project == getProject && wk.User == user).FirstOrDefaultAsync();

            if (getWiki == null)
            {
                throw new Exception("Wiki does not exist");
            }

            var getChildren = await context.Wikis.Where(pi => pi.ParentWiki.Id == wikiId).ToListAsync();

            if (isDeleteChild)
            {
                context.Wikis.RemoveRange(getChildren);
            }
            else
            {
                foreach (var child in getChildren)
                {
                    child.ParentWiki = null;
                }
            }
            context.Wikis.Remove(getWiki);

            await context.SaveChangesAsync();

            return "Wiki deleted";

        }

        public async Task<GetWikiDto> GetWiki(Guid projectId, Guid wikiId, string userId)
        {
            var user = await config.GetUserById(userId);

            var retrieveWikiOwner = await context.Wikis
                .Where(wo => wo.Id == wikiId
                //&& wo.CreatedBy == user
                && wo.Project.Id == projectId).SingleOrDefaultAsync();

            if (retrieveWikiOwner == null)
            {
                throw new Exception("Wiki does not exist");
            }

            var wiki = new GetWikiDto
            {
                Id = retrieveWikiOwner.Id,
                CreatedBy = user.Email,
                Title = retrieveWikiOwner.Title,
                Content = retrieveWikiOwner.Content,
                DateCreated = retrieveWikiOwner.CreatedTime,
            };

            return wiki;
        }

        public async Task<IEnumerable<GetWikiDto>> GetWikis(Guid projectId, string userId)
        {
            var user = await config.GetUserById(userId);

            var getProject = await context.Projects.FindAsync(projectId);

            var getWiki = await context.Wikis
                .Where(wk => wk.Project == getProject && wk.User == user).ToListAsync();

            if (getWiki == null)
            {
                return Enumerable.Empty<GetWikiDto>();
            }

            else
            {
                var response = getWiki.Select(wk => new GetWikiDto
                {
                    Id = wk.Id,
                    Title = wk.Title,
                    Content = wk.Content,
                    DateCreated = wk.CreatedTime,
                    Wiki = context.Wikis.Where(cw => cw.ParentWiki.Id == wk.Id).Select(w => new GetWikiDto
                    {
                        Id = w.Id,
                        Title = w.Title,
                    }).ToList()
                }).ToList();

                return response;
            }
        }

        public async Task<string> UpdateWiki(Guid projectId, Guid wikiId, WikiDto wikiDto, string userId)
        {
            var user = await config.GetUserById(userId);

            var retrieveWikiOwner = await context.Wikis
                .Where(wo => wo.Id == wikiId
                //&& wo.CreatedBy == user
                && wo.Project.Id == projectId).SingleOrDefaultAsync();

            if (retrieveWikiOwner == null)
            {
                throw new Exception("Wiki does not exist");
            }

            else
            {
                if (!string.IsNullOrEmpty(wikiDto.Title))
                {
                    retrieveWikiOwner.Title = wikiDto.Title;
                }

                if (!string.IsNullOrEmpty(wikiDto.Content))
                {
                    retrieveWikiOwner.Content = wikiDto.Content;
                }
                retrieveWikiOwner.UpdatedTime = DateTime.Now;
                retrieveWikiOwner.LastUpdateBy = user.Id;

                await context.Wikis.SingleUpdateAsync(retrieveWikiOwner);

                return "Updated Successfully";
            }
        }
    }
}
