using Application.Activities;
using AutoMapper;
using Domain;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;

namespace Tests.Activities;

public class EditHandlerTests
{
    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }

    [Fact]
    public async Task Edit_ShouldUpdateActivity_WhenActivityExists()
    {
        await using var context = CreateContext();

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = "Old title",
            Description = "Old description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Old venue"
        };

        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        var mapper = new Mock<IMapper>();

        mapper
            .Setup(x => x.Map(
                It.IsAny<Activity>(),
                It.IsAny<Activity>()))
            .Callback<Activity, Activity>((source, destination) =>
            {
                destination.Title = source.Title;
                destination.Description = source.Description;
                destination.Date = source.Date;
                destination.Category = source.Category;
                destination.City = source.City;
                destination.Venue = source.Venue;
            });

        var handler = new Edit.Handler(
            context,
            mapper.Object);

        var updatedActivity = new Activity
        {
            Id = activity.Id,
            Title = "New title",
            Description = "New description",
            Date = DateTime.UtcNow.AddDays(5),
            Category = "Music",
            City = "Kaunas",
            Venue = "New venue"
        };

        var command = new Edit.Command
        {
            Activity = updatedActivity
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var savedActivity = await context.Activities
            .FirstAsync(x => x.Id == activity.Id);

        Assert.Equal("New title", savedActivity.Title);
        Assert.Equal("New description", savedActivity.Description);
        Assert.Equal("Music", savedActivity.Category);
        Assert.Equal("Kaunas", savedActivity.City);
        Assert.Equal("New venue", savedActivity.Venue);

        mapper.Verify(
            x => x.Map(updatedActivity, activity),
            Times.Once);
    }

    [Fact]
    public async Task Edit_ShouldReturnFailure_WhenActivityDoesNotExist()
    {
        await using var context = CreateContext();

        var mapper = new Mock<IMapper>();

        var handler = new Edit.Handler(
            context,
            mapper.Object);

        var command = new Edit.Command
        {
            Activity = new Activity
            {
                Id = Guid.NewGuid(),
                Title = "Test activity",
                Description = "Test description",
                Date = DateTime.UtcNow.AddDays(1),
                Category = "Sports",
                City = "Vilnius",
                Venue = "Test venue"
            }
        };

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Activity not found", result.Error);

        mapper.Verify(
            x => x.Map(
                It.IsAny<Activity>(),
                It.IsAny<Activity>()),
            Times.Never);
    }
}