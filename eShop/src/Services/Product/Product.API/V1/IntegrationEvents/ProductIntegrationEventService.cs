namespace eShop.Services.Product.API.V1.IntegrationEvents
{
    using Microsoft.EntityFrameworkCore;
    using eShop.EventsModule.EventBus.V1.Abstractions;
    using eShop.EventsModule.EventBus.V1.Events;
    using eShop.EventsModule.IntegrationEventLogEF.V1.Services;
    using eShop.EventsModule.IntegrationEventLogEF.V1.Utilities;
    using eShop.Services.Product.API.V1.Infrastructure;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public class ProductIntegrationEventService : 
        IProductIntegrationEventService
    {
        private readonly Func<DbConnection, IIntegrationEventLogService> 
            _integrationEventLogServiceFactory;
        private readonly IEventBus _eventBus;
        private readonly ProductContext _context;
        private readonly IIntegrationEventLogService _eventLogService;
        private readonly ILogger<ProductIntegrationEventService> _logger;

        public ProductIntegrationEventService(
            ILogger<ProductIntegrationEventService> logger,
            IEventBus eventBus,
            ProductContext context,
            Func<DbConnection, IIntegrationEventLogService> 
                integrationEventLogServiceFactory
        )
        {
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
            _context = context ?? 
                throw new ArgumentNullException(nameof(context));
            _integrationEventLogServiceFactory = 
                integrationEventLogServiceFactory ?? 
                    throw new ArgumentNullException(
                        nameof(integrationEventLogServiceFactory));
            _eventBus = eventBus ?? 
                throw new ArgumentNullException(nameof(eventBus));
            _eventLogService = _integrationEventLogServiceFactory(
                _context.Database.GetDbConnection());
        }

        public async Task PublishThroughEventBusAsync(IntegrationEvent evt)
        {
            try
            {
                _logger.LogInformation("----- Publishing integration event: {IntegrationEventId_published} - ({@IntegrationEvent})", evt.Id, evt);

                await _eventLogService.MarkEventAsInProgressAsync(evt.Id);
                _eventBus.Publish(evt);
                await _eventLogService.MarkEventAsPublishedAsync(evt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} - ({@IntegrationEvent})", evt.Id, evt);
                await _eventLogService.MarkEventAsFailedAsync(evt.Id);
            }
        }

        public async Task SaveEventAndProductContextChangesAsync(
            IntegrationEvent evt)
        {
            _logger.LogInformation("----- ProductIntegrationEventService - Saving changes and integrationEvent: {IntegrationEventId}", evt.Id);

            await ResilientTransaction.New(_context).ExecuteAsync(async () =>
            {
                await _context.SaveChangesAsync();
                await _eventLogService.SaveEventAsync(evt, _context.Database.CurrentTransaction);
            });
        }
    }
}