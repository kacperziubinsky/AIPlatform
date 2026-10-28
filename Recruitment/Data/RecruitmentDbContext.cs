using Microsoft.EntityFrameworkCore;
using Recruitment.Models;

namespace Recruitment.Data;

public class RecruitmentDbContext : DbContext
{
    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options) : base(options)
    {
    }

    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<Evaluation> Evaluations => Set<Evaluation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobApplication>(e =>
        {
            e.HasKey(a => a.Id);

            e.HasIndex(a => new { a.OfferId, a.ApplicantId }).IsUnique();

            e.Property(a => a.OfferTitle).HasMaxLength(200);
            e.Property(a => a.CoverLetter).HasMaxLength(3000);
            e.Property(a => a.RecruiterNote).HasMaxLength(1000);

            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.AiStatus).HasConversion<string>().HasMaxLength(30);
            e.Property(a => a.AiError).HasMaxLength(1000);

            e.HasIndex(a => new { a.AiStatus, a.AiNextAttemptAt });

            e.HasMany(a => a.Evaluations)
                .WithOne(ev => ev.Application)
                .HasForeignKey(ev => ev.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Evaluation>(e =>
        {
            e.HasKey(ev => ev.Id);
            e.Property(ev => ev.Comment).HasMaxLength(2000);
            e.Property(ev => ev.Source).HasConversion<string>().HasMaxLength(20);

            e.OwnsMany(ev => ev.Breakdown, b =>
            {
                b.ToJson();
                b.Property(x => x.Level).HasConversion<string>();
            });
        });
    }
}