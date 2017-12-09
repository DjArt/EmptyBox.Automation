using System;
using System.Collections.Generic;
using System.Text;

namespace EmptyBox.Automation
{
    public class InputBlock<TInput, TState> : IPipelineInput<TInput>, IPipelineOutputInformer<TInput, TState>
    {
        public Action<InputBlock<TInput, TState>, IPipelineOutput<TInput>, ulong, TInput> InputAction { get; protected set; }
        public event InformerOutputHandleDelegate<TInput, TState> InformerOutputHandle;

        public InputBlock(Action<InputBlock<TInput, TState>, IPipelineOutput<TInput>, ulong, TInput> action)
        {
            InputAction = action;
        }

        public void Input(IPipelineOutput<TInput> sender, ulong taskID, TInput output)
        {
            InputAction?.Invoke(this, sender, taskID, output);
        }

        public void LinkInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle += Input;
        }

        public void UnlinkInput(IPipelineOutput<TInput> output)
        {
            output.OutputHandle -= Input;
        }
    }
}
