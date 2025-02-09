
using ChatService.Application.Service;
using ChatService.Domain;
using ChatService.Infrastructure.Broker;
using Microsoft.Extensions.Options;

namespace ChatService.API.Jobs
{
    public class PostModerationBackgroundService : BackgroundService
    {
        private readonly ILogger<PostModerationBackgroundService> _logger;
        private readonly IMessageService _messageService;
        private readonly ModerationOptions _moderationOptions;
        private readonly string _channelID;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBusConsumer _consumer;

        public PostModerationBackgroundService(ILogger<PostModerationBackgroundService> logger,
            IMessageService messageService,
            IOptions<ModerationOptions> options,
            IHttpClientFactory httpClientFactory,
            IBusConsumer consumer,
            string channelID)
        {
            _logger = logger;
            _messageService = messageService;
            _moderationOptions = options.Value;
            _httpClientFactory = httpClientFactory;
            _consumer = consumer;
            _channelID = channelID;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Moderation post is starting...");
            string streamName = string.Format(BusConstant.PostModerationStream, _channelID);
            string subject = string.Format(BusConstant.PostModerationSubject, _channelID);
            await _consumer.CreateOrUpdateStream(streamName, subject);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _messageService.PopSideEffectAsync(new Domain.Request.PopSideEffectRequest()
                    {
                        ChannelID = _channelID,
                        ModerationType = ModerationType.Post
                    });
                    if (message != null)
                    {
                        if (_moderationOptions.Post != null && _moderationOptions.Post.Any())
                        {
                            foreach (string link in _moderationOptions.Post)
                            {
                                try
                                {
                                    var client = _httpClientFactory.CreateClient();
                                    var response = await client.PostAsJsonAsync(link, message, stoppingToken);
                                }
                                catch { }
                            }
                        }
                    }

                    await Task.Delay(_moderationOptions.PostInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Moderation post is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Moderation post.");
                }
            }

            _logger.LogInformation("Moderation post is stopping...");
        }
    }
}
