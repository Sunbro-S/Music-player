using Domain;
using Domain.Interfaces;
using Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Infrastructure.Data.Repository
{
    public class MusicRepository : IMusicRepository
    {
        private readonly ContextDb _context;
        public MusicRepository(ContextDb contextDb) 
        {
            _context = contextDb;
        }

        public async Task<bool> AddMusicAsync(string musicName, string author, Guid uniqueId, string filePath)
        {
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

        public async Task<Music?> GetByMusicNameAsync(string name)
        {
            return await _context.Musics.FirstOrDefaultAsync(m => m.MusicName == name);
        }

        public async Task<List<MusicSearchResponse>> GetPaginatedByNameAsync(string name, int page, int pageSize)
        {
            var lowerCaseRequest = name.ToLower();

            var query = _context.Musics
                .Where(m => m.MusicName.ToLower().Contains(lowerCaseRequest) ||
                            m.Author.ToLower().Contains(lowerCaseRequest));

            int totalCount = await query.CountAsync();

            var musicList = await query
                .Select(m => new MusicSearchResponse
                {
                    MusicName = m.MusicName,
                    Author = m.Author
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return musicList;
        }
    }
}
