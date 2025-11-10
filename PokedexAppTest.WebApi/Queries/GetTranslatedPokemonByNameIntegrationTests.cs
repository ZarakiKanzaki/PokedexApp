using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.Common.Dto;
using PokedexApp.Translator.Queries;
using PokedexAppTest.WebApi.TestData;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace PokedexAppTest.WebApi.Queries;

[TestClass]
public class GetTranslatedPokemonByNameIntegrationTests
{
    private const string BaseRequestUri = "/pokemon/translated";
    private const string RequestUriForPikachu = $"{BaseRequestUri}/pikachu";
    private const string RequestUriForMewtwo = $"{BaseRequestUri}/mewtwo";
    private const string RequestUriForZubat = $"{BaseRequestUri}/zubat";
    private const string RequestUriForCharizard = $"{BaseRequestUri}/CHARIZARD";
    private const string RequestUriForInvalidPokemon = $"{BaseRequestUri}/invalidpokemon";
    private const string RequestUriForMissingno = $"{BaseRequestUri}/missingno";

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
    public async Task GetTranslatedPokemon__WithValidName__ReturnsOkWithTranslatedDescription()
    {
        var originalPokemon = PokemonTestDataFactory.CreatePikachu();
        var translatedDescription = "In the forest, small electric sacs it has.";

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "pikachu"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(translatedDescription);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForPikachu);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Name.ShouldBe("pikachu");
        pokemon.Description.ShouldBe(translatedDescription);
        pokemon.Habitat.ShouldBe("forest");
        pokemon.IsLegendary.ShouldBeFalse();
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WithCaveHabitat__ReturnsYodaTranslation()
    {
        var cavePokemon = PokemonTestDataFactory.CreateCustomPokemon(
            "zubat",
            "Forms colonies in perpetually dark places.",
            "cave",
            false);
        var yodaTranslation = "In perpetually dark places, colonies forms.";

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "zubat"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(cavePokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yodaTranslation);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForZubat);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Description.ShouldBe(yodaTranslation);
        pokemon.Habitat.ShouldBe("cave");
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WithLegendaryPokemon__ReturnsYodaTranslation()
    {
        var legendaryPokemon = PokemonTestDataFactory.CreateMewtwo();
        var yodaTranslation = "By recombining mew's genes, created a pokemon was.";

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "mewtwo"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(legendaryPokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(yodaTranslation);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForMewtwo);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.IsLegendary.ShouldBeTrue();
        pokemon.Description.ShouldBe(yodaTranslation);
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WithNonExistentPokemon__ReturnsNotFound()
    {
        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
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
    public async Task GetTranslatedPokemon__WithCaseInsensitiveName__ReturnsOk()
    {
        var expectedPokemon = PokemonTestDataFactory.CreateCharizard();
        var translatedDescription = "Spits fire yond is hot enow to melt boulders.";

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(translatedDescription);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForCharizard);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Name.ShouldBe("charizard");
        pokemon.Description.ShouldBe(translatedDescription);
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WithEmptyDescription__ReturnsEmptyTranslatedDescription()
    {
        var pokemonWithEmptyDescription = PokemonTestDataFactory.CreateMissingno();

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(pokemonWithEmptyDescription);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(string.Empty);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForMissingno);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Description.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WhenTranslationFails__ReturnsOriginalDescription()
    {
        var originalPokemon = PokemonTestDataFactory.CreatePikachu();

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.Is<GetPokemonByNameQuery>(q => q.Name == "pikachu"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(originalPokemon.Description);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForPikachu);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var pokemon = await response.Content.ReadFromJsonAsync<Pokemon>();
        pokemon.ShouldNotBeNull();
        pokemon.Description.ShouldBe(originalPokemon.Description);
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WhenMediatorThrowsException__ReturnsInternalServerError()
    {
        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForPikachu);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetTranslatedPokemon__WithMultipleConcurrentRequests__HandlesCorrectly()
    {
        var expectedPokemon = PokemonTestDataFactory.CreatePikachu();
        var translatedDescription = "Translated text";

        var factory = CreateFactoryWithMockedMediator((mediator, pokemon, translation) =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);

            mediator
                .Setup(m => m.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(translatedDescription);
        });

        var client = factory.CreateClient();
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.GetAsync(RequestUriForPikachu))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
    }

    private WebApplicationFactory<Program> CreateFactoryWithMockedMediator(
        Action<Mock<IMediator>, Pokemon?, string?> configureMock)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    RemoveRealMediator(services);

                    var mockMediator = new Mock<IMediator>();
                    configureMock(mockMediator, null, null);

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