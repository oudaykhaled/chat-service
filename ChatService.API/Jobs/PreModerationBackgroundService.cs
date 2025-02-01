
using ChatService.Application.Service;
using ChatService.Domain;
using ChatService.Infrastructure.Broker;
using Microsoft.Extensions.Options;

namespace ChatService.API.Jobs
{
    public class PreModerationBackgroundService : BackgroundService
    {
        private readonly ILogger<PreModerationBackgroundService> _logger;
        private readonly IMessageService _messageService;
        private readonly ModerationOptions _moderationOptions;
        private readonly string _channelID;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBusConsumer _consumer;

        public PreModerationBackgroundService(ILogger<PreModerationBackgroundService> logger,
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
            _logger.LogInformation("Moderation pre is starting...");
            string streamName = string.Format(BusConstant.PreModerationStream, _channelID);
            string subject = string.Format(BusConstant.PreModerationSubject, _channelID);
            await _consumer.CreateOrUpdateStream(streamName, subject);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _messageService.PopSideEffectAsync(new Domain.Request.PopSideEffectRequest()
                    {
                        ChannelID = _channelID,
                        ModerationType = ModerationType.Pre
                    });
                    if (message != null)
                    {
                        if (_moderationOptions.Pre != null && _moderationOptions.Pre.Any())
                        {
                            foreach (string link in _moderationOptions.Pre)
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

                    await Task.Delay(_moderationOptions.PreInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Moderation pre is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Moderation pre.");
                }
            }

            _logger.LogInformation("Moderation pre is stopping...");
        }
    }
}
