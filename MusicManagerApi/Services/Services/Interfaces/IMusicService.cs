using Domain;
using Microsoft.AspNetCore.Http;

namespace Services.Services;

public interface IMusicService
{
    Task<bool> PushMusicAsync(IFormFile musicFile, string musicName, string author, string contentPath);
    Task<FileStream> PlaySelectedMusicAsync(string musicName);

    Task<List<MusicSerchResponse>> GetPotentialMusic(string request, int page = 1, int pageSize = 10);
    // Task<bool> GetMusicPlaylistAsync();
}