using Application.Interfaces;
using Application.Photos;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Photoes;

public class AddPhotoHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly Mock<IPhotoAccessor> _photoAccessorMock;
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public AddPhotoHandlerTests()
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

    private Add.Handler CreateHandler()
    {
        return new Add.Handler(
            _context,
            _photoAccessorMock.Object,
            _userAccessorMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("unknown-user");

        var fileMock = new Mock<IFormFile>();

        var command = new Add.Command
        {
            File = fileMock.Object
        };

        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);

        _photoAccessorMock.Verify(
            x => x.AddPhoto(It.IsAny<IFormFile>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPhotoUploadFails()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@test.com",
            DispayName = "Test User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        _photoAccessorMock
            .Setup(x => x.AddPhoto(It.IsAny<IFormFile>()))
            .ReturnsAsync((PhotoUploadResult?)null);

        var fileMock = new Mock<IFormFile>();

        var command = new Add.Command
        {
            File = fileMock.Object
        };

        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Problem uploading photo", result.Error);

        _photoAccessorMock.Verify(
            x => x.AddPhoto(fileMock.Object),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddPhotoAndSetAsMain_WhenUserHasNoMainPhoto()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@test.com",
            DispayName = "Test User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var fileMock = new Mock<IFormFile>();

        var uploadResult = new PhotoUploadResult
        {
            PublicId = "photo-id",
            Url = "https://example.com/photo.jpg"
        };

        _photoAccessorMock
            .Setup(x => x.AddPhoto(fileMock.Object))
            .ReturnsAsync(uploadResult);

        var command = new Add.Command
        {
            File = fileMock.Object
        };

        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("photo-id", result.Value.Id);
        Assert.Equal("https://example.com/photo.jpg", result.Value.Url);
        Assert.True(result.Value.IsMain);

        _photoAccessorMock.Verify(
            x => x.AddPhoto(fileMock.Object),
            Times.Once);

        var savedPhoto = await _context.Photos
            .FirstOrDefaultAsync(x => x.Id == "photo-id");

        Assert.NotNull(savedPhoto);
        Assert.Equal("https://example.com/photo.jpg", savedPhoto.Url);
        Assert.True(savedPhoto.IsMain);
    }

    [Fact]
    public async Task Handle_ShouldAddPhotoAsNotMain_WhenUserAlreadyHasMainPhoto()
    {
        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@test.com",
            DispayName = "Test User"
        };

        var mainPhoto = new Photo
        {
            Id = "main-photo",
            Url = "https://example.com/main.jpg",
            IsMain = true
        };

        user.Photos.Add(mainPhoto);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var fileMock = new Mock<IFormFile>();

        var uploadResult = new PhotoUploadResult
        {
            PublicId = "new-photo",
            Url = "https://example.com/new.jpg"
        };

        _photoAccessorMock
            .Setup(x => x.AddPhoto(fileMock.Object))
            .ReturnsAsync(uploadResult);

        var command = new Add.Command
        {
            File = fileMock.Object
        };

        var handler = CreateHandler();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("new-photo", result.Value.Id);
        Assert.Equal("https://example.com/new.jpg", result.Value.Url);
        Assert.False(result.Value.IsMain);

        var photos = await _context.Photos
            .Where(x => x.Id == "main-photo" || x.Id == "new-photo")
            .ToListAsync();

        Assert.Equal(2, photos.Count);

        var savedMainPhoto = photos.Single(x => x.Id == "main-photo");
        var savedNewPhoto = photos.Single(x => x.Id == "new-photo");

        Assert.True(savedMainPhoto.IsMain);
        Assert.False(savedNewPhoto.IsMain);

        _photoAccessorMock.Verify(
            x => x.AddPhoto(fileMock.Object),
            Times.Once);
    }
}