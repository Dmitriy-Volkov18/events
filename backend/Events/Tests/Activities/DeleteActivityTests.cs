using Application.Activities;
using Domain;

namespace Tests.Activities;

public class DeleteActivityTests
{
    [Fact]
    public async Task Handle_ShouldFail_WhenActivityDoesNotExist()
    {
        await using var context = TestDbContextFactory.Create();

        var handler = new Delete.Handler(context);

        var command = new Delete.Command
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
    public async Task Handle_ShouldDeleteActivity_WhenActivityExists()
    {
        await using var context = TestDbContextFactory.Create();

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Test Activity",
            Description = "Test Description",
            Date = DateTime.UtcNow,
            Category = "Test",
            City = "Vilnius",
            Venue = "Test Venue"
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var handler = new Delete.Handler(context);

        var command = new Delete.Command
        {
            Id = activity.Id
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var deletedActivity = await context.Activities
            .FindAsync(activity.Id);

        Assert.Null(deletedActivity);
    }
}