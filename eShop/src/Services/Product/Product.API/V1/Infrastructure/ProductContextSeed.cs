namespace eShop.Services.Product.API.V1.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Retry;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using V1.Models;

    public class ProductContextSeed
    {
        public static async Task SeedAsync(ProductContext context,
            ILogger<ProductContextSeed> logger
        )
        {
            context.Database.Migrate();
            var policy = CreatePolicy(logger, nameof(ProductContextSeed));

            await policy.ExecuteAsync(async () =>
            {
                if (!context.ProductItems.Any())
                {
                    await context.ProductItems.AddRangeAsync(
                        GetInitialItems()
                    );
                    await context.SaveChangesAsync();
                }
            });
        }

        private static AsyncRetryPolicy CreatePolicy(
            ILogger<ProductContextSeed> logger, string prefix,int retries = 3
        )
        {
            return Policy.Handle<SqlException>().
                WaitAndRetryAsync(
                    retryCount: retries,
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, timeSpan, retry, ctx) =>
                    {
                        logger.LogWarning(exception, 
                            $"[{prefix}] Exception {exception.GetType().Name} with message {exception.Message} detected on attempt {retry} of {retries}", 
                            prefix, 
                            exception.GetType().Name, 
                            exception.Message, 
                            retry, 
                            retries
                        );
                    }
                );
        }

        private static IEnumerable<ProductItem> GetInitialItems()
        {
            return new List<ProductItem>()
            {
                new ProductItem {
                    Name = "testName",
                    Category = "testCategory",
                    Description = "testDescription",
                    Price = 100
                }
            };
        }
    }
}