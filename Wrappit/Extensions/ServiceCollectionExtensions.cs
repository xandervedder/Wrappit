using Microsoft.Extensions.DependencyInjection;
using Wrappit.Configuration;
using Wrappit.Exceptions;
using Wrappit.Messaging;
using Wrappit.Runtime;
using Wrappit.Services;

namespace Wrappit.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EnvHostName = "Wrappit_HostName";
    private const string EnvPortNumber = "Wrappit_Port";
    private const string EnvUserName = "Wrappit_UserName";
    private const string EnvPassword = "Wrappit_Password";
    private const string EnvExchangeName = "Wrappit_ExchangeName";
    private const string EnvQueueName = "Wrappit_QueueName";
    private const string EnvDeliveryLimit = "Wrappit_DeliveryLimit";
    
    public static IServiceCollection AddWrappit(this IServiceCollection collection)
    {
        var options = new WrappitOptions
        {
            HostName = Environment.GetEnvironmentVariable(EnvHostName) ?? throw MissingEnvVariable(EnvHostName),
            Port = int.Parse(Environment.GetEnvironmentVariable(EnvPortNumber) ?? throw MissingEnvVariable(EnvPortNumber)),
            UserName = Environment.GetEnvironmentVariable(EnvUserName) ?? throw MissingEnvVariable(EnvUserName),
            Password = Environment.GetEnvironmentVariable(EnvPassword) ?? throw MissingEnvVariable(EnvPassword),
            ExchangeName = Environment.GetEnvironmentVariable(EnvExchangeName) ?? throw MissingEnvVariable(EnvExchangeName),
            QueueName = Environment.GetEnvironmentVariable(EnvQueueName) ?? throw MissingEnvVariable(EnvQueueName),
        };

        var deliveryLimit = Environment.GetEnvironmentVariable(EnvDeliveryLimit);
        if (deliveryLimit == null)
        {
            return AddWrappit(collection, options);
        }
        
        options.DeliveryLimit = int.Parse(deliveryLimit);
        return AddWrappit(collection, options);
    }
    
    public static IServiceCollection AddWrappit(this IServiceCollection collection, IWrappitOptions options)
    {
        collection.AddSingleton(_ => options);
        collection.AddSingleton<IWrappitContext, WrappitContext>();
        collection.AddTransient<IBasicReciever, BasicReciever>();
        collection.AddTransient<IBasicSender, BasicSender>();
        collection.AddTransient<IWrappitPublisher, WrappitPublisher>();
        collection.AddTransient<ITypeFinder, TypeFinder>();
        collection.AddTransient<IRuntimeBuilder, RuntimeBuilder>();
        collection.AddTransient(provider =>
            provider.GetRequiredService<IRuntimeBuilder>().DiscoverAndRegisterAllEventListeners().Build());
        collection.AddHostedService<WrappitHostedService>();

        return collection;
    }
    
    private static WrappitException MissingEnvVariable(string variable)
    {
        return new WrappitException($"Environment variable '{variable}' not set.");
    }
}
