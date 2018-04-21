using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation.Network
{
    public enum MessageBuggerIndexer
    {
        Raw,
        Splitted
    }

    public enum MessageBuggerControlStates
    {
        EjectBuffers,
        LoadBuffers,
        DublicatePacket
    }

    public struct MessageBuggerControl
    {
        public MessageBuggerControlStates State;
        public ulong SequenceID;
    }
}
