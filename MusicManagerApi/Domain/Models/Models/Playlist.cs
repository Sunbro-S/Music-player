namespace Infrastructure.Data.Models;

public class Playlist
{
    public string CreatorId { get; set; }
    public string PlaylistId { get; set; }
    public List<string> SongsId { get; set; }
    public bool PrivateStatus { get; set; }
    
}