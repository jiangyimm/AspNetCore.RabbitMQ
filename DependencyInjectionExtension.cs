using Jiangyi.EventBus.Abstractions;
using Jiangyi.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jiangyi.EventBus;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
           {
               var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

               var factory = new ConnectionFactory
               {
                   Uri = new Uri("amqp://guest:guest@localhost:5672/"),
                   DispatchConsumersAsync = true
               };

               //    var factory = new ConnectionFactory()
               //    {
               //        HostName = configuration.GetRequiredConnectionString("EventBus"),
               //        DispatchConsumersAsync = true
               //    };

               //    if (!string.IsNullOrEmpty(eventBusSection["UserName"]))
               //    {
               //        factory.UserName = eventBusSection["UserName"];
               //    }

               //    if (!string.IsNullOrEmpty(eventBusSection["Password"]))
               //    {
               //        factory.Password = eventBusSection["Password"];
               //    }

               //var retryCount = eventBusSection.GetValue("RetryCount", 5);

               var retryCount = 5;

               return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
           });

        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            //var subscriptionClientName = eventBusSection.GetRequiredValue("SubscriptionClientName");

            var subscriptionClientName = "Order";

            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            //var retryCount = eventBusSection.GetValue("RetryCount", 5);

            var retryCount = 5;

            return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
        });

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();


        // services.AddTransient<OrderIntegrationEventHandler1>();
        // services.AddTransient<OrderIntegrationEventHandler2>();



        return services;
    }
}