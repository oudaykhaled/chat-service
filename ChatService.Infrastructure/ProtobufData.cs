using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatService.Infrastructure
{
    [ProtoContract(SkipConstructor = true)]
    public class ProtobufData<T>
    {
        [ProtoMember(1)]
        public T Data { get; set; }

        public ProtobufData<T> Build(T data)
        {
            Data = data;
            return this;
        }

        public byte[] ToBytes()
        {
            using (var memStream = new MemoryStream())
            {
                Serializer.Serialize(memStream, this);
                memStream.Seek(0, SeekOrigin.Begin);
                return memStream.ToArray();
            }
        }

        public ProtobufData<T> FromBytes(byte[] serialized)
        {
            using (var memStream = new MemoryStream(serialized))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                return Serializer.Deserialize<ProtobufData<T>>(memStream);
            }
        }
    }
}
