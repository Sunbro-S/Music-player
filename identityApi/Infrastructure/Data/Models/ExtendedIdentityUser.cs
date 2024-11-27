using Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data.Models;

public class ExtendedIdentityUser : IUser
{
    [Required]
    public string UserName { get; set; }
    public int UserID { get; set; }
    [Required]
    public string Password { get; set; }
    public string Email { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
}