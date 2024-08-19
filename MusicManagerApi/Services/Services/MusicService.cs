using Domain;
using Infrastructure;
using Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Services.Services;

public class MusicService : IMusicService
{
    private readonly ContextDb _context;

    public MusicService(ContextDb context)
    {
        _context = context;
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

            var response = new Music()
            {
                MusicName = musicName,
                Author = author,
                Id = uniqueId.ToString(),
                FilePath = filePath
            };
            _context.Musics.Add(response);
            await _context.SaveChangesAsync();
            return true; 
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<FileStream> PlaySelectedMusicAsync(string musicName)
    {
        var searchResponse = _context.Musics.FirstOrDefault(m => m.MusicName == musicName);
        if (searchResponse == null || !System.IO.File.Exists(searchResponse.FilePath))
            throw new FileNotFoundException("Music file not found.");
        return new FileStream(searchResponse.FilePath, FileMode.Open, FileAccess.Read);

    }

    public async Task<List<MusicSerchResponse>> GetPotentialMusic(string request, int page = 1, int pageSize = 10)
    {
        var lowerCaseRequest = request.ToLower();

        var query = _context.Musics
            .Where(m => m.MusicName.ToLower().Contains(lowerCaseRequest) ||
                        m.Author.ToLower().Contains(lowerCaseRequest));

        int totalCount = await query.CountAsync();

        var musicList = await query
            .Select(m => new MusicSerchResponse
            {
                MusicName = m.MusicName,
                Author = m.Author
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (musicList == null || musicList.Count == 0)
            throw new FileNotFoundException("Music not found.");

        return musicList;
    }
}