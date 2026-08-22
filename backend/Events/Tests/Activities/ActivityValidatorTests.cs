using Application.Activities;
using Domain;
using FluentValidation.TestHelper;

namespace Tests.Activities;

public class ActivityValidatorTests
{
    private readonly ActivityValidator _validator = new();

    private static Activity CreateValidActivity()
    {
        return new Activity
        {
            Title = "Test activity",
            Description = "Test description",
            Date = DateTime.UtcNow.AddDays(1),
            Category = "Sports",
            City = "Vilnius",
            Venue = "Sports Hall"
        };
    }

    [Fact]
    public void ValidActivity_ShouldNotHaveValidationErrors()
    {
        var activity = CreateValidActivity();

        var result = _validator.TestValidate(activity);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptyTitle_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.Title = string.Empty;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void EmptyDescription_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.Description = string.Empty;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void EmptyDate_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.Date = default;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void EmptyCategory_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.Category = string.Empty;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void EmptyCity_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.City = string.Empty;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void EmptyVenue_ShouldHaveValidationError()
    {
        var activity = CreateValidActivity();
        activity.Venue = string.Empty;

        var result = _validator.TestValidate(activity);

        result.ShouldHaveValidationErrorFor(x => x.Venue);
    }
}