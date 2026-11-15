using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Offer.DTOs;
using Offer.Services;

namespace Offer.Tests;

public class OfferControllerTests
{
    private readonly Mock<IOfferService> _service = new();

    private Controllers.OfferController CreateOfferController(Guid? userId = null)
    {
        var controller = new Controllers.OfferController(_service.Object);
        var claims = new List<Claim>();

        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };

        return controller;
    }


    [Fact]
    public async Task CreateOffer_Valid_Returns201()
    {
        var userId = Guid.NewGuid();
        var response = TestData.Response(authorId: userId);
        _service.Setup(s => s.CreateOfferAsync(It.IsAny<CreateOfferRequest>(), userId))
                .ReturnsAsync(response);

        var result = await CreateOfferController(userId).CreateOffer(TestData.CreateRequest());

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(Controllers.OfferController.GetOfferById), created.ActionName);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task CreateOffer_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateOfferController(userId: null).CreateOffer(TestData.CreateRequest());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateOffer_InvalidData_Returns400()
    {
        _service.Setup(s => s.CreateOfferAsync(It.IsAny<CreateOfferRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException("Title cannot be empty."));

        var result = await CreateOfferController(Guid.NewGuid()).CreateOffer(TestData.CreateRequest());

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Title cannot be empty.", bad.Value);
    }
    
    [Fact]
    public async Task GetMyOffers_Valid_Returns200WithOffers()
    {
        var userId = Guid.NewGuid();
        var offers = new List<OfferResponse> { TestData.Response(authorId: userId) };
        _service.Setup(s => s.GetOffersByAuthorAsync(userId)).ReturnsAsync(offers);

        var result = await CreateOfferController(userId).GetMyOffers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(offers, ok.Value);
    }

    [Fact]
    public async Task GetMyOffers_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateOfferController(userId: null).GetMyOffers();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }
    
    [Fact]
    public async Task GetOffers_ReturnsOk()
    {
        var offers = new List<OfferResponse> { TestData.Response() };
        _service.Setup(s => s.GetOffersAsync(null)).ReturnsAsync(offers);

        var result = await CreateOfferController().GetOffers(null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(offers, ok.Value);
    }
    
    [Fact]
    public async Task GetOfferById_Existing_Returns200()
    {
        var id = Guid.NewGuid();
        var response = TestData.Response(id);
        _service.Setup(s => s.GetOfferByIdAsync(id)).ReturnsAsync(response);

        var result = await CreateOfferController().GetOfferById(id);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task GetOfferById_Missing_Returns404()
    {
        _service.Setup(s => s.GetOfferByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((OfferResponse?)null);

        var result = await CreateOfferController().GetOfferById(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
    
    [Fact]
    public async Task UpdateOffer_Valid_Returns200()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var response = TestData.Response(id, userId);
        _service.Setup(s => s.UpdateOfferAsync(id, It.IsAny<UpdateOfferRequest>(), userId))
                .ReturnsAsync(response);

        var result = await CreateOfferController(userId).UpdateOffer(id, TestData.UpdateRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(response, ok.Value);
    }

    [Fact]
    public async Task UpdateOffer_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateOfferController(userId: null)
            .UpdateOffer(Guid.NewGuid(), TestData.UpdateRequest());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOffer_NotFound_Returns404()
    {
        _service.Setup(s => s.UpdateOfferAsync(It.IsAny<Guid>(), It.IsAny<UpdateOfferRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new KeyNotFoundException("Offer not found."));

        var result = await CreateOfferController(Guid.NewGuid())
            .UpdateOffer(Guid.NewGuid(), TestData.UpdateRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOffer_Forbidden_Returns403()
    {
        _service.Setup(s => s.UpdateOfferAsync(It.IsAny<Guid>(), It.IsAny<UpdateOfferRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new UnauthorizedAccessException("no"));

        var result = await CreateOfferController(Guid.NewGuid())
            .UpdateOffer(Guid.NewGuid(), TestData.UpdateRequest());

        var obj = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, obj.StatusCode);
    }

    [Fact]
    public async Task UpdateOffer_InvalidData_Returns400()
    {
        _service.Setup(s => s.UpdateOfferAsync(It.IsAny<Guid>(), It.IsAny<UpdateOfferRequest>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException("bad"));

        var result = await CreateOfferController(Guid.NewGuid())
            .UpdateOffer(Guid.NewGuid(), TestData.UpdateRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
    
    [Fact]
    public async Task DeleteOffer_Valid_Returns204()
    {
        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        _service.Setup(s => s.DeleteOfferAsync(id, userId)).ReturnsAsync(true);

        var result = await CreateOfferController(userId).DeleteOffer(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteOffer_NoUserClaim_ReturnsUnauthorized()
    {
        var result = await CreateOfferController(userId: null).DeleteOffer(Guid.NewGuid());

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task DeleteOffer_NotFound_Returns404()
    {
        _service.Setup(s => s.DeleteOfferAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new KeyNotFoundException("Offer not found."));

        var result = await CreateOfferController(Guid.NewGuid()).DeleteOffer(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteOffer_Forbidden_Returns403()
    {
        _service.Setup(s => s.DeleteOfferAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ThrowsAsync(new UnauthorizedAccessException("no"));

        var result = await CreateOfferController(Guid.NewGuid()).DeleteOffer(Guid.NewGuid());

        var obj = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, obj.StatusCode);
    }
}