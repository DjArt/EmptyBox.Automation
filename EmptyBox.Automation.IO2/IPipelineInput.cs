using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineInput<TIn>
    {
        void Input(object source, TIn output);
    }

    public interface IPipelineInput<TIn, TIndex>
    {
        void Input(object source, TIn input, TIndex index);
    }

    public interface IPipelineInput<TIn, TIndex0, TIndex1>
    {
        void Input(object source, TIn input, TIndex0 index0, TIndex1 index1);
    }
}
