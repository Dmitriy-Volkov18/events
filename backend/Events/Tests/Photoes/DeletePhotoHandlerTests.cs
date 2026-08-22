using Application.Interfaces;
using Application.Photos;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Photoes;

public class DeletePhotoHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly Mock<IPhotoAccessor> _photoAccessorMock;
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public DeletePhotoHandlerTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new DataContext(options);
        _context.Database.EnsureCreated();

        _photoAccessorMock = new Mock<IPhotoAccessor>();
        _userAccessorMock = new Mock<IUserAccessor>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();

        GC.SuppressFinalize(this);
    }

    private Delete.Handler CreateHandler()
    {
        return new Delete.Handler(
            _context,
            _photoAccessorMock.Object,
            _userAccessorMock.Object);
    }

    private async Task<AppUser> AddUser(
        string id = "user-id",
        string username = "testuser")
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

    private async Task<Photo> AddPhoto(
        AppUser user,
        string id = "photo-id",
        bool isMain = false)
    {
        var photo = new Photo
        {
            Id = id,
            Url = "https://test.com/photo.jpg",
            IsMain = isMain
        };

        user.Photos.Add(photo);
        await _context.SaveChangesAsync();

        return photo;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("unknown-user");

        var handler = CreateHandler();

        var command = new Delete.Command
        {
            Id = "photo-id"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);

        _photoAccessorMock.Verify(
            x => x.DeletePhoto(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPhotoDoesNotExist()
    {
        var user = await AddUser();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        var handler = CreateHandler();

        var command = new Delete.Command
        {
            Id = "unknown-photo"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Photo not found", result.Error);

        _photoAccessorMock.Verify(
            x => x.DeletePhoto(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPhotoIsMain()
    {
        var user = await AddUser();
        var photo = await AddPhoto(
            user,
            "main-photo",
            isMain: true);

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        var handler = CreateHandler();

        var command = new Delete.Command
        {
            Id = photo.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "You cannot delete your main photo",
            result.Error);

        _photoAccessorMock.Verify(
            x => x.DeletePhoto(photo.Id),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCloudinaryDeleteFails()
    {
        var user = await AddUser();
        var photo = await AddPhoto(
            user,
            "photo-id",
            isMain: false);

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        _photoAccessorMock
            .Setup(x => x.DeletePhoto(photo.Id))
            .ReturnsAsync((string?)null);

        var handler = CreateHandler();

        var command = new Delete.Command
        {
            Id = photo.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "Problem deleting photo from Cloudinary",
            result.Error);

        _photoAccessorMock.Verify(
            x => x.DeletePhoto(photo.Id),
            Times.Once);

        Assert.Single(user.Photos);
    }

    [Fact]
    public async Task Handle_ShouldDeletePhoto_WhenPhotoExists()
    {
        var user = await AddUser();
        var photo = await AddPhoto(
            user,
            "photo-id",
            isMain: false);

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        _photoAccessorMock
            .Setup(x => x.DeletePhoto(photo.Id))
            .ReturnsAsync("ok");

        var handler = CreateHandler();

        var command = new Delete.Command
        {
            Id = photo.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        _photoAccessorMock.Verify(
            x => x.DeletePhoto(photo.Id),
            Times.Once);

        var photoInDatabase = await _context.Photos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == photo.Id);

        Assert.Null(photoInDatabase);
    }
}