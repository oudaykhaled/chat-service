
using ChatService.Application.Service;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Application
{
    public static class ServiceExtensions
    {
        public static void ConfigureApplication(this IServiceCollection services)
        {
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IChannelService, ChannelService>();
            services.AddScoped<IMessageService, MessageService>();
        }
    }
}
