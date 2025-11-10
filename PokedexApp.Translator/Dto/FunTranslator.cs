using System.Text.Json.Serialization;

namespace PokedexApp.Translator.Dto;

public class FunTranslator
{
    [JsonPropertyName("success")]
    public Success Success { get; set; } = null!;

    [JsonPropertyName("contents")]
    public Contents Contents { get; set; } = null!;
}

public class Success
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class Contents
{
    [JsonPropertyName("translated")]
    public string Translated { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("translation")]
    public string Translation { get; set; } = string.Empty;
}