using Microsoft.AspNetCore.Mvc;

namespace Offer.Controllers;

[ApiController]
[Route("[controller]")]
public class OfferController : ControllerBase
{
    
    [HttpGet]
    public IActionResult Get()
    {
        
    }
}