using project.Data;
using project.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        // Create a new scope to retrieve scoped services
        using (var scope = serviceProvider.CreateScope())
        {
            // Get the DbContext instance using the scope's service provider
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Now you can use 'context' to seed data
            // Look for any products.
            if (context.Products.Any())
            {
                return; // DB has been seeded
            }

            context.Products.AddRange(
            new Product
            {
                Name = "Sourdough Bread",
                Price = 4.99M,
                Description = "A loaf of fresh sourdough bread.",
               // ImageUrl = "/images/Croissant.jpg",
                Category = "Bread" // Provide a default category value
            },
            new Product
            {
                Name = "Croissant",
                Price = 2.99M,
                //ImageUrl = "/images/Croissant.jpg",
                Description = "Buttery and flaky croissant.",
                Category = "Pastry" // Provide a default category value
            }
            // Add as many products as you'd like here

            );
            context.SaveChanges();
        }
    }
}
