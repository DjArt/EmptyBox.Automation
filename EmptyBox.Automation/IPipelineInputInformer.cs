using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineInputInformer<TOutput, TState> : IPipeline
    {
        void InformInput(IPipelineInput<TOutput> sender, ulong? taskID, TOutput input, TState state);
    }
}
