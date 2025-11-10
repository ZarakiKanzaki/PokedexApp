using MediatR;
using PokedexApp.Common.Dto;
using PokedexApp.Translator.Dto;
using System.Text.Json;

namespace PokedexApp.Translator.Queries;

public class GetTranslatedDescriptionQuery(Pokemon pokemon): IRequest<string>
{
    public Pokemon Pokemon { get; } = pokemon;
}

public class GetTranslatedDescriptionQueryHandler(HttpClient httpClient) 
    : IRequestHandler<GetTranslatedDescriptionQuery, string>
{
    private readonly HttpClient _httpClient = httpClient;
    private const string ApiUrlTemplate = "https://api.funtranslations.com/translate/";
    private const string YodaTranslationEndpoint = "yoda.json";
    private const string ShakespeareTranslationEndpoint = "shakespeare.json";
    private const string CaveHabitat = "cave";

    public async Task<string> Handle(GetTranslatedDescriptionQuery request, CancellationToken cancellationToken)
    {
        var endpoint = ShouldUseYodaTranslation(request)
            ? $"{ApiUrlTemplate}{YodaTranslationEndpoint}"
            : $"{ApiUrlTemplate}{ShakespeareTranslationEndpoint}";

        var httpResponse = await _httpClient.GetAsync($"{endpoint}?text={request.Pokemon.Description}", cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            return request.Pokemon.Description;
        }

        try
        {
            var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var response = JsonSerializer.Deserialize<FunTranslator>(json);

            return response?.Contents?.Translated ?? request.Pokemon.Description;
        }
        catch (JsonException)
        {
            return request.Pokemon.Description;
        }
    }

    private static bool ShouldUseYodaTranslation(GetTranslatedDescriptionQuery request) 
        => IsCaveHabitat(request) || request.Pokemon.IsLegendary;

    private static bool IsCaveHabitat(GetTranslatedDescriptionQuery request) 
        => request.Pokemon.Habitat.Equals(CaveHabitat, StringComparison.OrdinalIgnoreCase);
}