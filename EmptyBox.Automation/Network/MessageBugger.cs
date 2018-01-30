using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using EmptyBox.Automation;
using EmptyBox.ScriptRuntime.Extensions;
using EmptyBox.IO.Serializator;
using System.Threading.Tasks;

namespace EmptyBox.Automation.Network
{
    //"Как назовёшь корабль, так он и поплывёт." (Капитан Врунгель)

    public class MessageBugger<TIndexer> : IPipelineInput<byte[], MessageBuggerIndexer, TIndexer>,
                                 IPipelineOutput<byte[], MessageBuggerIndexer, TIndexer>,
                                 IPipelineInput<MessageBuggerControl>,
                                 IPipelineOutput<MessageBuggerControl>
    {
        enum PacketID : byte
        {
            Header = 0,
            Packet = 1,
            PacketWithoutHeader = 2
        }

        struct Message
        {
            public PacketID ID;
            public byte[] Data;
        }

        struct Header
        {
            public ulong SequenceID;
            public uint PartsCount;
            public byte[] Data;
        }

        struct Packet
        {
            public ulong SequenceID;
            public uint Part;
            public byte[] Data;
        }

        EventHandler<byte[]> IPipelineOutput<byte[], MessageBuggerIndexer, TIndexer>.this[MessageBuggerIndexer index0, TIndexer index1]
        {
            get
            {
                switch (index0)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        if (!RawMessagesOutput.ContainsKey(index1))
                        {
                            RawMessagesOutput.Add(index1, null);
                        }
                        return RawMessagesOutput[index1];
                    case MessageBuggerIndexer.Splitted:
                        if (!SplittedMessagesOutput.ContainsKey(index1))
                        {
                            SplittedMessagesOutput.Add(index1, null);
                        }
                        return SplittedMessagesOutput[index1];
                }
            }
            set
            {
                switch (index0)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        RawMessagesOutput[index1] = value;
                        break;
                    case MessageBuggerIndexer.Splitted:
                        SplittedMessagesOutput[index1] = value;
                        break;
                }
            }
        }

        EventHandler<byte[]> IPipelineInput<byte[], MessageBuggerIndexer, TIndexer>.this[MessageBuggerIndexer index0, TIndexer index1]
        {
            get
            {
                switch (index0)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        return (object sender, byte[] message) => RawMessagesInput(sender, message, index1);
                    case MessageBuggerIndexer.Splitted:
                        return (object sender, byte[] message) => SplittedMessagesInput(sender, message, index1);
                }
            }
        }

        event EventHandler<MessageBuggerControl> IPipelineOutput<MessageBuggerControl>.Output { add => ControlOutput += value; remove => ControlOutput -= value; }

        private BinarySerializer Serializer;
        private Dictionary<TIndexer, Dictionary<ulong, Dictionary<uint, byte[]>>> ID2Packet;
        private Dictionary<TIndexer, Dictionary<ulong, Header>> ID2Header;
        private Dictionary<TIndexer, EventHandler<byte[]>> SplittedMessagesOutput;
        private Dictionary<TIndexer, EventHandler<byte[]>> RawMessagesOutput;
        private event EventHandler<MessageBuggerControl> ControlOutput;

        public uint PacketSize { get; set; }

        public MessageBugger(BinarySerializer serializer, uint packetsize)
        {
            ID2Packet = new Dictionary<TIndexer, Dictionary<ulong, Dictionary<uint, byte[]>>>();
            ID2Header = new Dictionary<TIndexer, Dictionary<ulong, Header>>();
            SplittedMessagesOutput = new Dictionary<TIndexer, EventHandler<byte[]>>();
            RawMessagesOutput = new Dictionary<TIndexer, EventHandler<byte[]>>();
            Serializer = serializer;
            if (packetsize < Serializer.GetLength(new Message()) + Serializer.GetLength(new Header()))
            {
                throw new ArgumentOutOfRangeException("packetsize");
            }
            PacketSize = packetsize;
        }


        private void SplittedMessagesInput(object sender, byte[] message, TIndexer index)
        {
            Message packet = Serializer.Deserialize<Message>(message);
            switch (packet.ID)
            {
                case PacketID.PacketWithoutHeader:
                    RawMessagesOutput[index]?.Invoke(this, packet.Data);
                    break;
                case PacketID.Header:
                    Header header = Serializer.Deserialize<Header>(packet.Data);
                    if (ID2Header[index].ContainsKey(header.SequenceID))
                    {
                        (this as IPipelineOutput<MessageBuggerControl, byte>)[0]?.Invoke(this, new MessageBuggerControl() { State = MessageBuggerControlStates.DublicatePacket, SequenceID = header.SequenceID });
                    }
                    else
                    {
                        ID2Header[index].Add(header.SequenceID, header);
                        CheckPackets(header.SequenceID, index);
                    }
                    break;
                case PacketID.Packet:
                    Packet packet0 = Serializer.Deserialize<Packet>(packet.Data);
                    if (ID2Packet[index].ContainsKey(packet0.SequenceID))
                    {
                        if (ID2Packet[index][packet0.SequenceID].ContainsKey(packet0.Part))
                        {
                            (this as IPipelineOutput<MessageBuggerControl, byte>)[0]?.Invoke(this, new MessageBuggerControl() { State = MessageBuggerControlStates.DublicatePacket, SequenceID = packet0.SequenceID });
                        }
                        else
                        {
                            ID2Packet[index][packet0.SequenceID].Add(packet0.Part, packet0.Data);
                            CheckPackets(packet0.SequenceID, index);
                        }
                    }
                    else
                    {
                        ID2Packet[index].Add(packet0.SequenceID, new Dictionary<uint, byte[]>());
                        ID2Packet[index][packet0.SequenceID].Add(packet0.Part, packet0.Data);
                        CheckPackets(packet0.SequenceID, index);
                    }
                    break;
            }
        }

        private void CheckPackets(ulong id, TIndexer index)
        {
            if (ID2Header[index].ContainsKey(id) && ID2Packet[index].ContainsKey(id))
            {
                if (ID2Packet[index][id].Keys.Count == ID2Header[index][id].PartsCount)
                {
                    List<byte> pool = new List<byte>(4096);
                    pool.AddRange(ID2Header[index][id].Data);
                    for (uint i0 = 0; i0 < ID2Header[index][id].PartsCount; i0++)
                    {
                        pool.AddRange(ID2Packet[index][id][i0]);
                    }
                    ID2Header[index].Remove(id);
                    ID2Packet[index].Remove(id);
                    RawMessagesOutput[index]?.Invoke(this, pool.ToArray());
                }
            }
        }

        private void RawMessagesInput(object sender, byte[] message, TIndexer index)
        {
            uint length = Serializer.GetLength(new Message());
            if (length + message.Length <= PacketSize)
            {
                SplittedMessagesOutput[index]?.Invoke
                (
                    this,
                    Serializer.Serialize
                    (
                        new Message()
                        {
                            ID = PacketID.PacketWithoutHeader,
                            Data = message
                        }
                    )
                );
            }
            else
            {
                IEnumerable<byte> pool = message;
                uint headerLength = Serializer.GetLength(new Header());
                uint packetLength = Serializer.GetLength(new Packet());
                uint messageLength = Serializer.GetLength(new Message());
                uint header_size = PacketSize - headerLength - messageLength;
                uint packet_size = PacketSize - packetLength - messageLength;
                uint parts_count = (uint)Math.Ceiling((double)(message.Length - header_size) / PacketSize);
                Message head = new Message() { ID = PacketID.Header };
                Header header = new Header() { PartsCount = parts_count };
                if (header_size > 0)
                {
                    header.Data = pool.Take((int)header_size).ToArray();
                    pool = pool.Skip((int)header_size);
                }
                else
                {
                    header.Data = new byte[0];
                }
                head.Data = Serializer.Serialize(header);
                SplittedMessagesOutput[index]?.Invoke(this, Serializer.Serialize(head));
                for (uint i0 = 0; i0 < parts_count; i0++)
                {
                    Message packet0 = new Message()
                    {
                        ID = PacketID.Packet,
                        Data = Serializer.Serialize(new Packet()
                        {
                            Part = i0,
                            Data = pool.Take((int)packet_size).ToArray()
                        })
                    };
                    pool = pool.Skip((int)packet_size);
                    SplittedMessagesOutput[index]?.Invoke(this, Serializer.Serialize(packet0));
                }
            }
        }

        void IPipelineInput<MessageBuggerControl>.Input(object sender, MessageBuggerControl output)
        {
            throw new NotImplementedException();
        }
    }
}
