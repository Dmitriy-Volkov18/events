using Application.Comments;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Comments;

public class CreateCommentHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DataContext _context;
    private readonly Mock<IUserAccessor> _userAccessorMock;
    private readonly Mock<IMapper> _mapperMock;

    public CreateCommentHandlerTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DataContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new DataContext(options);
        _context.Database.EnsureCreated();

        _userAccessorMock = new Mock<IUserAccessor>();
        _mapperMock = new Mock<IMapper>();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();

        GC.SuppressFinalize(this);
    }

    private Create.Handler CreateHandler()
    {
        return new Create.Handler(
            _context,
            _mapperMock.Object,
            _userAccessorMock.Object);
    }

    private async Task<AppUser> AddUser(
        string id,
        string username)
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

    private async Task<Activity> AddActivity()
    {
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test activity",
            Description = "Test description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Test venue"
        };

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        return activity;
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        var handler = CreateHandler();

        var command = new Create.Command
        {
            ActivityId = Guid.NewGuid(),
            Body = "Test comment"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Activity not found", result.Error);

        _userAccessorMock.Verify(
            x => x.GetUsername(),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var activity = await AddActivity();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("unknown-user");

        var handler = CreateHandler();

        var command = new Create.Command
        {
            ActivityId = activity.Id,
            Body = "Test comment"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
    }

    [Fact]
    public async Task Handle_ShouldCreateComment_WhenActivityAndUserExist()
    {
        var activity = await AddActivity();

        var user = await AddUser(
            "user-id",
            "testuser");

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        var commentDto = new CommentDto
        {
            Body = "Test comment",
            Username = user.UserName!,
            DisplayName = user.DispayName
        };

        _mapperMock
            .Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(commentDto);

        var handler = CreateHandler();

        var command = new Create.Command
        {
            ActivityId = activity.Id,
            Body = "Test comment"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("Test comment", result.Value.Body);
        Assert.Equal("testuser", result.Value.Username);
        Assert.Equal("testuser", result.Value.DisplayName);

        var comment = Assert.Single(activity.Comments);

        Assert.Equal("Test comment", comment.Body);
        Assert.Same(user, comment.Author);
        Assert.Same(activity, comment.Activity);
    }

    [Fact]
    public async Task Handle_ShouldMapCreatedCommentToDto()
    {
        var activity = await AddActivity();

        var user = await AddUser(
            "user-id",
            "testuser");

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns(user.UserName!);

        var comment = new CommentDto
        {
            Body = "Mapped comment",
            Username = user.UserName!,
            DisplayName = user.DispayName
        };

        _mapperMock
            .Setup(x => x.Map<CommentDto>(It.IsAny<Comment>()))
            .Returns(comment);

        var handler = CreateHandler();

        var command = new Create.Command
        {
            ActivityId = activity.Id,
            Body = "Mapped comment"
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("Mapped comment", result.Value.Body);
        Assert.Equal("testuser", result.Value.Username);

        _mapperMock.Verify(
            x => x.Map<CommentDto>(It.IsAny<Comment>()),
            Times.Once);
    }
}