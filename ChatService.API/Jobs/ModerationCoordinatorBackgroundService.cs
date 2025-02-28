using ChatService.Application.Service;
using ChatService.Domain;
using ChatService.Infrastructure.Broker;
using Microsoft.Extensions.Options;
using static Google.Rpc.Context.AttributeContext.Types;

namespace ChatService.API.Jobs
{
    public class ModerationCoordinatorBackgroundService : BackgroundService
    {
        private readonly ILogger<ModerationCoordinatorBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<ModerationOptions> _options;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HashSet<string> _runningChannels = new();

        public ModerationCoordinatorBackgroundService(ILogger<ModerationCoordinatorBackgroundService> logger,
            IServiceProvider serviceProvider,
            IOptions<ModerationOptions> options,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Moderation Manager is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var channelService = scope.ServiceProvider.GetRequiredService<IChannelService>();

                        bool hasDone = false;
                        string nextPageToken = null;
                        do
                        {
                            var channels = await channelService.GetPagedAsync(new Domain.Request.GetPagedRequest()
                            {
                                PageSize = 100,
                                StartAfterDocumentId = nextPageToken
                            });
                            if (channels != null && channels.Items != null && channels.Items.Any())
                            {
                                foreach (var channel in channels.Items)
                                {
                                    if (!_runningChannels.Contains(channel.ID))
                                    {
                                        var consumer = scope.ServiceProvider.GetRequiredService<IBusConsumer>();

                                        string streamName = string.Format(BusConstant.ModerationStream, channel.ID);
                                        string subjectName = string.Format(BusConstant.ModerationSubject, channel.ID);
                                        await consumer.CreateOrUpdateStream(streamName, subjectName);

                                        streamName = string.Format(BusConstant.PreModerationStream, channel.ID);
                                        subjectName = string.Format(BusConstant.PreModerationSubject, channel.ID);
                                        await consumer.CreateOrUpdateStream(streamName, subjectName);

                                        streamName = string.Format(BusConstant.PostModerationStream, channel.ID);
                                        subjectName = string.Format(BusConstant.PostModerationSubject, channel.ID);
                                        await consumer.CreateOrUpdateStream(streamName, subjectName);

                                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ModerationBackgroundService>>();
                                        var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();
                                        
                                        var service = new ModerationBackgroundService(logger, messageService, _options, _httpClientFactory, channel.ID);
                                        await Task.Run(async () => await service.StartAsync(stoppingToken));

                                        var preLogger = scope.ServiceProvider.GetRequiredService<ILogger<PreModerationBackgroundService>>();
                                        var preService = new PreModerationBackgroundService(preLogger, messageService, _options, _httpClientFactory, consumer, channel.ID);
                                        await Task.Run(async () => await preService.StartAsync(stoppingToken));

                                        var postLogger = scope.ServiceProvider.GetRequiredService<ILogger<PostModerationBackgroundService>>();
                                        var postService = new PostModerationBackgroundService(postLogger, messageService, _options, _httpClientFactory, consumer, channel.ID);
                                        await Task.Run(async () => await postService.StartAsync(stoppingToken));

                                        _runningChannels.Add(channel.ID);
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(channels.NextPageToken))
                                {
                                    nextPageToken = channels.NextPageToken;
                                }
                                else
                                {
                                    hasDone = true;
                                }
                            }
                            else
                                hasDone = true;

                        } while (!hasDone);
                        await Task.Delay(_options.Value.CoordinatorInterval, stoppingToken);
                    }
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Moderation Manager is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Moderation manager.");
                }
            }
        }
    }
}
