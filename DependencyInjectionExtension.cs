using Jiangyi.EventBus.Abstractions;
using Jiangyi.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Jiangyi.EventBus;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services,
        Action<EventBusOption> option)
    {
        AddOption(services, option);
        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
           {
               var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
               var config = sp.GetRequiredService<EventBusOption>();
               var factory = new ConnectionFactory
               {
                   Uri = config.Uri,
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

               var retryCount = config.RetryCount;

               return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
           });

        services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

            var config = sp.GetRequiredService<EventBusOption>();
            var subscriptionClientName = config.SubscriptionClientName;
            var retryCount = config.RetryCount;

            return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
        });

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    private static void AddOption(this IServiceCollection serviceCollection, Action<EventBusOption> option)
    {
        if (option == null)
            throw new ArgumentNullException(nameof(option));
        if (serviceCollection == null)
            throw new ArgumentNullException(nameof(serviceCollection));
        var serviceDescriptor = serviceCollection.LastOrDefault(x =>
        {
            if (x.ServiceType == typeof(EventBusOption))
                return x.ImplementationInstance != null;
            return false;
        });
        var implementationInstance = (EventBusOption)serviceDescriptor?.ImplementationInstance ?? new EventBusOption();
        option(implementationInstance);
        if (serviceDescriptor == null)
            serviceCollection.AddSingleton(implementationInstance);
    }

    public class EventBusOption
    {
        public Uri Uri { get; set; }
        public string SubscriptionClientName { get; set; }
        public int RetryCount { get; set; }
    }
}