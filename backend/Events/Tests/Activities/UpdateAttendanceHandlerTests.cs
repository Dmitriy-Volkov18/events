using Application.Activities;
using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Activities;

public class UpdateAttendanceHandlerTests
{
    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    private static Mock<IUserAccessor> CreateUserAccessor(string username)
    {
        var userAccessor = new Mock<IUserAccessor>();

        userAccessor
            .Setup(x => x.GetUsername())
            .Returns(username);

        return userAccessor;
    }

    [Fact]
    public async Task UpdateAttendance_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        await using var context = CreateContext();

        var userAccessor = CreateUserAccessor("testuser");

        var handler = new UpdateAttendance.Handler(
            context,
            userAccessor.Object);

        var command = new UpdateAttendance.Command
        {
            Id = Guid.NewGuid()
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Activity not found", result.Error);
    }

    [Fact]
    public async Task UpdateAttendance_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        await using var context = CreateContext();

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test activity",
            Description = "Description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Test venue"
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var userAccessor = CreateUserAccessor("unknown-user");

        var handler = new UpdateAttendance.Handler(
            context,
            userAccessor.Object);

        var command = new UpdateAttendance.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("User not found", result.Error);
    }

    [Fact]
    public async Task UpdateAttendance_ShouldCancelActivity_WhenHostAttendsAgain()
    {
        await using var context = CreateContext();

        var host = new AppUser
        {
            Id = "host-id",
            UserName = "host"
        };

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test activity",
            Description = "Description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Test venue",
            isCancelled = false
        };

        var attendance = new ActivityAttendee
        {
            AppUserId = host.Id,
            AppUser = host,
            ActivityId = activity.Id,
            Activity = activity,
            isHost = true
        };

        activity.Attendees.Add(attendance);

        context.Users.Add(host);
        context.Activities.Add(activity);

        await context.SaveChangesAsync();

        var userAccessor = CreateUserAccessor("host");

        var handler = new UpdateAttendance.Handler(
            context,
            userAccessor.Object);

        var command = new UpdateAttendance.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedActivity = await context.Activities
            .FirstAsync(x => x.Id == activity.Id);

        Assert.True(savedActivity.isCancelled);
    }

    [Fact]
    public async Task UpdateAttendance_ShouldRemoveAttendance_WhenUserIsAlreadyAttending()
    {
        await using var context = CreateContext();

        var host = new AppUser
        {
            Id = "host-id",
            UserName = "host"
        };

        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser"
        };

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test activity",
            Description = "Description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Test venue"
        };

        activity.Attendees.Add(new ActivityAttendee
        {
            AppUserId = host.Id,
            AppUser = host,
            ActivityId = activity.Id,
            Activity = activity,
            isHost = true
        });

        activity.Attendees.Add(new ActivityAttendee
        {
            AppUserId = user.Id,
            AppUser = user,
            ActivityId = activity.Id,
            Activity = activity,
            isHost = false
        });

        context.Users.AddRange(host, user);
        context.Activities.Add(activity);

        await context.SaveChangesAsync();

        var userAccessor = CreateUserAccessor("testuser");

        var handler = new UpdateAttendance.Handler(
            context,
            userAccessor.Object);

        var command = new UpdateAttendance.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var attendanceExists = await context.ActivityAttendees
            .AnyAsync(x =>
                x.ActivityId == activity.Id &&
                x.AppUserId == user.Id);

        Assert.False(attendanceExists);
    }

    [Fact]
    public async Task UpdateAttendance_ShouldAddAttendance_WhenUserIsNotAttending()
    {
        await using var context = CreateContext();

        var host = new AppUser
        {
            Id = "host-id",
            UserName = "host"
        };

        var user = new AppUser
        {
            Id = "user-id",
            UserName = "testuser"
        };

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test activity",
            Description = "Description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Test venue"
        };

        activity.Attendees.Add(new ActivityAttendee
        {
            AppUserId = host.Id,
            AppUser = host,
            ActivityId = activity.Id,
            Activity = activity,
            isHost = true
        });

        context.Users.AddRange(host, user);
        context.Activities.Add(activity);

        await context.SaveChangesAsync();

        var userAccessor = CreateUserAccessor("testuser");

        var handler = new UpdateAttendance.Handler(
            context,
            userAccessor.Object);

        var command = new UpdateAttendance.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var attendance = await context.ActivityAttendees
            .FirstOrDefaultAsync(x =>
                x.ActivityId == activity.Id &&
                x.AppUserId == user.Id);

        Assert.NotNull(attendance);
        Assert.False(attendance.isHost);
    }
}

