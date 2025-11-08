using System.Net;
using System.Text.Json;
using PokedexApp.BasicInfo.Dto;
using PokedexApp.BasicInfo.Entities;
using PokedexApp.BasicInfo.Queries;
using PokedexAppTest.BasicInfo.Mocks;
using Shouldly;

namespace PokedexAppTest.BasicInfo;

[TestClass]
public class GetPokemonByNameQueryHandlerTests
{
    private const string ValidPokemonName = "pikachu";
    private const string ApiUrlTemplate = "https://pokeapi.co/api/v2/pokemon/";
    private const string NonExistentPokemonName = "nonexistent";
    private const string UpperCasePokemonName = "PIKACHU";

    [TestMethod]
    public async Task Handle__ValidPokemonName__ReturnsPokemon()
    {
        var expectedSpecies = CreateValidPokemonSpecies();
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.OK, expectedSpecies, out var converter);
        var query = new GetPokemonByNameQuery(ValidPokemonName);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(expectedSpecies.Name);
    }

    [TestMethod]
    public async Task Handle__ValidPokemonNameWithUpperCase__ConvertsToLowerCaseInUrl()
    {
        var expectedSpecies = CreateValidPokemonSpecies();
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.OK, expectedSpecies, out _);
        var query = new GetPokemonByNameQuery(UpperCasePokemonName);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
    }

    [TestMethod]
    public async Task Handle__PokemonNotFound__ThrowsHttpRequestException()
    {
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.NotFound, null, out _);
        var query = new GetPokemonByNameQuery(NonExistentPokemonName);

        var exception = await Should.ThrowAsync<HttpRequestException>(
            async () => await handler.Handle(query, CancellationToken.None));

        AssertExceptionContainsPokemonName(exception, NonExistentPokemonName);
        exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task Handle__ServerError__ThrowsHttpRequestException()
    {
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.InternalServerError, null, out _);
        var query = new GetPokemonByNameQuery(ValidPokemonName);

        var exception = await Should.ThrowAsync<HttpRequestException>(
            async () => await handler.Handle(query, CancellationToken.None));

        exception.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task Handle__BadRequest__ThrowsHttpRequestException()
    {
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.BadRequest, null, out _);
        var query = new GetPokemonByNameQuery(ValidPokemonName);

        var exception = await Should.ThrowAsync<HttpRequestException>(
            async () => await handler.Handle(query, CancellationToken.None));

        exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestMethod]
    public async Task Handle__ValidResponse__CallsConverter()
    {
        var expectedSpecies = CreateValidPokemonSpecies();
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.OK, expectedSpecies, out var converter);
        var query = new GetPokemonByNameQuery(ValidPokemonName);

        await handler.Handle(query, CancellationToken.None);

        AssertConverterWasCalledWithCorrectSpecies(converter, expectedSpecies.Name);
    }

    [TestMethod]
    public async Task Handle__EmptyResponseBody__DeserializesAsNull()
    {
        var httpClient = CreateHttpClientWithEmptyResponse(HttpStatusCode.OK);
        var converter = new TestPokemonSpeciesConverter();
        var handler = new GetPokemonByNameQueryHandler(httpClient, converter);
        var query = new GetPokemonByNameQuery(ValidPokemonName);

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        converter.WasCalled.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Handle__CancellationRequested__ThrowsTaskCanceledException()
    {
        var expectedSpecies = CreateValidPokemonSpecies();
        var handler = CreateHandlerWithMockResponse(HttpStatusCode.OK, expectedSpecies, out _);
        var query = new GetPokemonByNameQuery(ValidPokemonName);
        var cts = CreateCancelledCancellationTokenSource();

        await Should.ThrowAsync<TaskCanceledException>(
            async () => await handler.Handle(query, cts.Token));
    }

    private static GetPokemonByNameQueryHandler CreateHandlerWithMockResponse(
        HttpStatusCode statusCode,
        PokemonSpecies? species,
        out TestPokemonSpeciesConverter converter)
    {
        var httpClient = CreateHttpClientWithMockResponse(statusCode, species);
        converter = new TestPokemonSpeciesConverter();
        return new GetPokemonByNameQueryHandler(httpClient, converter);
    }

    private static PokemonSpecies CreateValidPokemonSpecies() =>
        new()
        {
            Name = ValidPokemonName,
            IsLegendary = false,
            Habitat = new Habitat { Name = "forest", Url = "https://pokeapi.co/api/v2/habitat/2/" },
            Flavor_Text_Entries =
            [
                new FlavorTextEntry
                {
                    Flavor_Text = "When several of these Pokémon gather, their electricity could build and cause lightning storms."
                }
            ]
        };

    private static HttpClient CreateHttpClientWithMockResponse(HttpStatusCode statusCode, PokemonSpecies? species)
    {
        var content = species is not null ? JsonSerializer.Serialize(species) : string.Empty;
        var mockHandler = new MockHttpMessageHandler(statusCode, content);
        return new HttpClient(mockHandler) { BaseAddress = new Uri(ApiUrlTemplate) };
    }

    private static HttpClient CreateHttpClientWithEmptyResponse(HttpStatusCode statusCode)
    {
        var mockHandler = new MockHttpMessageHandler(statusCode, "{}");
        return new HttpClient(mockHandler) { BaseAddress = new Uri(ApiUrlTemplate) };
    }

    private static CancellationTokenSource CreateCancelledCancellationTokenSource()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        return cts;
    }

    private static void AssertExceptionContainsPokemonName(HttpRequestException exception, string pokemonName)
    {
        exception.Message.ShouldContain("Failed to retrieve Pokemon");
        exception.Message.ShouldContain(pokemonName);
    }

    private static void AssertConverterWasCalledWithCorrectSpecies(TestPokemonSpeciesConverter converter, string expectedName)
    {
        converter.WasCalled.ShouldBeTrue();
        converter.ReceivedSpecies.ShouldNotBeNull();
        converter.ReceivedSpecies!.Name.ShouldBe(expectedName);
    }
}