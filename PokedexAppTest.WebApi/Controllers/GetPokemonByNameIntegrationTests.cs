using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.Common.Dto;
using PokedexAppTest.WebApi.TestData;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace PokedexAppTest.WebApi.Controllers;

[TestClass]
public class GetPokemonByNameIntegrationTests
{
    private const string BaseRequestUri = "/Pokedex/pokemon";
    private const string RequestUriForKnownPokemon = $"{BaseRequestUri}/pikachu";
    private const string RequestUriForMewtwo = $"{BaseRequestUri}/mewtwo";
    private const string RequestUriForCharizard = $"{BaseRequestUri}/CHARIZARD";
    private const string RequestUriForMissingno = $"{BaseRequestUri}/missingno";
    private const string RequestUriForInvalidPokemon = $"{BaseRequestUri}/invalidpokemon";
    private const string RequestUriForSpecialCharacters = $"{BaseRequestUri}/@#$%";
    private const string RequestUriForBulbasaur = $"{BaseRequestUri}/bulbasaur";
    
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public void Initialize()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task GetPokemon__WithValidName__ReturnsOkWithPokemonData()
    {
        var expectedPokemon = PokemonTestDataFactory.CreatePikachu();

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "pikachu"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForKnownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Name.ShouldBe("pikachu");
        pokemon.Description.ShouldBe("It has small electric sacs on both its cheeks.");
        pokemon.Habitat.ShouldBe("forest");
        pokemon.IsLegendary.ShouldBeFalse();
    }

    [TestMethod]
    public async Task GetPokemon__WithNonExistentPokemon__ReturnsNotFound()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Pokemon not found", null, HttpStatusCode.NotFound));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForInvalidPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetPokemon__WithLegendaryPokemon__ReturnsCorrectIsLegendaryFlag()
    {
        var legendaryPokemon = PokemonTestDataFactory.CreateMewtwo();

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "mewtwo"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(legendaryPokemon);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForMewtwo);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.IsLegendary.ShouldBeTrue();
        pokemon.Name.ShouldBe("mewtwo");
    }

    [TestMethod]
    public async Task GetPokemon__WithCaseInsensitiveName__ReturnsOk()
    {
        var expectedPokemon = PokemonTestDataFactory.CreateCharizard();

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForCharizard);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Name.ShouldBe("charizard");
    }

    [TestMethod]
    public async Task GetPokemon__WithEmptyDescription__ReturnsOkWithNullOrEmptyDescription()
    {
        var pokemonWithoutDescription = PokemonTestDataFactory.CreateMissingno();

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pokemonWithoutDescription);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForMissingno);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Description.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetPokemon__WhenMediatorThrowsException__ReturnsInternalServerError()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForKnownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetPokemon__WithSpecialCharactersInName__HandlesCorrectly()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Pokemon not found", null, HttpStatusCode.NotFound));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForSpecialCharacters);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task GetPokemon__WithMultipleConcurrentRequests__HandlesCorrectly()
    {
        var expectedPokemon = PokemonTestDataFactory.CreateBulbasaur();

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);
        });

        var client = factory.CreateClient();
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.GetAsync(RequestUriForBulbasaur))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
    }

    private WebApplicationFactory<Program> CreateFactoryWithMockedMediator(Action<Mock<IMediator>> configureMock)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    RemoveRealMediator(services);

                    var mockMediator = new Mock<IMediator>();
                    configureMock(mockMediator);

                    services.AddSingleton(mockMediator.Object);
                });
            });
    }

    private static void RemoveRealMediator(IServiceCollection services)
    {
        var mediatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
        if (mediatorDescriptor != null)
        {
            services.Remove(mediatorDescriptor);
        }
    }
}