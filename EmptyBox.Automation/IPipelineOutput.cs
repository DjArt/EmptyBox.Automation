using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineOutput<TOutput, TIndex>
    {
        OutputDelegate<TOutput, TIndex> this[TIndex index] { get; set; }
        void LinkOutput(TIndex outputIndex, IPipelineInput<TOutput, TIndex> pipelineInput, TIndex inputIndex);
        void UnlinkOutput(TIndex outputIndex, IPipelineInput<TOutput, TIndex> pipelineInput, TIndex inputIndex);
    }
}
