using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public interface IPipelineTaskIDMarker<T> : IPipelineInput<T>, IPipelineOutput<T>
    {
        
    }
}
