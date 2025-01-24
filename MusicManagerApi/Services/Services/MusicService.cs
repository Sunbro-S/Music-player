using Domain;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Services.Services;

public class MusicService : IMusicService
{
    private readonly IMusicRepository _musicRepository;
    public MusicService(IMusicRepository musicRepository)
    {
        _musicRepository = musicRepository;
    }

    public async Task<bool> PushMusicAsync(IFormFile musicFile,string musicName, string author, string contentPath)
    {
        if (musicFile == null || musicFile.Length == 0)
        {
            return false;
        }
        try
        {
            var fileExtension = Path.GetExtension(musicFile.FileName);
            if (fileExtension.ToLower() != ".mp3")
            {
                throw new InvalidOperationException("Invalid file format. Only MP3 files are allowed.");
            }

            var uniqueId = Guid.NewGuid();
            var uniqueFileName = uniqueId.ToString() + fileExtension;
            var filePath = Path.Combine(contentPath, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await musicFile.CopyToAsync(stream);
            }

            return await _musicRepository.AddMusicAsync(musicName, author, uniqueId, filePath); 
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<FileStream> PlaySelectedMusicAsync(string musicName)
    {
        var searchResponse = await _musicRepository.GetByMusicNameAsync(musicName);

        if (searchResponse == null || !System.IO.File.Exists(searchResponse.FilePath))
            throw new FileNotFoundException("Music file not found.");

        return new FileStream(searchResponse.FilePath, FileMode.Open, FileAccess.Read);

    }

    public async Task<List<MusicSearchResponse>> GetPotentialMusic(string request, int page = 1, int pageSize = 10)
    {
        var musicList = await _musicRepository.GetPaginatedByNameAsync(request, page, pageSize);

        if (musicList == null || musicList.Count == 0)
            throw new FileNotFoundException("Music not found.");

        return musicList;
    }
}