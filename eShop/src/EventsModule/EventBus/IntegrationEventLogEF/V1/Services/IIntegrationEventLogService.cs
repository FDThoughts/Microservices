namespace eShop.EventsModule.IntegrationEventLogEF.V1.Services
{
    using Microsoft.EntityFrameworkCore.Storage;    
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using eShop.EventsModule.EventBus.V1.Events;

    public interface IIntegrationEventLogService
    {
        Task<IEnumerable<IntegrationEventLogEntry>> 
            RetrieveEventLogsPendingToPublishAsync(
                Guid transactionId);
        Task SaveEventAsync(IntegrationEvent @event, 
            IDbContextTransaction transaction);
        Task MarkEventAsPublishedAsync(Guid eventId);
        Task MarkEventAsInProgressAsync(Guid eventId);
        Task MarkEventAsFailedAsync(Guid eventId);
    }
}