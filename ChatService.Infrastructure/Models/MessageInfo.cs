using ProtoBuf;

namespace ChatService.Infrastructure.Models
{
    [ProtoContract(SkipConstructor = true)]
    public class MessageInfo
    {
        [ProtoMember(1)]
        public string Guid { get; set; }
        [ProtoMember(2)]
        public DateTime CreatedAt { get; set; }
        [ProtoMember(3)]
        public string Text { get; set; }
        [ProtoMember(4)]
        public string Payload { get; set; }
        [ProtoMember(5)]
        public string MemberID { get; set; }
        [ProtoMember(6)]
        public string ChannelID { get; set; }
        [ProtoMember(7)]
        public string SessionID { get; set; }
        [ProtoMember(8)]
        public string ParentID { get; set; }
        [ProtoMember(9)]
        public string MessageID { get; set; }
    }
}
