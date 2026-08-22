using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Services;
using Domain;
using Microsoft.Extensions.Configuration;

namespace Tests;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TokenKey"] =
                    "test-token-key-1234567890-abcdefghijklmnopqrstuvwxyz-ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            })
            .Build();

        _tokenService = new TokenService(configuration);
    }

    [Fact]
    public void CreateToken_ShouldReturnValidJwtToken()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var token = _tokenService.CreateToken(user);

        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(
            "testuser",
            jwt.Claims.First(x => x.Type == "unique_name").Value);

        Assert.Equal(
            "user-id",
            jwt.Claims.First(x => x.Type == "nameid").Value);

        Assert.Equal(
            "test@example.com",
            jwt.Claims.First(x => x.Type == "email").Value);
    }

    [Fact]
    public void CreateToken_ShouldExpireInSevenDays()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var token = _tokenService.CreateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiration = DateTime.UtcNow.AddDays(7);

        Assert.InRange(
            jwt.ValidTo,
            expectedExpiration.AddMinutes(-1),
            expectedExpiration.AddMinutes(1));
    }
}