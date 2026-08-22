using Application.Followers;
using Application.Interfaces;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests;

public class FollowToggleTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public FollowToggleTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new DataContext(options);

        _context.Database.EnsureCreated();

        _userAccessorMock = new Mock<IUserAccessor>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private FollowToggle.Handler CreateHandler()
    {
        return new FollowToggle.Handler(
            _context,
            _userAccessorMock.Object);
    }

    private async Task<AppUser> AddUser(
    string username,
    string id)
    {
        var user = new AppUser
        {
            Id = id,
            UserName = username,
            Email = $"{username}@test.com",
            DispayName = username
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenObserverNotFound()
    {
        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("observer");

        await AddUser("target", "target-id");

        var handler = CreateHandler();

        var command = new FollowToggle.Command
        {
            TargetUsername = "target"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Observer not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTargetUserNotFound()
    {
        var observer = await AddUser(
            "observer",
            "observer-id");

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(observer.UserName!);

        var handler = CreateHandler();

        var command = new FollowToggle.Command
        {
            TargetUsername = "missing-user"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Target user not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldCreateFollowing_WhenNotAlreadyFollowing()
    {
        var observer = await AddUser(
            "observer",
            "observer-id");

        var target = await AddUser(
            "target",
            "target-id");

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(observer.UserName!);

        var handler = CreateHandler();

        var command = new FollowToggle.Command
        {
            TargetUsername = target.UserName!
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var following = await _context.UserFollowings
            .FindAsync(observer.Id, target.Id);

        Assert.NotNull(following);
    }

    [Fact]
    public async Task Handle_ShouldRemoveFollowing_WhenAlreadyFollowing()
    {
        var observer = await AddUser(
            "observer",
            "observer-id");

        var target = await AddUser(
            "target",
            "target-id");

        var following = new UserFollowing
        {
            ObserverId = observer.Id,
            TargetId = target.Id
        };

        _context.UserFollowings.Add(following);

        await _context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(observer.UserName!);

        var handler = CreateHandler();

        var command = new FollowToggle.Command
        {
            TargetUsername = target.UserName!
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var deletedFollowing = await _context.UserFollowings
            .FindAsync(observer.Id, target.Id);

        Assert.Null(deletedFollowing);
    }
}