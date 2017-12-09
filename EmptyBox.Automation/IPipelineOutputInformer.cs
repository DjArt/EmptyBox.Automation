using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public delegate void InformerOutputHandleDelegate<TInput, TState>(IPipelineInput<TInput> sender, ulong? taskID, TInput input, TState state);
    public interface IPipelineOutputInformer<TInput, TState> : IPipeline
    {
        event InformerOutputHandleDelegate<TInput, TState> InformerOutputHandle;
    }
}
