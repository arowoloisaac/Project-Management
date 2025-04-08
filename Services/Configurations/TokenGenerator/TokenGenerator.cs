using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Task_Management_System.Configurations;
using Task_Management_System.Models;

namespace Task_Management_System.Services.Configurations.TokenGenerator
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly JwtBearerSetting _tokenSetting;

        public TokenGenerator(IOptionsSnapshot<JwtBearerSetting> jwtBearer)
        {
            _tokenSetting = jwtBearer.Value;
        }


        public string GenerateToken(User user, IList<string>? userRoles)
        {
            var TokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_tokenSetting.SecretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Authentication, user.Id.ToString()),
            };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(_tokenSetting.ExpiryTimeInSeconds),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Audience = _tokenSetting.Audience,
                Issuer = _tokenSetting.Issuer,
            };

            var token = TokenHandler.CreateToken(descriptor);

            return TokenHandler.WriteToken(token);
        }

        public string GenerateToken(User user)
        {
            var TokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_tokenSetting.SecretKey);

            var credentials = new SigningCredentials
                (
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                );

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Authentication, user.Id.ToString()),
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddSeconds(_tokenSetting.ExpiryTimeInSeconds),
                SigningCredentials = credentials,
                Audience = _tokenSetting.Audience,
                Issuer = _tokenSetting.Issuer,
            };

            var token = TokenHandler.CreateToken(descriptor);

            return TokenHandler.WriteToken(token);
        }
    }
}
