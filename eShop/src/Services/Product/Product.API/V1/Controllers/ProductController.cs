namespace eShop.Services.Product.API.V1.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using V1.Infrastructure;
    using V1.Models;
    using V1.IntegrationEvents;
    using V1.IntegrationEvents.Events;

    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiversion}/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly IProductIntegrationEventService 
            _integrationEventService;

        public ProductController(ProductContext context,
            IProductIntegrationEventService integrationEventService)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _integrationEventService = integrationEventService ??
                throw new ArgumentNullException(nameof(integrationEventService));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductItem>),
            (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public virtual async Task<IActionResult> GetAsync()
        {
            var items = await _context.ProductItems
                .OrderBy(p => p.Name)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(ProductItem), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ProductItem>> ItemByIdAsync(
            int id
        )
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _context.ProductItems.SingleOrDefaultAsync(
                ci => ci.ProductId == id
            );
            if (item != null)
            {
                return item;
            }

            return NotFound();
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> CreateAsync(
            [FromBody]ProductItem product
        ) {
            var item = new ProductItem
            {
                Name = product.Name,
                Category = product.Category,
                Description = product.Description,
                Price = product.Price
            };
            _context.ProductItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> UpdateProductAsync(
            [FromBody]ProductItem productToUpdate
        )
        {
            try
            {
                var item = await _context.ProductItems.AsNoTracking()
                    .SingleOrDefaultAsync(
                        i => i.ProductId == productToUpdate.ProductId
                    );

                if (item == null)
                {
                    return NotFound(new
                    {
                        Message =
                        $"Item with id {productToUpdate.ProductId} not found."
                    });
                }

                var oldPrice = item.Price;
                var raiseProductPriceChangedEvent =
                    oldPrice != productToUpdate.Price;

                item = productToUpdate;
                _context.ProductItems.Update(item);

                if (raiseProductPriceChangedEvent)
                {
                    var priceChangedEvent = new ProductPriceChangedIntegrationEvent(
                        item.ProductId, productToUpdate.Price, oldPrice);


                    await _integrationEventService
                        .SaveEventAndProductContextChangesAsync(priceChangedEvent);


                    await _integrationEventService.PublishThroughEventBusAsync(
                        priceChangedEvent);
                }
                else
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(productToUpdate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}