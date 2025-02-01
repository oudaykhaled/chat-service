using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Persistence
{
    public static class ServiceExtensions
    {
        public static void ConfigurePersistence(this IServiceCollection services)
        {
            services.AddSingleton<ChatDbContext>();
            services.AddScoped<IRepository<Member>>(provider => new Repository<Member>(provider.GetRequiredService<ChatDbContext>()));
            services.AddScoped<IRepository<Channel>>(provider => new Repository<Channel>(provider.GetRequiredService<ChatDbContext>()));
            services.AddScoped<IRepository<Message>>(provider => new Repository<Message>(provider.GetRequiredService<ChatDbContext>()));
            services.AddScoped<IRepository<SessionMember>>(provider => new Repository<SessionMember>(provider.GetRequiredService<ChatDbContext>()));
        }
    }
}
