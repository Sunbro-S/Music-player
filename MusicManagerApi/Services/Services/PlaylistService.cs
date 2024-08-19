using Infrastructure;

namespace Services.Services;

public class PlaylistService : IPlaylistService
{
    private readonly ContextDb _context;
    
    public PlaylistService(ContextDb context)
    {
        _context = context;
    }

    public void CreatePlaylist()
    {
        
    }
    
    public void DeletePlaylist()
    {
        
    }
    public void RedactPlaylist()
    {
        
    }

    public void AddSingleMusic()
    {
        
    }
}