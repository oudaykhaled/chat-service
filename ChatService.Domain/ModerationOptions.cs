namespace ChatService.Domain
{
    public class ModerationOptions
    {
        public const string Moderation = "Moderation";
        public List<string> Pre {  get; set; }
        public List<string> Post { get; set; }
        public string Dispatcher { get; set; }
        public bool EnableDispatcher { get; set; }
        public int CoordinatorInterval { get; set; } = 1000;
        public int PreInterval { get; set; } = 200;
        public int PostInterval { get; set; } = 200;
    }
}
