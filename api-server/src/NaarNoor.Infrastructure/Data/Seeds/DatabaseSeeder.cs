using Microsoft.EntityFrameworkCore;
using NaarNoor.Domain.Entities;
using NaarNoor.Domain.Enums;

namespace NaarNoor.Infrastructure.Data.Seeds;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();
        await SeedDataAsync(context);
    }

    public static async Task SeedDataAsync(ApplicationDbContext context)
    {
        if (!await context.MenuItems.AnyAsync())
            await SeedMenuItemsAsync(context);

        if (!await context.Chefs.AnyAsync())
            await SeedChefsAsync(context);

        if (!await context.Reviews.AnyAsync())
            await SeedReviewsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedMenuItemsAsync(ApplicationDbContext context)
    {
        var items = new List<MenuItem>
        {
            new() { Name = "Momos (Steamed)", Description = "Handcrafted Himalayan dumplings filled with spiced vegetables or chicken, served with tangy tomato chutney.", Price = 8.95m, Category = MenuCategory.Starters, IsVegetarian = false, SortOrder = 1 },
            new() { Name = "Veg Momos", Description = "Handcrafted dumplings filled with seasoned cabbage, carrots and mushrooms, served with tomato chutney.", Price = 7.95m, Category = MenuCategory.Starters, IsVegetarian = true, IsVegan = true, SortOrder = 2 },
            new() { Name = "Sekuwa", Description = "Himalayan-style flame-grilled skewers of marinated lamb or chicken, seasoned with timur (Sichuan pepper).", Price = 11.95m, Category = MenuCategory.Starters, IsVegetarian = false, SortOrder = 3 },
            new() { Name = "Aloo Tama", Description = "Traditional Nepali curry with bamboo shoots, black-eyed peas and potatoes in a tangy sauce.", Price = 12.95m, Category = MenuCategory.Mains, IsVegetarian = true, IsVegan = true, SortOrder = 1 },
            new() { Name = "Dal Bhat", Description = "Nepal's national dish — lentil soup, steamed rice, seasonal vegetables and achaar (pickle).", Price = 14.95m, Category = MenuCategory.Mains, IsVegetarian = true, SortOrder = 2 },
            new() { Name = "Lamb Rogan Josh", Description = "Slow-braised Himalayan lamb with aromatic spices, served with saffron rice.", Price = 18.95m, Category = MenuCategory.Mains, IsVegetarian = false, SortOrder = 3 },
            new() { Name = "Butter Chicken", Description = "Tender chicken in a rich, creamy tomato-based sauce with fenugreek and cardamom.", Price = 16.95m, Category = MenuCategory.Mains, IsVegetarian = false, SortOrder = 4 },
            new() { Name = "Sel Roti", Description = "Traditional Nepali rice bread, crispy on the outside, soft inside. Served with yoghurt.", Price = 4.50m, Category = MenuCategory.Breads, IsVegetarian = true, SortOrder = 1 },
            new() { Name = "Garlic Naan", Description = "Freshly baked leavened bread with roasted garlic and coriander from our tandoor.", Price = 3.95m, Category = MenuCategory.Breads, IsVegetarian = true, SortOrder = 2 },
            new() { Name = "Kheer", Description = "Creamy Himalayan rice pudding infused with cardamom, rose water and topped with pistachios.", Price = 6.95m, Category = MenuCategory.Desserts, IsVegetarian = true, SortOrder = 1 },
            new() { Name = "Sikarni", Description = "Spiced strained yoghurt dessert with saffron, cardamom and dried fruits.", Price = 5.95m, Category = MenuCategory.Desserts, IsVegetarian = true, SortOrder = 2 },
            new() { Name = "Butter Tea (Po Cha)", Description = "Traditional Tibetan butter tea brewed with Pu-erh tea, yak butter and salt.", Price = 3.95m, Category = MenuCategory.Beverages, IsVegetarian = true, SortOrder = 1 },
            new() { Name = "Masala Chai", Description = "Aromatic spiced tea with ginger, cardamom, cinnamon and steamed milk.", Price = 3.50m, Category = MenuCategory.Beverages, IsVegetarian = true, SortOrder = 2 },
            new() { Name = "Chef's Himalayan Tasting Menu", Description = "A seven-course journey through the Himalayas. Ask your server for today's selection.", Price = 55.00m, Category = MenuCategory.Specials, IsVegetarian = false, SortOrder = 1 },
        };
        await context.MenuItems.AddRangeAsync(items);
    }

    private static async Task SeedChefsAsync(ApplicationDbContext context)
    {
        var chefs = new List<Chef>
        {
            new() { Name = "Aryan Thapa", Title = "Executive Chef", Bio = "Born in Kathmandu, Aryan trained under master chefs across Nepal and India before bringing authentic Himalayan flavours to London. His philosophy: fire is the soul of every dish.", Specialty = "Himalayan Grills & Sekuwa", IsActive = true, SortOrder = 1 },
            new() { Name = "Nisha Gurung", Title = "Head Pastry Chef", Bio = "A native of Pokhara, Nisha blends traditional Newari sweet-making techniques with modern patisserie to create desserts that tell stories.", Specialty = "Himalayan Desserts & Breads", IsActive = true, SortOrder = 2 },
            new() { Name = "Rohan Shrestha", Title = "Sous Chef", Bio = "Rohan's deep knowledge of Tibetan and Sherpa cuisines brings the high-altitude flavours of the mountain communities to every plate.", Specialty = "Tibetan Cuisine & Noodles", IsActive = true, SortOrder = 3 },
        };
        await context.Chefs.AddRangeAsync(chefs);
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context)
    {
        var reviews = new List<Review>
        {
            new() { CustomerName = "Sarah M.", Rating = 5, Comment = "The momos were absolutely divine — light, perfectly seasoned and the chutney had just the right amount of heat. Naar & Noor is now my favourite restaurant in London.", Source = "Google", IsApproved = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new() { CustomerName = "James T.", Rating = 5, Comment = "An extraordinary culinary journey. The lamb sekuwa was cooked to perfection over flame, and the butter tea was a revelation. We'll be back.", Source = "TripAdvisor", IsApproved = true, CreatedAt = DateTime.UtcNow.AddDays(-21) },
            new() { CustomerName = "Priya K.", Rating = 5, Comment = "Dal Bhat like my grandmother used to make, but elevated. The atmosphere is warm and intimate. The staff are incredibly knowledgeable about every dish.", Source = "Google", IsApproved = true, CreatedAt = DateTime.UtcNow.AddDays(-14) },
            new() { CustomerName = "Oliver W.", Rating = 4, Comment = "Stunning food and a beautifully designed space. The tasting menu is a must — seven courses of pure Himalayan magic.", Source = "Direct", IsApproved = true, CreatedAt = DateTime.UtcNow.AddDays(-7) },
        };
        await context.Reviews.AddRangeAsync(reviews);
    }
}
