namespace Product.UnitTests.V1
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Xunit;
    using eShop.Services.Product.API.V1.Controllers;
    using eShop.Services.Product.API.V1.Infrastructure;
    using eShop.Services.Product.API.V1.Models;
    using eShop.Services.Product.API.V1.IntegrationEvents;

    public class ProductControllerTest
    {
        private readonly DbContextOptions<ProductContext> _dbOptions;

        public ProductControllerTest()
        {
            _dbOptions = new DbContextOptionsBuilder<ProductContext>()
                .UseInMemoryDatabase(
                    databaseName: "inmemory"
                ).Options;
            using (var dbContext = new ProductContext(
                _dbOptions
            )) {
                dbContext.AddRange(GetTestProducts());
                dbContext.SaveChanges();
            }
        }

        [Fact]
        public async Task Get_Product_items_success()
        {
            // Arrange
            var productContext = new ProductContext(_dbOptions);
            var integrationEventServiceMock = 
                new Mock<IProductIntegrationEventService>();

            // Act
            var productController = new ProductController(
                productContext,
                integrationEventServiceMock.Object
            );
            var actionResult =  await productController
                .GetAsync();

            // Assert
            var result = Assert.IsAssignableFrom<OkObjectResult>(
                actionResult
            );
            Assert.IsType<List<ProductItem>>(
                result.Value
            );
            var list = Assert.IsAssignableFrom<List<ProductItem>>(
                result.Value
            );
            Assert.Equal(GetTestProducts(), list);
        }

        private List<ProductItem> GetTestProducts()
        {
            return new List<ProductItem>()
            {
                new ProductItem {
                    ProductId = 1,
                    Name = "testName1",
                    Category = "testCategory1",
                    Description = "testDescription1",
                    Price = 100
                },
                new ProductItem {
                    ProductId = 2,
                    Name = "testName2",
                    Category = "testCategory2",
                    Description = "testDescription2",
                    Price = 250
                },
                new ProductItem {
                    ProductId = 3,
                    Name = "testName3",
                    Category = "testCategory1",
                    Description = "testDescription3",
                    Price = 50
                },
                new ProductItem {
                    ProductId = 4,
                    Name = "testName4",
                    Category = "testCategory2",
                    Description = "testDescription4",
                    Price = 150
                }
            };
        }
    }
}
