using Application.Interfaces;
using Application.Photos;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Photoes;

public class SetMainPhotoTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public SetMainPhotoTests()
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

    private SetMain.Handler CreateHandler()
    {
        return new SetMain.Handler(
            _context,
            _userAccessorMock.Object);
    }

    private async Task<AppUser> AddUser()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com",
            DispayName = "Test User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("unknown-user");

        var handler = CreateHandler();

        var command = new SetMain.Command
        {
            Id = "photo-id"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPhotoDoesNotExist()
    {
        await AddUser();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var handler = CreateHandler();

        var command = new SetMain.Command
        {
            Id = "unknown-photo"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Photo not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldSetPhotoAsMain_AndUnsetPreviousMain()
    {
        var user = await AddUser();

        var currentMain = new Photo
        {
            Id = "photo-1",
            Url = "photo1.jpg",
            IsMain = true
        };

        var newMain = new Photo
        {
            Id = "photo-2",
            Url = "photo2.jpg",
            IsMain = false
        };

        user.Photos.Add(currentMain);
        user.Photos.Add(newMain);

        await _context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var handler = CreateHandler();

        var command = new SetMain.Command
        {
            Id = "photo-2"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var photos = await _context.Photos
            .Where(x => x.Id == "photo-1" || x.Id == "photo-2")
            .ToListAsync();

        var oldMain = photos.Single(x => x.Id == "photo-1");
        var newMainPhoto = photos.Single(x => x.Id == "photo-2");

        Assert.False(oldMain.IsMain);
        Assert.True(newMainPhoto.IsMain);
    }
}