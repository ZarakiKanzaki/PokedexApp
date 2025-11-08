using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PokedexApp.BasicInfo.Entities;

public class PokemonSpecies
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("is_legendary")]
    public bool IsLegendary { get; set; }
    
    [JsonPropertyName("habitat")]
    public Habitat Habitat { get; set; }
    
    [JsonPropertyName("flavor_text_entries")]
    public ICollection<FlavorTextEntry> Flavor_Text_Entries { get; set; } = [];
}

public class Habitat
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class FlavorTextEntry
{
    [JsonPropertyName("flavor_text")]
    public string Flavor_Text { get; set; }
}

