using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class InputBlock<TInput> : IPipelineInput<TInput>
    {
        public EventHandler<TInput> Action { get; set; }

        public InputBlock(EventHandler<TInput> action)
        {
            Action = action;
        }

        void IPipelineInput<TInput>.Input(object sender, TInput output)
        {
            Action?.Invoke(sender, output);
        }
    }
}
