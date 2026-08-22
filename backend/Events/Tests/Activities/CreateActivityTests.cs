using Application.Activities;
using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Activities;

public class CreateActivityTests
{
    private readonly Mock<IUserAccessor> _userAccessorMock;

    public CreateActivityTests()
    {
        _userAccessorMock = new Mock<IUserAccessor>();
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var handler = new Create.Handler(
            context,
            _userAccessorMock.Object);

        var command = new Create.Command
        {
            Activity = new Activity
            {
                Title = "Test Activity",
                Description = "Test Description",
                Date = DateTime.UtcNow,
                Category = "Test",
                City = "Vilnius",
                Venue = "Test Venue"
            }
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
    }


    [Fact]
    public async Task Handle_ShouldCreateActivity_WhenUserExists()
    {
        await using var context = TestDbContextFactory.Create();

        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser",
            Email = "test@example.com"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        _userAccessorMock
            .Setup(x => x.GetUsername())
            .Returns("testuser");

        var handler = new Create.Handler(
            context,
            _userAccessorMock.Object);

        var activity = new Activity
        {
            Title = "Test Activity",
            Description = "Test Description",
            Date = DateTime.UtcNow,
            Category = "Test",
            City = "Vilnius",
            Venue = "Test Venue"
        };

        var command = new Create.Command
        {
            Activity = activity
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var createdActivity = await context.Activities
            .Include(x => x.Attendees)
            .FirstOrDefaultAsync(x => x.Id == activity.Id);

        Assert.NotNull(createdActivity);

        Assert.Single(createdActivity.Attendees);

        var attendee = createdActivity.Attendees.First();

        Assert.Equal(user.Id, attendee.AppUserId);
        Assert.True(attendee.isHost);
    }
}