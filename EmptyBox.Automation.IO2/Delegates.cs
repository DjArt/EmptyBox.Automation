using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public delegate void OutputEvent<TOut>(object source, TOut output);
    public delegate void OutputEvent<TOut, TIndex>(object source, TOut output, TIndex index);
    public delegate void OutputEvent<TOut, TIndex0, TIndex1>(object source, TOut output, TIndex0 index0, TIndex1 index);
}
