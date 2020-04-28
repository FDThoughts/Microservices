namespace eShop.Services.Product.API.V1.IntegrationEvents.Events
{
    using eShop.EventsModule.EventBus.V1.Events;

    public class ProductPriceChangedIntegrationEvent : 
        IntegrationEvent
    {        
        public long ProductId { get; private set; }

        public decimal NewPrice { get; private set; }

        public decimal OldPrice { get; private set; }

        public ProductPriceChangedIntegrationEvent(
            long productId, 
            decimal newPrice, 
            decimal oldPrice
        )
        {
            ProductId = productId;
            NewPrice = newPrice;
            OldPrice = oldPrice;
        }
    }
}