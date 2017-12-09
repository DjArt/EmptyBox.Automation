using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.IO.Automation
{
    public struct SelectorMessage<T> where T : struct
    {
        public T Data;
        public byte[] Message;
    }
}
