using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Tests;

public static class TestDbContextFactory
{
    public static DataContext Create()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new DataContext(options);
    }
}