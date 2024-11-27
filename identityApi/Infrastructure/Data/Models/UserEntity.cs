using Infrastructure.Data.Interfaces;
using System.ComponentModel.DataAnnotations;
namespace Infrastructure.Data.Models;


public class UserEntity : IUser
{
    [Key] 
    public int UserID { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    
    public string? Icon { get; set; } = null;
    
    public List<string>? AllMusicId { get; set; }
    public List<string>? AllPlaylistId { get; set; }
    
}