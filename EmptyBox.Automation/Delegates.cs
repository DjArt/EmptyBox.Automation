using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public delegate void OutputDelegate<TOutput, TIndex>(IPipelineOutput<TOutput, TIndex> sender, TOutput output, TIndex index);
}
