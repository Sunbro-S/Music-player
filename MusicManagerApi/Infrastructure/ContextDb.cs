using Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ContextDb : DbContext
{
    public ContextDb(DbContextOptions<ContextDb> options) : base(options)
    {
    }
    
    public DbSet<Music> Musics { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
}
    
