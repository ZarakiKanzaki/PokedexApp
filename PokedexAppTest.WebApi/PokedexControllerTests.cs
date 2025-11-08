using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PokedexApp.BasicInfo.Dto;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.WebApi.Controllers;
using Shouldly;
using System.Net;

namespace PokedexAppTest.WebApi;

[TestClass]
public class PokedexControllerTests
{
    private Mock<ILogger<PokedexController>> _loggerMock = null!;
    private Mock<IMediator> _mediatorMock = null!;
    private PokedexController _controller = null!;

    private const string ValidPokemonName = "pikachu";
    private const string NonExistentPokemonName = "nonexistent";

    [TestInitialize]
    public void Initialize()
    {
        _loggerMock = new Mock<ILogger<PokedexController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new PokedexController(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task GetPokemonByName__ValidName__ReturnsOkWithPokemon()
    {
        var expectedPokemon = CreateValidPokemon();
        SetupMediatorToReturnPokemon(expectedPokemon);

        var result = await _controller.GetPokemonByName(ValidPokemonName);

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Name.ShouldBe(expectedPokemon.Name);
        pokemon.Description.ShouldBe(expectedPokemon.Description);
        pokemon.Habitat.ShouldBe(expectedPokemon.Habitat);
        pokemon.IsLegendary.ShouldBe(expectedPokemon.IsLegendary);
        VerifyMediatorSendWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetPokemonByName__LegendaryPokemon__ReturnsOkWithLegendaryFlag()
    {
        var legendaryPokemon = CreateLegendaryPokemon();
        SetupMediatorToReturnPokemon(legendaryPokemon);

        var result = await _controller.GetPokemonByName("mewtwo");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.IsLegendary.ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetPokemonByName__UpperCaseName__ReturnsOk()
    {
        var expectedPokemon = CreateValidPokemon();
        SetupMediatorToReturnPokemon(expectedPokemon);

        var result = await _controller.GetPokemonByName("PIKACHU");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBeOfType<Pokemon>();
        VerifyMediatorSendWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetPokemonByName__MixedCaseName__ReturnsOk()
    {
        var expectedPokemon = CreateValidPokemon();
        SetupMediatorToReturnPokemon(expectedPokemon);

        var result = await _controller.GetPokemonByName("PiKaChU");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBeOfType<Pokemon>();
    }

    [TestMethod]
    public async Task GetPokemonByName__PokemonNotFound__ThrowsHttpRequestException()
    {
        var exception = new HttpRequestException(
            $"Failed to retrieve Pokemon '{NonExistentPokemonName}'. Status code: {HttpStatusCode.NotFound}",
            null,
            HttpStatusCode.NotFound);
        SetupMediatorToThrowException(exception);

        await Should.ThrowAsync<HttpRequestException>(
            async () => await _controller.GetPokemonByName(NonExistentPokemonName));
    }

    [TestMethod]
    public async Task GetPokemonByName__ServerError__ThrowsHttpRequestException()
    {
        var exception = new HttpRequestException(
            $"Failed to retrieve Pokemon '{ValidPokemonName}'. Status code: {HttpStatusCode.InternalServerError}",
            null,
            HttpStatusCode.InternalServerError);
        SetupMediatorToThrowException(exception);

        var thrownException = await Should.ThrowAsync<HttpRequestException>(
            async () => await _controller.GetPokemonByName(ValidPokemonName));

        thrownException.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
    }

    [TestMethod]
    public async Task GetPokemonByName__EmptyName__SendsQueryToMediator()
    {
        var expectedPokemon = CreateValidPokemon();
        SetupMediatorToReturnPokemon(expectedPokemon);

        var result = await _controller.GetPokemonByName(string.Empty);

        result.ShouldBeOfType<OkObjectResult>();
        VerifyMediatorSendWasCalled(Times.Once());
    }

    [TestMethod]
    public async Task GetPokemonByName__CancellationRequested__ThrowsTaskCanceledException()
    {
        SetupMediatorToThrowException(new TaskCanceledException());

        await Should.ThrowAsync<TaskCanceledException>(
            async () => await _controller.GetPokemonByName(ValidPokemonName));
    }

    [TestMethod]
    public async Task GetPokemonByName__ValidName__CreatesCorrectQuery()
    {
        var expectedPokemon = CreateValidPokemon();
        GetPokemonByNameQuery? capturedQuery = null;

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Pokemon>, CancellationToken>((query, _) => 
                capturedQuery = query as GetPokemonByNameQuery)
            .ReturnsAsync(expectedPokemon);

        await _controller.GetPokemonByName(ValidPokemonName);

        capturedQuery.ShouldNotBeNull();
        capturedQuery.Name.ShouldBe(ValidPokemonName);
    }

    [TestMethod]
    public async Task GetPokemonByName__MultipleRequests__EachCallsMediator()
    {
        var expectedPokemon = CreateValidPokemon();
        SetupMediatorToReturnPokemon(expectedPokemon);

        await _controller.GetPokemonByName("pikachu");
        await _controller.GetPokemonByName("charizard");
        await _controller.GetPokemonByName("mewtwo");

        VerifyMediatorSendWasCalled(Times.Exactly(3));
    }

    [TestMethod]
    public async Task GetPokemonByName__PokemonWithNoHabitat__ReturnsOkWithEmptyHabitat()
    {
        var pokemonWithoutHabitat = new Pokemon
        {
            Name = "unknown",
            Description = "A mysterious Pokemon",
            Habitat = string.Empty,
            IsLegendary = false
        };
        SetupMediatorToReturnPokemon(pokemonWithoutHabitat);

        var result = await _controller.GetPokemonByName("unknown");

        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var pokemon = okResult.Value.ShouldBeOfType<Pokemon>();
        pokemon.Habitat.ShouldBeEmpty();
    }

    private static Pokemon CreateValidPokemon() => new()
    {
        Name = ValidPokemonName,
        Description = "When several of these Pokémon gather, their electricity could build and cause lightning storms.",
        Habitat = "forest",
        IsLegendary = false
    };

    private static Pokemon CreateLegendaryPokemon() => new()
    {
        Name = "mewtwo",
        Description = "It was created by a scientist after years of horrific gene splicing and DNA engineering experiments.",
        Habitat = "rare",
        IsLegendary = true
    };

    private void SetupMediatorToReturnPokemon(Pokemon pokemon)
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pokemon);
    }

    private void SetupMediatorToThrowException(Exception exception)
    {
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);
    }

    private void VerifyMediatorSendWasCalled(Times times)
    {
        _mediatorMock.Verify(
            x => x.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()),
            times);
    }
}