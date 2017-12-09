using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineInput<TInput> : IPipeline
    {
        void Input(IPipelineOutput<TInput> sender, ulong taskID, TInput output);
    }
}
