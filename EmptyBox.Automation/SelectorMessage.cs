using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public struct SelectorMessage<T> where T : struct
    {
        public T Data;
        public byte[] Message;
    }

    public sealed class RatioUpdate
    {
        public uint[] Ratio;
    }
}
