namespace eShop.Services.Product.API.V1.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using V1.Models;

    public class ProductContext: DbContext
    {
        public ProductContext(
            DbContextOptions<ProductContext> options
        ) : base(options) { }

        public virtual DbSet<ProductItem> ProductItems { get; set; }

        protected override void OnModelCreating(
            ModelBuilder builder
        ) {
            builder.Entity<ProductItem>();
        }
    }
}