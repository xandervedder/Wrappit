using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Wrappit.Configuration;
using Wrappit.Exceptions;
using Wrappit.Extensions;
using Wrappit.Messaging;
using Wrappit.Routing;
using Wrappit.Runtime;
using Wrappit.Services;

namespace Wrappit.Test.Extensions;

[TestClass]
public class ServiceCollectionExtensionTest
{
    [TestCleanup]
    public void Cleanup()
    {
        Environment.SetEnvironmentVariable("Wrappit_HostName", null);
        Environment.SetEnvironmentVariable("Wrappit_Port", null);
        Environment.SetEnvironmentVariable("Wrappit_UserName", null);
        Environment.SetEnvironmentVariable("Wrappit_Password", null);
        Environment.SetEnvironmentVariable("Wrappit_ExchangeName", null);
        Environment.SetEnvironmentVariable("Wrappit_QueueName", null);
    }

    [TestMethod]
    public void ServiceCollectionExtensions_ShouldContainNecessaryDependencies()
    {
        // Arrange
        var collection = new ServiceCollection().AddWrappit(new WrappitOptions());

        // Act
        bool ShouldContainOptions(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Singleton &&
            s.ServiceType == typeof(IWrappitOptions) && 
            s.ImplementationType == null;

        bool ShouldContainContext(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Singleton &&
            s.ServiceType == typeof(IWrappitContext) &&
            s.ImplementationType == typeof(WrappitContext);

        bool ShouldContainBasicReciever(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(IBasicReciever) &&
            s.ImplementationType == typeof(BasicReciever);

        bool ShouldContainBasicSender(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(IBasicSender) &&
            s.ImplementationType == typeof(BasicSender);

        bool ShouldContainPublisher(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient && 
            s.ServiceType == typeof(IWrappitPublisher) &&
            s.ImplementationType == typeof(WrappitPublisher);

        bool ShouldContainTypeFinder(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(ITypeFinder) &&
            s.ImplementationType == typeof(TypeFinder);

        bool ShouldContainRuntimeBuilder(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient &&
            s.ServiceType == typeof(IRuntimeBuilder) &&
            s.ImplementationType == typeof(RuntimeBuilder);

        bool ShouldContainRouter(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Transient && 
            s.ServiceType == typeof(IWrappitRouter) &&
            s.ImplementationType == null;

        bool ShouldContainHostedService(ServiceDescriptor s) =>
            s.Lifetime == ServiceLifetime.Singleton &&
            s.ServiceType == typeof(IHostedService) &&
            s.ImplementationType == typeof(WrappitHostedService);
        
        // Assert
        Assert.IsTrue(collection.Any(ShouldContainOptions));
        Assert.IsTrue(collection.Any(ShouldContainContext));
        Assert.IsTrue(collection.Any(ShouldContainBasicReciever));
        Assert.IsTrue(collection.Any(ShouldContainBasicSender));
        Assert.IsTrue(collection.Any(ShouldContainPublisher));
        Assert.IsTrue(collection.Any(ShouldContainTypeFinder));
        Assert.IsTrue(collection.Any(ShouldContainRuntimeBuilder));
        Assert.IsTrue(collection.Any(ShouldContainRouter));
        Assert.IsTrue(collection.Any(ShouldContainHostedService));
    }

    [TestMethod]
    public void ServiceCollectionExtensions_ShouldContainWrappitOptions()
    {
        // Arrange
        var collection = new ServiceCollection().AddWrappit(new WrappitOptions());
    
        // Act
        var options = collection.BuildServiceProvider().GetService<IWrappitOptions>()!;

        // Assert
        Assert.AreEqual("Wrappit.DefaultExchangeName", options.ExchangeName);
    }

    [TestMethod]
    public void ServiceCollectionExtensions_WhenRequiringRuntimeBuilder_ShouldFail()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices((_, services) =>
        {
            services.AddScoped<ILogger, NullLogger<WrappitContext>>();
            services.AddWrappit(new WrappitOptions());
        });
        var app = builder.Build();
        
        // Act
        void ShouldFail() => app.Run();

        // Assert
        // Note: this fails, because the RuntimeBuilder looks at the (intentionally) faulty EventListeners.
        Assert.ThrowsException<WrappitException>(ShouldFail);
    }

    [TestMethod]
    public void ServiceCollectionExtensions_WithoutOptions_ShouldNotThrow()
    {
        // Arrange
        var collection = new ServiceCollection();
        Environment.SetEnvironmentVariable("Wrappit_HostName", "localhost");
        Environment.SetEnvironmentVariable("Wrappit_Port", "1234");
        Environment.SetEnvironmentVariable("Wrappit_UserName", "Mr.X");
        Environment.SetEnvironmentVariable("Wrappit_Password", "1234");
        Environment.SetEnvironmentVariable("Wrappit_ExchangeName", "Wrappit.Exchange");
        Environment.SetEnvironmentVariable("Wrappit_QueueName", "Wrappit.Queue");

        // Act
        collection.AddWrappit();

        // Assert
        var options = collection.BuildServiceProvider().GetService<IWrappitOptions>()!;
        Assert.AreEqual("localhost", options.HostName);
        Assert.AreEqual(1234, options.Port);
        Assert.AreEqual("Mr.X", options.UserName);
        Assert.AreEqual("1234", options.Password);
        Assert.AreEqual("Wrappit.Exchange", options.ExchangeName);
        Assert.AreEqual("Wrappit.Queue", options.QueueName);
        // Optional
        Assert.AreEqual(10, options.DeliveryLimit);
    }

    [TestMethod]
    public void ServiceCollectionExtensions_WithNoEnvVariables_ShouldFail()
    {
        // Arrange
        var collection = new ServiceCollection();

        // Act
        void ShouldFail() => collection.AddWrappit();

        // Assert
        Assert.ThrowsException<WrappitException>(ShouldFail);
    }

    [TestMethod]
    [DataRow(null, "1234", "Mr.X", "1234", "Wrappit.Exchange", "Wrappit.Queue", "Environment variable 'Wrappit_HostName' not set.")]
    [DataRow("localhost", null, "Mr.X", "1234", "Wrappit.Exchange", "Wrappit.Queue", "Environment variable 'Wrappit_Port' not set.")]
    [DataRow("localhost", "1234", null, "1234", "Wrappit.Exchange", "Wrappit.Queue", "Environment variable 'Wrappit_UserName' not set.")]
    [DataRow("localhost", "1234", "Mr.X", null, "Wrappit.Exchange", "Wrappit.Queue", "Environment variable 'Wrappit_Password' not set.")]
    [DataRow("localhost", "1234", "Mr.X", "1234", null, "Wrappit.Queue", "Environment variable 'Wrappit_ExchangeName' not set.")]
    [DataRow("localhost", "1234", "Mr.X", "1234", "Wrappit.Exchange", null, "Environment variable 'Wrappit_QueueName' not set.")]
    public void ServiceCollectionExtensions_WithGivenEnvVariables_ShouldThrow(
        string hostName, 
        string port,
        string username,
        string password, 
        string exchangeName, 
        string queueName,
        string message)
    {
        // Arrange
        var collection = new ServiceCollection();
        Environment.SetEnvironmentVariable("Wrappit_HostName", hostName);
        Environment.SetEnvironmentVariable("Wrappit_Port", port);
        Environment.SetEnvironmentVariable("Wrappit_UserName", username);
        Environment.SetEnvironmentVariable("Wrappit_Password", password);
        Environment.SetEnvironmentVariable("Wrappit_ExchangeName", exchangeName);
        Environment.SetEnvironmentVariable("Wrappit_QueueName", queueName);
        
        // Act
        void ShouldFail() => collection.AddWrappit();

        // Assert
        var exception = Assert.ThrowsException<WrappitException>(ShouldFail);
        Assert.AreEqual(exception.Message, message);
    }

    [TestMethod]
    public void ServiceCollectionExtensions_WhenGivingOptionalDeliveryLimit_ShouldReturnCorrectDeliveryLimit()
    {
        // Arrange
        var collection = new ServiceCollection();
        Environment.SetEnvironmentVariable("Wrappit_HostName", "Test");
        Environment.SetEnvironmentVariable("Wrappit_Port", "200");
        Environment.SetEnvironmentVariable("Wrappit_UserName", "guest");
        Environment.SetEnvironmentVariable("Wrappit_Password", "guest");
        Environment.SetEnvironmentVariable("Wrappit_ExchangeName", "TestExchange");
        Environment.SetEnvironmentVariable("Wrappit_QueueName", "TestQueue");
        Environment.SetEnvironmentVariable("Wrappit_DeliveryLimit", "5");
        
        // Act
        collection.AddWrappit();

        // Assert
        var options = collection.BuildServiceProvider().GetService<IWrappitOptions>()!;
        Assert.AreEqual(5, options.DeliveryLimit);
    }
}
