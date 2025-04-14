using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Task_Management_System.Data;
using Task_Management_System.DTOs.UserDto;
using Task_Management_System.Models;
using Task_Management_System.Services.Configurations.TokenGenerator;

namespace Task_Management_System.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IMapper _mapper;
        //private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _applicationDbContext;

        public UserService(UserManager<User> userManager, ITokenGenerator tokenGenerator, IMapper mapper, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
            _mapper = mapper;
            _applicationDbContext = dbContext;
        }


        public async Task<TokenResponse> LoginUser(LoginDto loginDto)
        {
            var user = await ValidateUser(loginDto);

            var token = _tokenGenerator.GenerateToken(user, await _userManager.GetRolesAsync(user));

            return new TokenResponse(token);
        }

        public async Task<TokenResponse> RegisterUser(RegisterDto registerDto)
        {
            var findUser = await _userManager.FindByEmailAsync(registerDto.Email);

            if (findUser != null)
            {
                throw new Exception($"User with Email - {registerDto.Email} already exist in the database");
            }

            else
            {
                var today = DateOnly.FromDateTime(DateTime.Now);

                var minimumBirthDate = today.AddYears(-10);

                if (registerDto.BirthDate >= minimumBirthDate)
                {
                    throw new Exception("BirthDate must be at least 10 years in the past.");
                }

                //default avatar
                var getAvatar = _applicationDbContext.Avatars.FirstOrDefault();
                var createUser = await _userManager.CreateAsync(new User
                {
                    Id = Guid.NewGuid(),
                    Email = registerDto.Email,
                    UserName = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    Birthdate = registerDto.BirthDate,
                    CreatedAt = DateTime.UtcNow,
                    Avatar = getAvatar,
                    AvatarUrl = getAvatar.AvatarUrl
                }, registerDto.Password);

                if (!createUser.Succeeded)
                {
                    throw new Exception("Unable to register user into the database");
                }

                else
                {
                    var retrieveRegisteredUser = await _userManager.FindByEmailAsync(registerDto.Email);

                    var token = _tokenGenerator.GenerateToken(retrieveRegisteredUser);
                    return new TokenResponse(token);
                }
            }

        }

        public async Task<GetProfileDto> UpdateProfile(UpdateDto? updateDto, Guid? avatar, string userId)
        {
            var getUser = await _userManager.FindByIdAsync(userId);

            if (getUser == null)
            {
                throw new Exception("User not found");
            }
            else
            {
                if (!string.IsNullOrEmpty(updateDto.FirstName) && updateDto.FirstName != "string")
                {
                    getUser.FirstName = updateDto.FirstName;
                }

                if (!string.IsNullOrEmpty(updateDto.LastName) && updateDto.LastName != "string")
                {
                    getUser.LastName = updateDto.LastName;
                }

                if (!string.IsNullOrEmpty(updateDto.PhoneNumber) && !updateDto.PhoneNumber.Equals("string"))
                {
                    if (updateDto.PhoneNumber == null)
                    {
                        updateDto.PhoneNumber = getUser.PhoneNumber;
                    }
                    getUser.PhoneNumber = updateDto.PhoneNumber;
                }
                if (avatar != null)
                {
                    var getAvatar = await _applicationDbContext.Avatars.FindAsync(avatar);

                    if (getAvatar != null)
                    {
                        getUser.Avatar = getAvatar;
                        getUser.AvatarUrl = getAvatar.AvatarUrl;
                    }
                }
                if (updateDto.BirthDate != null)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);

                    var minimumBirthDate = today.AddYears(-10);

                    if (updateDto.BirthDate >= minimumBirthDate)
                    {
                        throw new Exception("BirthDate must be at least 10 years in the past.");
                    }
                    else
                    {
                        getUser.Birthdate = updateDto.BirthDate;
                    }
                }

                var updateUser = await _userManager.UpdateAsync(getUser);

                if (updateUser.Succeeded)
                {
                    var newProfile = _mapper.Map<GetProfileDto>(getUser);
                    return newProfile;
                }

                throw new Exception("can't");
            }
        }


        public async Task<GetProfileDto> UserProfile(string userId)
        {
            var getUser = await _userManager.FindByIdAsync(userId);

            if (getUser == null)
            {
                throw new Exception("User doesn't exist");
            }
            else
            {
                var profile = _mapper.Map<GetProfileDto>(getUser);

                return profile;
            }
        }


        private async Task<User> ValidateUser(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                throw new Exception($"Email is either wrong or not registered");
            }

            else
            {
                var validatePassword = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

                return validatePassword == PasswordVerificationResult.Success ? user : throw new Exception("Password incorrect");
            }
        }
    }
}
