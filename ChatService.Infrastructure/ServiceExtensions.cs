using ChatService.Infrastructure.Broker;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Infrastructure
{
    public static class ServiceExtensions
    {
        public static void ConfigureInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IBusConnection, NatsConnection>();
            services.AddScoped<IBusStream, NatsStream>();
            services.AddScoped<IBusConsumer, NatsConsumer>();
            services.AddScoped<IBusPublisher, NatsPublisher>();
            services.AddScoped<IDistributedCache, NatsDistributedCache>();
            services.AddScoped<NatsConnectionFactory>();
        }
    }
}
