using MarketPlaceService.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace MarketPlaceService.Data {
    public static class DbInitializer {
        public static void Initialize(MarketPlaceContext context) {
            //using Microsoft.EntityFrameworkCore;
            context.Database.Migrate();

            // Look for any products.
            if (context.Products.Any()) {
                return;   // DB has been seeded
            }

            var products = new Product[] {
                new Product { Name = "Product 1", Description = "First Sample Product", Price = 1234, UserName="alice@gmail.com" },
                new Product { Name = "Product 2", Description = "Lorem Ipsum", Price = 555 , UserName="alice@gmail.com"},
                new Product { Name = "Product 3", Description = "Third Sample Product", Price = 333 , UserName="bob@gmail.com"},
                new Product { Name = "Product 4", Description = "Fourth Sample Product", Price = 44 , UserName="bob@gmail.com"}
            };

            foreach (var product in products) {
                context.Products.Add(product);
            }

            context.SaveChanges();
        }
    }
}