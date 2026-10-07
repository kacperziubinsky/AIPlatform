using Microsoft.EntityFrameworkCore;
using Offer.Data;
using Offer.DTOs;
using Offer.Models;

namespace Offer.Services;

public class OfferService : IOfferService
{
    private readonly OfferDbContext _dbContext;

    public OfferService(OfferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OfferResponse> CreateOfferAsync(CreateOfferRequest request, Guid userId)
    {
        ValidateOfferData(request.Title, request.Description, request.Category, request.Experience, request.StartDate, request.EndDate);

        var offer = new JobOffer
        {
            Id = Guid.NewGuid(),
            AuthorId = userId,
            Title = request.Title,
            Description = request.Description,
            Salary = request.Salary,
            Category = request.Category,
            Stack = request.Stack ?? new List<string>(),
            Experience = request.Experience,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Company = request.Company,
            CreatedOn = DateTime.UtcNow,
        };

        _dbContext.Offers.Add(offer);
        await _dbContext.SaveChangesAsync();

        return MapToOfferResponse(offer);
    }

    public async Task<List<OfferResponse>> GetOffersAsync(OfferFilterRequest? filter)
    {
        var query = _dbContext.Offers
            .AsNoTracking()
            .Where(o => o.EndDate > DateTime.UtcNow);

        int limit = 50;

        if (filter != null)
        {
            limit = filter.Limit > 0 ? filter.Limit : 50;

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(o => o.Title.Contains(filter.SearchTerm) || o.Description.Contains(filter.SearchTerm));
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(o => o.Category == filter.Category.Value);
            }

            if (filter.Experience.HasValue)
            {
                query = query.Where(o => o.Experience == filter.Experience.Value); // Poprawiono z >= na ==
            }

            // POPRAWIONE: Min i Max Salary
            if (filter.MinSalary.HasValue)
            {
                query = query.Where(o => o.Salary >= filter.MinSalary.Value);
            }

            if (filter.MaxSalary.HasValue)
            {
                query = query.Where(o => o.Salary <= filter.MaxSalary.Value);
            }
        }

        return await query
            .OrderByDescending(o => o.CreatedOn)
            .Take(limit)
            .Select(o => new OfferResponse
            {
                Id = o.Id,
                Title = o.Title,
                Description = o.Description,
                Salary = o.Salary,
                Category = o.Category,
                Stack = o.Stack,
                Experience = o.Experience,
                StartDate = o.StartDate,
                EndDate = o.EndDate,
                CreatedOn = o.CreatedOn,
                Company = o.Company
            })
            .ToListAsync();
    }

    public async Task<List<OfferResponse>> GetAllOffersAsync(int limit = 50)
    {
        return await GetOffersAsync(new OfferFilterRequest { Limit = limit });
    }

    public async Task<OfferResponse?> GetOfferByIdAsync(Guid id)
    {
        var offer = await _dbContext.Offers
            .AsNoTracking() // Dodano AsNoTracking dla optymalizacji
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
        {
            return null;
        }

        return MapToOfferResponse(offer);
    }

    public async Task<OfferResponse?> UpdateOfferAsync(Guid id, UpdateOfferRequest request, Guid userId)
    {
        var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
        {
            throw new KeyNotFoundException("Offer not found.");
        }

        if (offer.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this offer.");
        }

        ValidateOfferData(request.Title, request.Description, request.Category, request.Experience, request.StartDate, request.EndDate);

        offer.Title = request.Title;
        offer.Description = request.Description;
        offer.Salary = request.Salary;
        offer.Category = request.Category;
        offer.Stack = request.Stack ?? new List<string>();
        offer.Experience = request.Experience;
        offer.StartDate = request.StartDate;
        offer.EndDate = request.EndDate;
        offer.Company = request.Company;

        await _dbContext.SaveChangesAsync();

        return MapToOfferResponse(offer);
    }

    public async Task<bool> DeleteOfferAsync(Guid id, Guid userId)
    {
        var offer = await _dbContext.Offers.FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null)
        {
            throw new KeyNotFoundException("Offer not found.");
        }

        if (offer.AuthorId != userId)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this offer.");
        }

        _dbContext.Offers.Remove(offer);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    // --- HELPER METHODS ---

    private static void ValidateOfferData(string title, string description, object category, object experience, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.");

        if (!Enum.IsDefined(category.GetType(), category))
            throw new ArgumentException("Invalid Category specified.");

        if (!Enum.IsDefined(experience.GetType(), experience))
            throw new ArgumentException("Invalid Experience level specified.");

        if (startDate < DateTime.UtcNow.AddMinutes(-1))
            throw new ArgumentException("Start date cannot be in the past.");

        if (endDate <= startDate)
            throw new ArgumentException("End date must be strictly after Start date.");
    }

    private static OfferResponse MapToOfferResponse(JobOffer offer)
    {
        return new OfferResponse
        {
            Id = offer.Id,
            Title = offer.Title,
            Description = offer.Description,
            Salary = offer.Salary,
            Category = offer.Category,
            Experience = offer.Experience,
            StartDate = offer.StartDate,
            EndDate = offer.EndDate,
            Company = offer.Company,
            CreatedOn = offer.CreatedOn,
            Stack = offer.Stack
        };
    }
}