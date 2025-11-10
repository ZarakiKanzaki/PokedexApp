using System.Text.Json.Serialization;

namespace PokedexApp.BasicInfo.Entities;

public record PokemonSpecies(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("is_legendary")] bool IsLegendary,
    [property: JsonPropertyName("habitat")] Habitat Habitat,
    [property: JsonPropertyName("flavor_text_entries")] ICollection<FlavorTextEntry> FlavorTextEntries
)
{
    public PokemonSpecies() : this(default!, default, default!, default!)
    {
    }
}

public record Habitat(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);

public record FlavorTextEntry(
    [property: JsonPropertyName("flavor_text")] string FlavorText,
    [property: JsonPropertyName("language")] Language Language
);

public record Language(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);

