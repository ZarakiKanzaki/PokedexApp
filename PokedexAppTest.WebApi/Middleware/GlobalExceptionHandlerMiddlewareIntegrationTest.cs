using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PokedexApp.BasicInfo.Queries;
using PokedexApp.Common.Dto;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace PokedexAppTest.WebApi.Middleware;

[TestClass]
public class GlobalExceptionHandlerMiddlewareIntegrationTests
{
    private const string RequestUriForUnknownPokemon = "/Pokedex/pokemon/unknownpokemon";
    private const string RequestUriForKnownPokemon = "/Pokedex/pokemon/pikachu";
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
    public async Task Middleware__WhenHttpRequestExceptionThrown__Returns404StatusCode()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Pokemon not found", null, HttpStatusCode.NotFound));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForUnknownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.ShouldNotBeNull();
        content.Message.ShouldBe("Resource not found.");
    }

    [TestMethod]
    public async Task Middleware__WhenGenericExceptionThrown__Returns500StatusCode()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Something went wrong"));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForKnownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.ShouldNotBeNull();
        content.Message.ShouldBe("An unexpected error occurred.");
    }

    [TestMethod]
    public async Task Middleware__WhenNoExceptionThrown__ReturnsSuccessStatusCode()
    {
        var expectedPokemon = new Pokemon { Name = "pikachu" };

        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPokemon);
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForKnownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.IsSuccessStatusCode.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Middleware__WhenHttpRequestExceptionWithBadGateway__Returns500StatusCode()
    {
        var factory = CreateFactoryWithMockedMediator(mediator =>
        {
            mediator
                .Setup(m => m.Send(It.IsAny<GetPokemonByNameQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("External service unavailable", null, HttpStatusCode.BadGateway));
        });

        var client = factory.CreateClient();
        var response = await client.GetAsync(RequestUriForKnownPokemon);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        content.ShouldNotBeNull();
        content.Message.ShouldBe("An error occurred while processing your request.");
    }

    private static WebApplicationFactory<Program> CreateFactoryWithMockedMediator(Action<Mock<IMediator>> configureMock)
        => new WebApplicationFactory<Program>()
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

    private static void RemoveRealMediator(IServiceCollection services)
    {
        var mediatorDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
        if (mediatorDescriptor != null)
        {
            services.Remove(mediatorDescriptor);
        }
    }

    private record ErrorResponse(string Message);
}