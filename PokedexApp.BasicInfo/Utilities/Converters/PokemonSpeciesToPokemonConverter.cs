using PokedexApp.BasicInfo.Dto;
using PokedexApp.BasicInfo.Entities;

namespace PokedexApp.BasicInfo.Utilities.Converters;

public sealed class PokemonSpeciesToPokemonConverter : BaseConverterWithValidation<PokemonSpecies, Pokemon, PokemonSpeciesToPokemonConverter>
{
    protected override Pokemon GetConvertedObject(PokemonSpecies objectToConvert)
        => new()
        {
            Name = objectToConvert.Name,
            Description = FormatDescription(objectToConvert) ?? string.Empty,
            IsLegendary = objectToConvert.IsLegendary,
            Habitat = objectToConvert.Habitat?.Name,
        };

    private static string? FormatDescription(PokemonSpecies objectToConvert) 
        => objectToConvert.FlavorTextEntries?
                        .FirstOrDefault()?.FlavorText?.Replace("\n", " ").Replace("\f", " ");

    protected override bool IsObjectInvalid(PokemonSpecies objectToValidate)
        => objectToValidate == null || string.IsNullOrWhiteSpace(objectToValidate.Name);
}