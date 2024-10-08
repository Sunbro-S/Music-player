﻿using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Data.Models;

public class ExtendedIdentityUser : IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
}