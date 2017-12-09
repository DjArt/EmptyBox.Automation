using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public delegate void OutputHandleDelegate<TOutput>(IPipelineOutput<TOutput> sender, ulong taskID, TOutput output);
    public interface IPipelineOutput<TOutput> : IPipeline
    {
        event OutputHandleDelegate<TOutput> OutputHandle;
    }
}
