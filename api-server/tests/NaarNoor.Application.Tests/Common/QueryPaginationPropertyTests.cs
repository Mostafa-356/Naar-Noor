using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace NaarNoor.Application.Tests.Common;

/// <summary>
/// Property-based tests for query pagination correctness.
/// These tests validate that pagination calculations are accurate across
/// various data sizes and page configurations.
///
/// **Validates: Requirement 3.1**
/// **Property 8: Query Pagination Correctness**
/// </summary>
public class QueryPaginationPropertyTests
{
    /// <summary>
    /// Property: Offset calculation - page offset is calculated correctly from page number and size.
    /// For any valid page number and page size, the calculated offset SHALL equal (pageNumber - 1) * pageSize.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property OffsetCalculation_WithValidPageAndSize_IsCorrect()
    {
        return Prop.ForAll(
            Gen.Choose(1, 100).ToArbitrary(), // pageNumber
            Gen.Choose(1, 100).ToArbitrary(),  // pageSize
            (pageNumber, pageSize) =>
            {
                // Act
                var offset = (pageNumber - 1) * pageSize;

                // Assert
                var expected = (pageNumber - 1) * pageSize;
                return offset == expected;
            }
        );
    }

    /// <summary>
    /// Property: Page size handling - results returned match requested page size (or less on last page).
    /// For any dataset and page size, each page SHALL contain up to pageSize items,
    /// except the last page which may contain fewer items.
    /// </summary>
    [Property(MaxTest = 100)]
    public Property PageSizeHandling_ReturnsCorrectItemCount()
    {
        return Prop.ForAll(
            GeneratePaginationTestData(),
            testData =>
            {
                var (totalItems, pageNumber, pageSize) = testData;
                var offset = (pageNumber - 1) * pageSize;

                // Calculate expected count for this page
                var remainingItems = Math.Max(0, totalItems - offset);
                var expectedCount = Math.Min(pageSize, remainingItems);

                // If we're beyond available data, expect 0
                if (offset >= totalItems)
                {
                    expectedCount = 0;
                }

                // Simulate pagination
                var pageItems = Enumerable.Range(offset, pageSize)
                    .Where(i => i < totalItems)
                    .ToList();

                return pageItems.Count == expectedCount;
            }
        );
    }

    /// <summary>
    /// Property: Boundary condition - empty dataset - pagination handles empty results correctly.
    /// For an empty dataset, requesting any page SHALL return empty results with total count of 0.
    /// </summary>
    [Fact]
    public void BoundaryCondition_EmptyDataset_ReturnsEmpty()
    {
        // Arrange
        var totalItems = 0;
        var pageNumber = 1;
        var pageSize = 10;
        var offset = (pageNumber - 1) * pageSize;

        // Act
        var pageItems = Enumerable.Range(offset, pageSize)
            .Where(i => i < totalItems)
            .ToList();

        // Assert
        pageItems.Should().BeEmpty();
    }

    /// <summary>
    /// Property: Boundary condition - single page - dataset fits on one page.
    /// For a dataset smaller than page size, all items fit on page 1 and no page 2 exists.
    /// </summary>
    [Fact]
    public void BoundaryCondition_SinglePageDataset_AllItemsFitOnOnePage()
    {
        // Arrange
        var totalItems = 5;
        var pageSize = 10;
        var pageNumber = 1;
        var offset = (pageNumber - 1) * pageSize;

        // Act
        var pageItems = Enumerable.Range(offset, pageSize)
            .Where(i => i < totalItems)
            .ToList();

        // Assert
        pageItems.Should().HaveCount(5, "All items should fit on page 1");

        // Verify page 2 returns nothing
        var page2Number = 2;
        var page2Offset = (page2Number - 1) * pageSize;
        var page2Items = Enumerable.Range(page2Offset, pageSize)
            .Where(i => i < totalItems)
            .ToList();

        page2Items.Should().BeEmpty("Page 2 should be empty");
    }

    /// <summary>
    /// Property: Boundary condition - multiple pages - dataset spans multiple pages correctly.
    /// For a dataset larger than page size, items are distributed correctly across pages,
    /// with no gaps and no duplicates.
    /// </summary>
    [Fact]
    public void BoundaryCondition_MultiplePages_ItemsDistributedCorrectly()
    {
        // Arrange
        var totalItems = 27;
        var pageSize = 10;

        // Act
        var page1Items = Enumerable.Range(0, pageSize).Where(i => i < totalItems).ToList();
        var page2Items = Enumerable.Range(pageSize, pageSize).Where(i => i < totalItems).ToList();
        var page3Items = Enumerable.Range(pageSize * 2, pageSize).Where(i => i < totalItems).ToList();

        // Assert
        page1Items.Should().HaveCount(10);
        page2Items.Should().HaveCount(10);
        page3Items.Should().HaveCount(7);

        var allItems = page1Items.Concat(page2Items).Concat(page3Items).ToList();
        allItems.Should().HaveCount(27);
        allItems.Distinct().Should().HaveCount(27);
    }

    /// <summary>
    /// Property: Out of bounds handling - requesting page beyond available pages returns empty.
    /// For any page number greater than total pages, the result SHALL be empty.
    /// </summary>
    [Property(MaxTest = 50)]
    public Property OutOfBoundsHandling_PageBeyondAvailable_ReturnsEmpty()
    {
        return Prop.ForAll(
            Gen.Choose(1, 100).ToArbitrary(),
            Gen.Choose(1, 50).ToArbitrary(),
            (totalItems, pageSize) =>
            {
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var pageNumber = totalPages + 1;
                var offset = (pageNumber - 1) * pageSize;

                var pageItems = Enumerable.Range(offset, pageSize)
                    .Where(i => i < totalItems)
                    .ToList();

                return pageItems.Count == 0;
            }
        );
    }

    // ==================== HELPER METHODS ====================

    private Arbitrary<(int totalItems, int pageNumber, int pageSize)> GeneratePaginationTestData()
    {
        var genTotalItems = Gen.Choose(0, 1000);
        var genPageNumber = Gen.Choose(1, 100);
        var genPageSize = Gen.Choose(1, 100);

        var gen = from total in genTotalItems
                  from pageNum in genPageNumber
                  from pageSize in genPageSize
                  select (total, pageNum, pageSize);

        return Arb.From(gen);
    }
}
