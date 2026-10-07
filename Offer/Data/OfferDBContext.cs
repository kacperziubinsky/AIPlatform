using Microsoft.EntityFrameworkCore;
using Offer.Models;

namespace Offer.Data;

public class OfferDbContext : DbContext
{
    public OfferDbContext(DbContextOptions<OfferDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.JobOffer> Offers { get; set; }
}