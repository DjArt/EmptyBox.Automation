using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineInput<TInput>
    {
        void Input(object sender, TInput output);
    }

    public interface IPipelineInput<TInput, TIndex>
    {
        EventHandler<TInput> this[TIndex index] { get; }
    }

    public interface IPipelineInput<TInput, TIndex0, TIndex1>
    {
        EventHandler<TInput> this[TIndex0 index0, TIndex1 index1] { get; }
    }
}
