using FluentAssertions;
using NaarNoor.Infrastructure.Data;
using Xunit;

namespace NaarNoor.Infrastructure.Tests.Infrastructure;

public class ApplicationDbContextFactoryTests
{
    [Fact]
    public async Task CreateDbContext_WithValidConnectionArgs_ReturnsContext()
    {
        // The ApplicationDbContextFactory in Infrastructure is a design-time factory
        // that is used by EF Core CLI tooling (migrations). We can instantiate it
        // and verify it creates a context correctly.
        var factory = new ApplicationDbContextFactory();

        ApplicationDbContext? context = null;
        Func<Task> act = async () =>
        {
            context = factory.CreateDbContext(Array.Empty<string>());
        };

        await act.Should().NotThrowAsync("The design-time factory should create a context without requiring a running database");
        context.Should().NotBeNull();
        context!.Dispose();
    }

    [Fact]
    public void CreateDbContext_ProducesContextWithExpectedDbSets()
    {
        var factory = new ApplicationDbContextFactory();
        using var context = factory.CreateDbContext(Array.Empty<string>());

        context.MenuItems.Should().NotBeNull();
        context.Chefs.Should().NotBeNull();
        context.Reviews.Should().NotBeNull();
        context.Reservations.Should().NotBeNull();
        context.Orders.Should().NotBeNull();
        context.ContactInquiries.Should().NotBeNull();
    }
}
