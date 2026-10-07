using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Offer.DTOs;
using Offer.Services;

namespace Offer.Controllers;

[ApiController]
[Route("offers")]
public class OfferController : ControllerBase
{
    private readonly IOfferService _offerService;

    public OfferController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OfferResponse>> CreateOffer([FromBody] CreateOfferRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized("Invalid user token claim.");
        }

        var offer = await _offerService.CreateOfferAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetOfferById), new { id = offer.Id }, offer);
    }

    [HttpGet]
    public async Task<ActionResult<List<OfferResponse>>> GetOffers([FromQuery] OfferFilterRequest? filter)
    {
        var offers = await _offerService.GetOffersAsync(filter);
        return Ok(offers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OfferResponse>> GetOfferById(Guid id)
    {
        var offer = await _offerService.GetOfferByIdAsync(id);
        if (offer == null)
        {
            return NotFound("Offer not found.");
        }

        return Ok(offer);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<OfferResponse>> UpdateOffer(Guid id, [FromBody] UpdateOfferRequest request)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized("Invalid user token claim.");
        }

        try
        {
            var updatedOffer = await _offerService.UpdateOfferAsync(id, request, userId.Value);
            return Ok(updatedOffer);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteOffer(Guid id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized("Invalid user token claim.");
        }

        try
        {
            await _offerService.DeleteOfferAsync(id, userId.Value);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
        }
    }

    private Guid? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                       ?? User.FindFirstValue("sub");

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}