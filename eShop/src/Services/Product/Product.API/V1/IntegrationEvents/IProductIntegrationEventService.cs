namespace eShop.Services.Product.API.V1.IntegrationEvents
{
    using eShop.EventsModule.EventBus.V1.Events;
    using System.Threading.Tasks;

    public interface IProductIntegrationEventService
    {
        Task SaveEventAndProductContextChangesAsync(IntegrationEvent evt);
        Task PublishThroughEventBusAsync(IntegrationEvent evt);
    }
}