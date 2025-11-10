using PokedexApp.Common.Dto;

namespace PokedexAppTest.WebApi.TestData;

public static class PokemonTestDataFactory
{
    public static Pokemon CreatePikachu() => new()
    {
        Name = "pikachu",
        Description = "It has small electric sacs on both its cheeks.",
        Habitat = "forest",
        IsLegendary = false
    };

    public static Pokemon CreateMewtwo() => new()
    {
        Name = "mewtwo",
        Description = "A Pokémon created by recombining Mew's genes.",
        Habitat = "rare",
        IsLegendary = true
    };

    public static Pokemon CreateCharizard() => new()
    {
        Name = "charizard",
        Description = "Spits fire that is hot enough to melt boulders.",
        Habitat = "mountain",
        IsLegendary = false
    };

    public static Pokemon CreateBulbasaur() => new()
    {
        Name = "bulbasaur",
        Description = "A strange seed was planted on its back at birth.",
        Habitat = "grassland",
        IsLegendary = false
    };

    public static Pokemon CreateMissingno() => new()
    {
        Name = "missingno",
        Description = string.Empty,
        Habitat = "unknown",
        IsLegendary = false
    };

    public static Pokemon CreatePokemonWithEmptyHabitat() => new()
    {
        Name = "unknown",
        Description = "A mysterious Pokemon",
        Habitat = string.Empty,
        IsLegendary = false
    };

    public static Pokemon CreateCustomPokemon(string name, string description, string habitat, bool isLegendary) => new()
    {
        Name = name,
        Description = description,
        Habitat = habitat,
        IsLegendary = isLegendary
    };
}