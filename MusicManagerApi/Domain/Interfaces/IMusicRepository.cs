using Infrastructure.Data.Models;

namespace Domain.Interfaces
{
    public interface IMusicRepository
    {
        public Task<bool> AddMusicAsync(string musicName, string author, Guid uniqueId, string filePath);
        public Task<Music?> GetByMusicNameAsync(string name);
        public Task<List<MusicSearchResponse>> GetPaginatedByNameAsync(string name, int page, int pageSize);
    }
}
