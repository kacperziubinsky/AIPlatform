namespace Offer.DTOs;

public record DictionaryItem(int Value, string Name, string Label);

public record OfferDictionariesResponse(List<DictionaryItem> Categories, List<DictionaryItem> Experiences);
