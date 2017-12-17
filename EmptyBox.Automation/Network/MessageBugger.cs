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

    public class MessageBugger : IPipelineInput<byte[], MessageBuggerIndexer>,
                                 IPipelineOutput<byte[], MessageBuggerIndexer>,
                                 IPipelineInput<MessageBuggerControl, byte>,
                                 IPipelineOutput<MessageBuggerControl, byte>
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

        OutputDelegate<MessageBuggerControl, byte> IPipelineOutput<MessageBuggerControl, byte>.this[byte index]
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        OutputDelegate<MessageBuggerControl, byte> IPipelineInput<MessageBuggerControl, byte>.this[byte index]
        {
            get
            {
                return null;
            }
        }

        OutputDelegate<byte[], MessageBuggerIndexer> IPipelineOutput<byte[], MessageBuggerIndexer>.this[MessageBuggerIndexer index]
        {
            get
            {
                switch (index)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        return RawMessagesOutput;
                    case MessageBuggerIndexer.Splitted:
                        return SplittedMessagesOutput;
                }
            }
            set
            {
                switch (index)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        RawMessagesOutput = value;
                        break;
                    case MessageBuggerIndexer.Splitted:
                        SplittedMessagesOutput = value;
                        break;
                }
            }
        }

        OutputDelegate<byte[], MessageBuggerIndexer> IPipelineInput<byte[], MessageBuggerIndexer>.this[MessageBuggerIndexer index]
        {
            get
            {
                switch (index)
                {
                    default:
                    case MessageBuggerIndexer.Raw:
                        return RawMessagesInput;
                    case MessageBuggerIndexer.Splitted:
                        return SplittedMessagesInput;
                }
            }
        }
        
        private BinarySerializer Serializer;
        private Dictionary<ulong, Dictionary<uint, byte[]>> ID2Packet;
        private Dictionary<ulong, Header> ID2Header;
        private event OutputDelegate<byte[], MessageBuggerIndexer> SplittedMessagesOutput;
        private event OutputDelegate<byte[], MessageBuggerIndexer> RawMessagesOutput;

        public uint PacketSize { get; set; }

        public MessageBugger(BinarySerializer serializer, uint packetsize)
        {
            ID2Packet = new Dictionary<ulong, Dictionary<uint, byte[]>>();
            ID2Header = new Dictionary<ulong, Header>();
            Serializer = serializer;
            if (packetsize < Serializer.GetLength(new Message()) + Serializer.GetLength(new Header()))
            {
                throw new ArgumentOutOfRangeException("packetsize");
            }
            PacketSize = packetsize;
        }

        private void SplittedMessagesInput(IPipelineOutput<byte[], MessageBuggerIndexer> sender, byte[] output, MessageBuggerIndexer index)
        {
            Message packet = Serializer.Deserialize<Message>(output);
            switch (packet.ID)
            {
                case PacketID.PacketWithoutHeader:
                    RawMessagesOutput?.Invoke(this, packet.Data, MessageBuggerIndexer.Raw);
                    break;
                case PacketID.Header:
                    Header header = Serializer.Deserialize<Header>(packet.Data);
                    if (ID2Header.ContainsKey(header.SequenceID))
                    {
                        (this as IPipelineOutput<MessageBuggerControl, byte>)[0]?.Invoke(this, new MessageBuggerControl() { State = MessageBuggerControlStates.DublicatePacket, SequenceID = header.SequenceID }, 0);
                    }
                    else
                    {
                        ID2Header.Add(header.SequenceID, header);
                        CheckPackets(header.SequenceID);
                    }
                    break;
                case PacketID.Packet:
                    Packet packet0 = Serializer.Deserialize<Packet>(packet.Data);
                    if (ID2Packet.ContainsKey(packet0.SequenceID))
                    {
                        if (ID2Packet[packet0.SequenceID].ContainsKey(packet0.Part))
                        {
                            (this as IPipelineOutput<MessageBuggerControl, byte>)[0]?.Invoke(this, new MessageBuggerControl() { State = MessageBuggerControlStates.DublicatePacket, SequenceID = packet0.SequenceID }, 0);
                        }
                        else
                        {
                            ID2Packet[packet0.SequenceID].Add(packet0.Part, packet0.Data);
                            CheckPackets(packet0.SequenceID);
                        }
                    }
                    else
                    {
                        ID2Packet.Add(packet0.SequenceID, new Dictionary<uint, byte[]>());
                        ID2Packet[packet0.SequenceID].Add(packet0.Part, packet0.Data);
                        CheckPackets(packet0.SequenceID);
                    }
                    break;
            }
        }

        private void CheckPackets(ulong id)
        {
            if (ID2Header.ContainsKey(id) && ID2Packet.ContainsKey(id))
            {
                if (ID2Packet[id].Keys.Count == ID2Header[id].PartsCount)
                {
                    List<byte> pool = new List<byte>(4096);
                    pool.AddRange(ID2Header[id].Data);
                    for (uint i0 = 0; i0 < ID2Header[id].PartsCount; i0++)
                    {
                        pool.AddRange(ID2Packet[id][i0]);
                    }
                    ID2Header.Remove(id);
                    ID2Packet.Remove(id);
                    RawMessagesOutput?.Invoke(this, pool.ToArray(), MessageBuggerIndexer.Raw);
                }
            }
        }

        private void RawMessagesInput(IPipelineOutput<byte[], MessageBuggerIndexer> sender, byte[] output, MessageBuggerIndexer index)
        {
            uint length = Serializer.GetLength(new Message());
            if (length + output.Length <= PacketSize)
            {
                SplittedMessagesOutput?.Invoke(this, Serializer.Serialize(new Message() { ID = PacketID.PacketWithoutHeader, Data = output }), MessageBuggerIndexer.Splitted);
            }
            else
            {
                IEnumerable<byte> pool = output;
                uint headerLength = Serializer.GetLength(new Header());
                uint packetLength = Serializer.GetLength(new Packet());
                uint messageLength = Serializer.GetLength(new Message());
                uint header_size = PacketSize - headerLength - messageLength;
                uint packet_size = PacketSize - packetLength - messageLength;
                uint parts_count = (uint)Math.Ceiling((double)(output.Length - header_size) / PacketSize);
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
                SplittedMessagesOutput?.Invoke(this, Serializer.Serialize(head), MessageBuggerIndexer.Splitted);
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
                    SplittedMessagesOutput?.Invoke(this, Serializer.Serialize(packet0), MessageBuggerIndexer.Splitted);
                }
            }
        }

        public void LinkInput(MessageBuggerIndexer inputIndex, IPipelineOutput<byte[], MessageBuggerIndexer> pipelineOutput, MessageBuggerIndexer outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<byte[], MessageBuggerIndexer>)[inputIndex];
        }

        public void UnlinkInput(MessageBuggerIndexer inputIndex, IPipelineOutput<byte[], MessageBuggerIndexer> pipelineOutput, MessageBuggerIndexer outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<byte[], MessageBuggerIndexer>)[inputIndex];
        }

        public void LinkInput(byte inputIndex, IPipelineOutput<MessageBuggerControl, byte> pipelineOutput, byte outputIndex)
        {
            pipelineOutput[outputIndex] += (this as IPipelineInput<MessageBuggerControl, byte>)[inputIndex];
        }

        public void UnlinkInput(byte inputIndex, IPipelineOutput<MessageBuggerControl, byte> pipelineOutput, byte outputIndex)
        {
            pipelineOutput[outputIndex] -= (this as IPipelineInput<MessageBuggerControl, byte>)[inputIndex];
        }

        public void LinkOutput(MessageBuggerIndexer outputIndex, IPipelineInput<byte[], MessageBuggerIndexer> pipelineInput, MessageBuggerIndexer inputIndex)
        {
            (this as IPipelineOutput<byte[], MessageBuggerIndexer>)[outputIndex] += pipelineInput[inputIndex];
        }

        public void UnlinkOutput(MessageBuggerIndexer outputIndex, IPipelineInput<byte[], MessageBuggerIndexer> pipelineInput, MessageBuggerIndexer inputIndex)
        {
            (this as IPipelineOutput<byte[], MessageBuggerIndexer>)[outputIndex] -= pipelineInput[inputIndex];
        }

        public void LinkOutput(byte outputIndex, IPipelineInput<MessageBuggerControl, byte> pipelineInput, byte inputIndex)
        {
            (this as IPipelineOutput<MessageBuggerControl, byte>)[outputIndex] += pipelineInput[inputIndex];
        }

        public void UnlinkOutput(byte outputIndex, IPipelineInput<MessageBuggerControl, byte> pipelineInput, byte inputIndex)
        {
            (this as IPipelineOutput<MessageBuggerControl, byte>)[outputIndex] -= pipelineInput[inputIndex];
        }
    }
}
