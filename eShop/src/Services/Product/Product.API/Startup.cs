using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Swashbuckle.AspNetCore.SwaggerGen;
using eShop.EventsModule.EventBus.V1;
using eShop.EventsModule.EventBus.V1.Abstractions;
using eShop.EventsModule.EventBusRabbitMQ.V1;
using eShop.EventsModule.IntegrationEventLogEF.V1;
using eShop.EventsModule.IntegrationEventLogEF.V1.Services;
using eShop.Startup.Swagger;

namespace eShop.Services.Product.API
{
    using V1.IntegrationEvents;
    using V1.Infrastructure;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IContainer ApplicationContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            string connectionString = Configuration[
                "ConnectionStrings:ProductConnection"
            ];
            services
                .AddDbContext<ProductContext>(options => {
                    options.UseSqlServer(connectionString,
                        sqlServerOptionsAction: sqlOptions => {
                            sqlOptions.MigrationsAssembly(
                                typeof(Startup).GetTypeInfo().Assembly.GetName().Name
                            );
                            sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 15, 
                                maxRetryDelay: TimeSpan.FromSeconds(30), 
                                errorNumbersToAdd: null
                            );
                        }
                    );
                });
            services.AddDbContext<IntegrationEventLogContext>(options =>
            {
                options.UseSqlServer(connectionString,
                    sqlServerOptionsAction: sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(
                            typeof(Startup).GetTypeInfo().Assembly.GetName().Name
                        );
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 15, 
                            maxRetryDelay: TimeSpan.FromSeconds(30), 
                            errorNumbersToAdd: null
                        );
                    });
            });
            services.AddControllers()
                .AddJsonOptions(opts => {
                    opts.JsonSerializerOptions.IgnoreNullValues = true;
                }).AddNewtonsoftJson();
            services.AddCors(options => {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
            services.AddApiVersioning(
                options => {
                    options.ReportApiVersions = true;
                }
            );
            services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
                sp => (DbConnection c) => new IntegrationEventLogService(c));

            services.AddTransient<IProductIntegrationEventService, ProductIntegrationEventService>();
            var subscriptionClientName = Configuration["EventBus:SubscriptionClientName"];
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = 
                    sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = 
                    sp.GetRequiredService<ILifetimeScope>();
                var logger = 
                    sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = 
                    sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(
                    Configuration["EventBus:EventBusRetryCount"]))
                {
                    retryCount = 
                        int.Parse(Configuration["EventBus:EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(
                    rabbitMQPersistentConnection, logger, iLifetimeScope, 
                    eventBusSubcriptionsManager, subscriptionClientName, 
                    retryCount
                );
            });
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = 
                    sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                var factory = new ConnectionFactory()
                {
                    HostName = Configuration["EventBus:EventBusConnection"],
                    DispatchConsumersAsync = true
                };

                if (!string.IsNullOrEmpty(Configuration["EventBus:EventBusUserName"]))
                {
                    factory.UserName = Configuration["EventBus:EventBusUserName"];
                }

                if (!string.IsNullOrEmpty(Configuration["EventBus:EventBusPassword"]))
                {
                    factory.Password = Configuration["EventBus:EventBusPassword"];
                }

                int port;

                if (!string.IsNullOrEmpty(Configuration["EventBus:EventBusPort"]) &&
                    int.TryParse(Configuration["EventBus:EventBusPort"], out port))
                {
                    factory.Port = port;
                }

                if (!string.IsNullOrEmpty(Configuration["EventBus:EventBusVirtualHost"]))
                {
                    factory.VirtualHost = Configuration["EventBus:EventBusVirtualHost"];
                }


                var retryCount = 5;
                if (!string.IsNullOrEmpty(Configuration["EventBus:EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBus:EventBusRetryCount"]);
                }

                return new DefaultRabbitMQPersistentConnection(
                    factory, logger, retryCount
                );
            });
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy())
                .AddSqlServer(
                    connectionString,
                    name: "ProductDB-check",
                    tags: new string[] { "productdb" })
                .AddRabbitMQ(
                    $"amqp://{Configuration["EventBus:EventBusConnection"]}",
                    name: "catalog-rabbitmqbus-check",
                    tags: new string[] { "rabbitmqbus" });
            services.AddVersionedApiExplorer(options => {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options => {
                options.OperationFilter<SwaggerDefaultValues>();
            });

            var container = new ContainerBuilder();
            container.Populate(services);
            this.ApplicationContainer = container.Build();
            return new AutofacServiceProvider(this.ApplicationContainer);            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, 
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider provider,
            IServiceProvider services
        )
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("CorsPolicy");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI(options => {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant()
                    );
                }
            });

            ProductContextSeed.SeedAsync(
                services.GetRequiredService<ProductContext>(),
                services.GetRequiredService<ILogger<ProductContextSeed>>()
            ).ConfigureAwait(true);
        }
    }
}
