using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NaarNoor.Application.Reviews.Queries.GetApprovedReviews;
using NaarNoor.Domain.Entities;
using NaarNoor.Infrastructure.Data;
using NaarNoor.Infrastructure.Repositories;
using Xunit;

namespace NaarNoor.Application.Tests.Reviews;

public class GetApprovedReviewsQueryHandlerTests : IAsyncLifetime
{
    private ApplicationDbContext _context = null!;
    private GetApprovedReviewsQueryHandler _handler = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("GetReviews_" + Guid.NewGuid())
            .Options;
        _context = new ApplicationDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        var unitOfWork = new UnitOfWork(_context);
        _handler = new GetApprovedReviewsQueryHandler(unitOfWork);
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    [Fact]
    public async Task Handle_ReturnsOnlyApprovedReviews()
    {
        _context.Reviews.AddRange(
            new Review { CustomerName = "Approved 1", Rating = 5, Comment = "Great!", Source = "Google", IsApproved = true },
            new Review { CustomerName = "Approved 2", Rating = 4, Comment = "Good", Source = "Google", IsApproved = true },
            new Review { CustomerName = "Pending", Rating = 3, Comment = "Ok", Source = "Direct", IsApproved = false }
        );
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.CustomerName.StartsWith("Approved"));
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        var result = await _handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NoApprovedReviews_ReturnsEmptyList()
    {
        _context.Reviews.AddRange(
            new Review { CustomerName = "Pending 1", Rating = 3, Comment = "Meh", Source = "Direct", IsApproved = false },
            new Review { CustomerName = "Pending 2", Rating = 2, Comment = "Nope", Source = "Yelp", IsApproved = false }
        );
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReviewDtos_HaveCorrectFields()
    {
        var createdAt = DateTime.UtcNow.AddDays(-5);
        _context.Reviews.Add(new Review
        {
            CustomerName = "James T.",
            Rating = 5,
            Comment = "Outstanding food!",
            Source = "TripAdvisor",
            IsApproved = true,
            CreatedAt = createdAt
        });
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        var dto = result[0];
        dto.CustomerName.Should().Be("James T.");
        dto.Rating.Should().Be(5);
        dto.Comment.Should().Be("Outstanding food!");
        dto.Source.Should().Be("TripAdvisor");
    }

    [Fact]
    public async Task Handle_MultipleApprovedReviews_OrderedByCreatedAtDescending()
    {
        var older = DateTime.UtcNow.AddDays(-10);
        var newer = DateTime.UtcNow.AddDays(-1);

        _context.Reviews.AddRange(
            new Review { CustomerName = "Older", Rating = 4, Comment = "Good", Source = "Google", IsApproved = true, CreatedAt = older },
            new Review { CustomerName = "Newer", Rating = 5, Comment = "Great", Source = "Google", IsApproved = true, CreatedAt = newer }
        );
        await _context.SaveChangesAsync();

        var result = await _handler.Handle(new GetApprovedReviewsQuery(), CancellationToken.None);

        result[0].CustomerName.Should().Be("Newer", "Most recent reviews should come first");
        result[1].CustomerName.Should().Be("Older");
    }
}
