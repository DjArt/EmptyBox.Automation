using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineOutput<TOutput>
    {
        event EventHandler<TOutput> Output;
    }

    public interface IPipelineOutput<TOutput, TIndex>
    {
        EventHandler<TOutput> this[TIndex index] { get; set; }
    }

    public interface IPipelineOutput<TOutput, TIndex0, TIndex1>
    {
        EventHandler<TOutput> this[TIndex0 index0, TIndex1 index1] { get; set; }
    }
}
