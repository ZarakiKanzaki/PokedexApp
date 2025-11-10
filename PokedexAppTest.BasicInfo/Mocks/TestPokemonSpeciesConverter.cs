using PokedexApp.Common.Dto;
using PokedexApp.BasicInfo.Entities;
using PokedexApp.Common.Utilities;

namespace PokedexAppTest.BasicInfo.Mocks;

internal sealed class TestPokemonSpeciesConverter : IConverter<PokemonSpecies, Pokemon>
{
    public bool WasCalled { get; private set; }
    public PokemonSpecies? ReceivedSpecies { get; private set; }

    public Pokemon Convert(PokemonSpecies objectToConvert)
    {
        WasCalled = true;
        ReceivedSpecies = objectToConvert;
        return new Pokemon
        {
            Name = objectToConvert?.Name ?? "test",
            Description = "Test Description",
            Habitat = objectToConvert?.Habitat?.Name ?? string.Empty,
            IsLegendary = objectToConvert?.IsLegendary ?? false
        };
    }
}