using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data.Models;

public class Music
{
    public string Id { get; set; }
    
    [Key]public string MusicName { get; set; }
    public string Author { get; set; }
    public string FilePath { get; set; }
}