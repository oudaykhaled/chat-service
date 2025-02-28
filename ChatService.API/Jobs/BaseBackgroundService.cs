using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using System.Net;

namespace ChatService.API.Jobs
{
    public class BaseBackgroundService : BackgroundService
    {
        private readonly Random RandomJitter = new();

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        protected AsyncRetryPolicy GetRetryPolicy
        {
            get
            {
                TimeSpan baseDelay = TimeSpan.FromSeconds(1);

                return Policy
                    .Handle<HttpRequestException>()
                    .Or<TaskCanceledException>()
                    .WaitAndRetryAsync(3, retryAttempt =>
                    {
                        double exponentialBackoff = Math.Pow(2, retryAttempt);

                        double baseDelayInSeconds = baseDelay.TotalSeconds;

                        double jitterFactor = RandomJitter.NextDouble() * 0.5 + 0.5;

                        double finalDelayInSeconds = baseDelayInSeconds + (exponentialBackoff * jitterFactor);

                        return TimeSpan.FromSeconds(finalDelayInSeconds);
                    },
                    (exception, timeSpan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds:F2} seconds due to {exception.Message}");
                    });
            }
        }
    }
}
