namespace ChatService.Infrastructure.Broker
{
    public class BusConstant
    {
        public const string ServerName = "chatservice";
        public const string ModerationName = "chatservice_{0}_moderation";
        public const string PreModerationName = "chatservice_{0}_pre_moderation";
        public const string PostModerationName = "chatservice_{0}_post_moderation";
        public const string ModerationStream = "chatservice_{0}_moderation_stream";
        public const string PreModerationStream = "chatservice_{0}_pre_moderation_stream";
        public const string PostModerationStream = "chatservice_{0}_post_moderation_stream";
        public const string ModerationSubject = "chatservice_{0}_moderation_subject";
        public const string PreModerationSubject = "chatservice_{0}_pre_moderation_subject";
        public const string PostModerationSubject = "chatservice_{0}_post_moderation_subject";
    }
}
