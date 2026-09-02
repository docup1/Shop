using System.Net;
using System.Text.Json;
using Application.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging.Abstractions;
using Presentation.Middleware;

namespace Tests.Presentation;

public class GlobalExceptionMiddlewareTests
{
    private static async Task<(int Status, JsonDocument Body)> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/orders";

        var middleware = new GlobalExceptionMiddleware(
            _ => throw exception,
            NullLogger<GlobalExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, body);
    }

    [Theory]
    [InlineData(typeof(ValidationException), StatusCodes.Status400BadRequest)]
    [InlineData(typeof(UnauthorizedException), StatusCodes.Status401Unauthorized)]
    [InlineData(typeof(NotFoundException), StatusCodes.Status404NotFound)]
    [InlineData(typeof(ConflictException), StatusCodes.Status409Conflict)]
    public async Task ApplicationExceptions_MapToCorrectStatus(Type exceptionType, int expectedStatus)
    {
        var exception = exceptionType == typeof(ValidationException)
            ? new ValidationException("boom")
            : (Exception)Activator.CreateInstance(exceptionType, "boom")!;
        var (status, body) = await InvokeAsync(exception);

        Assert.Equal(expectedStatus, status);
        Assert.Equal(expectedStatus, body.RootElement.GetProperty("status").GetInt32());

        var detail = body.RootElement.GetProperty("detail").GetString();
        Assert.Equal("boom", detail);
    }

    [Fact]
    public async Task ValidationException_ExposesPropertyName()
    {
        var (_, body) = await InvokeAsync(new ValidationException("Weight must be positive", nameof(Domain.Models.Order.Weight)));

        var detail = body.RootElement.GetProperty("detail").GetString();
        Assert.Equal("Weight must be positive", detail);
    }

    [Fact]
    public async Task UnknownException_Returns500_WithoutLeakingInternals()
    {
        var (status, body) = await InvokeAsync(new InvalidOperationException("secret internals"));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.Equal("Internal Server Error", body.RootElement.GetProperty("title").GetString());
        Assert.Equal("An unexpected error occurred.", body.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("secret internals", body.RootElement.ToString());
    }

    [Fact]
    public async Task StartedResponse_IsNotOverwritten()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseTestServer();

        var app = builder.Build();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status202Accepted;
            await context.Response.WriteAsync("partial");
            throw new NotFoundException("too late");
        });

        await app.StartAsync();
        try
        {
            var response = await app.GetTestClient().GetAsync("/");

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Equal("partial", await response.Content.ReadAsStringAsync());
        }
        finally
        {
            await app.StopAsync();
        }
    }
}