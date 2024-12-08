using Domain;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Repositories
{
    public class UserAuthSQLRepository<T> : IUserAuthRepository<T> where T : ExtendedIdentityUser
    {
        private readonly ContextDb _context;
        private readonly UserManager<ExtendedIdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly DbSet<T> _dbSet;
        public UserAuthSQLRepository(ContextDb context, IConfiguration config, UserManager<ExtendedIdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
            _config = config;
            _dbSet = context.Set<T>();
        }
        private async Task<bool> UserExistsAsync(string userName, string email)
        {
            return await _dbSet.AnyAsync(u => u.UserName == userName || u.Email == email);
        }
        public async Task<ExtendedIdentityUser> GetUserByLogin(string login)
        {
            return await _context.ExtendedIdentityUsers
                                 .FirstOrDefaultAsync(u => u.UserName == login || u.Email == login);
        }

        public async Task<ExtendedIdentityUser> TokenRefresh(ExtendedIdentityUser user, LoginResponse response)
        {
            user.RefreshToken = response.Tokens.RefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            return user;
        }

        public async Task<ExtendedIdentityUser> TokenReset(ExtendedIdentityUser user, LoginResponse response)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = DateTime.UtcNow;
            return user;
        }

        public async Task<string> GetAccessToken(HttpRequest request)
        {
            string authHeader = request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            string accessToken = authHeader.Substring("Bearer ".Length).Trim();
            return accessToken;
        }
    }
}