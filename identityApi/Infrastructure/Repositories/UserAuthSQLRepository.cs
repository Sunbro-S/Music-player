using Domain;
using Domain.Interfaces;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UserAuthSQLRepository<T> : IUserAuthRepository<T> where T : class, IUser
    {
        private readonly ContextDb _context;
        private readonly IConfiguration _config;
        private readonly DbSet<T> _dbSet;
        public UserAuthSQLRepository(DbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _dbSet = context.Set<T>();
        }
        private async Task<bool> UserExistsAsync(string userName, string email)
        {
            return await _dbSet.AnyAsync(u => u.UserName == userName || u.Email == email);
        }
        private async Task<ExtendedIdentityUser> GetUserByEmailAsync(string email)
        {
            return await _context.ExtendedIdentityUsers
                                 .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> CreateUserAsync(RegisterRequest userInfo)
        {
            var userCount = await GetUsersCountAsync();
            if (await UserExistsAsync(userInfo.UserName, userInfo.Email))
            {
                return false;
            }
            var user = new ExtendedIdentityUser { UserName = userInfo.UserName, 
                UserID = userCount+1,
                Email = userInfo.Email,
                Password = EncryptPassword(userInfo.Password),
            };

            var fullname = userInfo.FullName.Split(" ");
            var userinfo = new UserEntity
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,
                Name = fullname[1],
                Lastname = fullname[0]
            };
            _context.ExtendedIdentityUsers.Add(user);
            _context.Users.Add(userinfo);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LoginResponse> DeleteUserAsync(HttpRequest request)
        {
            string authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            string accessToken = authHeader.Substring("Bearer ".Length).Trim();
            var userEmail = GetClaimFromAccessToken(accessToken, ClaimTypes.Email);
            var user = await _context.ExtendedIdentityUsers.FindAsync(userEmail);
            var response = new LoginResponse();
            _context.ExtendedIdentityUsers.Remove(user);
            var result = UserExistsAsync(user.UserName, user.Email).Result;
            if (!result)
            {
                return null;
            }

            var userInfo = await _context.Users.FindAsync(user.UserID);
            if (userInfo != null)
            {
                _context.Remove(userInfo);
            }

            await _context.SaveChangesAsync();

            response = BadLoginResponse();
            return response;
        }

        public async Task<LoginResponse> LoginUserAsync(LoginRequest user)
        {
            ExtendedIdentityUser? identityUser = null;

            var response = BadLoginResponse();
            if (user.Login != null)
            {
                identityUser = await _context.ExtendedIdentityUsers.FindAsync(user.Login);
                if (identityUser == null)
                    identityUser = await _context.(user.Login);
            }

            if (identityUser is null || (await _userManager.CheckPasswordAsync(identityUser, user.Password)) == false)
            {
                return response;
            }

            var userInfo = await _context.Users.FindAsync(identityUser.Id);
            response = GoodLoginResponse(identityUser, userInfo);

            identityUser.RefreshToken = response.Tokens.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);

            await _userManager.UpdateAsync(identityUser);

            return response;
        }

        public async Task<LoginResponse> LogoutUserAsync(HttpRequest request)
        {

            string authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            string accessToken = authHeader.Substring("Bearer ".Length).Trim();
            var response = new LoginResponse();
            var userEmail = GetClaimFromAccessToken(accessToken, ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                return null;
            }


            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);


            response = BadLoginResponse();

            return response;
        }

        private string EncryptPassword(string password)
        {
            var secretKey = _config.GetSection("Secret:Key").Value;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = hmac.ComputeHash(passwordBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private async Task<int> GetUsersCountAsync()
        {
            return await _dbSet.OfType<ExtendedIdentityUser>().CountAsync();
        }
        private string GetClaimFromAccessToken(string accessToken, string claimType)
        {
            var claims = DecodeAccessToken(accessToken);
            var claim = claims.FirstOrDefault(c => c.Type == claimType);
            return claim?.Value;
        }

        private List<Claim> DecodeAccessToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _config.GetSection("Jwt:Issuer").Value,
                ValidAudience = _config.GetSection("Jwt:Audience").Value,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters,
                    out SecurityToken validatedToken);
                return principal.Claims.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при расшифровке токена: {ex.Message}");
                return null;
            }
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
            {
                numberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber);
        }

        public string GenerateTokenString(ExtendedIdentityUser user)
        {
            var role = _userManager.GetRolesAsync(user).Result.First();
            var claims = new List<Claim>
        {

            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, role)
        };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value));

            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                issuer: _config.GetSection("Jwt:Issuer").Value,
                audience: _config.GetSection("Jwt:Audience").Value,
                signingCredentials: signingCred);

            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToken);
            return tokenString;
        }
        private LoginResponse BadLoginResponse()
        {
            var response = new LoginResponse()
            {
                Tokens = new Tokens
                {
                    IsLogedIn = false,
                    JwtToken = null,
                    JwtTokenExpiry = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                    RefreshToken = null
                },
                User = new User
                {
                    Login = null,
                    Fullname = null,
                    Email = null
                }
            };
            return response;
        }
    }
}
