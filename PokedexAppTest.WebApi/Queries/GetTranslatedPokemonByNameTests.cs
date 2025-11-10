using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokedexApp.Common.Dto;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.Translator.Queries;
using PokedexApp.WebApi.Controllers;
using PokedexAppTest.WebApi.TestData;
using Shouldly;
using System.Net;

namespace PokedexAppTest.WebApi.Queries;

[TestClass]
public class GetTranslatedPokemonByNameTests
{
    private Mock<ILogger<PokedexController>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;
    private PokedexController _controller = null!;

    private const string ValidPokemonName = "pikachu";
    private const string CavePokemonName = "zubat";
    private const string LegendaryPokemonName = "mewtwo";

    [TestInitialize]
    public void Initialize()
    {
        _loggerMock = new Mock<ILogger<PokedexController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new PokedexController(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__ValidName__ReturnsOkWithTranslatedDescription()
    {
        var originalPokemon = PokemonTestDataFactory.CreatePikachu();
        var translatedDescription = "In the forest, small electric sacs it has.";
        
        SetupMediatorForPokemon(originalPokemon);
        SetupMediatorForTranslation(translatedDescription);

        var result = await _controller.GetTranslatedPokemonByName(ValidPokemonName);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Name.ShouldBe(originalPokemon.Name);
        pokemon.Description.ShouldBe(translatedDescription);
        pokemon.Habitat.ShouldBe(originalPokemon.Habitat);
        pokemon.IsLegendary.ShouldBe(originalPokemon.IsLegendary);
        
        VerifyPokemonQueryWasCalled(Times.Once());
        VerifyTranslationQueryWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__CaveHabitat__UsesYodaTranslation()
    {
        var cavePokemon = PokemonTestDataFactory.CreateCustomPokemon(
            CavePokemonName, 
            "Forms colonies in perpetually dark places.", 
            "cave", 
            false);
        var yodaTranslation = "In perpetually dark places, colonies forms.";
        
        SetupMediatorForPokemon(cavePokemon);
        SetupMediatorForTranslation(yodaTranslation);

        var result = await _controller.GetTranslatedPokemonByName(CavePokemonName);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Description.ShouldBe(yodaTranslation);
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__LegendaryPokemon__UsesYodaTranslation()
    {
        var legendaryPokemon = PokemonTestDataFactory.CreateMewtwo();
        var yodaTranslation = "By recombining mew's genes, created a pokemon was.";
        
        SetupMediatorForPokemon(legendaryPokemon);
        SetupMediatorForTranslation(yodaTranslation);

        var result = await _controller.GetTranslatedPokemonByName(LegendaryPokemonName);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.IsLegendary.ShouldBeTrue();
        pokemon.Description.ShouldBe(yodaTranslation);
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__NonCaveNonLegendary__UsesShakespeareTranslation()
    {
        var pokemon = PokemonTestDataFactory.CreateCharizard();
        var shakespeareTranslation = "Spits fire yond is hot enow to melt boulders.";
        
        SetupMediatorForPokemon(pokemon);
        SetupMediatorForTranslation(shakespeareTranslation);

        var result = await _controller.GetTranslatedPokemonByName("charizard");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var resultPokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        resultPokemon.Description.ShouldBe(shakespeareTranslation);
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__TranslationFails__ReturnsOriginalDescription()
    {
        var originalPokemon = PokemonTestDataFactory.CreatePikachu();
        
        SetupMediatorForPokemon(originalPokemon);
        SetupMediatorForTranslation(originalPokemon.Description);

        var result = await _controller.GetTranslatedPokemonByName(ValidPokemonName);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Description.ShouldBe(originalPokemon.Description);
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__PokemonNotFound__ThrowsHttpRequestException()
    {
        var exception = new HttpRequestException(
            "Failed to retrieve Pokemon 'nonexistent'. Status code: NotFound",
            null,
            HttpStatusCode.NotFound);
        
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Should.ThrowAsync<HttpRequestException>(
            async () => await _controller.GetTranslatedPokemonByName("nonexistent"));
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__EmptyDescription__ReturnsEmptyTranslatedDescription()
    {
        var pokemonWithEmptyDescription = PokemonTestDataFactory.CreateMissingno();
        
        SetupMediatorForPokemon(pokemonWithEmptyDescription);
        SetupMediatorForTranslation(string.Empty);

        var result = await _controller.GetTranslatedPokemonByName("missingno");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Description.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__CaseInsensitiveName__ReturnsOk()
    {
        var expectedPokemon = PokemonTestDataFactory.CreatePikachu();
        var translatedDescription = "Electric sacs small on both its cheeks, it has.";
        
        SetupMediatorForPokemon(expectedPokemon);
        SetupMediatorForTranslation(translatedDescription);

        var result = await _controller.GetTranslatedPokemonByName("PIKACHU");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBeOfType<Pokemon>();
        VerifyPokemonQueryWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__ValidName__CallsBothQueries()
    {
        var originalPokemon = PokemonTestDataFactory.CreatePikachu();
        var translatedDescription = "Translated description";
        
        SetupMediatorForPokemon(originalPokemon);
        SetupMediatorForTranslation(translatedDescription);

        await _controller.GetTranslatedPokemonByName(ValidPokemonName);

        VerifyPokemonQueryWasCalled(Times.Once());
        VerifyTranslationQueryWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetTranslatedPokemonByName__MultipleRequests__EachCallsMediatorTwice()
    {
        var pokemon1 = PokemonTestDataFactory.CreatePikachu();
        var pokemon2 = PokemonTestDataFactory.CreateCharizard();
        var pokemon3 = PokemonTestDataFactory.CreateMewtwo();
        
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IRequest<Pokemon> request, CancellationToken _) =>
            {
                var query = request as GetPokemonByNameQuery;
                return query?.Name switch
                {
                    "pikachu" => pokemon1,
                    "charizard" => pokemon2,
                    "mewtwo" => pokemon3,
                    _ => pokemon1
                };
            });

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Translated");

        await _controller.GetTranslatedPokemonByName("pikachu");
        await _controller.GetTranslatedPokemonByName("charizard");
        await _controller.GetTranslatedPokemonByName("mewtwo");

        VerifyPokemonQueryWasCalled(Times.Exactly(3));
        VerifyTranslationQueryWasCalled(Times.Exactly(3));
    }

    private void SetupMediatorForPokemon(Pokemon pokemon)
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pokemon);
    }

    private void SetupMediatorForTranslation(string translatedDescription)
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(translatedDescription);
    }

    private void VerifyPokemonQueryWasCalled(Times times)
    {
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()),
            times);
    }

    private void VerifyTranslationQueryWasCalled(Times times)
    {
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetTranslatedDescriptionQuery>(), It.IsAny<CancellationToken>()),
            times);
    }
}