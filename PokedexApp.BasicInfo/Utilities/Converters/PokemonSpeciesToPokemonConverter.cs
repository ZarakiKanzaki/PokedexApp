using PokedexApp.BasicInfo.Entities;
using PokedexApp.Common.Dto;
using PokedexApp.Common.Utilities;

namespace PokedexApp.BasicInfo.Utilities.Converters;

public sealed class PokemonSpeciesToPokemonConverter : BaseConverterWithValidation<PokemonSpecies, Pokemon, PokemonSpeciesToPokemonConverter>
{
    private const string EnglishLanguageCode = "en";

    protected override Pokemon GetConvertedObject(PokemonSpecies objectToConvert)
        => new()
        {
            Name = objectToConvert.Name,
            Description = FormatDescription(objectToConvert) ?? string.Empty,
            IsLegendary = objectToConvert.IsLegendary,
            Habitat = objectToConvert.Habitat?.Name ?? string.Empty,
        };

    private static string? FormatDescription(PokemonSpecies objectToConvert) 
        => objectToConvert.FlavorTextEntries?
                        .FirstOrDefault(entry => entry.Language?.Name == EnglishLanguageCode)
                        ?.FlavorText?.Replace("\n", " ").Replace("\f", " ");

    protected override bool IsObjectInvalid(PokemonSpecies objectToValidate)
        => objectToValidate == null || string.IsNullOrWhiteSpace(objectToValidate.Name);
}