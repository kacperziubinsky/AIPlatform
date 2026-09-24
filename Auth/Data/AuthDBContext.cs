using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDBContext : DbContext
{
    public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options) { }
    
}