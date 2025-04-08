using Microsoft.EntityFrameworkCore;
using Task_Management_System.Data;
using Task_Management_System.DTOs.AvatarDto;
using Task_Management_System.Models;

namespace Task_Management_System.Services.AvatarService
{
    public class AvatarService : IAvatarService
    {
        private readonly ApplicationDbContext _dbContext;

        public AvatarService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAvatar(string avatarUrl)
        {

            var avatarExist = await _dbContext.Avatars.FirstOrDefaultAsync(url => url.AvatarUrl == avatarUrl);

            if (avatarExist != null)
            {
                throw new ArgumentException("Already exists");
            }

            var newAvatar = new Avatar
            {
                Id = Guid.NewGuid(),
                AvatarUrl = avatarUrl,
            };
            await _dbContext.Avatars.AddAsync(newAvatar);

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAvatar(Guid Id)
        {
            var avatarExist = await _dbContext.Avatars.FindAsync(Id);

            if (avatarExist == null)
            {
                throw new ArgumentNullException("Doesn't exist");
            }
            _dbContext.Avatars.Remove(avatarExist);
            _dbContext.RemoveRange(avatarExist, avatarExist.AvatarUrl);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<GetAvatarDto> GetAvatar(Guid Id)
        {
            var avatar = await _dbContext.Avatars.FindAsync(Id);

            if (avatar == null)
            {
                throw new ArgumentNullException("No avatar with id");
            }

            return new GetAvatarDto
            {
                Id = avatar.Id,
                Url = avatar.AvatarUrl
            };
        }

        public async Task<IEnumerable<GetAvatarDto>> GetAvatars()
        {
            IQueryable<Avatar> avatars = _dbContext.Avatars;

            var getAll = await avatars.ToListAsync();

            if (getAll.Count < 1)
            {
                return new List<GetAvatarDto>();
            }

            var selectAvatar = getAll.Select(ava => new GetAvatarDto
            {
                Id = ava.Id,
                Url = ava.AvatarUrl
            }).ToList();
            return selectAvatar;
        }
    }
}
