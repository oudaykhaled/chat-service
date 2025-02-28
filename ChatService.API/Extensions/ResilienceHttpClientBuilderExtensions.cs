using Microsoft.Extensions.Http.Resilience;

namespace ChatService.API.Extensions
{
    public static class ResilienceHttpClientBuilderExtensions
    {
        public static IHttpClientBuilder RemoveAllResilienceHandlers(this IHttpClientBuilder builder)
        {
            _ = builder.ConfigureAdditionalHttpMessageHandlers(static (handlers, _) =>
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    if (handlers[i] is ResilienceHandler)
                    {
                        handlers.RemoveAt(i);
                    }
                }
            });
            return builder;
        }
    }
}
