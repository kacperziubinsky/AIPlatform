using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Offer.DTOs;
using Offer.Models.Enums;

namespace Offer.Controllers;

[ApiController]
[Route("offers/dictionaries")]
public class DictionaryController : ControllerBase
{
    private static readonly OfferDictionariesResponse Dictionaries = new(
        ToItems<Category>(),
        ToItems<Experience>());

    [HttpGet]
    [ResponseCache(Duration = 3600)]
    public ActionResult<OfferDictionariesResponse> Get() => Ok(Dictionaries);

    private static List<DictionaryItem> ToItems<TEnum>() where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(value =>
            {
                var name = value.ToString();
                var label = typeof(TEnum).GetField(name)?.GetCustomAttribute<DisplayAttribute>()?.Name ?? name;
                return new DictionaryItem(Convert.ToInt32(value), name, label);
            })
            .ToList();
}
