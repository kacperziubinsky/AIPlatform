using Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}