using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests.Common;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebFactory>
{
    protected readonly ApplicationDbContext Context;
    protected readonly HttpClient Client;
    //private readonly IJwtProvider _jwtProvider;

    protected BaseIntegrationTest(IntegrationTestWebFactory factory)
    {
        var scope = factory.Services.CreateScope();
        Context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        //_jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        Client = factory.WithWebHostBuilder(builder => { })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

        // Use test data for generating a JWT token
        //var user = UsersData.MainUser(RolesData.AdminRole.Id, ProfileId.Empty());
        //Client.DefaultRequestHeaders.Authorization =
        //    new AuthenticationHeaderValue("Bearer", _jwtProvider.Generate(user, RolesData.AdminRole));
    }

    protected async Task<int> SaveChangesAsync()
    {
        var result = await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        return result;
    }
}

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Role, "Admin"), new Claim("userId", "admin") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}