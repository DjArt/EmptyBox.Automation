using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineInput<TInput, TIndex>
    {
        OutputDelegate<TInput, TIndex> this[TIndex index] { get; }
        void LinkInput(TIndex inputIndex, IPipelineOutput<TInput, TIndex> pipelineOutput, TIndex outputIndex);
        void UnlinkInput(TIndex inputIndex, IPipelineOutput<TInput, TIndex> pipelineOutput, TIndex outputIndex);
    }
}
