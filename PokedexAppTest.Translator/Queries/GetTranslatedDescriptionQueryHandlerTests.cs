using Moq;
using Moq.Protected;
using PokedexApp.Common.Dto;
using PokedexApp.Translator.Queries;
using Shouldly;
using System.Net;
using System.Text.Json;

namespace PokedexAppTest.Translator.Queries;

[TestClass]
public class GetTranslatedDescriptionQueryHandlerTests
{
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;
    private GetTranslatedDescriptionQueryHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _handler = new GetTranslatedDescriptionQueryHandler(_httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _httpClient.Dispose();
    }

    [TestMethod]
    public async Task Handle__WithCaveHabitat__UsesYodaTranslation()
    {
        var pokemon = new Pokemon
        {
            Name = "zubat",
            Description = "Forms colonies in perpetually dark places.",
            Habitat = "cave",
            IsLegendary = false
        };
        var translatedText = "In perpetually dark places, colonies forms.";
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = translatedText,
                text = pokemon.Description,
                translation = "yoda"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(translatedText);
    }

    [TestMethod]
    public async Task Handle__WithNonCaveHabitat__UsesShakespeareTranslation()
    {
        var pokemon = new Pokemon
        {
            Name = "pikachu",
            Description = "When several of these Pokémon gather, their electricity could build and cause lightning storms.",
            Habitat = "forest",
            IsLegendary = false
        };
        var translatedText = "When several of these pokémon gather,  their electricity couldst buildeth and cause lightning storms.";
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = translatedText,
                text = pokemon.Description,
                translation = "shakespeare"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(translatedText);
    }

    [TestMethod]
    public async Task Handle__WithLegendaryPokemon__UsesYodaTranslation()
    {
        var pokemon = new Pokemon
        {
            Name = "mewtwo",
            Description = "It was created by a scientist after years of horrific gene splicing and DNA engineering experiments.",
            Habitat = "rare",
            IsLegendary = true
        };
        var translatedText = "Created by a scientist after years of horrific gene splicing and dna engineering experiments, it was.";
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = translatedText,
                text = pokemon.Description,
                translation = "yoda"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(translatedText);
    }

    [TestMethod]
    public async Task Handle__WithCaseInsensitiveCaveHabitat__UsesYodaTranslation()
    {
        var pokemon = new Pokemon
        {
            Name = "geodude",
            Description = "Found in fields and mountains.",
            Habitat = "CAVE",
            IsLegendary = false
        };
        var translatedText = "In fields and mountains, found.";
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = translatedText,
                text = pokemon.Description,
                translation = "yoda"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(translatedText);
    }

    [TestMethod]
    public async Task Handle__WhenApiReturnsNotFound__ReturnsOriginalDescription()
    {
        var pokemon = new Pokemon
        {
            Name = "pikachu",
            Description = "When several of these Pokémon gather, their electricity could build and cause lightning storms.",
            Habitat = "forest",
            IsLegendary = false
        };

        SetupHttpResponse(HttpStatusCode.NotFound, null);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(pokemon.Description);
    }

    [TestMethod]
    public async Task Handle__WhenApiReturnsInternalServerError__ReturnsOriginalDescription()
    {
        var pokemon = new Pokemon
        {
            Name = "charizard",
            Description = "Spits fire that is hot enough to melt boulders.",
            Habitat = "mountain",
            IsLegendary = false
        };

        SetupHttpResponse(HttpStatusCode.InternalServerError, null);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(pokemon.Description);
    }

    [TestMethod]
    public async Task Handle__WhenApiReturnsBadRequest__ReturnsOriginalDescription()
    {
        var pokemon = new Pokemon
        {
            Name = "bulbasaur",
            Description = "A strange seed was planted on its back at birth.",
            Habitat = "grassland",
            IsLegendary = false
        };

        SetupHttpResponse(HttpStatusCode.BadRequest, null);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(pokemon.Description);
    }

    [TestMethod]
    public async Task Handle__WhenResponseContentIsNull__ReturnsOriginalDescription()
    {
        var pokemon = new Pokemon
        {
            Name = "squirtle",
            Description = "After birth, its back swells and hardens into a shell.",
            Habitat = "waters-edge",
            IsLegendary = false
        };
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = (string?)null,
                text = pokemon.Description,
                translation = "shakespeare"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(pokemon.Description);
    }

    [TestMethod]
    public async Task Handle__WhenResponseIsInvalidJson__ReturnsOriginalDescription()
    {
        var pokemon = new Pokemon
        {
            Name = "charmander",
            Description = "Obviously prefers hot places.",
            Habitat = "mountain",
            IsLegendary = false
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json")
            });

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(pokemon.Description);
    }

    [TestMethod]
    public async Task Handle__WithEmptyDescription__SendsEmptyTextToApi()
    {
        var pokemon = new Pokemon
        {
            Name = "missingno",
            Description = string.Empty,
            Habitat = "unknown",
            IsLegendary = false
        };
        var funTranslatorResponse = new
        {
            success = new { total = 1 },
            contents = new
            {
                translated = string.Empty,
                text = string.Empty,
                translation = "shakespeare"
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, funTranslatorResponse);

        var query = new GetTranslatedDescriptionQuery(pokemon);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(string.Empty);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? responseContent)
    {
        var responseMessage = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = responseContent != null
                ? new StringContent(JsonSerializer.Serialize(responseContent))
                : new StringContent(string.Empty)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);
    }

    private void VerifyHttpRequest(string expectedUrl, Times times)
    {
        _mockHttpMessageHandler
            .Protected()
            .Verify(
                "SendAsync",
                times,
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.GetLeftPart(UriPartial.Path).Equals(expectedUrl, StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>());
    }
}