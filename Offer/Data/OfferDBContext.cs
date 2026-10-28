using Microsoft.EntityFrameworkCore;

namespace Offer.Data;

public class OfferDbContext : DbContext
{
    public OfferDbContext(DbContextOptions<OfferDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobOffer> Offers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
 
        modelBuilder.Entity<JobOffer>(offer =>
        {
            offer.OwnsMany(o => o.Requirements, r =>
            {
                r.ToTable("Requirements");
                r.WithOwner().HasForeignKey("JobOfferId");
                r.HasKey(x => x.Id);
                r.Property(x => x.Id).ValueGeneratedNever();
                r.Property(x => x.Name).HasMaxLength(200).IsRequired();
                r.Property(x => x.Description).HasMaxLength(2000);
            });
        });
    }
}