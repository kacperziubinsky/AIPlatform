using Microsoft.EntityFrameworkCore;
using User.Models;

namespace User.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
    
    public DbSet<UserProfile> Users { get; set; }
    public DbSet<CV> CVs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasMany(x => x.CVs)
                .WithOne()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<CV>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.ParseStatus).HasConversion<string>();

            e.OwnsOne(x => x.ParsedData, pd =>
            {
                pd.ToJson();
                pd.OwnsMany(p => p.WorkHistory);
                pd.OwnsMany(p => p.Education);
                pd.OwnsMany(p => p.Languages);
            });
        });
    }

}