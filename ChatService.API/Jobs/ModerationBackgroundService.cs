﻿using ChatService.Application.Service;
using ChatService.Domain;
using ChatService.Domain.Response;
using ChatService.Persistence;
using Microsoft.Extensions.Options;
using Polly;
using System.Linq.Expressions;
using System.Text.Json;

namespace ChatService.API.Jobs
{
    public class ModerationBackgroundService : BaseBackgroundService
    {
        private readonly ILogger<ModerationBackgroundService> _logger;
        private readonly IMessageService _messageService;
        private readonly ModerationOptions _moderationOptions;
        private readonly string _channelID;
        private readonly IHttpClientFactory _httpClientFactory;

        public ModerationBackgroundService(ILogger<ModerationBackgroundService> logger,
            IMessageService messageService,
            IOptions<ModerationOptions> options,
            IHttpClientFactory httpClientFactory,
            string channelID)
        {
            _logger = logger;
            _messageService = messageService;
            _moderationOptions = options.Value;
            _httpClientFactory = httpClientFactory;
            _channelID = channelID;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Moderation is starting...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _messageService.PopMessageAsync(new Domain.Request.PopMessageRequest()
                    {
                        ChannelID = _channelID
                    });
                    if (message != null)
                    {
                        if (_moderationOptions.Pre != null && _moderationOptions.Pre.Any())
                        {
                            var sideEffectResponse = await _messageService.SideEffectAsync(new Domain.Request.SideEffectRequest()
                            {
                                ModerationType = ModerationType.Pre,
                                ChannelID = _channelID,
                                CreatedAt = message.CreatedAt,
                                Guid = message.Guid,
                                MemberID = message.MemberID,
                                MessageID = message.MessageID,
                                ParentID = message.ParentID,
                                SessionID = message.SessionID,
                                Payload = message.Payload,
                                Text = message.Text,
                            });
                        }

                        if (_moderationOptions.EnableDispatcher
                            && !string.IsNullOrWhiteSpace(_moderationOptions.Dispatcher))
                        {
                            try
                            {
                                await GetRetryPolicy.ExecuteAsync(async () =>
                                {
                                    var client = _httpClientFactory.CreateClient(ClientIntegration.Integration);
                                    var response = await client.PostAsJsonAsync(_moderationOptions.Dispatcher, message, stoppingToken);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        string json = await response.Content.ReadAsStringAsync();
                                        if (!string.IsNullOrWhiteSpace(json))
                                        {
                                            message = JsonSerializer.Deserialize<MessageResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                        }
                                    }
                                });
                            }
                            catch (Exception ex)
                            {                           
                                _logger.LogError($"Moderation request failed after retries (not reachable)");
                                await RetryMessage(message);
                                message = null;
                            }
                        }

                        if (message != null)
                        {
                            var moderationResponse = await _messageService.ModerateMessageAsync(new Domain.Request.ModerateMessageRequest()
                            {
                                ChannelID = _channelID,
                                CreatedAt = message.CreatedAt,
                                Guid = message.Guid,
                                MemberID = message.MemberID,
                                MessageID = message.MessageID,
                                ParentID = message.ParentID,
                                SessionID = message.SessionID,
                                Payload = message.Payload,
                                Text = message.Text,
                            });

                            if (_moderationOptions.Post != null && _moderationOptions.Post.Any())
                            {
                                var sideEffectResponse = await _messageService.SideEffectAsync(new Domain.Request.SideEffectRequest()
                                {
                                    ModerationType = ModerationType.Post,
                                    ChannelID = _channelID,
                                    CreatedAt = message.CreatedAt,
                                    Guid = message.Guid,
                                    MemberID = message.MemberID,
                                    MessageID = message.MessageID,
                                    ParentID = message.ParentID,
                                    SessionID = message.SessionID,
                                    Payload = message.Payload,
                                    Text = message.Text,
                                });
                            }
                        }
                    }

                    await Task.Delay(200, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Moderation is stopping.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Moderation.");
                }
            }

            _logger.LogInformation("Moderation is stopping...");
        }

        private async Task<IdentityResponse> RetryMessage(MessageResponse message)
        {
            return await _messageService.RetryMessageAsync(new Domain.Request.RetryMessageRequest()
            {
                Guid = message.Guid,
                ChannelID = message.ChannelID,
                MemberID = message.MemberID,
                MessageID = message.MessageID,
                ParentID = message.ParentID,
                Payload = message.Payload,
                SessionID = message.SessionID,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            });
        }
    }
}
