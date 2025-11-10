using System.Text.Json;
using MediatR;
using PokedexApp.Common.Dto;
using PokedexApp.BasicInfo.Entities;
using PokedexApp.Common.Utilities;

namespace PokedexApp.BasicInfo.Queries;

public class GetPokemonByNameQuery(string name) : IRequest<Pokemon>
{
    public string Name { get; } = name;
}

public class GetPokemonByNameQueryHandler(HttpClient httpClient,
                                    IConverter<PokemonSpecies, Pokemon> pokemonSpeciesToPokemonConverter) 
    : IRequestHandler<GetPokemonByNameQuery, Pokemon>
{
    private const string ApiUrlTemplate = "https://pokeapi.co/api/v2/pokemon-species/";
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConverter<PokemonSpecies, Pokemon> converter = pokemonSpeciesToPokemonConverter;

    public async Task<Pokemon> Handle(GetPokemonByNameQuery request, CancellationToken cancellationToken)
    {
        var apiUrl = $"{ApiUrlTemplate}{request.Name.ToLower()}";
        var httpResponse = await _httpClient.GetAsync(apiUrl, cancellationToken);
        
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Failed to retrieve Pokemon '{request.Name}'. Status code: {httpResponse.StatusCode}",
                null,
                httpResponse.StatusCode);
        }
        
        var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonSerializer.Deserialize<PokemonSpecies>(json);

        return converter.Convert(response);
    }
}