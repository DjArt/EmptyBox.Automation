using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineOutput<TOut>
    {
        event OutputEvent<TOut> Output;
    }

    public interface IPipelineOutput<TOut, TIndex>
    {
        event OutputEvent<TOut, TIndex> Output;
    }

    public interface IPipelineOutput<TOut, TIndex0, TIndex1>
    {
        event OutputEvent<TOut, TIndex0, TIndex1> Output;
    }
}
