using API.Controllers;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace Tests;

public class AccountControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
    private readonly TokenService _tokenService;

    public AccountControllerTests()
    {
        var userStore = new Mock<IUserStore<AppUser>>();

        _userManagerMock = new Mock<UserManager<AppUser>>(
            userStore.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);

        var contextAccessor =
            new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

        _signInManagerMock = new Mock<SignInManager<AppUser>>(
            _userManagerMock.Object,
            contextAccessor.Object,
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            null!,
            null!,
            null!,
            null!);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["TokenKey"] =
                        "test-token-key-1234567890-abcdefghijklmnopqrstuvwxyz-ABCDEFGHIJKLMNOPQRSTUVWXYZ"
                })
            .Build();

        _tokenService = new TokenService(configuration);
    }

    private AccountController CreateController()
    {
        return new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);
    }


    private void SetupUsers(IEnumerable<AppUser> users)
    {
        var queryable = new TestAsyncEnumerable<AppUser>(users);

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(queryable);
    }


    private static IQueryable<AppUser> CreateAsyncQueryable(List<AppUser> users)
    {
        return new TestAsyncEnumerable<AppUser>(users);
    }


    [Fact]
    public async Task Register_ShouldCreateUser_WhenEmailAndUsernameAreAvailable()
    {
        var controller = CreateController();

        var registerDto = new RegisterDto
        {
            DispayName = "Test User",
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password1"
        };

        var users = new List<AppUser>();

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(users.BuildMock());

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<AppUser>(),
                registerDto.Password))
            .ReturnsAsync(IdentityResult.Success);

        var result = await controller.Register(registerDto);

        var userDto = Assert.IsType<UserDto>(result.Value);

        Assert.Equal("Test User", userDto.DispayName);
        Assert.Equal("testuser", userDto.Username);
        Assert.NotEmpty(userDto.Token);

        _userManagerMock.Verify(
            x => x.CreateAsync(
                It.Is<AppUser>(u =>
                    u.DispayName == registerDto.DispayName &&
                    u.Email == registerDto.Email &&
                    u.UserName == registerDto.Username),
                registerDto.Password),
            Times.Once);
    }


    [Fact]
    public async Task Register_ShouldReturnValidationProblem_WhenEmailAlreadyExists()
    {
        var users = new List<AppUser>
        {
            new()
            {
                Id = "existing-user",
                Email = "existing@example.com",
                UserName = "existinguser"
            }
        };

        SetupUsers(users);

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var registerDto = new RegisterDto
        {
            DispayName = "Test User",
            Email = "existing@example.com",
            Username = "newuser",
            Password = "Password1"
        };

        var result = await controller.Register(registerDto);

        var validationResult =
            Assert.IsType<ObjectResult>(result.Result);

        Assert.IsType<ValidationProblemDetails>(validationResult.Value);

        Assert.True(controller.ModelState.ContainsKey("email"));
        Assert.Contains(
            controller.ModelState["email"]!.Errors,
            error => error.ErrorMessage == "Email taken");
    }

    [Fact]
    public async Task Register_ShouldReturnValidationProblem_WhenUsernameAlreadyExists()
    {
        var users = new List<AppUser>
        {
            new()
            {
                Id = "existing-user",
                Email = "existing@example.com",
                UserName = "existinguser"
            }
        };

        SetupUsers(users);

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var registerDto = new RegisterDto
        {
            DispayName = "Test User",
            Email = "new@example.com",
            Username = "existinguser",
            Password = "Password1"
        };

        var result = await controller.Register(registerDto);

        var validationResult =
            Assert.IsType<ObjectResult>(result.Result);

        Assert.IsType<ValidationProblemDetails>(validationResult.Value);

        Assert.True(controller.ModelState.ContainsKey("username"));
        Assert.Contains(
            controller.ModelState["username"]!.Errors,
            error => error.ErrorMessage == "Username taken");
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenUserCreationFails()
    {
        SetupUsers([]);

        _userManagerMock
            .Setup(x => x.CreateAsync(
                It.IsAny<AppUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "User creation failed"
                    }));

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var registerDto = new RegisterDto
        {
            DispayName = "Test User",
            Email = "test@example.com",
            Username = "testuser",
            Password = "Password1"
        };

        var result = await controller.Register(registerDto);

        var badRequestResult =
            Assert.IsType<BadRequestObjectResult>(result.Result);

        Assert.Equal(
            "Problem registering user",
            badRequestResult.Value);
    }


    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        SetupUsers([]);

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var loginDto = new LoginDto
        {
            Email = "notfound@example.com",
            Password = "Password1"
        };

        var result = await controller.Login(loginDto);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsInvalid()
    {
        var user = new AppUser
        {
            Id = "user-id",
            Email = "test@example.com",
            UserName = "testuser"
        };

        SetupUsers([user]);

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(
                user,
                "wrong-password",
                false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "wrong-password"
        };

        var result = await controller.Login(loginDto);

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task Login_ShouldReturnUserDto_WhenCredentialsAreValid()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        var users = new List<AppUser>
        {
            user
        };

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(CreateAsyncQueryable(users));

        _signInManagerMock
            .Setup(x => x.CheckPasswordSignInAsync(
                user,
                "correct-password",
                false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        var dto = new LoginDto
        {
            Email = "test@example.com",
            Password = "correct-password"
        };

        var result = await controller.Login(dto);

        var userDto = Assert.IsType<UserDto>(result.Value);

        Assert.Equal("testuser", userDto.Username);
        Assert.NotEmpty(userDto.Token);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUserDto_WhenUserExists()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com",
            DispayName = "Test User"
        };

        var users = new List<AppUser>
    {
        user
    };

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(CreateAsyncQueryable(users));

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.Email, "test@example.com")
                        },
                        "TestAuth"))
            }
        };

        var result = await controller.GetCurrentUser();

        var userDto = Assert.IsType<UserDto>(result.Value);

        Assert.Equal("testuser", userDto.Username);
        Assert.Equal("Test User", userDto.DispayName);
        Assert.NotEmpty(userDto.Token);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var users = new List<AppUser>();

        _userManagerMock
            .Setup(x => x.Users)
            .Returns(CreateAsyncQueryable(users));

        var controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenService);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                        new Claim(ClaimTypes.Email, "unknown@example.com")
                        },
                        "TestAuth"))
            }
        };

        var result = await controller.GetCurrentUser();

        Assert.IsType<NotFoundResult>(result.Result);
    }
}