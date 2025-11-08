using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PokedexApp.WebApi.Middleware;
using Shouldly;

namespace PokedexAppTest.WebApi.Middleware;

[TestClass]
public class GlobalExceptionHandlerMiddlewareTests
{
    private Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = null!;
    private DefaultHttpContext _httpContext = null!;

    [TestInitialize]
    public void Initialize()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        _httpContext = new DefaultHttpContext();
        _httpContext.Response.Body = new MemoryStream();
    }

    [TestMethod]
    public async Task InvokeAsync__NoException__CallsNextDelegate()
    {
        var nextDelegateCalled = false;
        RequestDelegate next = _ =>
        {
            nextDelegateCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        nextDelegateCalled.ShouldBeTrue();
    }

    [TestMethod]
    public async Task InvokeAsync__HttpRequestExceptionWithNotFound__Returns404WithJsonResponse()
    {
        var exception = new HttpRequestException("Resource not found", null, HttpStatusCode.NotFound);
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        _httpContext.Response.ContentType.ShouldBe("application/json; charset=utf-8");

        var responseBody = await GetResponseBodyAsync();
        responseBody.ShouldContain("Resource not found.");

        VerifyLoggerWasCalled(LogLevel.Error, "HTTP request failed");
    }

    [TestMethod]
    public async Task InvokeAsync__HttpRequestExceptionWithOtherStatus__Returns500WithJsonResponse()
    {
        var exception = new HttpRequestException("Bad gateway", null, HttpStatusCode.BadGateway);
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        _httpContext.Response.ContentType.ShouldBe("application/json; charset=utf-8");

        var responseBody = await GetResponseBodyAsync();
        responseBody.ShouldContain("An error occurred while processing your request.");

        VerifyLoggerWasCalled(LogLevel.Error, "HTTP request failed");
    }

    [TestMethod]
    public async Task InvokeAsync__GenericException__Returns500WithJsonResponse()
    {
        var exception = new InvalidOperationException("Something went wrong");
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        _httpContext.Response.ContentType.ShouldBe("application/json; charset=utf-8");

        var responseBody = await GetResponseBodyAsync();
        responseBody.ShouldContain("An unexpected error occurred.");

        VerifyLoggerWasCalled(LogLevel.Error, "An unexpected error occurred");
    }

    [TestMethod]
    public async Task InvokeAsync__ArgumentNullException__Returns500WithJsonResponse()
    {
        var exception = new ArgumentNullException("parameter");
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        _httpContext.Response.ContentType.ShouldBe("application/json; charset=utf-8");

        var responseBody = await GetResponseBodyAsync();
        responseBody.ShouldContain("An unexpected error occurred.");
    }

    [TestMethod]
    public async Task InvokeAsync__NoException__DoesNotModifyResponse()
    {
        RequestDelegate next = context =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        VerifyLoggerWasNotCalled();
    }

    [TestMethod]
    public async Task InvokeAsync__HttpRequestExceptionWithoutStatusCode__Returns500()
    {
        var exception = new HttpRequestException("Request failed");
        RequestDelegate next = _ => throw exception;
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object);

        await middleware.InvokeAsync(_httpContext);

        _httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        var responseBody = await GetResponseBodyAsync();
        responseBody.ShouldContain("An error occurred while processing your request.");
    }

    private async Task<string> GetResponseBodyAsync()
    {
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_httpContext.Response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    private void VerifyLoggerWasCalled(LogLevel logLevel, string message)
    {
        _loggerMock.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void VerifyLoggerWasNotCalled()
    {
        _loggerMock.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}

    
