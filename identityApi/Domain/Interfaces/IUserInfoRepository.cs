using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserInfoRepository<T> where T : class
    {
        Task<T> AddFavorite(Music musicInfo);
        Task<T> PutUserChangesAsync(HttpRequest request, UpdateUserDataRequest updateUserDataRequest);
    }
}
