using System.Text.Json.Serialization;

namespace PokedexApp.BasicInfo.Dto;

public class Pokemon
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("habitat")]
    public string Habitat { get; set; }

    [JsonPropertyName("islegendary")]
    public bool IsLegendary { get; set; }
}
