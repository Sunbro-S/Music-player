using Domain;
using Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Data.Interfaces
{
    public interface IUserAuthRepository<T> where T : class
    {
        Task<ExtendedIdentityUser> GetUserByLogin(string email);
        Task<ExtendedIdentityUser> TokenRefresh(ExtendedIdentityUser user, LoginResponse response);
        Task<ExtendedIdentityUser> TokenReset(ExtendedIdentityUser user, LoginResponse response);
        Task<string> GetAccessToken(HttpRequest request); 

    }
}