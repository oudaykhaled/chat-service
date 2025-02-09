using ChatService.Domain.Request;
using ChatService.Domain.Response;
using ChatService.Infrastructure;
using ChatService.Infrastructure.Broker;
using ChatService.Infrastructure.Models;
using ChatService.Persistence;
using ChatService.Persistence.Exception;
using Microsoft.Extensions.Options;

namespace ChatService.Application.Service
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message> _messageRepository;
        private readonly IRepository<SessionMember> _sessionMemberRepository;
        private readonly IBusConsumer _consumer;
        private readonly IBusPublisher _publisher;
        private readonly IDistributedCache _distributedCache;
        private readonly ProtobufData<MessageInfo> _protobufData;
        private readonly NatsConfiguration _natsConfiguration;

        public MessageService(IRepository<Message> messageRepository,
            IRepository<SessionMember> sessionMemberRepository,
            IBusConsumer consumer,
            IBusPublisher publisher,
            IDistributedCache distributedCache,
            ProtobufData<MessageInfo> protobufData,
            IOptions<NatsConfiguration> options)
        {
            _messageRepository = messageRepository;
            _sessionMemberRepository = sessionMemberRepository;
            _consumer = consumer;
            _publisher = publisher;
            _distributedCache = distributedCache;
            _protobufData = protobufData;
            _natsConfiguration = options.Value;
        }

        public async Task<IdentityResponse> AddMessageAsync(AddMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    MessageInfo message = new MessageInfo();

                    message.Guid = response.ID = Guid.NewGuid().ToString();
                    message.CreatedAt = DateTime.UtcNow;
                    message.Text = request.Text;
                    message.Payload = request.Payload;
                    message.MemberID = request.MemberID;
                    message.ChannelID = request.ChannelID;
                    message.SessionID = request.SessionID;

                    var data = _protobufData.Build(message).ToBytes();

                    string streamName = string.Format(BusConstant.ModerationStream, request.ChannelID);
                    string subjectName = string.Format(BusConstant.ModerationSubject, request.ChannelID);

                    await _publisher.Publish(data, streamName, subjectName);
                }
                else
                {
                    throw new BadRequestException($"Invalid Session provided.");
                }
            }
            else
            {
                throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
            }

            return response;
        }

        public async Task<IdentityResponse> EditMessageAsync(EditMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var originalMessage = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (originalMessage != null)
                    {
                        MessageInfo message = new MessageInfo();

                        message.Guid = response.ID = originalMessage.Guid;
                        message.CreatedAt = DateTime.UtcNow;
                        message.Text = request.Text;
                        message.Payload = request.Payload;
                        message.MemberID = request.MemberID;
                        message.ChannelID = request.ChannelID;
                        message.SessionID = request.SessionID;
                        message.MessageID = request.MessageID;

                        var data = _protobufData.Build(message).ToBytes();

                        string streamName = string.Format(BusConstant.ModerationStream, request.ChannelID);
                        string subjectName = string.Format(BusConstant.ModerationSubject, request.ChannelID);

                        await _publisher.Publish(data, streamName, subjectName);
                    }
                    else
                    {
                        throw new BadRequestException($"Invalid Source Message provided.");
                    }
                }
                else
                {
                    throw new BadRequestException($"Invalid Session provided.");
                }
            }
            else
            {
                throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
            }

            return response;
        }

        public async Task<IdentityResponse> SideEffectAsync(SideEffectRequest request)
        {
            IdentityResponse response = new IdentityResponse();
            bool updateCache = false;
            if (Enum.IsDefined(typeof(NatsDeliverPolicy), _natsConfiguration.DeliverPolicy))
            {
                updateCache = (NatsDeliverPolicy)_natsConfiguration.DeliverPolicy == NatsDeliverPolicy.ByStartSequence;
            }

            string streamName = null, subjectName = null;

            switch (request.ModerationType)
            {
                case Domain.ModerationType.Pre:
                    streamName = string.Format(BusConstant.PreModerationStream, request.ChannelID);
                    subjectName = string.Format(BusConstant.PreModerationSubject, request.ChannelID);
                    break;
                case Domain.ModerationType.Post:
                    streamName = string.Format(BusConstant.PostModerationStream, request.ChannelID);
                    subjectName = string.Format(BusConstant.PostModerationSubject, request.ChannelID);
                    break;
            }

            if (!string.IsNullOrWhiteSpace(streamName) &&
                !string.IsNullOrWhiteSpace(subjectName))
            {
                MessageInfo messageInfo = new MessageInfo();
                messageInfo.Guid = response.ID = request.Guid;
                messageInfo.CreatedAt = request.CreatedAt;
                messageInfo.Text = request.Text;
                messageInfo.Payload = request.Payload;
                messageInfo.MemberID = request.MemberID;
                messageInfo.ChannelID = request.ChannelID;
                messageInfo.SessionID = request.SessionID;
                messageInfo.ParentID = request.ParentID;
                messageInfo.MessageID = request.MessageID;

                var data = _protobufData.Build(messageInfo).ToBytes();
                await _publisher.Publish(data, streamName, subjectName);
                if (updateCache)
                {
                    ulong offset = await _distributedCache.GetAsync<ulong>(request.Guid);
                    streamName = string.Format(BusConstant.PreModerationStream, request.ChannelID);
                    await _distributedCache.SetAsync(streamName, offset + 1);
                }
            }

            return response;
        }

        public async Task<MessageResponse> PopSideEffectAsync(PopSideEffectRequest request)
        {
            MessageResponse response = null;

            string streamName = null, durable = null;

            switch (request.ModerationType)
            {
                case Domain.ModerationType.Pre:
                    streamName = string.Format(BusConstant.PreModerationStream, request.ChannelID);
                    durable = BusConstant.PreModerationName;
                    break;
                case Domain.ModerationType.Post:
                    streamName = string.Format(BusConstant.PostModerationStream, request.ChannelID);
                    durable = BusConstant.PostModerationName;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(streamName)
                && !string.IsNullOrWhiteSpace(durable))
            {
                bool updateCache = false;
                NatsDeliverPolicy deliverPolicy = NatsDeliverPolicy.All;
                if (Enum.IsDefined(typeof(NatsDeliverPolicy), _natsConfiguration.DeliverPolicy))
                {
                    deliverPolicy = (NatsDeliverPolicy)_natsConfiguration.DeliverPolicy;
                }

                switch (deliverPolicy)
                {
                    case NatsDeliverPolicy.ByStartSequence:
                        updateCache = true;
                        ulong startSeq = await _distributedCache.GetAsync<ulong>(streamName);
                        await _consumer.Subscribe(streamName, durable, startSeq);
                        break;
                    case NatsDeliverPolicy.All:
                        await _consumer.Subscribe(streamName, durable);
                        break;
                }

                var result = await _consumer.NextAsync();
                if (result.Data != null)
                {
                    response = new MessageResponse();
                    var message = _protobufData.FromBytes(result.Data);

                    response.CreatedAt = message.Data.CreatedAt;
                    response.Guid = message.Data.Guid;
                    response.Text = message.Data.Text;
                    response.Payload = message.Data.Payload;
                    response.MemberID = message.Data.MemberID;
                    response.ChannelID = message.Data.ChannelID;
                    response.SessionID = message.Data.SessionID;
                    response.ParentID = message.Data.ParentID;
                    response.MessageID = message.Data.MessageID;

                    if (updateCache)
                    {
                        await _distributedCache.SetAsync(response.Guid, result.Offset);
                    }
                }
            }

            return response;
        }

        public async Task<IdentityResponse> ModerateMessageAsync(ModerateMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();
            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    bool updateCache = false;
                    if (Enum.IsDefined(typeof(NatsDeliverPolicy), _natsConfiguration.DeliverPolicy))
                    {
                        updateCache = (NatsDeliverPolicy)_natsConfiguration.DeliverPolicy == NatsDeliverPolicy.ByStartSequence;
                    }

                    if (string.IsNullOrWhiteSpace(request.MessageID))
                    {
                        Message message = new Message();
                        message.Guid = request.Guid;
                        message.CreatedAt = DateTime.SpecifyKind(request.CreatedAt, DateTimeKind.Utc);
                        message.Text = request.Text;
                        message.Payload = request.Payload;
                        message.MemberID = request.MemberID;
                        message.ChannelID = request.ChannelID;
                        message.SessionID = request.SessionID;
                        message.ParentID = request.ParentID;

                        response.ID = await _messageRepository.AddAsync(message);
                    }
                    else
                    {
                        var originalMessage = await _messageRepository.GetByIdAsync(request.MessageID);
                        if (originalMessage != null)
                        {
                            originalMessage.ModifiedAt = DateTime.UtcNow;
                            originalMessage.Text = request.Text;
                            originalMessage.Payload = request.Payload;

                            response.ID = await _messageRepository.UpdateAsync(originalMessage);
                        }
                    }
                }
                else
                {
                    throw new BadRequestException($"Invalid Session provided.");
                }
            }
            else
            {
                throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
            }

            return response;
        }

        public async Task<MessageResponse> PopMessageAsync(PopMessageRequest request)
        {
            MessageResponse response = null;

            string streamName = string.Format(BusConstant.ModerationStream, request.ChannelID);

            if (!string.IsNullOrWhiteSpace(streamName))
            {
                bool updateCache = false;
                NatsDeliverPolicy deliverPolicy = NatsDeliverPolicy.All;
                if (Enum.IsDefined(typeof(NatsDeliverPolicy), _natsConfiguration.DeliverPolicy))
                {
                    deliverPolicy = (NatsDeliverPolicy)_natsConfiguration.DeliverPolicy;
                }

                switch (deliverPolicy)
                {
                    case NatsDeliverPolicy.ByStartSequence:
                        updateCache = true;
                        ulong startSeq = await _distributedCache.GetAsync<ulong>(streamName);
                        await _consumer.Subscribe(streamName, BusConstant.ServerName, startSeq);
                        break;
                    case NatsDeliverPolicy.All:
                        await _consumer.Subscribe(streamName, BusConstant.ServerName);
                        break;
                }

                var result = await _consumer.NextAsync();
                if (result.Data != null)
                {
                    response = new MessageResponse();
                    var message = _protobufData.FromBytes(result.Data);

                    response.CreatedAt = message.Data.CreatedAt;
                    response.Guid = message.Data.Guid;
                    response.Text = message.Data.Text;
                    response.Payload = message.Data.Payload;
                    response.MemberID = message.Data.MemberID;
                    response.ChannelID = message.Data.ChannelID;
                    response.SessionID = message.Data.SessionID;
                    response.ParentID = message.Data.ParentID;
                    response.MessageID = message.Data.MessageID;

                    if (updateCache)
                    {
                        await _distributedCache.SetAsync(response.Guid, result.Offset);
                    }
                }
            }

            return response;
        }

        public async Task<IdentityResponse> ReplyToMessageAsync(ReplyToMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var parentMessage = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (parentMessage != null)
                    {
                        MessageInfo message = new MessageInfo();

                        message.Guid = Guid.NewGuid().ToString();
                        message.CreatedAt = DateTime.UtcNow;
                        message.Text = request.Text;
                        message.Payload = request.Payload;
                        message.MemberID = request.MemberID;
                        message.ChannelID = request.ChannelID;
                        message.SessionID = request.SessionID;
                        message.ParentID = request.MessageID;

                        var data = _protobufData.Build(message).ToBytes();

                        string streamName = string.Format(BusConstant.PreModerationStream, request.ChannelID);
                        string subjectName = string.Format(BusConstant.PreModerationSubject, request.ChannelID);

                        await _publisher.Publish(data, streamName, subjectName);
                        return response;
                    }
                    throw new BadRequestException($"Parent Message '{request.MessageID}' does not exists..");
                }
                throw new BadRequestException($"Invalid Session provided.");
            }
            throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
        }

        public async Task<IdentityResponse> BindMessageAsync(BindMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var message = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (message != null)
                    {
                        message.ModifiedAt = DateTime.UtcNow;
                        if (message.Context == null)
                            message.Context = new List<string>();

                        message.Context.Add(request.Text);

                        response.ID = await _messageRepository.UpdateAsync(message);
                        return response;
                    }
                    throw new BadRequestException($"Message '{request.MessageID}' does not exists..");
                }
                throw new BadRequestException($"Invalid Session provided.");
            }
            throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
        }

        public async Task<IdentityResponse> DeleteAsync(DeleteMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var message = await _messageRepository.GetByIdAsync(request.ID);
                    if (message != null)
                    {
                        await _messageRepository.DeleteAsync(message);
                        response.ID = request.ID;
                    }
                }
            }

            return response;
        }

        public async Task<IdentityResponse> MaskMessageAsync(MaskMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var message = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (message != null)
                    {
                        message.ModifiedAt = DateTime.UtcNow;
                        message.IsMaskedText = request.Mask;

                        response.ID = await _messageRepository.UpdateAsync(message);
                        return response;
                    }
                    throw new BadRequestException($"Message '{request.MessageID}' does not exists..");
                }
                throw new BadRequestException($"Invalid Session provided.");
            }
            throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
        }

        public async Task<IdentityResponse> HideMessageAsync(HideMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var message = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (message != null)
                    {
                        message.ModifiedAt = DateTime.UtcNow;
                        message.IsHidden = request.Hide;

                        response.ID = await _messageRepository.UpdateAsync(message);
                        return response;
                    }
                    throw new BadRequestException($"Message '{request.MessageID}' does not exists..");
                }
                throw new BadRequestException($"Invalid Session provided.");
            }
            throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
        }

        public async Task<IdentityResponse> MarkMessageAsync(MarkMessageRequest request)
        {
            IdentityResponse response = new IdentityResponse();

            var sessionMember = await _sessionMemberRepository.GetByIdAsync(request.SessionID);
            if (sessionMember != null)
            {
                if (sessionMember.MemberID == request.MemberID &&
                    sessionMember.ChannelID == request.ChannelID)
                {
                    var message = await _messageRepository.GetByIdAsync(request.MessageID);
                    if (message != null)
                    {
                        bool isValid = false;
                        switch (request.Status)
                        {
                            case Domain.MessageStatus.Delivered:
                                if (message.DeliveredTo == null)
                                    message.DeliveredTo = new List<string>();

                                if (!message.DeliveredTo.Contains(request.MemberID))
                                {
                                    message.DeliveredTo.Add(request.MemberID);
                                    isValid = true;
                                }
                                break;
                            case Domain.MessageStatus.Seen:
                                if (message.SeenBy == null)
                                    message.SeenBy = new List<string>();

                                if (!message.SeenBy.Contains(request.MemberID))
                                {
                                    message.SeenBy.Add(request.MemberID);
                                    isValid = true;
                                }
                                break;
                        }

                        if (isValid)
                        {
                            message.ModifiedAt = DateTime.UtcNow;
                            response.ID = await _messageRepository.UpdateAsync(message);
                        }
                        else
                            response.ID = message.ID;

                        return response;
                    }
                    throw new BadRequestException($"Message '{request.MessageID}' does not exists..");
                }
                throw new BadRequestException($"Invalid Session provided.");
            }
            throw new BadRequestException($"Session '{request.SessionID}' does not exists.");
        }
    }
}
