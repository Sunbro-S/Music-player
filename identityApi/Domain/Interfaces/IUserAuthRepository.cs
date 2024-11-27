using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserAuthRepository<T> where T : class
    {
        Task<bool> CreateUserAsync(RegisterRequest userInfo);
        Task<LoginResponse> LoginUserAsync(LoginRequest user);
        Task<LoginResponse> LogoutUserAsync(HttpRequest request);
        Task<LoginResponse> DeleteUserAsync(HttpRequest request);
    }
}
