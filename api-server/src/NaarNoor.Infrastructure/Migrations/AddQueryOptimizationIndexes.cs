using Microsoft.EntityFrameworkCore.Migrations;

namespace NaarNoor.Infrastructure.Migrations;

/// <summary>
/// Migration: Add indexes for query optimization on common filtering operations
/// Created: July 1, 2026
/// Purpose: Improve performance of frequently-used queries by adding indexes on commonly filtered columns
/// </summary>
public partial class AddQueryOptimizationIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Index for Reservations.Email - Used in filtering and lookup queries
        migrationBuilder.CreateIndex(
            name: "ix_reservations_email",
            table: "Reservations",
            column: "Email");

        // Index for Reviews.IsApproved - Used for filtering published reviews
        migrationBuilder.CreateIndex(
            name: "ix_reviews_is_approved",
            table: "Reviews",
            column: "IsApproved");

        // Index for MenuItems.Category - Used for category-based filtering
        migrationBuilder.CreateIndex(
            name: "ix_menu_items_category",
            table: "MenuItems",
            column: "Category");

        // Composite Index for Orders (Email, CreatedAt) - Used in user history and reporting
        migrationBuilder.CreateIndex(
            name: "ix_orders_email_created_at",
            table: "Orders",
            columns: new[] { "Email", "CreatedAt" },
            descending: new[] { false, true }); // Email asc, CreatedAt desc

        // Index for Reservations (ReservationDate) - Used for availability queries
        migrationBuilder.CreateIndex(
            name: "ix_reservations_reservation_date",
            table: "Reservations",
            column: "ReservationDate");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_reservations_email",
            table: "Reservations");

        migrationBuilder.DropIndex(
            name: "ix_reviews_is_approved",
            table: "Reviews");

        migrationBuilder.DropIndex(
            name: "ix_menu_items_category",
            table: "MenuItems");

        migrationBuilder.DropIndex(
            name: "ix_orders_email_created_at",
            table: "Orders");

        migrationBuilder.DropIndex(
            name: "ix_reservations_reservation_date",
            table: "Reservations");
    }
}
